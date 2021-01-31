using System.Collections.Generic;

namespace SeeAsWee.Core.MemberOrder
{
	public class SkippingMemberOrderResolver
	{
		public int ParseHeader(in int bytesRead, byte[] buffer, in byte nextLineByte)
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