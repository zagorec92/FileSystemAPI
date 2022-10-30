using FileSystem.Core;
using FileSystem.Core.Enums;
using FileSystem.Core.Models.Requests;
using FileSystem.Infrastructure;
using FileSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using MediaTypeNames = System.Net.Mime.MediaTypeNames;

namespace FileSystem.Controllers
{
	/// <summary>
	/// Encapsulates actions available only on <see cref="ContentType.File"/> types.
	/// </summary>
	[Route("{customerId:guid}")]
	[ApiController]
	[Produces(MediaTypeNames.Application.Json)]
	public class FilesController : ControllerBase
	{
		private readonly IContentService _contentService;

		/// <summary>
		/// Creates a new instance of <see cref="FilesController"/> type.
		/// </summary>
		/// <param name="contentService">The injected <see cref="IContentService"/> instance.</param>
		public FilesController(IContentService contentService) => _contentService = contentService;

		/// <summary>
		/// Gets the customer's file by an exact name.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="name">The <see cref="string"/> name of the file.</param>
		/// <returns>The <see cref="OkObjectResult"/> containing a <see cref="ContentViewModelBase"/> instance.</returns>
		[HttpGet("files")]
		[Produces(typeof(ContentViewModelBase))]
		[ProducesResponseType(typeof(ContentViewModelBase), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Get(Guid customerId, [FromQuery] string name)
		{
			var file = (await _contentService.Get(new SearchContentRequestByName(customerId, name)
			{
				ContentType = ContentType.File
			}))?.SingleOrDefault();

			if (file == null)
				return NotFound();

			return Ok(new ContentViewModelBase(file));
		}

		/// <summary>
		/// Gets the customer's file by an exact name relative to the parent directory.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="directoryId">The <see cref="Guid"/> directory identifier.</param>
		/// <param name="name">The <see cref="string"/> name of the file.</param>
		/// <returns>The <see cref="OkObjectResult"/> containing a <see cref="ContentViewModelBase"/> instance.</returns>
		[HttpGet("{directoryId:guid}/files")]
		[Produces(typeof(ContentViewModelBase))]
		[ProducesResponseType(typeof(ContentViewModelBase), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetByParent(
			Guid customerId,
			[FromRoute] Guid? directoryId,
			[FromQuery] string name)
		{
			var file = (await _contentService.Get(new SearchContentRequestByName(customerId, name)
			{
				ParentId = directoryId,
				ContentType = ContentType.File
			}))?.SingleOrDefault();

			if (file == null)
				return NotFound();

			return Ok(new ContentViewModelBase(file));
		}

		/// <summary>
		/// Searches the customer's files using StartsWith comparer.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="name">The <see cref="string"/> name of the file.</param>
		/// <param name="directoryId">The <see cref="Guid"/> directory identifier.</param>
		/// <param name="top">The <see cref="int"/> value indicating the result limit.</param>
		/// <remarks>
		/// Enables a more flexible autocomplete search and it provides a way to limit the number of items in a result.
		/// </remarks>
		/// <returns>The <see cref="OkObjectResult"/> containing a collection of <see cref="ContentViewModelSimple"/> instances.</returns>
		[HttpGet("files/search")]
		[Produces(typeof(IEnumerable<ContentViewModelSimple>))]
		[ProducesResponseType(typeof(IEnumerable<ContentViewModelSimple>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Search(
			Guid customerId,
			[FromQuery] string name,
			[FromQuery] Guid? directoryId,
			[FromQuery] int? top)
		{
			var searchRequest = new SearchContentRequestByName(customerId, name)
			{
				ParentId = directoryId,
				ContentType = ContentType.File,
				MatchType = StringMatchType.StartsWith,
				Top = top ?? Constants.DefaultMaxRows,
				SortCriteria = new()
				{
					new(x => x.Name, ListSortDirection.Descending)
				}
			};

			var files = await _contentService.Get(searchRequest);

			if (!files.Any())
				return NotFound();

			return Ok(files.Select(x => new ContentViewModelSimple(x)).ToList());
		}
	}
}
