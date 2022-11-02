using FileSystem.API.ViewModels;
using FileSystem.Core.Entities;
using FileSystem.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Text.Json;

namespace FileSystem.Middleware
{
	/// <summary>
	/// Exception handling middleware.
	/// </summary>
	public class ExceptionMiddleware
	{
		struct LogTemplates
		{
			internal const string RootExists = "Root directory already exists. CustomerId: {customerId}, RequestPath: {requestPath}.";
			internal const string NotFound = "Content could not be found. CustomerId: {customerId}, ContentId: {contentId}, RequestPath: {requestPath}.";
			internal const string ConcurrencyViolation = "Content state has been changed since it was last retrieved. CustomerId: {customerId}, ContentId: {contentId}, ContentName: {contentName}, RequestPath: {requestPath}.";
			internal const string Unexpected = "Something went wrong.";
		}

		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionMiddleware> _logger;

		/// <summary>
		/// Creates a new <see cref="ExceptionMiddleware"/> instance.
		/// </summary>
		/// <param name="next">The <see cref="RequestDelegate"/> function which processes HTTP request.</param>
		/// <param name="logger">The <see cref="ILogger"/> instance.</param>
		public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		/// <summary>
		/// Invokes the middleware.
		/// </summary>
		/// <param name="httpContext">The <see cref="HttpContext"/> instance.</param>
		/// <returns></returns>
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

		private async Task HandleException(HttpContext httpContext, Exception ex)
		{
			var requestpath = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
			var errorResponse = new ErrorResponseViewModel()
			{
				StatusCode = StatusCodes.Status500InternalServerError,
				Message = LogTemplates.Unexpected
			};

			if(ex is NotFoundException nfe)
			{
				errorResponse.StatusCode = StatusCodes.Status404NotFound;
				errorResponse.Message = nfe.Message;
				_logger.LogWarning(nfe, LogTemplates.NotFound, nfe.CustomerId, nfe.ContentId, requestpath);
			}
			else if(ex is RootExistsException ree)
			{
				errorResponse.StatusCode = StatusCodes.Status400BadRequest;
				errorResponse.Message = ree.Message;
				_logger.LogWarning(ree, LogTemplates.RootExists, ree.CustomerId, requestpath);
			}
			else if (ex is DbUpdateConcurrencyException duce)
			{
				errorResponse.StatusCode = StatusCodes.Status409Conflict;
				errorResponse.Message = "Content state has been changed since it was last retrieved.";

				var entity = (Content)duce.Entries[0].Entity;
				_logger.LogError(duce, LogTemplates.ConcurrencyViolation, entity.CustomerId, entity.Id, entity.Name, requestpath);
			}

			httpContext.Response.ContentType = MediaTypeNames.Application.Json;
			httpContext.Response.StatusCode = errorResponse.StatusCode;

			await httpContext.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
		}
	}

	/// <summary>
	/// Adds the <see cref="ExceptionMiddleware"/> to the application's request pipeline.
	/// </summary>
	internal static class ExceptionMiddlewareExtensions
	{
		public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder application)
			=> application.UseMiddleware<ExceptionMiddleware>();
	}
}
