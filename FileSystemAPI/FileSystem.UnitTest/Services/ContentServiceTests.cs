using FileSystem.Core;
using FileSystem.Core.Entities;
using FileSystem.Core.Enums;
using FileSystem.Core.Models.Requests;
using FileSystem.Infrastructure;
using FileSystem.UnitTest.Setup;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileSystem.UnitTest.Services
{
	[TestClass]
	public class ContentServiceTests
	{
		private struct Guids
		{
			public struct Customer_1
			{
				public const string Id = "b7edec43-5f34-4d78-94db-9e92a02c6b58";

				public const string Home = "e85ead6c-91b4-4db9-a4c4-2513af3c1329";
				public const string Pictures = "46aaba10-fbfd-4005-85cb-abaafdfb3012";
				public const string Documents = "437645e7-472a-4372-bd96-b502c344a3e8";
				public const string Documents_File_1 = "3bb1ad1a-0519-4568-a886-d013c2a88391";
				public const string Pictures_Vacation = "29e13e2d-920b-4fe0-b659-e44cf5600438";
				public const string Pictures_Vacation_File_1 = "e8c45b23-72fb-4406-8898-25cab5b8036b";
				public const string Pictures_Vacation_File_2 = "f63ab4a3-1d9f-45e4-abaf-f0921ac70e2f";
				public const string Pictures_Vacation_File_3 = "e63ab4a3-1d2a-11e4-abaf-ab241ac76e2f";
			}

			public struct Customer_2
			{
				public const string Id = "2022be9e-970e-47b0-b460-52fe973d5c2a";

				public const string Home = "4e2c6091-ca28-4155-b33c-6148247ee500";
				public const string File_1 = "a48efaf5-5fbc-4594-9f38-8370577516cd";
			}
		}

		private readonly Mock<IContentOperationContext> _mockContext;
		private readonly Mock<ILogger<ContentService>> _mockLogger;
		private List<Content> _mockData;
		private MockAsyncEnumerable<Content> _mockDataSource
			=> new(_mockData);

		public ContentServiceTests()
		{
			_mockData = CreateMockData();
			_mockContext = new Mock<IContentOperationContext>();
			_mockContext
				.Setup(x => x.Content)
				.Returns(_mockDataSource);
			_mockContext
				.Setup(x => x.Remove(It.IsAny<IEnumerable<Content>>()))
				.Callback((IEnumerable<Content> x) =>
					_mockData = _mockData.ExceptBy(x.Select(x => x.Id), x => x.Id).ToList());

			_mockContext
				.Setup(x => x.AddAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
				.Callback((Content x, CancellationToken y) => _mockData.Add(x));

			_mockLogger = new Mock<ILogger<ContentService>>();
		}

		[TestMethod]
		[DataRow(Guids.Customer_1.Id, null, "Document", StringMatchType.Exact, ContentType.File, null)]
		[DataRow(Guids.Customer_1.Id, Guids.Customer_1.Documents, "Document", StringMatchType.Exact, ContentType.File, null)]
		[DataRow(Guids.Customer_1.Id, Guids.Customer_1.Pictures_Vacation, "Pict", StringMatchType.StartsWith, ContentType.File, null)]
		[DataRow(Guids.Customer_1.Id, null, "Pictures", StringMatchType.Exact, ContentType.Directory, null)]
		[DataRow(Guids.Customer_1.Id, null, "ctur", StringMatchType.Contains, ContentType.Directory, null)]
		[DataRow(Guids.Customer_1.Id, null, "ent", StringMatchType.EndsWith, ContentType.File, 1)]
		public async Task Get_ByName_Should_Return_Data(string customerId, string parentId, string name, StringMatchType matchType, ContentType contentType, int? top)
		{
			IContentService service = GetServiceInstance();
			Guid customerIdGuid = Guid.Parse(customerId);
			Guid? parentIdGuid = parentId != null ? Guid.Parse(parentId) : null;
			var request = new SearchContentRequestByName(customerIdGuid, name)
			{
				ContentType = contentType,
				ParentId = parentIdGuid,
				MatchType = matchType,
				Top = top
			};

			var items = await service.Get(request);

			items.Should().NotBeNull().And.NotBeEmpty();
			if (top.HasValue)
				items.Should().HaveCountLessThanOrEqualTo(top.Value);
		}

		[TestMethod]
		public async Task Save_Should_Add_Data()
		{
			IContentService service = GetServiceInstance();
			Guid customerId = Guid.Parse(Guids.Customer_2.Id);
			var request = new SaveContentRequest(customerId, Guid.Parse(Guids.Customer_2.Home), "New", ContentType.Directory);
			int defaultCount = _mockData.Count;

			await service.Save(request);

			_mockData.Should().HaveCount(defaultCount + 1);
			_mockData.FirstOrDefault(x => x.Id == Guid.Empty).Should().NotBeNull();
		}

		[TestMethod]
		[DataRow(Guids.Customer_1.Id, Guids.Customer_1.Documents, Guids.Customer_1.Home, "Documents_Changed")]
		[DataRow(Guids.Customer_1.Id, Guids.Customer_1.Pictures_Vacation, Guids.Customer_1.Documents, "Vacation")]
		public async Task Update_Should_Change_Data(string customerId, string id, string parentId, string name)
		{
			IContentService service = GetServiceInstance();

			Guid customerIdGuid = Guid.Parse(customerId);
			Guid parentIdGuid = Guid.Parse(parentId);
			Guid idGuid = Guid.Parse(id);
			var request = new UpdateContentRequest(customerIdGuid, idGuid, parentIdGuid, name);

			var currentItem = _mockData.First(x => x.CustomerId == customerIdGuid && x.Id == idGuid);
			string currentPath = currentItem.Path;
			var descendantPaths = currentItem.Children.Select(x => new { x.Id, x.Path }).ToArray();

			await service.Update(request);

			var changedItem = _mockData.First(x => x.CustomerId == customerIdGuid && x.Id == idGuid);
			changedItem.Name.Should().Be(name);
			changedItem.ParentId.Should().Be(parentId);
			changedItem.Children.As<List<Content>>()
				.ForEach(x => x.Path.Should().Be(descendantPaths.First(y => y.Id == x.Id).Path.Replace(currentPath, changedItem.Path)));
		}

		[TestMethod]
		[DataRow(Guids.Customer_1.Id, Guids.Customer_1.Documents)]
		public async Task Delete_Should_Remove_Data(string customerId, string id)
		{
			IContentService service = GetServiceInstance();

			Guid customerIdGuid = Guid.Parse(customerId);
			Guid idGuid = Guid.Parse(id);
			var request = new DeleteContentRequest(customerIdGuid, idGuid);

			await service.Delete(request);

			_mockData.FirstOrDefault(x => x.CustomerId == customerIdGuid && x.Id == idGuid).Should().BeNull();
			_mockData.Where(x => x.CustomerId == customerIdGuid && x.ParentId == idGuid).Should().BeEmpty();
		}

		#region Private

		private List<Content> CreateMockData()
		{
			// customer_1
			Content pictures_vacation_file_1_customer_1 = new Content
			{
				Id = Guid.Parse(Guids.Customer_1.Pictures_Vacation_File_1),
				CustomerId = Guid.Parse(Guids.Customer_1.Id),
				ParentId = Guid.Parse(Guids.Customer_1.Pictures_Vacation),
				Name = "Picture_1",
				Path = "Home/Pictures/Vacation/Picture_1.jpg",
				Type = (byte)ContentType.File,
			};

			Content pictures_vacation_file_2_customer_1 = new Content
			{
				Id = Guid.Parse(Guids.Customer_1.Pictures_Vacation_File_2),
				CustomerId = Guid.Parse(Guids.Customer_1.Id),
				ParentId = Guid.Parse(Guids.Customer_1.Pictures_Vacation),
				Name = "Picture_2",
				Path = "Home/Pictures/Vacation/Picture_2.jpg",
				Type = (byte)ContentType.File,
			};

			Content pictures_vacation_file_3_customer_1 = new Content
			{
				Id = Guid.Parse(Guids.Customer_1.Pictures_Vacation_File_3),
				CustomerId = Guid.Parse(Guids.Customer_1.Id),
				ParentId = Guid.Parse(Guids.Customer_1.Pictures_Vacation),
				Name = "Document",
				Path = "Home/Pictures/Vacation/Document.jpg",
				Type = (byte)ContentType.File,
			};

			Content documents_file_1_customer_1 = new Content
			{
				Id = Guid.Parse(Guids.Customer_1.Documents_File_1),
				CustomerId = Guid.Parse(Guids.Customer_1.Id),
				ParentId = Guid.Parse(Guids.Customer_1.Documents),
				Name = "Document",
				Path = "Home/Documents/Document.pdf",
				Type = (byte)ContentType.File,
			};

			Content pictures_vacation_customer_1 = new Content
			{
				Id = Guid.Parse(Guids.Customer_1.Pictures_Vacation),
				CustomerId = Guid.Parse(Guids.Customer_1.Id),
				ParentId = Guid.Parse(Guids.Customer_1.Pictures),
				Name = "Vacation",
				Path = "Home/Pictures/Vacation",
				Type = (byte)ContentType.Directory,
				Children = new List<Content>() { pictures_vacation_file_1_customer_1, pictures_vacation_file_2_customer_1 }
			};

			Content pictures_customer_1 = new Content
			{
				Id = Guid.Parse(Guids.Customer_1.Pictures),
				CustomerId = Guid.Parse(Guids.Customer_1.Id),
				ParentId = Guid.Parse(Guids.Customer_1.Home),
				Name = "Pictures",
				Path = "Home/Pictures",
				Type = (byte)ContentType.Directory,
				Children = new List<Content>() { pictures_vacation_customer_1 }
			};

			Content documents_customer_1 = new Content
			{
				Id = Guid.Parse(Guids.Customer_1.Documents),
				CustomerId = Guid.Parse(Guids.Customer_1.Id),
				ParentId = Guid.Parse(Guids.Customer_1.Home),
				Name = "Documents",
				Path = "Home/Documents",
				Type = (byte)ContentType.Directory,
				Children = new List<Content>() { documents_file_1_customer_1 }
			};

			Content home_customer_1 = new Content
			{
				Id = Guid.Parse(Guids.Customer_1.Home),
				CustomerId = Guid.Parse(Guids.Customer_1.Id),
				Name = "Home",
				Path = "Home",
				Type = (byte)ContentType.Directory,
				Children = new List<Content>() { documents_customer_1, pictures_customer_1 }
			};

			//customer_2
			Content file_1_customer_2 = new Content
			{
				Id = Guid.Parse(Guids.Customer_2.File_1),
				CustomerId = Guid.Parse(Guids.Customer_2.Id),
				ParentId = Guid.Parse(Guids.Customer_2.Home),
				Name = "File_1",
				Path = "Home/File_1.docx",
				Type = (byte)ContentType.File
			};

			Content home_customer_2 = new Content
			{
				Id = Guid.Parse(Guids.Customer_2.Home),
				CustomerId = Guid.Parse(Guids.Customer_2.Id),
				Name = "Home",
				Path = "Home",
				Type = (byte)ContentType.Directory,
				Children = new List<Content>() { file_1_customer_2 }
			};

			return new List<Content>() {
				home_customer_1,
				documents_customer_1,
				documents_file_1_customer_1,
				pictures_customer_1,
				pictures_vacation_customer_1,
				pictures_vacation_file_1_customer_1,
				pictures_vacation_file_2_customer_1,
				home_customer_2,
				file_1_customer_2
			};

			//if (deleted != null)
			//	items= items.ExceptBy(deleted.Select(x => x.Id), x => x.Id).ToList();

			//return new MockAsyncEnumerable<Content>(items);
		}

		private void AddMockData()
		{

		}

		private void RemoveMockData(IEnumerable<Content>? deleted)
		{

		}

		private IContentService GetServiceInstance()
			=> (IContentService)Activator.CreateInstance(typeof(ContentService), _mockContext.Object, _mockLogger.Object)!;

		#endregion
	}
}
