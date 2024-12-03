using LibraryWorkstation.Data.Entities.Common;

namespace LibraryWorkstation.Data.Entities;

public class Reader : Person, IBaseEntity
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; } = DateTime.Now;
    public ICollection<Borrowing> Borrowings { get; set; }
}