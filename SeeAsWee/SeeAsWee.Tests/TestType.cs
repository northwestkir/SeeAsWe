using System;
using System.Buffers.Text;
using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Tests
{
	public class TestType
	{
		public string Field1 { get; set; }
		public decimal Field2 { get; set; }
		public long Field3 { get; set; }

		public override string ToString() => $"{nameof(Field1)}={Field1},{nameof(Field2)}={Field2},{nameof(Field3)}={Field3}";

		public TestType Clone()
		{
			return (TestType) MemberwiseClone();
		}
	}

	public class Utf8TestTypeField2MemberBuilder : MemberBuilder<TestType>
	{
		public override void SetValue(ReadOnlySpan<byte> data, TestType target)
		{
			if (Utf8Parser.TryParse(data, out decimal value, out _, '\0'))
				target.Field2 = value;
		}
	}
}