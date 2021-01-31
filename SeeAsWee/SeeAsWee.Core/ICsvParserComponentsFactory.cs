using SeeAsWee.Core.MemberOrder;

namespace SeeAsWee.Core
{
	public interface ICsvParserComponentsFactory<T>
	{
		ResultBuilder<T> CreateResultBuilder();
		IMemberOrderResolver CreateMemberOrderResolver();
	}
}