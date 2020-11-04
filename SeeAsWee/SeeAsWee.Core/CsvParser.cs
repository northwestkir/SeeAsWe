using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SeeAsWee.Core
{
	public class CsvParser<T>
	{
		private readonly CsvParserConfig<T> _config;

		public CsvParser(CsvParserConfig<T> config)
		{
			_config = config;
		}

		public async IAsyncEnumerable<T> Read(Stream stream, [EnumeratorCancellation] CancellationToken ct = default)
		{
			var buffer = _config.ArrayPool.Rent(_config.RentBytesBuffer);
			var idx = 0;
			var separator = (byte) _config.Separator;
			const byte nextLineByte = (byte) '\n';

			var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
			if (_config.HasHeader)
			{
				idx = _config.BuildMapFromHeader
					? ParseHeader(bytesRead, buffer, nextLineByte, separator)
					: ReadHeader(bytesRead, buffer, nextLineByte);
			}

			var builder = _config.ResultBuilder;
			var currentFieldFirstIndex = idx;
			var newFieldStarted = false;
			var length = 0;
			do
			{
				while (idx < bytesRead)
				{
					newFieldStarted = true;
					var currentByte = buffer[idx++];

					switch (currentByte)
					{
						case (byte) ',':
							newFieldStarted = false;
							length = idx - currentFieldFirstIndex;
							builder.NextMember(buffer, currentFieldFirstIndex, length - 1);
							currentFieldFirstIndex += length;
							continue;
						case (byte) '\r' when buffer[idx] == (byte) '\n':
							newFieldStarted = false;
							length = idx - currentFieldFirstIndex;
							builder.NextMember(buffer, currentFieldFirstIndex, length - 1);
							currentFieldFirstIndex += length + 1;
							idx += 1;
							yield return builder.Complete();
							continue;
						case (byte) '\n':
							newFieldStarted = false;
							length = idx - currentFieldFirstIndex;
							builder.NextMember(buffer, currentFieldFirstIndex, length - 1);
							currentFieldFirstIndex += length;
							yield return builder.Complete();
							break;
					}
				}

				var incomplete = 0;
				if (newFieldStarted)
				{
					incomplete = idx - currentFieldFirstIndex;
					Buffer.BlockCopy(buffer, currentFieldFirstIndex, buffer, 0, incomplete);
					idx -= currentFieldFirstIndex;
				}
				else
				{
					idx = 0;
				}

				bytesRead = await stream.ReadAsync(buffer, idx, buffer.Length - incomplete, ct);
				currentFieldFirstIndex = 0;
			} while (bytesRead != 0);

			if (newFieldStarted)
			{
				builder.NextMember(buffer, currentFieldFirstIndex, length);
				yield return builder.Complete();
			}

			_config.ArrayPool.Return(buffer);
		}

		private int ReadHeader(in int bytesRead, byte[] buffer, in byte nextLineByte)
		{
			var idx = 0;
			while (idx++ < bytesRead)
			{
				if (buffer[idx] == nextLineByte)
				{
					break;
				}
			}

			return idx + 1;
		}

		private int ParseHeader(int bytesRead, byte[] buffer, byte nextLineByte, byte separator)
		{
			int fieldFirstByteIdx = 0, fieldIdx = 0, idx = 0;
			for (; idx < bytesRead; idx++)
			{
				if (buffer[idx] == nextLineByte)
				{
					MakeField(buffer, fieldFirstByteIdx, idx - fieldFirstByteIdx, fieldIdx);
					break;
				}

				if (buffer[idx] == separator)
				{
					MakeField(buffer, fieldFirstByteIdx, idx - fieldFirstByteIdx, fieldIdx);
					fieldFirstByteIdx += 1;
					fieldIdx += 1;
				}
			}

			return idx + 1;
		}

		private void MakeField(byte[] buffer, in int fieldFirstByteIdx, int index, in int fieldIdx)
		{
			throw new NotImplementedException();
		}
	}
}