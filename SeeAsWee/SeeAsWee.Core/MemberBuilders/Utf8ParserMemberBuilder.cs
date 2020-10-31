using System;
using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Core
{
	public abstract class Utf8ParserMemberBuilder<T, TMemberType> : MemberBuilder<T>
	{
		private readonly Action<T, TMemberType> _setValue;

		protected Utf8ParserMemberBuilder(Action<T, TMemberType> setValue)
		{
			_setValue = setValue;
		}

		public sealed override void SetValue(byte[] buffer, in int start, in int length, T result)
		{
			if (ParseInternal(buffer, start, length, out TMemberType value))
			{
				_setValue(result, value);
			}
		}

		protected abstract bool ParseInternal(byte[] buffer, in int start, in int length, out TMemberType memberType);
	}
}