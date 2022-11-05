using FileSystem.Controllers;
using FileSystem.Core.Models.Requests;

namespace FileSystem.API.ViewModels
{
	/// <summary>
	/// Type that encapsulates the <see cref="HttpRequest.Body"/> parameters.
	/// </summary>
	/// <remarks>
	/// Used in <see cref="ContentController.Update(Guid, Guid, UpdateContentRequestViewModel)"/> action.
	/// </remarks>
	public class UpdateContentRequestViewModel
	{
		/// <summary>
		/// The <see cref="Guid"/> content parent id.
		/// </summary>
		public Guid ParentId { get; set; }

		/// <summary>
		/// The <see cref="string"/> content name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Creates a new <see cref="UpdateContentRequest"/> instance from the current one.
		/// </summary>
		/// <param name="customerId">The <see cref="Guid"/> customer identifier.</param>
		/// <param name="id">The <see cref="Guid"/> content identifier.</param>
		/// <returns>The <see cref="UpdateContentRequest"/> instance.</returns>
		public UpdateContentRequest ToUpdateContentRequest(Guid customerId, Guid id)
			=> new(customerId, id, ParentId, Name);
	}
}
