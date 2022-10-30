using FileSystem.Core.Enums;
using FileSystem.Core.Models.Requests;

namespace FileSystem.API.ViewModels
{
	public class SaveContentRequestViewModel : UpdateContentRequestViewModel
	{
		public byte Type { get; private set; }

		public SaveContentRequestViewModel(string parentId, string name, byte type)
			: base(parentId, name) => Type = type;

		public SaveContentRequest ToSaveContentRequest(Guid customerId)
			=> new(customerId, Guid.Parse(ParentId), Name, (ContentType)Type);
	}
}
