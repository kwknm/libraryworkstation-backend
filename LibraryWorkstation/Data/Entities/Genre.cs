using LibraryWorkstation.Data.Entities.Common;

namespace LibraryWorkstation.Data.Entities;

public class Genre : IBaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<Book> Books { get; set; }
}