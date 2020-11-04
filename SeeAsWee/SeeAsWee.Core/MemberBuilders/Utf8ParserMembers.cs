using System;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SeeAsWee.Core.MemberBuilders
{
	public static class Utf8ParserMembers
	{
		private static readonly ModuleBuilder ModuleBuilder;
		private static readonly ConcurrentDictionary<string, Type> CreatedBuilders = new ConcurrentDictionary<string, Type>();

		static Utf8ParserMembers()
		{
			var assemblyName = new AssemblyName("SeeAsWee.Core.Dynamic");
			var builder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder = builder.DefineDynamicModule(assemblyName.Name + ".dll");
		}

		public static MemberBuilder<T> Create<T>(Utf8ParserPropertyMetadata propertyMetadata)
		{
			var objectType = typeof(T);
			var targetProperty = objectType.GetProperty(propertyMetadata.PropertyName, BindingFlags.Instance | BindingFlags.Public);
			var memberBuilderTypeName = $"{objectType.Name}_{targetProperty.Name}_{targetProperty.PropertyType}_MemberBuilder";
			var resultType = CreatedBuilders.GetOrAdd(memberBuilderTypeName, (key, arg) =>
			{
				var memberBuilderType = ModuleBuilder.DefineType(memberBuilderTypeName, TypeAttributes.Class, typeof(MemberBuilder<T>));
				var setValueMethod = memberBuilderType.DefineMethod(nameof(MemberBuilder<T>.SetValue), MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.Standard);
				var setValueMethodParameters = typeof(MemberBuilder<T>).GetMethod(nameof(MemberBuilder<T>.SetValue)).GetParameters();

				setValueMethod.SetParameters(setValueMethodParameters.Select(it => it.ParameterType).ToArray());
				foreach (var p in setValueMethodParameters)
				{
					setValueMethod.DefineParameter(p.Position + 1, p.Attributes, p.Name);
				}

				var il = setValueMethod.GetILGenerator();
				if (targetProperty.PropertyType == typeof(string))
				{
					GenerateViaUtf8Encoding(il, targetProperty);
				}
				else
				{
					GenerateViaUtf8Parser(propertyMetadata, il, targetProperty);
				}

				return memberBuilderType.CreateType();
			}, 1);

			//TODO: maybe we can optimize instance initialization somehow...
			return (MemberBuilder<T>) Activator.CreateInstance(resultType);
		}

		private static void GenerateViaUtf8Encoding(ILGenerator il, PropertyInfo targetProperty)
		{
			il.Emit(OpCodes.Ldarg_2);
			var getUtf8EncodingProperty = typeof(System.Text.Encoding).GetProperty(nameof(System.Text.Encoding.UTF8), BindingFlags.Public | BindingFlags.Static);
			if(getUtf8EncodingProperty==null)
				throw new Exception($"{nameof(System.Text.Encoding)} doesn't have property {nameof(System.Text.Encoding.UTF8)}");
			il.Emit(OpCodes.Call, getUtf8EncodingProperty.GetGetMethod());
			il.Emit(OpCodes.Ldarg_1);
			var getStringMethod = typeof(System.Text.Encoding).GetMethod(nameof(System.Text.Encoding.GetString), new[] {typeof(ReadOnlySpan<byte>)});
			il.EmitCall(OpCodes.Callvirt, getStringMethod, null);
			il.EmitCall(OpCodes.Callvirt, targetProperty.GetSetMethod(), null);
			il.Emit(OpCodes.Ret);
		}

		private static void GenerateViaUtf8Parser(Utf8ParserPropertyMetadata propertyMetadata, ILGenerator il, PropertyInfo targetProperty)
		{
			var localValue = il.DeclareLocal(targetProperty.PropertyType);
			var localBytesRead = il.DeclareLocal(typeof(int));
			var returnLabel = il.DefineLabel();

			il.Emit(OpCodes.Ldarg_1); //put data parameter onto stack
			il.Emit(OpCodes.Ldloca_S, localValue); //put value parameter onto stack
			il.Emit(OpCodes.Ldloca_S, localBytesRead); //put empty parameter onto stack
			il.Emit(OpCodes.Ldc_I4, propertyMetadata.DefaultFormat);
			var utf8ParserType = typeof(Utf8Parser);
			var methodInfo = utf8ParserType.GetMethod(
				nameof(Utf8Parser.TryParse),
				BindingFlags.Public | BindingFlags.Static,
				null,
				new[] {typeof(ReadOnlySpan<byte>), localValue.LocalType.MakeByRefType(), localBytesRead.LocalType.MakeByRefType(), typeof(char)}, null);
			if (methodInfo == null)
				throw new Exception($"{nameof(Utf8Parser)} doesn't have {nameof(Utf8Parser.TryParse)} to parse {nameof(ReadOnlySpan<byte>)} into {localValue.LocalType}");
			il.EmitCall(OpCodes.Call, methodInfo, null); //call TryParse
			il.Emit(OpCodes.Brfalse_S, returnLabel); //if TryParse == false go to returnLabel
			il.Emit(OpCodes.Ldarg_2); //put object
			il.Emit(OpCodes.Ldloc, localValue); //put localValue
			il.EmitCall(OpCodes.Callvirt, targetProperty.GetSetMethod(), null);
			il.MarkLabel(returnLabel);
			il.Emit(OpCodes.Ret);
		}
	}
}