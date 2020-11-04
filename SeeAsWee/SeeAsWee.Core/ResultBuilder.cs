using System;
using System.Collections.Generic;
using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Core
{
	public class ResultBuilder<T>
	{
		private readonly T _result;
		private MemberBuilder<T> _current;
		private MemberBuilder<T> _first;

		public ResultBuilder(T result, MemberBuilder<T>[] builders)
		{
			_result = result;
			_first = builders[0];
			_current = _first;
			for (var i = 1; i < builders.Length; i++)
			{
				var next = builders[i];
				_current.Next = next;
				_current = next;
			}

			_current = _first;
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

		public void ReorderMembers(List<string> fields)
		{
			var builders = new Dictionary<string, MemberBuilder<T>>();
			var builder = _first;
			while (builder != null)
			{
				builders.Add(builder.MemberName, builder);
				builder = builder.Next;
			}

			if (!builders.TryGetValue(fields[0], out var next)) next = new EmptyMemberBuilder<T>(fields[0]);
			_first = next;
			_current = _first;
			for (var i = 1; i < fields.Count; i++)
			{
				if (!builders.TryGetValue(fields[i], out next))
					next = new EmptyMemberBuilder<T>(fields[i]);
				_current.Next = next;
				_current = next;
			}

			_current = _first;
		}
	}
}