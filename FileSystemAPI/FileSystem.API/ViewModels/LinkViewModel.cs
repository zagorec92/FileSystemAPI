namespace FileSystem.ViewModels
{
	public class LinkViewModel
	{
		public string Href { get; set; }
		public string Rel { get; set; }
		public string Method { get; set; }
		public LinkViewModel(string href, string rel, string method)
		{
			Href = href;
			Rel = rel;
			Method = method;
		}
	}
}

