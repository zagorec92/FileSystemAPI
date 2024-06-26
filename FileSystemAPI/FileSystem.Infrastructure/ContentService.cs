﻿using FileSystem.Core;
using FileSystem.Core.Entities;
using FileSystem.Core.Enums;
using FileSystem.Core.Models;
using FileSystem.Core.Models.Requests;
using FileSystem.Infrastructure.Enums;
using FileSystem.Infrastructure.Exceptions;
using FileSystem.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace FileSystem.Infrastructure
{
	public class ContentService : IContentService
	{
		private const string LoggingTemplate = "CustomerId: {CustomerId}, ContentId: {ContentId}";

		private readonly IContentOperationContext _context;
		private readonly ILogger<ContentService> _logger;

		public ContentService(IContentOperationContext context, ILogger<ContentService> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task Delete(DeleteContentRequest request)
		{
			Content? content = (await Get(new SearchContentRequestByIds(request.CustomerId, new() { request.Id })))
				?.SingleOrDefault();

			if (content == null)
				throw new NotFoundException(request.CustomerId, request.Id);

			List<Content> itemsToDelete = [content];

			var descendants = await GetDescendants(content.CustomerId, content.Id, content.Path);
			if (descendants.Any())
				itemsToDelete.AddRange(descendants);

			_context.Remove(itemsToDelete);

			await _context.SaveChangesAsync();

			_logger.LogInformation(LogEvent.ContentDeleted.ToEventId(), LoggingTemplate, request.CustomerId, content.Id);
		}

		public async Task<Content> Save(SaveContentRequest request)
		{
			Content? parent = null;
			if(request.Name == Constants.RootDirectory && !request.ParentId.HasValue)
			{
				SearchContentRequest searchRequest = new(request.CustomerId);
				Content? root = (await Get(searchRequest))?.FirstOrDefault();
				if (root != null)
					throw new RootExistsException(request.CustomerId);
			}
			else
			{
				SearchContentRequestByIds searchRequest = new(request.CustomerId, [request.ParentId!.Value]);
				parent = (await Get(searchRequest)).SingleOrDefault() ?? throw new NotFoundException(request.CustomerId, request.ParentId!.Value);
			}

			Content newContent = new()
			{
				CustomerId = request.CustomerId,
				Name = request.Name,
				ParentId = parent?.Id,
				Path = (parent != null ? parent.Path + "/" : string.Empty) + request.Name,
				Type = (byte)request.Type
			};

			await _context.AddAsync(newContent);
			await _context.SaveChangesAsync();

			_logger.LogInformation(LogEvent.ContentAdded.ToEventId(), LoggingTemplate, request.CustomerId, newContent.Id);

			return newContent;
		}

		public async Task Update(UpdateContentRequest request)
		{
			Content? content = (await Get(new SearchContentRequestByIds(request.CustomerId, [request.Id])))?.SingleOrDefault();
			if (content == null)
				throw new NotFoundException(request.CustomerId, request.Id);

			string oldPath = content.Path;

			if (request.ParentId != Guid.Empty && request.ParentId != content.ParentId)
			{
				IEnumerable<Content> combinedParents = await Get(new SearchContentRequestByIds(request.CustomerId, [content.ParentId!.Value, request.ParentId]));

				Content oldParent = combinedParents.Single(x => x.Id == content.ParentId);
				Content newParent = combinedParents.Single(x => x.Id == request.ParentId);

				content.ParentId = request.ParentId;
				content.Path = content.Path.Replace(oldParent.Path, newParent.Path);
				await UpdateDescendants();
			}

			if (!content.Name.Equals(request.Name, StringComparison.InvariantCulture))
			{
				content.Path = content.Path.Replace(content.Name, request.Name);
				content.Name = request.Name;
				await UpdateDescendants();
			}

			async Task UpdateDescendants()
			{
				IEnumerable<Content> allDescendants = await GetDescendants(request.CustomerId, content.Id, oldPath);
				if (allDescendants.Any())
				{
					foreach (Content item in allDescendants)
						item.Path = item.Path.Replace(oldPath, content.Path);

					_context.Update(allDescendants);
				}
			}

			_context.Update(content);

			await _context.SaveChangesAsync();

			_logger.LogInformation(LogEvent.ContentModified.ToEventId(), LoggingTemplate, request.CustomerId, request.Id);
		}

		public async Task<IEnumerable<Content>> Get(SearchContentRequestByName request)
		{
			IQueryable<Content> query = ApplyBaseQueries(request);

			query = request.MatchType switch
			{
				StringMatchType.Contains => query.Where(x => x.Name.Contains(request.Name)),
				StringMatchType.Exact => query.Where(x => x.Name == request.Name),
				StringMatchType.EndsWith => query.Where(x => x.Name.EndsWith(request.Name)),
				StringMatchType.StartsWith => query.Where(x => x.Name.StartsWith(request.Name)),
				_ => query
			};

			return await query.ToListAsync();
		}

		public async Task<IEnumerable<Content>> Get(SearchContentRequestByIds request)
		{
			IQueryable<Content> query = ApplyBaseQueries(request);

			query = request.Ids?.Count == 1
				? query.Where(x => x.Id == request.Ids.First())
				: query.Where(x => request.Ids!.Contains(x.Id));

			return await query.ToListAsync();
		}

		public async Task<IEnumerable<Content>> Get(SearchContentRequestByPath request)
		{
			IQueryable<Content> query = ApplyBaseQueries(request);

			query = request.IsRoot
				? query.Where(x => x.ParentId == null)
				: query.Where(x => x.Path == request.Path);

			return await query.ToListAsync();
		}

		public async Task<IEnumerable<Content>> Get(SearchContentRequest request)
		{
			IQueryable<Content> query = ApplyBaseQueries(request);

			return await query.ToListAsync();
		}

		#region Private

		private IQueryable<Content> ApplyBaseQueries(SearchContentRequest request)
		{
			IQueryable<Content> query = _context.Content
				.Where(x => x.CustomerId == request.CustomerId);

			if (request.ParentId.HasValue)
				query = query.Where(x => x.ParentId == request.ParentId.Value);

			if (request.ContentType.HasValue)
				query = query.Where(x => x.Type == (byte)request.ContentType);

			if (request.Top.HasValue)
				query = query.Take(request.Top.Value);

			query = ApplyIncludes(query, request);
			query = ApplySortCriteria(query, request);

			return query;
		}

		private static IQueryable<Content> ApplySortCriteria(IQueryable<Content> query, SearchContentRequest request)
		{
			if (!(request.SortCriteria?.Any()).GetValueOrDefault())
				return query;

			foreach (SortCriteria sortCriteria in request.SortCriteria!)
			{
				query = sortCriteria.SortDirection switch
				{
					ListSortDirection.Ascending => query.OrderBy(sortCriteria.Selector),
					ListSortDirection.Descending => query.OrderByDescending(sortCriteria.Selector),
					_ => throw new InvalidOperationException()
				};
			}

			return query;
		}

		private static IQueryable<Content> ApplyIncludes(IQueryable<Content> query, SearchContentRequest request)
		{
			if (!(request.Includes?.Any()).GetValueOrDefault())
				return query;

			foreach (var selector in request.Includes!)
				query = query.Include(selector);

			return query;
		}

		private async Task<IEnumerable<Content>> GetDescendants(Guid customerId, Guid id, string parentPath)
		{
			IQueryable<Content> query = ApplyBaseQueries(new(customerId))
				.Where(x => x.Path.StartsWith(parentPath) && x.Id != id);

			return await query.ToListAsync();
		}

		#endregion
	}
}
