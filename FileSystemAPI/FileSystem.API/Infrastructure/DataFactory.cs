using FileSystem.Core.Entities;
using FileSystem.Core.Enums;
using FileSystem.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileSystem.Infrastructure
{
	internal class DataFactory
	{
		internal static async Task Seed(IServiceProvider services, string[] args, ILogger<DataFactory> logger)
		{
			using var scope = services.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<FileSystemDbContext>();

			var creatorService = context.Database.GetService<IRelationalDatabaseCreator>();
			if (args.Length == 4 || !await creatorService?.ExistsAsync()!)
			{
				if (args.Length == 4)
					await context.Database.EnsureDeletedAsync();

				context.Database.Migrate();

				if (
					int.TryParse(args[0], out int minCustomers) &&
					int.TryParse(args[1], out int maxCustomers) &&
					int.TryParse(args[2], out int minContent) &&
					int.TryParse(args[3], out int maxContent))
				{

					List<Content> items = new();
					List<Content> batch = new();
					for (int i = 0; i < Random.Shared.Next(minCustomers, maxCustomers + 1); i++)
					{
						Guid customerId = Guid.NewGuid();

						Content root = new()
						{
							Id = Guid.NewGuid(),
							CustomerId = customerId,
							Name = "Root",
							Path = "Home",
							Type = (byte)ContentType.Directory
						};

						items.Clear();
						items.Add(root);
						batch.Add(root);

						for (int j = 0; j < Random.Shared.Next(minContent, maxContent + 1); j++)
						{
							ContentType type = Random.Shared.Next(1, 11) < 7 ? ContentType.File : ContentType.Directory;
							var customerItems = items.Where(x => x.Type == (byte)ContentType.Directory).ToList();
							Content parent = Random.Shared.Next(1, 11) < 7 ?
								customerItems.ElementAt(Random.Shared.Next(0, customerItems.Count)) :
								root;
							string name = $"{(type == ContentType.Directory ? "Directory" : "File")}_{i}_{j}";

							Content content = new()
							{
								Id = Guid.NewGuid(),
								CustomerId = customerId,
								ParentId = parent.Id,
								Name = name,
								Path = $"{parent.Path}/{name}{(type == ContentType.File ? ".png" : string.Empty)}",
								Type = (byte)type
							};

							items.Add(content);
							batch.Add(content);

							if (Random.Shared.Next(0, 11) < 1)
							{
								try
								{
									context.AddRange(batch);
									await context.SaveChangesAsync();
									logger.LogInformation($"Saved {batch.Count} content items.");
									batch.Clear();
								}
								catch (Exception ex)
								{
									logger.LogError($"Id: {content.Id}, Parent: {parent.Id}\n\n{ex}");
									return;
								}
							}
						}
					}

					context.AddRange(batch);
					await context.SaveChangesAsync();
				}
			}
		}
	}
}
