using FileSystem.Core.Entities;
using FileSystem.Core.Enums;
using System.Linq.Expressions;

namespace FileSystem.Core.Models.Requests
{
    public class SearchContentRequest
    {
        public Guid CustomerId { get; private set; }
        public Guid? ParentId { get; set; }
        public int? Top { get; set; }
        public List<SortCriteria> SortCriteria { get; set; }
        public ContentType? ContentType { get; set; }
        public List<Expression<Func<Content, object>>>? Includes { get; set; }

        public SearchContentRequest(Guid customerId) => CustomerId = customerId;
    }

    public class SearchContentRequestByName : SearchContentRequest
    {
        public string Name { get; private set; }
        public StringMatchType MatchType { get; set; } = StringMatchType.Exact;

        public SearchContentRequestByName(Guid customerId, string name)
            : base(customerId) => Name = name;
    }

    public class SearchContentRequestByIds : SearchContentRequest
    {
        public List<Guid> Ids { get; private set; }

        public SearchContentRequestByIds(Guid customerId, List<Guid> ids)
            : base(customerId) => Ids = ids;
    }

    public class SearchContentRequestByPath : SearchContentRequest
    {
        public string Path { get; private set; }
        public bool IsRoot { get; private set; }

        public SearchContentRequestByPath(Guid customerId, string path, bool isRoot = false)
            : base(customerId)
        {
            Path = path;
            IsRoot = isRoot;
            Includes = new() { x => x.Children };
        }
    }
}
