using System;

namespace SeeAsWee.Core.MemberBuilders
{
	public abstract class MemberBuilder<T>
	{
		public MemberBuilder<T> Next { get; set; }
		public string MemberName { get; set; }
		public string FieldName { get; set; }

		public abstract void SetValue(ReadOnlySpan<byte> data, T target);
	}
}