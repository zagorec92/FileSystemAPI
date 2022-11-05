namespace FileSystem.Infrastructure.Exceptions
{
	public class RootExistsException : Exception
	{
		public Guid CustomerId { get; set; }

		public RootExistsException(Guid customerId)
			: base("Root directory has already been created.") => CustomerId = customerId;
	}
}
