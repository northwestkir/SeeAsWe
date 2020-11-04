using System;

namespace SeeAsWee.Core.MemberBuilders
{
	public class EmptyMemberBuilder<T> : MemberBuilder<T>
	{
		public EmptyMemberBuilder(string memberName)
		{
			Next = this;
			MemberName = memberName;
		}

		public override void SetValue(ReadOnlySpan<byte> data, T target)
		{
		}
	}
}