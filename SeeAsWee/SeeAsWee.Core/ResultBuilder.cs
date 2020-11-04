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

		public void NextMember(in ReadOnlySpan<byte> data)
		{
			_current.SetValue(data, _result);
			_current = _current.Next;
		}

		public T Complete()
		{
			_current = _first;
			return _result;
		}
	}
}