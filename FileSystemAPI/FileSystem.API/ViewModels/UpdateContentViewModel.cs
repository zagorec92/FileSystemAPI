using FileSystem.Core.Models.Requests;

namespace FileSystem.API.ViewModels
{
	public class UpdateContentRequestViewModel
	{
		public string ParentId { get; set; }
		public string Name { get; set; }

		public UpdateContentRequestViewModel(string parentId, string name)
		{
			ParentId = parentId;
			Name = name;
		}

		public UpdateContentRequest ToUpdateContentRequest(Guid customerId, Guid id)
			=> new(customerId, id, Guid.Parse(ParentId), Name);
	}
}
