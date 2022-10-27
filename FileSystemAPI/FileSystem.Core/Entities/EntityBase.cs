using System.ComponentModel.DataAnnotations.Schema;

namespace FileSystem.Core.Entities
{
	public class EntityBase
	{
		public Guid Id { get; set; }
		public long Created { get; set; }
		public long Modified { get; set; }
		public byte[] RowVersion { get; set; }
	}
}
