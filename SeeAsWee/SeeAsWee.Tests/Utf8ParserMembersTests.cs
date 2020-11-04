using System;
using System.Collections.Generic;
using NUnit.Framework;
using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Tests
{
	[TestFixture]
	public class Utf8ParserMembersTests
	{
		[TestCaseSource(nameof(GetCasesForSupportedTypesParsingTest))]
		public void SupportedTypesParsingTest<T>(string value, T expected, char format)
		{
			var builder = Utf8ParserMembers.Create<ValueHolder<T>>(new Utf8ParserPropertyMetadata(nameof(ValueHolder<T>.Value), format));
			var obj = new ValueHolder<T>();
			builder.SetValue(new ReadOnlySpan<byte>(System.Text.Encoding.UTF8.GetBytes(value)), obj);
			Assert.AreEqual(expected, obj.Value);
		}

		private static IEnumerable<TestCaseData> GetCasesForSupportedTypesParsingTest()
		{
			var defaultFormat = '\0';
			yield return new TestCaseData("true", (bool) true, defaultFormat);
			yield return new TestCaseData("false", (bool) false, defaultFormat);
			yield return new TestCaseData("0", (bool) false, defaultFormat);
			yield return new TestCaseData("1", (bool) false, defaultFormat);
			yield return new TestCaseData("2020-05-22T21:48:59.3210000", new DateTime(2020, 05, 22, 21, 48, 59, 321), 'O');
			yield return new TestCaseData("12.8", (double) 12.8, defaultFormat);
			yield return new TestCaseData("12.8", (float) 12.8, defaultFormat);
			yield return new TestCaseData("12.8", (decimal) 12.8, defaultFormat);
			yield return new TestCaseData("12", (int) 12, defaultFormat);
			var newGuid = Guid.NewGuid();
			yield return new TestCaseData(newGuid.ToString(), newGuid,defaultFormat);
			yield return new TestCaseData("12", (sbyte) 12, defaultFormat);
			yield return new TestCaseData("12", (short) 12, defaultFormat);
			yield return new TestCaseData("12", (long) 12, defaultFormat);
			yield return new TestCaseData("12", (byte) 12, defaultFormat);
			yield return new TestCaseData("12", (ushort) 12, defaultFormat);
			yield return new TestCaseData("12", (uint) 12, defaultFormat);
			yield return new TestCaseData("12", (ulong) 12, defaultFormat);
			var timeSpan = new TimeSpan(09, 23, 59, 58, 987);
			yield return new TestCaseData("09.23:59:58.987", timeSpan, defaultFormat);
			yield return new TestCaseData("09.23:59:58.987", "09.23:59:58.987", defaultFormat);
		}

		public class ValueHolder<T>
		{
			public T Value { get; set; }
		}
	}
}