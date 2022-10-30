using System.Linq.Expressions;

namespace FileSystem.UnitTest.Setup
{
	internal class MockAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
	{
		public MockAsyncEnumerable(IEnumerable<T> enumerable)
			: base(enumerable) { }

		public MockAsyncEnumerable(Expression expression)
			: base(expression) { }

		public IAsyncEnumerator<T> GetEnumerator()
			=> new MockAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			=> new MockAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

		IQueryProvider IQueryable.Provider => new MockAsyncQueryProvider<T>(this);
	}
}
