using FileSystem.Core.Entities;
using FileSystem.Core.Enums;
using System.Text.Json.Serialization;

namespace FileSystem.ViewModels
{
	public class ContentViewModelBase
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string Type { get; set; }
		public Guid? ParentId { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }
		public List<LinkViewModel> Links { get; set; }

		public ContentViewModelBase(Content entity)
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

	public class ContentViewModelSimple
	{
		public Guid Id { get; set; }
		public string Name { get; set; }

		public ContentViewModelSimple(Content entity)
		{
			Id = entity.Id;
			Name = entity.Name;

		}
	}

	public class ContentViewModel : ContentViewModelBase
	{
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// The <see cref="JsonPropertyOrderAttribute"/> was added only to make the serialized output more readable.
		/// Without setting this value, serializer takes this as the first property in default order.
		/// </remarks>
		[JsonPropertyOrder(100)]
		public ICollection<ContentViewModelBase> Descendants { get; set; }

		public ContentViewModel(Content entity)
			: base(entity)
		{
			Descendants = entity.Children?.Select(x => new ContentViewModelBase(x)).ToList() ?? new();
		}
	}
}
