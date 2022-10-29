using FileSystem.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileSystem.Infrastructure
{
	internal class DataFactory
	{
		internal static async Task Seed(IServiceProvider services, string[] args)
		{
			//using var scope = services.CreateScope();
			//var context = scope.ServiceProvider.GetRequiredService<FileSystemDbContext>();

			//var creatorService = context.Database.GetService<IRelationalDatabaseCreator>();
			//if (args.Length > 0 || !await creatorService?.ExistsAsync()!)
			//{
			//	if (args.Length > 0)
			//		await context.Database.EnsureDeletedAsync();

			//	context.Database.Migrate();

			//	int customersCount = Random.Shared.Next(3, 10);
			//	for (int i = 0; i < customersCount; i++)
			//	{
			//		Guid customerId = Guid.NewGuid();

			//		int directoryCount = Random.Shared.Next(0, 15);
			//		List<Directory> directories = new(directoryCount);
			//		for (int j = 0; j < directoryCount; j++)
			//		{
			//			directories.Add(new Directory
			//			{
			//				CustomerId = customerId,
			//				Name = $"Directory_{i}_{j}"
			//			});
			//		}

			//		context.Directories.AddRange(directories);
			//		await context.SaveChangesAsync();

			//		for (int j = 0; j < directoryCount; j++)
			//		{
			//			Guid? parentId = Random.Shared.Next(0, 10) < 3 ? directories[Random.Shared.Next(0, directories.Count - 1)].Id : null;
			//			if (parentId is not null)
			//				directories
			//					.Where(x => x.Id != parentId)
			//					.ElementAt(Random.Shared.Next(0, directories.Count - 2)).ParentId = parentId;
			//		}

			//		int fileCount = Random.Shared.Next(0, 35);
			//		List<File> files = new(directoryCount);
			//		for (int j = 0; j < directoryCount; j++)
			//		{
			//			Guid directoryId = directories[Random.Shared.Next(0, directories.Count - 1)].Id;
			//			files.Add(new File
			//			{
			//				CustomerId = customerId,
			//				DirectoryId = directoryId,
			//				Name = $"File_{i}_{j}",
			//				MimeType = "pdf"
			//			});
			//		}

			//		context.Files.AddRange(files);
			//		await context.SaveChangesAsync();
			//	}
			//}
		}
	}
}
