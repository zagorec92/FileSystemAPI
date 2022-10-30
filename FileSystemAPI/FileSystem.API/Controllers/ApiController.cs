using FileSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FileSystem.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public abstract class ApiController : ControllerBase
	{
		private readonly LinkGenerator _linkGenerator;

		public ApiController(LinkGenerator linkGenerator) => _linkGenerator = linkGenerator;

		/// <summary>
		/// Generates available action links to support HATEOAS constraints. For additional details, visit <see href="https://en.wikipedia.org/wiki/HATEOAS" />.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="item">The <see cref="ContentViewModel"/> instance containing content data.</param>
		protected void GenerateLinks(Guid customerId, ContentViewModel item)
		{
			item.Links.Add(GetLink(customerId, item, nameof(ContentController.Get), "self", HttpMethods.Get));

			if (item.ParentId != null)
				item.Links.AddRange(new List<LinkViewModel>()
				{
					GetLink(customerId, item, nameof(ContentController.Delete), "self", HttpMethods.Delete),
					GetLink(customerId, item, nameof(ContentController.Update), "self", HttpMethods.Put)
				});

			if ((item.Descendants?.Any()).GetValueOrDefault())
			{
				foreach (var descendant in item.Descendants!)
				{
					descendant.Links.AddRange(new List<LinkViewModel>()
					{
						GetLink(customerId, descendant, nameof(ContentController.Get), "self", HttpMethods.Get),
						GetLink(customerId, descendant, nameof(ContentController.Delete), "self", HttpMethods.Delete),
						GetLink(customerId, descendant, nameof(ContentController.Update), "self", HttpMethods.Put)
					});
				}
			}
		}

		private LinkViewModel GetLink(Guid customerId, ContentViewModelBase item, string actionName, string rel, string method)
		{
			object values = actionName == nameof(ContentController.Get) ?
				new { customerId, item.Path } :
				new { customerId, item.Id };

			return new LinkViewModel(
				WebUtility.UrlDecode(_linkGenerator.GetUriByAction(
					HttpContext,
					actionName,
					values: values)!),
				rel,
				method);
		}
	}
}
