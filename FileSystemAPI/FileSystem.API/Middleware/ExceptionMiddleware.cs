using FileSystem.Core.Entities;
using FileSystem.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
			internal const string NotFound = "Content could not be found. CustomerId: {customerId}, ContentId: {contentId}, ContentPath: {contentPath}, RequestPath: {requestPath}.";
			internal const string ConcurrencyViolation = "Content state has been changed since it was last retrieved. CustomerId: {customerId}, ContentId: {contentId}, ContentName: {contentName}, RequestPath: {requestPath}.";
			internal const string Unexpected = "Something went wrong.";
		}

		private readonly static Uri _rfcBaseUri = new Uri("https://www.rfc-editor.org/rfc/");

		private readonly IDictionary<int, Uri> _problemDetailTypeMapping = new Dictionary<int, Uri>()
		{
			{ StatusCodes.Status400BadRequest, new Uri(_rfcBaseUri, "rfc7231#section-6.5.1") },
			{ StatusCodes.Status404NotFound, new Uri(_rfcBaseUri, "rfc7231#section-6.5.4") },
			{ StatusCodes.Status409Conflict, new Uri(_rfcBaseUri, "rfc7231#section-6.5.8") },
			{ StatusCodes.Status500InternalServerError, new Uri(_rfcBaseUri, "rfc7231#section-6.6.1") }
		};

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

			var problemDetails = new ProblemDetails();
			problemDetails.Instance = httpContext.Request.Path + httpContext.Request.QueryString;
			problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier ?? Activity.Current?.Id;

			if (ex is RootExistsException ree)
			{
				problemDetails.Type = _problemDetailTypeMapping[StatusCodes.Status400BadRequest].ToString();
				problemDetails.Status = StatusCodes.Status400BadRequest;
				problemDetails.Title = ree.Message;
				_logger.LogWarning(ree, LogTemplates.RootExists, ree.CustomerId, requestpath);
			}
			else if (ex is NotFoundException nfe)
			{
				problemDetails.Type = _problemDetailTypeMapping[StatusCodes.Status404NotFound].ToString();
				problemDetails.Status = StatusCodes.Status404NotFound;
				problemDetails.Title = nfe.Message;
				_logger.LogWarning(nfe, LogTemplates.NotFound, nfe.CustomerId, nfe.ContentId, nfe.Path, requestpath);
			}
			else if (ex is DbUpdateConcurrencyException duce)
			{
				problemDetails.Type = _problemDetailTypeMapping[StatusCodes.Status409Conflict].ToString();
				problemDetails.Status = StatusCodes.Status409Conflict;
				problemDetails.Title = "Content state has been changed since it was last retrieved.";

				var entity = (Content)duce.Entries[0].Entity;
				_logger.LogError(duce, LogTemplates.ConcurrencyViolation, entity.CustomerId, entity.Id, entity.Name, requestpath);
			}
			else
			{
				problemDetails.Type = _problemDetailTypeMapping[StatusCodes.Status500InternalServerError].ToString();
				problemDetails.Status = StatusCodes.Status500InternalServerError;
				problemDetails.Title = LogTemplates.Unexpected;
				_logger.LogError(ex, LogTemplates.Unexpected);
			}

			httpContext.Response.ContentType = MediaTypeNames.Application.Json;
			httpContext.Response.StatusCode = problemDetails.Status.Value;

			await httpContext.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
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
