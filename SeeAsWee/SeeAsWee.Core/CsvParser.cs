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
			int bytesRead;
			var buffer = _config.ArrayPool.Rent(_config.RentBytesBuffer);
			var idx = 0;
			var separator = (byte) _config.Separator;
			const byte nextLineByte = (byte) '\n';

			if (_config.HasHeader)
			{
				bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
				idx = _config.BuildMapFromHeader
					? ParseHeader(bytesRead, buffer, nextLineByte, separator)
					: ReadHeader(bytesRead, buffer, nextLineByte);
			}

			var builder = _config.ResultBuilder;
			do
			{
				var newLineStarted = false;
				bytesRead = await stream.ReadAsync(buffer, idx, buffer.Length - idx, ct);
				while (idx < bytesRead)
				{
					newLineStarted = true;
					var currentByte = buffer[idx++];
					if (currentByte == (byte) '\r' && buffer[idx] == (byte) '\n')
					{
						newLineStarted = false;
						builder.NextMember(buffer, idx);
						idx += 1;
						yield return builder.Complete(1);
						continue;
					}

					if (currentByte == (byte) '\n')
					{
						newLineStarted = false;
						builder.NextMember(buffer, idx);
						yield return builder.Complete(0);
						continue;
					}

					if (currentByte == (byte) ',')
					{
						builder.NextMember(buffer, idx);
					}
				}

				if (newLineStarted)
				{
					idx -= builder.CurrentFieldFirstIndex;
					builder.CurrentFieldFirstIndex = 0;
				}
			} while (bytesRead != 0);

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