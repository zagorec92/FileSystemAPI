using FileSystem.Core.Persistence;
using FileSystem.Core.Settings;
using FileSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FileSystem
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Configuration
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", false, true)
				.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json");

			// Add services to the container.
			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services
				.AddEndpointsApiExplorer()
				.AddSwaggerGen()
				.AddDbContext<FileSystemDbContext>((serviceProvider, optionsBuilder) =>
				{
					var settings = serviceProvider.GetRequiredService<IOptions<Settings>>().Value;
					optionsBuilder.UseSqlServer(settings.ConnectionStrings!.FileSystemDb!);
#if DEBUG
					optionsBuilder.EnableSensitiveDataLogging();
#endif
				})
				.Configure<Settings>(builder.Configuration);

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
				await DataFactory.Seed(app.Services, args);
			}

			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapControllers();

			app.Run();
		}
	}
}