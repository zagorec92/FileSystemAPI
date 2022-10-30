using FileSystem.Core;
using FileSystem.Core.Persistence;
using FileSystem.Core.Settings;
using FileSystem.Infrastructure;
using FileSystem.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace FileSystem
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Configuration
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", false, true)
				.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json");

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			// TODO: add api versioning
			builder.Services
				.AddEndpointsApiExplorer()
				.AddSwaggerGen(setup =>
				{
					setup.SwaggerDoc("v1", new OpenApiInfo
					{
						Version = Assembly.GetExecutingAssembly().GetName().Name + " v1",
						Title = Assembly.GetExecutingAssembly().GetName().Name,
						Description = "File System API providing endpoint for simple content manipulation (create, update, move, delete).",
						Contact = new OpenApiContact
						{
							Name = "Contact",
							Url = new Uri("https://github.com/zagorec92")
						}
					});

					var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
					setup.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
				})
				.AddScoped<IContentService, ContentService>()
				.AddDbContext<IContentOperationContext, FileSystemDbContext>((serviceProvider, optionsBuilder) =>
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
				app.UseSwaggerUI(
					options =>
				{
					options.SwaggerEndpoint("/swagger/v1/swagger.json", Assembly.GetExecutingAssembly().GetName().Name + " v1");
				}
				);
				Task.Run(() => DataFactory.Seed(app.Services, args, LoggerFactory.Create(x => x.AddConsole()).CreateLogger<DataFactory>()));
				//await DataFactory.Seed(app.Services, args);
			}

			// TODO: implement custom Exception handler
			//app.UseMiddleware<ExceptionMiddleware>();
			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.UseExceptionMiddleware();
			app.MapControllers();

			app.Run();
		}
	}
}