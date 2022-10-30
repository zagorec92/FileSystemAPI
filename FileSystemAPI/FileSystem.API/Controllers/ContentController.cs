using FileSystem.API.ViewModels;
using FileSystem.Core;
using FileSystem.Core.Models.Requests;
using FileSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;

namespace FileSystem.Controllers
{
	/// <summary>
	/// Encapsulates generic actions available on all content types.
	/// </summary>
	[ApiController]
	[Route("{customerId:guid}")]
	[Produces(MediaTypeNames.Application.Json)]
	public class ContentController : ApiController
	{
		private readonly IContentService _contentService;

		/// <summary>
		/// Creates a new instance of <see cref="ContentController"/> type.
		/// </summary>
		/// <param name="contentService">The injected <see cref="IContentService"/> instance.</param>
		/// <param name="linkGenerator">The injected <see cref="LinkGenerator"/> instance.</param>
		public ContentController(IContentService contentService, LinkGenerator linkGenerator)
			: base(linkGenerator) => _contentService = contentService;

		/// <summary>
		/// Gets the customer's content by path.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="path">The <see cref="string"/> relative path to the content.</param>
		/// <remarks>
		/// If <paramref name="path"/> is empty, it is treated as a root directory path.
		/// The <paramref name="path"/> argument matches everything after {<paramref name="customerId"/>}/ which is considered a valid relative url in this context.
		/// </remarks>
		/// <returns>The <see cref="OkObjectResult"/> containing a collection of <see cref="ContentViewModelRich"/> instances.</returns>
		[HttpGet("{*path}")]
		[Produces(typeof(ContentViewModelRich))]
		[ProducesResponseType(typeof(ContentViewModelRich), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Get(Guid customerId, string? path)
		{
			var content = await _contentService.Get(new SearchContentRequestByPath(
				customerId,
				WebUtility.UrlDecode(path)!,
				string.IsNullOrEmpty(path)
			));

			if (!content.Any())
				return NotFound();

			ContentViewModelRich item = new(content.Single());
			GenerateLinks(customerId, item);

			return Ok(item);
		}

		/// <summary>
		/// Creates a new content.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="request">The <see cref="SaveContentRequestViewModel"/> instance containing the required data.</param>
		/// <returns>The <see cref="CreatedResult"/> containing an <see cref="Uri"/> pointing to the new content and <see cref="ContentViewModel"/> instance.</returns>
		[HttpPost]
		public async Task<IActionResult> Save(
			[FromRoute] Guid customerId,
			[FromBody] SaveContentRequestViewModel request)
		{
			var saveRequest = request.ToSaveContentRequest(customerId);
			var content = await _contentService.Save(saveRequest);

			return Created($"{customerId}/content{content.Path}", new ContentViewModel(content));
		}

		/// <summary>
		/// Updates the content.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="id">The <see cref="Guid"/> content identifier.</param>
		/// <param name="request">The <see cref="UpdateContentRequestViewModel"/> instance containing the required data.</param>
		/// <remarks>
		/// <see cref="HttpMethods.Put"/> action could produce a new resource which would require the method to return <see cref="CreatedResult"/>.
		/// This is avoided in implementation, so a call to this method will never result in a resource (content) being created.
		/// </remarks>
		/// <returns>The <see cref="OkObjectResult"/> instance.</returns>
		[HttpPatch("{id:guid}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Update(
			[FromRoute] Guid customerId,
			[FromRoute] Guid id,
			[FromBody] UpdateContentRequestViewModel request)
		{
			var updateRequest = request.ToUpdateContentRequest(customerId, id);
			await _contentService.Update(updateRequest);

			return Ok();
		}

		/// <summary>
		/// Deletes the content.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="id">The <see cref="Guid"/> content identifier.</param>
		/// <returns>The <see cref="NoContentResult"/> instance.</returns>
		[HttpDelete("{id:guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> Delete(Guid customerId, Guid id)
		{
			await _contentService.Delete(new(customerId, id));

			return NoContent();
		}
	}
}