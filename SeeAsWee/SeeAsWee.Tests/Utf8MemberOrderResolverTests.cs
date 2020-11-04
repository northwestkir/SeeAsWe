using System.Collections.Generic;
using NUnit.Framework;
using SeeAsWee.Core.MemberOrder;

namespace SeeAsWee.Tests
{
	[TestFixture]
	public class Utf8MemberOrderResolverTests
	{
		[TestCaseSource(nameof(GetCasesForHeaderParsingTests))]
		public void HeaderParsingTests(string header, List<string> expected)
		{
			var target = new Utf8MemberOrderResolver();
			var bytes = System.Text.Encoding.UTF8.GetBytes(header);
			var result = new List<string>();
			target.ParseHeader(bytes.Length, bytes, (byte) '\n', (byte) ',', result);
			Assert.That(result, Is.EqualTo(expected).AsCollection);
		}

		private static IEnumerable<TestCaseData> GetCasesForHeaderParsingTests()
		{
			yield return new TestCaseData("Field1,Field2,Field3\n", new List<string> {"Field1", "Field2", "Field3"});
			yield return new TestCaseData("Field1,Field2,Field3\r\n", new List<string> {"Field1", "Field2", "Field3"});
			yield return new TestCaseData("Field1,Field2,Field3", new List<string> {"Field1", "Field2", "Field3"});
		}
	}
}