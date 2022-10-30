namespace FileSystem.Core.Models.Requests
{
	public class UpdateContentRequest
	{
		public Guid CustomerId { get; set; }
		public Guid? Id { get; set; }
		public Guid ParentId { get; set; }
		public string Name { get; set; }

		public UpdateContentRequest(Guid customerId, Guid parentId, string name)
		{
			CustomerId = customerId;
			ParentId = parentId;
			Name = name;
		}

		public UpdateContentRequest(Guid customerId, Guid id, Guid parentId, string name)
		{
			CustomerId = customerId;
			Id = id;
			ParentId = parentId;
			Name = name;
		}
	}
}
