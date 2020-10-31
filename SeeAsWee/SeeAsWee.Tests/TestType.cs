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
}