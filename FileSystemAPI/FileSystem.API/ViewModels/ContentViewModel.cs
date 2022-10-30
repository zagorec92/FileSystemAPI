using FileSystem.Core.Entities;
using FileSystem.Core.Enums;
using System.Text.Json.Serialization;

namespace FileSystem.ViewModels
{
	public class ContentViewModelSimple
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public List<LinkViewModel> Links { get; set; }

		public ContentViewModelSimple(Content entity)
		{
			Id = entity.Id;
			Name = entity.Name;
			Path = entity.Path;
			Links = new();
		}
	}

	public class ContentViewModel : ContentViewModelSimple
	{
		public string Type { get; set; }
		public Guid? ParentId { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }

		public ContentViewModel(Content entity)
			: base(entity)
		{
			Id = entity.Id;
			ParentId = entity.ParentId;
			Name = entity.Name;
			Path = entity.Path;
			Type = ((ContentType)entity.Type).ToString();
			Created = DateTimeOffset.FromUnixTimeSeconds(entity.Created).DateTime;
			Modified = DateTimeOffset.FromUnixTimeSeconds(entity.Modified).DateTime;
			Links = new();
		}
	}

	public class ContentViewModelRich : ContentViewModel
	{
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// The <see cref="JsonPropertyOrderAttribute"/> was added only to make the serialized output more readable.
		/// Without setting this value, serializer takes this as the first property in default order.
		/// </remarks>
		[JsonPropertyOrder(100)]
		public ICollection<ContentViewModel> Descendants { get; set; }

		public ContentViewModelRich(Content entity)
			: base(entity)
		{
			Descendants = entity.Children?.Select(x => new ContentViewModel(x)).ToList() ?? new();
		}
	}
}
