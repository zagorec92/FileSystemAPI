using FileSystem.Core.Enums;

namespace FileSystem.Core.Models.Requests
{
	public class SaveContentRequest : UpdateContentRequest
	{
		public ContentType Type { get; private set; }

		public SaveContentRequest(Guid customerId, Guid parentId, string name, ContentType type)
			: base(customerId, parentId, name)
		{
			Type = type;
		}
	}
}
