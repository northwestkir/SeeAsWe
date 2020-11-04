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
		private static ModuleBuilder _moduleBuilder;
		private static readonly ConcurrentDictionary<string, Type> _createdBuilders = new ConcurrentDictionary<string, Type>();

		static Utf8ParserMembers()
		{
			var assemblyName = new AssemblyName("SeeAsWee.Core.Dynamic");
			var builder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			_moduleBuilder = builder.DefineDynamicModule(assemblyName.Name + ".dll");
		}

		public static MemberBuilder<T> Create<T>(Utf8ParserPropertyMetadata propertyMetadata)
		{
			var objectType = typeof(T);
			//TODO: check if property type is supported by Utf8Parser
			var targetProperty = objectType.GetProperty(propertyMetadata.PropertyName, BindingFlags.Instance | BindingFlags.Public);
			var memberBuilderTypeName = $"{objectType.Name}_{targetProperty.Name}_{targetProperty.PropertyType}_MemberBuilder";
			var resultType = _createdBuilders.GetOrAdd(memberBuilderTypeName, (key, arg) =>
			{
				var memberBuilderType = _moduleBuilder.DefineType(memberBuilderTypeName, TypeAttributes.Class, typeof(MemberBuilder<T>));
				var setValueMethod = memberBuilderType.DefineMethod(nameof(MemberBuilder<T>.SetValue), MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.Standard);
				var setValueMethodParameters = typeof(MemberBuilder<T>).GetMethod(nameof(MemberBuilder<T>.SetValue)).GetParameters();
				var utf8ParserType = typeof(Utf8Parser);

				setValueMethod.SetParameters(setValueMethodParameters.Select(it => it.ParameterType).ToArray());
				foreach (var p in setValueMethodParameters)
				{
					setValueMethod.DefineParameter(p.Position + 1, p.Attributes, p.Name);
				}

				var il = setValueMethod.GetILGenerator();
				var localValue = il.DeclareLocal(targetProperty.PropertyType);
				var localBytesRead = il.DeclareLocal(typeof(int));
				var returnLabel = il.DefineLabel();

				il.Emit(OpCodes.Ldarg_1); //put data parameter onto stack
				il.Emit(OpCodes.Ldloca_S, localValue); //put value parameter onto stack
				il.Emit(OpCodes.Ldloca_S, localBytesRead); //put empty parameter onto stack
				il.Emit(OpCodes.Ldc_I4, propertyMetadata.DefaultFormat);
				var methodInfo = utf8ParserType.GetMethod(nameof(Utf8Parser.TryParse), BindingFlags.Public | BindingFlags.Static, null, new[] {typeof(ReadOnlySpan<byte>), localValue.LocalType.MakeByRefType(), localBytesRead.LocalType.MakeByRefType(), typeof(char)}, null);
				il.EmitCall(OpCodes.Call, methodInfo, null); //call TryParse
				il.Emit(OpCodes.Brfalse_S, returnLabel); //if TryParse == false go to returnLabel
				il.Emit(OpCodes.Ldarg_2); //put object
				il.Emit(OpCodes.Ldloc, localValue); //put localValue
				il.EmitCall(OpCodes.Callvirt, targetProperty.GetSetMethod(), null);
				il.MarkLabel(returnLabel);
				il.Emit(OpCodes.Ret);
				return memberBuilderType.CreateType();
			}, 1);

			//TODO: maybe we can optimize instance initialization somehow...
			return (MemberBuilder<T>) Activator.CreateInstance(resultType);
		}
	}
}