namespace FileSystem.Core.Entities
{
	public class File : EntityBase
	{
		public string Name { get; set; }
		public Guid CustomerId { get; set; }
		public Guid DirectoryId { get; set; }
		public string MimeType { get; set; }
		public long ContentLength { get; set; }

		public Directory Directory { get; set; }
	}
}
