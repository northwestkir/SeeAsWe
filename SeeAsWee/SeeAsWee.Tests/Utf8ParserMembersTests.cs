using System;
using NUnit.Framework;
using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Tests
{
	[TestFixture]
	public class Utf8ParserMembersTests
	{
		[Test]
		public void BuildForTestType()
		{
			var builder = Utf8ParserMembers.Create<TestType>(nameof(TestType.Field2));
			var obj = new TestType();
			builder.SetValue(new ReadOnlySpan<byte>(System.Text.Encoding.UTF8.GetBytes("12.8")), obj);
			Assert.AreEqual(12.8, obj.Field2);
		}
	}
}