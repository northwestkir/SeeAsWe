namespace SeeAsWee.Core.MemberBuilders
{
	public abstract class MemberBuilder<T>
	{
		public MemberBuilder<T> Next { get; set; }

		public abstract void SetValue(byte[] buffer, in int start, in int length, T result);
	}
}