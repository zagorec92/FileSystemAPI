namespace FileSystem.Core.Settings
{
	public class Settings
	{
		public ConnectionStrings? ConnectionStrings { get; set; }
	}

	public class ConnectionStrings
	{
		public string? FileSystemDb { get; set; }
	}
}
