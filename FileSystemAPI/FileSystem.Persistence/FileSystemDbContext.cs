using FileSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileSystem.Core.Persistence
{
	public class FileSystemDbContext : DbContext, IContentOperationContext
	{
		private DbSet<Content> Content { get; set; }

		IQueryable<Content> IContentOperationContext.Content => Content.AsNoTracking();

		public FileSystemDbContext(DbContextOptions<FileSystemDbContext> options)
			: base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Content>()
				.Property(x => x.RowVersion)
				.IsRowVersion();

			modelBuilder.Entity<Content>()
				.Property(x => x.Name)
				.HasMaxLength(byte.MaxValue);

			modelBuilder.Entity<Content>()
				.Property(x => x.Type)
				.HasComment("Denotes whether the item is a file or a directory (0 - directory, 1 - file");

			modelBuilder.Entity<Content>()
				.HasIndex(x => new { x.Path, x.CustomerId })
				.IsUnique();
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			foreach(var entry in ChangeTracker.Entries())
			{
				if (entry.Entity is Content entity)
				{
					switch (entry.State)
					{
						case EntityState.Modified:
							entity.Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
							break;
						case EntityState.Added:
							entity.Created = entity.Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
							break;
						default:
							break;
					}
				}
			}

			return base.SaveChangesAsync(cancellationToken);
		}

		public void Remove(IEnumerable<Content> entities) => Content.RemoveRange(entities);
		public void Update(IEnumerable<Content> entities) => Content.UpdateRange(entities);
		public void Update(Content content) => Content.Update(content);
		public async Task AddAsync(Content content, CancellationToken cancellationToken = default)
			=> await Content.AddAsync(content, cancellationToken);
	}
}
