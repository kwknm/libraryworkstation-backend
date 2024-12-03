using LibraryWorkstation.Data.Entities.Common;

namespace LibraryWorkstation.Data.Entities;

public class Author : Person, IBaseEntity
{
    public Guid Id { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}