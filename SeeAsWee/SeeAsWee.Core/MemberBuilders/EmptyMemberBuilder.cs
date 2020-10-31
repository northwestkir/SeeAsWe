namespace SeeAsWee.Core.MemberBuilders
{
	public class EmptyMemberBuilder<T> : MemberBuilder<T>
	{
		public EmptyMemberBuilder()
		{
			Next = this;
		}

		public override void SetValue(byte[] buffer, in int start, in int length, T result)
		{
		}
	}
}