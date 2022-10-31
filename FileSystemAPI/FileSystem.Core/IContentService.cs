using FileSystem.Core.Entities;
using FileSystem.Core.Models.Requests;

namespace FileSystem.Core
{
	public interface IContentService
	{
		Task Delete(DeleteContentRequest request);
		Task<IEnumerable<Content>> Get(SearchContentRequest request);
		Task<IEnumerable<Content>> Get(SearchContentRequestByName request);
		Task<IEnumerable<Content>> Get(SearchContentRequestByIds request);
		Task<IEnumerable<Content>> Get(SearchContentRequestByPath request);
		Task<Content> Save(SaveContentRequest request);
		Task Update(UpdateContentRequest request);
	}
}
