using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace FileSystem.UnitTest.Setup
{
	internal class MockAsyncQueryProvider<TEntity> : IAsyncQueryProvider
	{
		private readonly IQueryProvider _inner;

		internal MockAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

		public IQueryable CreateQuery(Expression expression) => new MockAsyncEnumerable<TEntity>(expression);

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
			=> new MockAsyncEnumerable<TElement>(expression);

		public object? Execute(Expression expression) => _inner.Execute(expression);

		public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

		public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
			=> new MockAsyncEnumerable<TResult>(expression);

		public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
			=> Task.FromResult(Execute<TResult>(expression));

		TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
			=> Task.FromResult(Execute<TResult>(expression)).Result;
	}
}
