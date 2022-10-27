using FileSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Directory = FileSystem.Core.Entities.Directory;
using File = FileSystem.Core.Entities.File;

namespace FileSystem.Core.Persistence
{
	public class FileSystemDbContext : DbContext
	{
		public DbSet<Directory> Directories { get; set; }
		public DbSet<File> Files { get; set; }

		public FileSystemDbContext(DbContextOptions<FileSystemDbContext> options)
			: base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Directory>()
				.Property(x => x.RowVersion)
				.IsRowVersion();

			modelBuilder.Entity<File>()
				.Property(x => x.RowVersion)
				.IsRowVersion();

			modelBuilder.Entity<File>()
				.HasOne(x => x.Directory)
				.WithMany(x => x.Files);
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			foreach(var entry in ChangeTracker.Entries())
			{
				if (entry.Entity is EntityBase entity)
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

		private void Seed(ModelBuilder modelBuilder)
		{
			List<Directory> directories = new();
			List<File> files = new();

			Random random = new();
			for(int i = 1; i <= 10; i++)
			{
				bool isChild = random.Next(0, 10) < 2;
				Guid? parentId = isChild && directories.Count > 0 ? directories.ElementAt(random.Next(0, directories.Count)).Id : null;
				Guid newId = Guid.NewGuid();

				directories.Add(new Directory
				{
					Id = newId,
					Name = "Directory_" + i,
					ParentId = parentId
				});

				if (random.Next(0, 2) == 1)
				{
					for (int j = 1 + files.Count; j <= random.Next(files.Count, files.Count + 5); j++)
						files.Add(new File
						{
							Id = Guid.NewGuid(),
							Name = "File_" + j + ".pdf",
							MimeType = "pdf",
							DirectoryId = newId,
							ContentLength = random.Next(0, 100000)
						});
				}
			}

			modelBuilder.Entity<Directory>().HasData(directories);
			modelBuilder.Entity<File>().HasData(files);
		}
	}
}
