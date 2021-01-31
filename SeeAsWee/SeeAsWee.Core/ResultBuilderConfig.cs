using SeeAsWee.Core.MemberBuilders;

namespace SeeAsWee.Core
{
	public class ResultBuilderConfig<T>
	{
		public MemberBuilder<T> First { get; }
		public MemberBuilder<T> Current { get; }

		public ResultBuilderConfig(MemberBuilder<T>[] builders)
		{
			First = builders[0];
			Current = First;
			for (var i = 1; i < builders.Length; i++)
			{
				var next = builders[i];
				Current.Next = next;
				Current = next;
			}

			Current = First;
		}
	}
}