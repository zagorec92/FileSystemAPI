using System;

namespace FileSystem.Core.Entities
{
	public class Directory : EntityBase
	{
		public string Name { get; set; }
		public Guid CustomerId { get; set; }
		public Guid? ParentId { get; set; }

		public Directory Parent { get; set; }
		public ICollection<Directory> Children { get; set; }
		public ICollection<File> Files { get; set; }
	}
}
