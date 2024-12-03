using LibraryWorkstation.Data.Entities.Common;

namespace LibraryWorkstation.Data.Entities;

public class Book : IBaseEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public int AvailableCount { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public int YearPublished { get; set; }
    
    public Author Author { get; set; }
    public ICollection<Genre> Genres { get; set; }
}