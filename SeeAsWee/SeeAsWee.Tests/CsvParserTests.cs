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
		[Test]
		public async Task SimpleTest()
		{
			MemberBuilder<TestType> first = new EmptyMemberBuilder<TestType>();
			var second = Utf8ParserMembers.ForDecimal<TestType>((it, value) => it.Field2 = value);
			first.Next = second;
			second.Next = Utf8ParserMembers.ForInt<TestType>((it, value) => it.Field3 = value);
			var config = new CsvParserConfig<TestType>
			{
				Encoding = Encoding.UTF8,
				BuildMapFromHeader = false,
				HasHeader = true,
				ResultBuilder = new ResultBuilder<TestType>(new TestType(),first)
			};
			var target = new CsvParser<TestType>(config);
			var sb = new StringBuilder()
				.AppendLine("Field1,Field2,Field3")
				.AppendLine("text,0.58,12");
			await using var stream = new MemoryStream();
			stream.Write(Encoding.UTF8.GetBytes(sb.ToString()));
			stream.Position = 0;
			var items = await target.Read(stream).Select(it => it.Clone()).ToListAsync();

			Assert.That(items, Is.EquivalentTo(new[] {new TestType {Field1 = null, Field2 = 0.58m, Field3 = 12}}).Using<TestType>((a, b) => a.Field1 == b.Field1 && a.Field2 == b.Field2 && a.Field3 == b.Field3));
		}
	}

	public class TestType
	{
		public string Field1 { get; set; }
		public decimal Field2 { get; set; }
		public long Field3 { get; set; }

		public TestType Clone()
		{
			return (TestType) MemberwiseClone();
		}
	}
}