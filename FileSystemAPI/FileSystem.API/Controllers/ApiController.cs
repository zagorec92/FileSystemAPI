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
		/// <param name="item">The <see cref="ContentViewModelRich"/> instance containing content data.</param>
		protected void GenerateLinks(Guid customerId, ContentViewModelRich item)
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

		/// <summary>
		/// Generates available action links to support HATEOAS constraints. For additional details, visit <see href="https://en.wikipedia.org/wiki/HATEOAS" />.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="item">The <see cref="ContentViewModelSimple"/> instance containing content data.</param>
		protected void GenerateLinks(Guid customerId, ContentViewModelSimple item)
		{
			item.Links.AddRange(new List<LinkViewModel>()
			{
				GetLink(customerId, item, nameof(ContentController.Get), "self", HttpMethods.Get),
				GetLink(customerId, item, nameof(ContentController.Delete), "self", HttpMethods.Delete),
				GetLink(customerId, item, nameof(ContentController.Update), "self", HttpMethods.Put)
			});
		}

		/// <summary>
		/// Generates a model by utilizing <see cref="LinkGenerator"/> to create an absolute URI pointing to the available resource actions.
		/// </summary>
		/// <param name="customerId">Customer identifier.</param>
		/// <param name="item">Instance containing the content data.</param>
		/// <param name="actionName">Controller action name.</param>
		/// <param name="rel">Relationship.</param>
		/// <param name="method">HTTP method.</param>
		/// <returns>The <see cref="LinkViewModel"/> instance containing the HATEOAS specified data.</returns>
		private LinkViewModel GetLink(Guid customerId, ContentViewModelSimple item, string actionName, string rel, string method)
		{
			object values = actionName == nameof(ContentController.Get) ?
				new { customerId, item.Path } :
				new { customerId, item.Id };

			return new LinkViewModel(
				WebUtility.UrlDecode(_linkGenerator.GetUriByAction(
					HttpContext!,
					actionName,
					"Content",
					values: values)!),
				rel,
				method);
		}
	}
}
