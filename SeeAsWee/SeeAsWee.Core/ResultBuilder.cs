using System;
using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Core
{
	public class ResultBuilder<T>
	{
		private readonly T _result;
		private MemberBuilder<T> _current;
		private readonly MemberBuilder<T> _first;


		public ResultBuilder(T result, MemberBuilder<T> first)
		{
			_result = result;
			_first = first;
			_current = first;
		}

		public void NextMember(byte[] buffer, in int start, in int length)
		{
			_current.SetValue(new ReadOnlySpan<byte>(buffer, start, length), _result);
			_current = _current.Next;
		}

		public T Complete()
		{
			_current = _first;
			return _result;
		}
	}
}