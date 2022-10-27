using Directory = FileSystem.Core.Entities.Directory;

namespace FileSystem.ViewModels
{
    public class DirectoryViewModel
    {
        public Guid Identifier { get; set; }
        public string Name { get; set; }
        public Guid? ParentIdentifier { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public DirectoryViewModel(Directory entity)
        {
            Identifier = entity.Id;
            ParentIdentifier = entity.ParentId;
            Name = entity.Name;
            Created = DateTimeOffset.FromUnixTimeSeconds(entity.Created).DateTime;
            Modified = DateTimeOffset.FromUnixTimeSeconds(entity.Modified).DateTime;
        }

    }
}
