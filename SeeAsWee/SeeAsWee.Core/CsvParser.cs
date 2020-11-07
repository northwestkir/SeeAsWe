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
		private readonly ResultBuilder<T> _resultBuilder;
		private readonly IMemberOrderResolver _memberOrderResolver;

		public CsvParser(CsvParserConfig config, ResultBuilder<T> resultBuilder) : this(config, resultBuilder, new SkippingMemberOrderResolver())
		{
		}

		public CsvParser(CsvParserConfig config, ResultBuilder<T> resultBuilder, IMemberOrderResolver memberOrderResolver)
		{
			_config = config;
			_resultBuilder = resultBuilder;
			_memberOrderResolver = memberOrderResolver;
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

			var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
			if (_config.HasHeader)
			{
				var members = new List<string>();
				idx = _memberOrderResolver.ParseHeader(bytesRead, buffer, nextLineByte, separator, members);
				if (_config.SetMembersFromHeader)
				{
					_resultBuilder.ReorderMembers(members);
				}
			}

			var currentFieldFirstIndex = idx;
			var newFieldStarted = false;
			var length = 0;
			do
			{
				while (idx < bytesRead)
				{
					newFieldStarted = true;
					var currentByte = buffer[idx++];

					if (currentByte == separator)
					{
						newFieldStarted = false;
						length = idx - currentFieldFirstIndex;
						_resultBuilder.NextMember(new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length;
						continue;
					}

					if (currentByte == (byte) '\r' && buffer[idx] == (byte) '\n')
					{
						newFieldStarted = false;
						length = idx - currentFieldFirstIndex;
						_resultBuilder.NextMember(new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length + 1;
						idx += 1;
						yield return _resultBuilder.Complete();
						continue;
					}

					if (currentByte == (byte) '\n')
					{
						newFieldStarted = false;
						length = idx - currentFieldFirstIndex;
						_resultBuilder.NextMember(new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length - 1));
						currentFieldFirstIndex += length;
						yield return _resultBuilder.Complete();
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
				_resultBuilder.NextMember(new ReadOnlySpan<byte>(buffer, currentFieldFirstIndex, length));
				yield return _resultBuilder.Complete();
			}

			_config.ArrayPool.Return(buffer);
		}
	}
}