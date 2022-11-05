using FileSystem.Core.Enums;
using FileSystem.Core.Models.Requests;
using FileSystem.Infrastructure;

namespace FileSystem.API.ViewModels
{
	public class SaveContentRequestViewModel
	{
		public Guid? ParentId { get; set; }
		public string Name { get; set; }
		public ContentType Type { get; set; }

		public SaveContentRequestViewModel()
		{
			Name = Constants.RootDirectory;
			Type = (byte)ContentType.Directory;
		}

		public SaveContentRequest ToSaveContentRequest(Guid customerId)
			=> new(customerId, ParentId, Name, (ContentType)Type);
	}
}
