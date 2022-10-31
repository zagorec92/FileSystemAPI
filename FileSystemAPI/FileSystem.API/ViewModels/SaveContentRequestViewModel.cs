using FileSystem.Core.Enums;
using FileSystem.Core.Models.Requests;
using FileSystem.Infrastructure;

namespace FileSystem.API.ViewModels
{
	public class SaveContentRequestViewModel
	{
		public Guid? ParentId { get; set; }
		public string Name { get; set; }
		public byte Type { get; private set; }

		public SaveContentRequestViewModel()
		{
			Name = Constants.RootDirectory;
			Type = (byte)ContentType.Directory;
		}

		public SaveContentRequestViewModel(string parentId, string name, byte type)
		{
			if(Guid.TryParse(parentId, out Guid parentGuid))
				ParentId = parentGuid;

			Name = name;
			Type = type;
		}

		public SaveContentRequest ToSaveContentRequest(Guid customerId)
			=> new(customerId, ParentId, Name, (ContentType)Type);
	}
}
