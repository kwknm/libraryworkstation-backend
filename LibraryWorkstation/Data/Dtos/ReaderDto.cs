namespace LibraryWorkstation.Data.Dtos;

public class ReaderDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ShortReaderId { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
}