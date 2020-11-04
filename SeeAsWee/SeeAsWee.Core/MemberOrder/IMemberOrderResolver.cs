using System.Collections.Generic;

namespace SeeAsWee.Core.MemberOrder
{
	public interface IMemberOrderResolver
	{
		int ParseHeader(in int bytesRead, byte[] buffer, in byte nextLineByte, in byte separator, List<string> members);
	}
}