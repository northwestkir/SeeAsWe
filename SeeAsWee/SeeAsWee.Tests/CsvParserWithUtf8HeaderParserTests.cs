using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SeeAsWee.Core;
using SeeAsWee.Core.MemberBuilders;
using SeeAsWee.Core.MemberOrder;

namespace SeeAsWee.Tests
{
	[TestFixture]
	public class CsvParserWithUtf8HeaderParserTests
	{
		[TestCaseSource(nameof(GetCasesForHeaderParserEnabledTest))]
		public async Task HeaderParserEnabledTest(string csv, string[] propertiesToSet, TestType[] expected)
		{
			var memberBuilders = new MemberBuilder<TestType>[propertiesToSet.Length];
			for (var i = 0; i < propertiesToSet.Length; i++)
			{
				memberBuilders[i] = Utf8ParserMembers.Create<TestType>(new Utf8ParserPropertyMetadata(propertiesToSet[i]));
			}

			var resultBuilder = new ResultBuilder<TestType>(new TestType(), memberBuilders);

			var config = new CsvParserConfig
			{
				Encoding = Encoding.UTF8,
				HasHeader = true,
				RentBytesBuffer = 128,
				SetMembersFromHeader = true
			};

			var target = new CsvParser<TestType>(config, resultBuilder, new Utf8MemberOrderResolver());
			await using var stream = new MemoryStream();
			stream.Write(Encoding.UTF8.GetBytes(csv));
			stream.Position = 0;
			var items = await target.Read(stream).Select(it => it.Clone()).ToListAsync();
			Assert.That(items, Is.EquivalentTo(expected).Using(new TestTypeComparer()));
		}

		private static IEnumerable<TestCaseData> GetCasesForHeaderParserEnabledTest()
		{
			var tc1Data = new TestType {Field1 = null, Field2 = 0.58m, Field3 = 12};
			yield return new TestCaseData("Field1,Field2,Field3\r\ntext,0.58,12\r\ntext,0.58,12\r\n", new[] {"Field2", "Field3"}, new[] {tc1Data, tc1Data});
			var tc2Data = new TestType {Field1 = "text", Field2 = 0.58m, Field3 = 123};
			yield return new TestCaseData("Field1,Field2,Field3\r\ntext,0.58,123\r\ntext,0.58,123", new[] {"Field1", "Field2", "Field3"}, new[] {tc2Data, tc2Data});
			var tc3Data = new TestType {Field2 = 0.58m, Field3 = 123};
			yield return new TestCaseData("Field2,Field3\r\n0.58,123\r\n0.58,123\r\n", new[] {"Field1", "Field2", "Field3"}, new[] {tc3Data, tc3Data});
		}
	}
}