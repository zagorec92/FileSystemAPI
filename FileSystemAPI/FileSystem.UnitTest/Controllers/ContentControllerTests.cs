﻿using FileSystem.API.ViewModels;
using FileSystem.Controllers;
using FileSystem.Core;
using FileSystem.Core.Entities;
using FileSystem.Core.Models.Requests;
using FileSystem.Infrastructure.Exceptions;
using FileSystem.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using ContentType = FileSystem.Core.Enums.ContentType;

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
			result.As<OkObjectResult>().Value.As<ContentViewModelRich>().Links.Should().NotBeNull().And.NotBeEmpty();
		}

		[TestMethod]
		public async Task Get_ByPath_NoItem_Should_Throw_NotFoundException()
		{
			_mockContentService
				.Setup(x => x.Get(It.IsAny<SearchContentRequestByPath>()))
				.ReturnsAsync(_mockData.Where(x => x.Id == Guid.NewGuid()));

			Func<Task<IActionResult>> action = () => GetController().Get(Guid.Empty, string.Empty);

			await action.Should().ThrowAsync<NotFoundException>();
		}

		[TestMethod]
		[DataRow(ContentType.Directory)]
		[DataRow(ContentType.File)]
		public async Task Save_Should_Return_201Created(ContentType contentType)
		{
			_mockContentService
				.Setup(x => x.Save(It.IsAny<SaveContentRequest>()))
				.ReturnsAsync(new Content() { Path = "Test", Type = (byte)contentType });

			var controller = GetController();

			Guid testCustomerId = Guid.NewGuid();
			var request = new SaveContentRequestViewModel()
			{
				Name = String.Empty,
				ParentId = testCustomerId,
				Type = contentType
			};
			var result = await controller.Save(testCustomerId, request);

			result.Should().BeOfType<CreatedResult>().Which.StatusCode.Should().Be(StatusCodes.Status201Created);
			result.As<CreatedResult>().Location.Should().BeEquivalentTo($"{testCustomerId}/Test");
			result.As<CreatedResult>().Value.As<ContentViewModel>().Links.Should().NotBeNull().And.NotBeEmpty();
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
			var request = new UpdateContentRequestViewModel()
			{
				Name = String.Empty,
				ParentId = parentId
			};

			var result = await controller.Update(testCustomerId, id, request);

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
