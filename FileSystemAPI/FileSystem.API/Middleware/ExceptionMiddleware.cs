using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json.Serialization;

namespace FileSystem.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger _logger;

		public ExceptionMiddleware(RequestDelegate next, ILogger<Program> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext httpContext)
		{
			try
			{
				await _next(httpContext);
			}
			catch (Exception ex)
			{
				await HandleException(httpContext, ex);
			}
		}

		private Task HandleException(HttpContext httpContext, Exception ex)
		{
			// TODO: create custom exceptions
			// TODO: depending on the exception type, set the status code and extract the message
			// TODO: use the inijected logger to log the exception to a log source

			httpContext.Response.ContentType = MediaTypeNames.Application.Json;
			httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

			return httpContext.Response.WriteAsync("");
		}
	}

	public class ErrorResponse
	{
		[JsonPropertyName("statusCode")]
		public int StatusCode { get; set; }

		[JsonPropertyName("message")]
		public string? Message { get; set; }
	}

	public static class ExceptionMiddlewareExtensions
	{
		public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder application)
			=> application.UseMiddleware<ExceptionMiddleware>();
	}
}
