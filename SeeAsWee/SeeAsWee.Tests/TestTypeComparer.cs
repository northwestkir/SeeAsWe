using System;
using System.Collections.Generic;

namespace SeeAsWee.Tests
{
	public class TestTypeComparer : IComparer<TestType>
	{
		public int Compare(TestType x, TestType y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (ReferenceEquals(null, y)) return 1;
			if (ReferenceEquals(null, x)) return -1;
			var field1Comparison = string.Compare(x.Field1, y.Field1, StringComparison.Ordinal);
			if (field1Comparison != 0) return field1Comparison;
			var field2Comparison = x.Field2.CompareTo(y.Field2);
			if (field2Comparison != 0) return field2Comparison;
			return x.Field3.CompareTo(y.Field3);
		}
	}
}