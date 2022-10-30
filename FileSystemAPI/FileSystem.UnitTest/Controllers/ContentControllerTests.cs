using FileSystem.Controllers;
using FileSystem.Core;
using FileSystem.Core.Entities;
using FileSystem.Core.Enums;
using FileSystem.Core.Models.Requests;
using FileSystem.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace FileSystem.UnitTest.Controllers
{
	[TestClass]
	public class ContentControllerTests
	{
		private readonly Mock<IContentService> _mockContentService;
		private readonly IEnumerable<Content> _mockData;

		public ContentControllerTests()
		{
			_mockContentService = new Mock<IContentService>();
			_mockData = new List<Content>()
			{
				new Content
				{
					Id = Guid.NewGuid(),
					Name = "Test",
					Path = "/Test",
					Type = (byte)ContentType.Directory
				}
			};
		}

		[TestMethod]
		public async Task Get_ByPath_HasItem_Should_Return_200Ok()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByPath>()))
				.ReturnsAsync(_mockData);

			var controller = GetController();

			var result = await controller.Get(Guid.NewGuid(), string.Empty);

			result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);
			result.As<OkObjectResult>().Value.Should().BeOfType<ContentViewModelRich>().And.NotBeNull();
		}

		[TestMethod]
		public async Task Get_ByPath_HasItem_Should_Be_Decorated_With_Links()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByPath>()))
				.ReturnsAsync(_mockData);

			var controller = GetController();

			var result = await controller.Get(Guid.NewGuid(), string.Empty);

			result.As<OkObjectResult>().Value.As<ContentViewModelRich>().Links.Should().NotBeNull();
		}

		[TestMethod]
		public async Task Get_ByPath_NoItem_Should_Return_404NotFound()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByPath>()))
				.ReturnsAsync(_mockData.Where(x => x.Id == Guid.NewGuid()));

			var controller = GetController();

			var result = await controller.Get(Guid.Empty, string.Empty);

			result.Should().BeOfType<NotFoundResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
		}

		[TestMethod]
		public async Task Save_Should_Return_201Created()
		{
			_mockContentService
				.Setup(x => x.Save(It.IsAny<SaveContentRequest>()))
				.ReturnsAsync(new Content() { Path = "/Test" });

			var controller = GetController();

			Guid testCustomerId = Guid.NewGuid();
			var result = await controller.Save(testCustomerId, new(testCustomerId.ToString(), String.Empty, 1));

			result.Should().BeOfType<CreatedResult>().Which.StatusCode.Should().Be(StatusCodes.Status201Created);
			result.As<CreatedResult>().Location.Should().BeEquivalentTo($"{testCustomerId}/content/Test");
		}

		[TestMethod]
		public async Task Update_Should_Return_200Ok()
		{
			_mockContentService
				.Setup(x => x.Update(It.IsAny<UpdateContentRequest>()));

			var controller = GetController();

			Guid testCustomerId = Guid.NewGuid();
			Guid id = Guid.NewGuid();
			Guid parentId = Guid.NewGuid();
			var result = await controller.Update(testCustomerId, id, new(parentId.ToString(), "test"));

			result.Should().BeOfType<OkResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);
		}

		[TestMethod]
		public async Task Delete_Should_Return_204NoContent()
		{
			_mockContentService
				.Setup(x => x.Delete(It.IsAny<DeleteContentRequest>()));

			var controller = GetController();

			var result = await controller.Delete(Guid.Empty, Guid.Empty);

			result.Should().BeOfType<NoContentResult>().Which.StatusCode.Should().Be(StatusCodes.Status204NoContent);
		}

		private ContentController GetController()
		{
			ContentController controller = new(_mockContentService.Object, new Mock<LinkGenerator>().Object);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			return controller;
		}
	}
}
