namespace LibraryWorkstation.Data.Dtos;

public class BookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Genres { get; set; }
    public AuthorDto Author { get; set; }
    public int AvailableCount { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public int YearPublished { get; set; }
    public int BorrowedBooks { get; set; }
}