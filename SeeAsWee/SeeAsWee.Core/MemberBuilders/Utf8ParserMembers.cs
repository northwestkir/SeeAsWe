using System;
using System.Buffers.Text;

namespace SeeAsWee.Core.MemberBuilders
{
	public static class Utf8ParserMembers
	{
		public static Utf8ParserMemberBuilder<T, decimal> ForDecimal<T>(Action<T,decimal> setValue)
		{
			return new DecimalUtf8ParserMemberBuilder<T>(setValue);
		}

		private class DecimalUtf8ParserMemberBuilder<T>:Utf8ParserMemberBuilder<T,decimal>
		{
			public DecimalUtf8ParserMemberBuilder(Action<T,decimal> setValue):base(setValue)
			{
			}

			protected override bool ParseInternal(byte[] buffer, in int start, in int length, out decimal value) => Utf8Parser.TryParse(new ReadOnlySpan<byte>(buffer, start, length), out value, out _);
		}

		private class LongUtf8ParserMemberBuilder<T>:Utf8ParserMemberBuilder<T,long>
		{
			public LongUtf8ParserMemberBuilder(Action<T,long> setValue):base(setValue)
			{
			}

			protected override bool ParseInternal(byte[] buffer, in int start, in int length, out long value) => Utf8Parser.TryParse(new ReadOnlySpan<byte>(buffer, start, length), out value, out _);
		}
	}
}