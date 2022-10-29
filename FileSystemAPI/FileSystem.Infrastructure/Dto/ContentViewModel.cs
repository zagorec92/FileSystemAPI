using FileSystem.Core.Entities;

namespace FileSystem.Infrastructure.Dto
{
	public class ContentDto
	{
		public Guid Identifier { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public byte Type { get; set; }
		public Guid? ParentIdentifier { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }

		public ContentDto(Content entity)
		{
			Identifier = entity.Id;
			ParentIdentifier = entity.ParentId;
			Name = entity.Name;
			Path = entity.Path;
			Type = entity.Type;
			Created = DateTimeOffset.FromUnixTimeSeconds(entity.Created).DateTime;
			Modified = DateTimeOffset.FromUnixTimeSeconds(entity.Modified).DateTime;
		}
	}

	public class ContentDtoExtended : ContentDto
	{
		public List<ContentDto> Children { get; set; }

		public ContentDtoExtended(Content entity)
			: base(entity)
		{
			if ((entity.Children?.Any()).GetValueOrDefault())
				Children = entity.Children!.Select(x => new ContentDto(x)).ToList();
		}
	}
}
