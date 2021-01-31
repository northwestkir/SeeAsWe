using System;
using SeeAsWee.Core.MemberOrder;

namespace SeeAsWee.Core
{
	public class DelegatingCsvParserComponentsFactory<T> : ICsvParserComponentsFactory<T>
	{
		private readonly Func<ResultBuilder<T>> _resultBuilderFactory;
		private readonly Func<IMemberOrderResolver> _memberOrderResolverFactory;

		public DelegatingCsvParserComponentsFactory(Func<ResultBuilder<T>> resultBuilderFactory, Func<IMemberOrderResolver> memberOrderResolverFactory)
		{
			_resultBuilderFactory = resultBuilderFactory;
			_memberOrderResolverFactory = memberOrderResolverFactory;
		}
		
		public ResultBuilder<T> CreateResultBuilder() => _resultBuilderFactory();

		public IMemberOrderResolver CreateMemberOrderResolver() => _memberOrderResolverFactory();
	}
}