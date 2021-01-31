using System;
using System.Collections.Generic;
using SeeAsWee.Core.MemberBuilders;
using SeeAsWee.Core.MemberOrder;

namespace SeeAsWee.Core
{
	public class CsvParserBuilder<T>
	{
		private Func<ResultBuilder<T>> _resultBuilder;
		private Func<IMemberOrderResolver> _memberOrderResolver;
		private Func<T> _resultItemFactory;
		private readonly List<MemberBuilder<T>> _memberBuilders = new List<MemberBuilder<T>>();
		private CsvParserConfig _config;

		public CsvParserBuilder<T> Use(Func<ResultBuilder<T>> factory)
		{

			_resultBuilder = factory;
			return this;
		}

		public CsvParserBuilder<T> Use(Func<IMemberOrderResolver> factory)
		{
			_memberOrderResolver = factory;
			return this;
		}

		public CsvParserBuilder<T> Use(Func<T> factory)
		{
			_resultItemFactory = factory;
			return this;
		}

		public CsvParserBuilder<T> AddMemberBuilder(MemberBuilder<T> builder)
		{
			_memberBuilders.Add(builder);
			return this;
		}

		public CsvParserBuilder<T> Use(CsvParserConfig config)
		{
			_config = config;
			return this;
		}

		public CsvParser<T> Build()
		{
			var resultBuilder = _resultBuilder;
			if (_resultBuilder == null)
			{
				var resultItem = _resultItemFactory();
				_resultBuilder = () => new ResultBuilder<T>(resultItem, _memberBuilders.ToArray());
			}
			var memberOrderResolver = _memberOrderResolver ?? (() => new Utf8MemberOrderResolver());
			return new CsvParser<T>(_config, new DelegatingCsvParserComponentsFactory<T>(resultBuilder, memberOrderResolver));
		}
	}
}