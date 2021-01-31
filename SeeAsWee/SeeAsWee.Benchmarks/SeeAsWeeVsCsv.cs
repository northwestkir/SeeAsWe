using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CsvHelper;
using CsvHelper.Configuration;
using SeeAsWee.Core;
using SeeAsWee.Core.MemberBuilders;
using SeeAsWee.Core.MemberOrder;

namespace SeeAsWee.Benchmarks
{
	public class SeeAsWeeVsCsv
	{
		[Benchmark(Description = nameof(CsvHelperRun))]
		[ArgumentsSource(nameof(GetDataForCsvHelperRun))]
		public async Task CsvHelperRun(byte[] data)
		{
			var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) {HasHeaderRecord = true};
			csvConfiguration.RegisterClassMap(new TestRecordClassMap());

			await using var stream = new MemoryStream(data);
			using var reader = new CsvReader(new StreamReader(stream), csvConfiguration);
			var result = reader.EnumerateRecordsAsync(new TestRecord());
			await foreach (var r in result)
			{
			}
		}

		[Benchmark(Description = nameof(SeeAsWeeParcerRun))]
		[ArgumentsSource(nameof(GetDataForCsvHelperRun))]
		public async Task SeeAsWeeParcerRun(byte[] data)
		{
			CsvParserConfig config = new CsvParserConfig
			{
				Encoding = Encoding.UTF8,
				ArrayPool = ArrayPool<byte>.Shared,
				HasHeader = true,
				RentBytesBuffer = 5 * 1024,
				Separator = ',',
				SetMembersFromHeader = true
			};
			var memberBuilders = new[]
			{
				Utf8ParserMembers.Create<TestRecord>(new Utf8ParserPropertyMetadata(nameof(TestRecord.IntValue))),
				Utf8ParserMembers.Create<TestRecord>(new Utf8ParserPropertyMetadata(nameof(TestRecord.DecimalValue))),
				Utf8ParserMembers.Create<TestRecord>(new Utf8ParserPropertyMetadata(nameof(TestRecord.StringValue))),
			};

			var parser = new CsvParser<TestRecord>(config, new DelegatingCsvParserComponentsFactory<TestRecord>(() => new ResultBuilder<TestRecord>(new TestRecord(), memberBuilders), () => new Utf8MemberOrderResolver()));
			await using var stream = new MemoryStream(data);
			var result = parser.Read(stream);
			await foreach (var r in result)
			{
			}
		}

		public IEnumerable<byte[]> GetDataForCsvHelperRun()
		{
			var headerBytes = Encoding.UTF8.GetBytes("DecimalValue,IntValue,StringValue\r\n");
			var valueBytes = Encoding.UTF8.GetBytes("6547.325,6578,hello world\r\n");
			const int maxRows = 1_000_000;
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
					case 100 - 1:
					case 1000 - 1:
					case 10000 - 1:
					case 100000 - 1:
					case 1000000 - 1:
						yield return bytes.Slice(0, headerLength + valueLength + rowNumber * valueLength).ToArray();
						break;
				}
			}
		}

		private sealed class TestRecordClassMap : ClassMap<TestRecord>
		{
			public TestRecordClassMap()
			{
				Map(r => r.DecimalValue).Name("DecimalValue");
				Map(r => r.IntValue).Name("IntValue");
				Map(r => r.StringValue).Name("StringValue");
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