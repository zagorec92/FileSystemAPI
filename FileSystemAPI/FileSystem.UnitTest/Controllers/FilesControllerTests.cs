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
	public class FilesControllerTests
	{
		private readonly Mock<IContentService> _mockContentService;
		private readonly IEnumerable<Content> _mockData;

		public FilesControllerTests()
		{
			_mockContentService = new Mock<IContentService>();

			_mockData = new List<Content>()
			{
				new Content
				{
					Id = Guid.NewGuid(),
					Name = "Test",
					Path = "/Test.pdf",
					Type = (byte)ContentType.File
				}
			};
		}

		[TestMethod]
		public async Task Get_ByName_HasItem_Should_Return_200Ok()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData);

			var controller = GetController();

			var result = await controller.Get(Guid.NewGuid(), string.Empty);

			result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);
			result.As<OkObjectResult>().Value.Should().BeOfType<ContentViewModel>().And.NotBeNull();
		}

		[TestMethod]
		public async Task Get_ByName_HasItem_Should_Be_Decorated_With_Links()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData);

			var controller = GetController();

			var result = await controller.Get(Guid.NewGuid(), string.Empty);

			result.As<OkObjectResult>().Value.As<ContentViewModel>().Links.Should().NotBeEmpty();
		}

		[TestMethod]
		public async Task Get_ByName_NoItem_Should_Return_404NotFound()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData.Where(x => x.Id == Guid.NewGuid()));

			var controller = GetController();

			var result = await controller.Get(Guid.Empty, string.Empty);

			result.Should().BeOfType<NotFoundResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
		}

		[TestMethod]
		public async Task Get_ByDirectoryAndName_HasItem_Should_Return_200Ok()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData);

			var controller = GetController();

			var result = await controller.GetByParent(Guid.NewGuid(), Guid.Empty, string.Empty);

			result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);
			result.As<OkObjectResult>().Value.Should().BeOfType<ContentViewModel>().And.NotBeNull();
		}

		[TestMethod]
		public async Task Get_ByDirectoryAndName_HasItem_Should_Be_Decorated_With_Links()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData);

			var controller = GetController();

			var result = await controller.GetByParent(Guid.NewGuid(), Guid.Empty, string.Empty);

			result.As<OkObjectResult>().Value.As<ContentViewModel>().Links.Should().NotBeEmpty();
		}

		[TestMethod]
		public async Task Get_ByDirectoryAndName_NoItem_Should_Return_404NotFound()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData.Where(x => x.Id == Guid.NewGuid()));

			var controller = GetController();

			var result = await controller.GetByParent(Guid.Empty, Guid.Empty, string.Empty);

			result.Should().BeOfType<NotFoundResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
		}

		[TestMethod]
		public async Task Search_HasItems_Should_Return_200Ok()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData);

			var controller = GetController();

			var result = await controller.Search(Guid.Empty, string.Empty, Guid.Empty, default);

			result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(StatusCodes.Status200OK);
			result.As<OkObjectResult>().Value.Should().BeOfType<List<ContentViewModelSimple>>();
			result.As<OkObjectResult>().Value.As<List<ContentViewModelSimple>>().Should().NotBeEmpty();
		}

		[TestMethod]
		public async Task Search_HasItems_Should_Be_Decorated_With_Links()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData);

			var controller = GetController();

			var result = await controller.Search(Guid.Empty, string.Empty, Guid.Empty, default);

			result.As<OkObjectResult>().Value.As<List<ContentViewModelSimple>>().ForEach(x => x.Links.Should().NotBeEmpty());
		}

		[TestMethod]
		public async Task Search_NoItem_Should_Return_404NotFound()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByName>()))
				.ReturnsAsync(_mockData.Where(x => x.Id == Guid.NewGuid()));

			var controller = GetController();

			var result = await controller.Search(Guid.Empty, string.Empty, Guid.Empty, default);

			result.Should().BeOfType<NotFoundResult>().Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
		}

		private FilesController GetController()
		{
			FilesController controller = new(_mockContentService.Object, new Mock<LinkGenerator>().Object);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			return controller;
		}
	}
}
