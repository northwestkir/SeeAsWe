using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Core
{
	public class ResultBuilder<T>
	{
		private readonly T _result;
		private MemberBuilder<T> _current;
		private readonly MemberBuilder<T> _first;

		public int CurrentFieldFirstIndex;

		public void NextMember(byte[] buffer, int separatorIndex)
		{
			var length = separatorIndex - CurrentFieldFirstIndex;
			_current.SetValue(buffer, CurrentFieldFirstIndex, length - 1, _result);
			_current = _current.Next;
			CurrentFieldFirstIndex += length;
		}

		public T Complete(int offset)
		{
			_current = _first;
			CurrentFieldFirstIndex += offset;
			return _result;
		}
	}
}