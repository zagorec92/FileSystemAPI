namespace FileSystem.Infrastructure.Exceptions
{
	public class NotFoundException : Exception
	{
		public Guid CustomerId { get; set; }
		public Guid? ContentId { get; set; }
		public string? Path { get; set; }

		public NotFoundException(Guid customerId, Guid contentId)
			: base("Content could not be found.")
		{
			CustomerId = customerId;
			ContentId = contentId;
		}

		public NotFoundException(Guid customerId, string? path)
			: base("Content could not be found.")
		{
			CustomerId = customerId;
			Path = path;
		}
	}
}
