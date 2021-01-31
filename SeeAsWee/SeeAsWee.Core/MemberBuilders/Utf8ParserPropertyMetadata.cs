using System;

namespace SeeAsWee.Core.MemberBuilders
{
	public class Utf8ParserPropertyMetadata
	{
		public Utf8ParserPropertyMetadata(string name, string fieldName=null, char defaultFormat = '\0')
		{
			PropertyName = name ?? throw new ArgumentNullException(nameof(name));
			FieldName = fieldName ?? name;
			DefaultFormat = defaultFormat;

		}

		public string PropertyName { get; }
		public char DefaultFormat { get; }
		public string FieldName { get; }
	}
}