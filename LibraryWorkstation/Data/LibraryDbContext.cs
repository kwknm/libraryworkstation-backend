using LibraryWorkstation.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryWorkstation.Data;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();
}