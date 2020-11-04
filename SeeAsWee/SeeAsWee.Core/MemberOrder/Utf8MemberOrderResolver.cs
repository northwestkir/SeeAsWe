using System;
using System.Collections.Generic;

namespace SeeAsWee.Core.MemberOrder
{
	public class Utf8MemberOrderResolver : IMemberOrderResolver
	{
		public int ParseHeader(in int bytesRead, byte[] buffer, in byte nextLineByte, in byte separator, List<string> members)
		{
			if (members == null)
				throw new ArgumentNullException(nameof(members));
			int fieldFirstByteIdx = 0, idx = 0;
			bool searchStarted = false;
			for (; idx < bytesRead; idx++)
			{
				searchStarted = true;
				if (buffer[idx] == nextLineByte)
				{
					searchStarted = false;
					members.Add(System.Text.Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, fieldFirstByteIdx, idx - fieldFirstByteIdx)).TrimEnd());
					break;
				}

				if (buffer[idx] == separator)
				{
					searchStarted = false;
					members.Add(System.Text.Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, fieldFirstByteIdx, idx - fieldFirstByteIdx)));
					fieldFirstByteIdx = idx + 1;
				}
			}

			if (searchStarted)
				members.Add(System.Text.Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, fieldFirstByteIdx, idx - fieldFirstByteIdx)));
			return idx + 1;
		}
	}
}