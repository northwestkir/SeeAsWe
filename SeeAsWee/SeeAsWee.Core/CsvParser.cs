using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SeeAsWee.Core
{
	public class CsvParser<T>
	{
		private readonly CsvParserConfig _config;
		private readonly ICsvParserComponentsFactory<T> _componentsFactory;

		public CsvParser(CsvParserConfig config, ICsvParserComponentsFactory<T> componentsFactory)
		{
			_config = config;
			_componentsFactory = componentsFactory;
		}

		//TODO:implement reading from memory stream
		//TODO:implement reading into an array (without cloning)
		//TODO:resolve small ArrayPool.Rent value (<512)

		public async IAsyncEnumerable<T> Read(Stream stream, [EnumeratorCancellation] CancellationToken ct = default)
		{
			var buffer = _config.ArrayPool.Rent(_config.RentBytesBuffer);
			var idx = 0;
			var separator = (byte) _config.Separator;
			const byte nextLineByte = (byte) '\n';
			var resultBuilder = _componentsFactory.CreateResultBuilder();

			var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
			if (_config.HasHeader)
			{
				var memberOrderResolver = _componentsFactory.CreateMemberOrderResolver();
				var members = new List<string>();
				idx = memberOrderResolver.ParseHeader(bytesRead, buffer, nextLineByte, separator, members);
				if (_config.SetMembersFromHeader)
				{
					resultBuilder.ReorderMembers(members);
				}
			}

			var currentFieldFirstIndex = idx;
			var newFieldStarted = false;
			var length = 0;
			var incomplete = 0;
			do
			{
				while (idx < bytesRead + incomplete)
				{
					newFieldStarted = true;
					var currentByte = buffer[idx++];

					if (currentByte == separator)
					{
						newFieldStarted = false;
						length = idx - currentFieldFirstIndex;
						resultBuilder.NextMember(new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length;
						continue;
					}

					if (currentByte == (byte) '\r' && buffer[idx] == (byte) '\n')
					{
						newFieldStarted = false;
						length = idx - currentFieldFirstIndex;
						resultBuilder.NextMember(new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length + 1;
						idx += 1;
						yield return resultBuilder.Complete();
						continue;
					}

					if (currentByte == (byte) '\n')
					{
						newFieldStarted = false;
						length = idx - currentFieldFirstIndex;
						resultBuilder.NextMember(new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length;
						yield return resultBuilder.Complete();
					}
				}

				incomplete = 0;
				if (newFieldStarted)
				{
					incomplete = idx - currentFieldFirstIndex;
					Buffer.BlockCopy(buffer, currentFieldFirstIndex, buffer, 0, incomplete);
					idx = incomplete;
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
				resultBuilder.NextMember(new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, incomplete));
				yield return resultBuilder.Complete();
			}

			_config.ArrayPool.Return(buffer);
		}
	}
}