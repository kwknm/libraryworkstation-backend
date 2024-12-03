namespace LibraryWorkstation.Data.Dtos;

public class BorrowingDto
{
    public Guid Id { get; set; }
    public DateOnly BorrowedDate { get; set; }
    public DateOnly? ReturnedDate { get; set; }
    public DateOnly Deadline { get; set; }

    public ReaderDto Reader { get; set; }
    public BookDto Book { get; set; }
}