namespace FileSystem.Infrastructure.Exceptions
{

	public class NotFoundException : Exception
	{
		public Guid CustomerId { get; set; }
		public Guid ContentId { get; set; }

		public NotFoundException(Guid customerId, Guid contentId)
			: base("Content could not be found.")
		{
			CustomerId = customerId;
			ContentId = contentId;
		}
	}

	public class RootExistsException: Exception
	{
		public Guid CustomerId { get; set; }

		public RootExistsException(Guid customerId)
			: base("Root directory has already been created.") => CustomerId = customerId;
	}


}
