namespace FileSystem.UnitTest.Setup
{
	internal class MockAsyncEnumerator<T> : IAsyncEnumerator<T>
	{
		private readonly IEnumerator<T> _inner;

		public MockAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

		public void Dispose() => _inner.Dispose();

		public T Current => _inner.Current;

		public Task<bool> MoveNext(CancellationToken cancellationToken) => Task.FromResult(_inner.MoveNext());

		public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());

		public ValueTask DisposeAsync()
		{
			_inner.Dispose();
			return ValueTask.CompletedTask;
		}
	}
}
