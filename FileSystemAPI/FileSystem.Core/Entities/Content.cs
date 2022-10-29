namespace FileSystem.Core.Entities
{
	public class Content
	{
		public Guid Id { get; set; }
		public Guid CustomerId { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public byte Type { get; set; }
		public Guid? ParentId { get; set; }
		public long Created { get; set; }
		public long Modified { get; set; }
		public byte[] RowVersion { get; set; }

		public Content Parent { get; set; }
		public ICollection<Content> Children { get; set; }
	}
}
