using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SeeAsWee.Core;
using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Tests
{
	[TestFixture]
	public class CsvParserTests
	{
		//TODO: add cases when there is no header

		[TestCaseSource(nameof(GetSimpleTestCases))]
		public async Task SimpleTest(string csv, TestType[] expected, int bufferSize)
		{
			var memberBuilders = new[]
			{
				new EmptyMemberBuilder<TestType>(nameof(TestType.Field1)),
				Utf8ParserMembers.Create<TestType>(new Utf8ParserPropertyMetadata(nameof(TestType.Field2))),
				Utf8ParserMembers.Create<TestType>(new Utf8ParserPropertyMetadata(nameof(TestType.Field3)))
			};
			var resultBuilder = new ResultBuilder<TestType>(new TestType(), memberBuilders);

			var config = new CsvParserConfig
			{
				Encoding = Encoding.UTF8,
				HasHeader = true,
				RentBytesBuffer = bufferSize
			};
			var target = new CsvParser<TestType>(config,resultBuilder);
			await using var stream = new MemoryStream();
			stream.Write(Encoding.UTF8.GetBytes(csv));
			stream.Position = 0;
			var items = await target.Read(stream).Select(it => it.Clone()).ToListAsync();
			Assert.That(items, Is.EquivalentTo(expected).Using(new TestTypeComparer()));
		}

		private static IEnumerable<TestCaseData> GetSimpleTestCases()
		{
			var data = new TestType {Field1 = null, Field2 = 0.58m, Field3 = 12};
			yield return new TestCaseData("Field1,Field2,Field3\r\ntext,0.58,12\r\ntext,0.58,12\r\ntext,0.58,12", new[] {data, data, data}, 32).SetArgDisplayNames("Buffer 32 ends on field separator");
			yield return new TestCaseData("Field1,Field2,Field3\r\ntext,0.58,12\r\ntext,0.58,12\r\ntext,0.58,12\r\n", new[] {data, data, data}, 32).SetArgDisplayNames("Buffer 32 ends on field separator ends new line");
			yield return new TestCaseData("Field1,Field2,Field3\r\ntext,00.58,12\r\ntext,0.58,12\r\ntext,0.58,12", new[] {data, data, data}, 32).SetArgDisplayNames("Buffer 32 ends before field separator");
			yield return new TestCaseData("Field1,Field2,Field3\r\ntext,00.58,12\r\ntext,0.58,12\r\ntext,0.58,12\r\n", new[] {data, data, data}, 32).SetArgDisplayNames("Buffer 32 ends before field separator ends new line");
			yield return new TestCaseData("Field1,Field2,Field3\r\ntex,0.58,12\r\ntext,0.58,12\r\ntext,0.58,12", new[] {data, data, data}, 32).SetArgDisplayNames("Buffer 32 ends after field separator");
			yield return new TestCaseData("Field1,Field2,Field3\r\ntext,0.58,12\r\ntext,0.58,12\r\ntext,0.58,12\r\n", new[] {data, data, data}, 32).SetArgDisplayNames("Buffer 32 ends after field separator ends new line");

			//TODO: add test where buffer ends at \r after \r and after \n
			//TODO: add tests where new line is just \n
		}
	}
}