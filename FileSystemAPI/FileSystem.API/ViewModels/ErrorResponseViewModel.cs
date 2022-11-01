using System.Text.Json.Serialization;

namespace FileSystem.API.ViewModels
{
	internal class ErrorResponseViewModel
	{
		[JsonPropertyName("statusCode")]
		public int StatusCode { get; set; }

		[JsonPropertyName("message")]
		public string? Message { get; set; }
	}
}
