using LibraryWorkstation.Data.Entities.Common;

namespace LibraryWorkstation.Data.Entities;

public class Borrowing : IBaseEntity
{
    public Guid Id { get; set; }
    public Guid ReaderId { get; set; }
    public Guid BookId { get; set; }
    public DateOnly BorrowedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly? ReturnedDate { get; set; }
    public DateOnly Deadline { get; set; }

    public Reader Reader { get; set; }
    public Book Book { get; set; }
}