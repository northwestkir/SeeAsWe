using System;
using System.Buffers;
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
			var factory = new DelegatingCsvParserComponentsFactory<TestType>(() => new ResultBuilder<TestType>(new TestType(), memberBuilders), () => new SkippingMemberOrderResolver());

			var config = new CsvParserConfig
			{
				Encoding = Encoding.UTF8,
				HasHeader = true,
				RentBytesBuffer = bufferSize
			};
			var target = new CsvParser<TestType>(config, factory);
			await using var stream = new MemoryStream();
			stream.Write(Encoding.UTF8.GetBytes(csv));
			stream.Position = 0;
			var items = await target.Read(stream).Select(it => it.Clone()).ToListAsync();
			Assert.That(items, Is.EquivalentTo(expected).Using(new TestTypeComparer()));
		}

		[TestCaseSource(nameof(GetTestCasesForBigArrayTests))]
		public async Task BigArrayTest(byte[] data, int expectedRows)
		{
			CsvParserConfig config = new CsvParserConfig
			{
				Encoding = Encoding.UTF8,
				ArrayPool = ArrayPool<byte>.Shared,
				HasHeader = true,
				RentBytesBuffer = 64,
				Separator = ',',
				SetMembersFromHeader = true
			};
			var memberBuilders = new[]
			{
				Utf8ParserMembers.Create<TestRecord>(new Utf8ParserPropertyMetadata(nameof(TestRecord.IntValue))),
				Utf8ParserMembers.Create<TestRecord>(new Utf8ParserPropertyMetadata(nameof(TestRecord.DecimalValue))),
				Utf8ParserMembers.Create<TestRecord>(new Utf8ParserPropertyMetadata(nameof(TestRecord.StringValue))),
			};
			var factory = new DelegatingCsvParserComponentsFactory<TestRecord>(
				() => new ResultBuilder<TestRecord>(new TestRecord(), memberBuilders),
				() => new Utf8MemberOrderResolver());
			var parser = new CsvParser<TestRecord>(config, factory);
			await using var stream = new MemoryStream(data);
			var result = parser.Read(stream);
			var rows = await result.CountAsync();

			Assert.AreEqual(expectedRows, rows);
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
			//TODO: add support of badly formatted csv
		}

		private static IEnumerable<TestCaseData> GetTestCasesForBigArrayTests()
		{
			var headerBytes = Encoding.UTF8.GetBytes("DecimalValue,IntValue,StringValue\r\n");
			var valueBytes = Encoding.UTF8.GetBytes("6547.325,6578,hello world\r\n");
			var maxRows = 1_000_000;
			var bytes = new Memory<byte>(new byte[headerBytes.Length + maxRows * valueBytes.Length]);
			headerBytes.CopyTo(bytes);
			var valueSpan = new ReadOnlyMemory<byte>(valueBytes);
			var headerLength = headerBytes.Length;
			var valueLength = valueSpan.Length;
			for (int rowNumber = 0; rowNumber < maxRows; rowNumber++)
			{
				valueSpan.CopyTo(bytes.Slice(headerLength + rowNumber * valueLength, valueLength));
				switch (rowNumber)
				{
					case 10 - 1:
					case 100 - 1:
					case 1000 - 1:
					case 10000 - 1:
					case 100000 - 1:
					case 1000000 - 1:
						yield return new TestCaseData(bytes.Slice(0, headerLength + valueLength + rowNumber * valueLength).ToArray(), rowNumber + 1).SetArgDisplayNames($"rowsCount={rowNumber}");
						break;
				}
			}
		}

		public class TestRecord
		{
			public string StringValue { get; set; }
			public int IntValue { get; set; }
			public decimal DecimalValue { get; set; }
		}
	}
}