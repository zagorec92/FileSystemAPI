using FileSystem.Core.Models.Requests;

namespace FileSystem.API.ViewModels
{
	public class UpdateContentRequestViewModel
	{
		public Guid ParentId { get; set; }
		public string Name { get; set; }

		public UpdateContentRequest ToUpdateContentRequest(Guid customerId, Guid id)
			=> new(customerId, id, ParentId, Name);
	}
}
