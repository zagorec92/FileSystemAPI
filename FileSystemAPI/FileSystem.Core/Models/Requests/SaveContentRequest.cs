using FileSystem.Core.Enums;

namespace FileSystem.Core.Models.Requests
{
	public class SaveContentRequest
	{
		public Guid CustomerId { get; set; }
		public Guid? ParentId { get; set; }
		public string Name { get; set; }
		public ContentType Type { get; private set; }

		public SaveContentRequest(Guid customerId, Guid? parentId, string name, ContentType type)
		{
			CustomerId = customerId;
			ParentId = parentId;
			Name = name;
			Type = type;
		}
	}
}
