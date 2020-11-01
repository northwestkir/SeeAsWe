using System;

namespace SeeAsWee.Core.MemberBuilders
{
	public class EmptyMemberBuilder<T> : MemberBuilder<T>
	{
		public EmptyMemberBuilder()
		{
			Next = this;
		}

		public override void SetValue(ReadOnlySpan<byte> data, T target)
		{
		}
	}
}