using FileSystem.Core.Entities;

namespace FileSystem.Core
{
	public interface IContentOperationContext
	{
		public IQueryable<Content> Content { get; }

		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
		void Remove(IEnumerable<Content> content);
		Task AddAsync(Content content, CancellationToken cancellationToken = default);
		void Update(Content content);
		void Update(IEnumerable<Content> content);
	}
}
