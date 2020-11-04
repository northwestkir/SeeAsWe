using System.Collections.Generic;

namespace SeeAsWee.Core.MemberOrder
{
	public class SkippingMemberOrderResolver : IMemberOrderResolver
	{
		public int ParseHeader(in int bytesRead, byte[] buffer, in byte nextLineByte, in byte separator, List<string> members)
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
	}
}