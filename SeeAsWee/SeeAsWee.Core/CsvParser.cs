using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using SeeAsWee.Core.MemberOrder;

namespace SeeAsWee.Core
{
	public class CsvParser<T>
	{
		private readonly CsvParserConfig _config;
		private readonly ResultBuilderConfig<T> _resultBuilderConfig;

		public CsvParser(CsvParserConfig config, ResultBuilderConfig<T> resultBuilderConfig)
		{
			_config = config;
			_resultBuilderConfig = resultBuilderConfig;
		}

		//TODO:implement reading from memory stream
		//TODO:implement reading into an array (without cloning)
		//TODO:resolve small ArrayPool.Rent value (<512)

		public async IAsyncEnumerable<T> Read(T result, Stream stream, [EnumeratorCancellation] CancellationToken ct = default)
		{
			var buffer = _config.ArrayPool.Rent(_config.RentBytesBuffer);
			var idx = 0;
			var separator = (byte) _config.Separator;
			const byte nextLineByte = (byte) '\n';
			var resultBuilder = new ResultBuilder<T>(_resultBuilderConfig);

			var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
			if (_config.HasHeader)
			{
				if (_config.SetMembersFromHeader)
				{
					var memberOrderResolver = new Utf8MemberOrderResolver();
					var members = new List<string>();
					idx = memberOrderResolver.ParseHeader(bytesRead, buffer, nextLineByte, separator, members);
					resultBuilder.ReorderMembers(members);
				}
				else
				{
					var skipper = new SkippingMemberOrderResolver();
					idx = skipper.ParseHeader(bytesRead, buffer, nextLineByte);
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
						resultBuilder.NextMember(result,new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length;
						continue;
					}

					if (currentByte == (byte) '\r' && buffer[idx] == (byte) '\n')
					{
						newFieldStarted = false;
						length = idx - currentFieldFirstIndex;
						resultBuilder.NextMember(result, new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length + 1;
						idx += 1;
						resultBuilder.Complete();
						yield return result;
						continue;
					}

					if (currentByte == (byte) '\n')
					{
						newFieldStarted = false;
						length = idx - currentFieldFirstIndex;
						resultBuilder.NextMember(result, new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length;
						resultBuilder.Complete();
						yield return result;
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
				resultBuilder.NextMember(result, new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, incomplete));
				resultBuilder.Complete();
				yield return result;
			}

			_config.ArrayPool.Return(buffer);
		}
	}
}