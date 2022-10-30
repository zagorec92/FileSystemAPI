namespace FileSystem.Core.Models.Requests
{
    public class DeleteContentRequest
    {
        public Guid CustomerId { get; private set; }
        public Guid Id { get; private set; }

        public DeleteContentRequest(Guid customerId, Guid id)
        {
            CustomerId = customerId;
            Id = id;
        }
    }
}
