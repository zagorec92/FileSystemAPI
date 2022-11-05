using FileSystem.Controllers;
using FileSystem.Core.Enums;
using FileSystem.Core.Models.Requests;
using FileSystem.Infrastructure;

namespace FileSystem.API.ViewModels
{
	/// <summary>
	/// Type that encapsulates the <see cref="HttpRequest.Body"/> parameters.
	/// </summary>
	/// <remarks>
	/// Used in <see cref="ContentController.Save(Guid, SaveContentRequestViewModel)"/> action.
	/// </remarks>
	public class SaveContentRequestViewModel
	{
		/// <summary>
		/// The <see cref="Guid"/> content parent id.
		/// </summary>
		public Guid? ParentId { get; set; }
		
		/// <summary>
		/// The <see cref="string"/> content name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The <see cref="ContentType"/> content type.
		/// </summary>
		public ContentType Type { get; set; }

		/// <summary>
		/// Creates a new <see cref="SaveContentRequestViewModel"/> instance.
		/// </summary>
		public SaveContentRequestViewModel()
		{
			Name = Constants.RootDirectory;
			Type = (byte)ContentType.Directory;
		}

		/// <summary>
		/// Creates a new <see cref="SaveContentRequest"/> instance from the current one.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <returns>The <see cref="SaveContentRequest"/> instance.</returns>
		public SaveContentRequest ToSaveContentRequest(Guid customerId)
			=> new(customerId, ParentId, Name, Type);
	}
}
