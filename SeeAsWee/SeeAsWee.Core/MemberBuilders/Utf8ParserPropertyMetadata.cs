using System;

namespace SeeAsWee.Core.MemberBuilders
{
	public class Utf8ParserPropertyMetadata
	{
		public Utf8ParserPropertyMetadata(string name, char defaultFormat = '\0')
		{
			PropertyName = name ?? throw new ArgumentNullException(nameof(name));
			DefaultFormat = defaultFormat;
		}

		public string PropertyName { get; }
		public char DefaultFormat { get; }
	}
}