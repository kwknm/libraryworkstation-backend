using AutoMapper;
using LibraryWorkstation.Data;
using LibraryWorkstation.Data.Dtos;
using LibraryWorkstation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWorkstation.Endpoints;

public record CreateBookRequest(
    string Title,
    string? Description,
    ICollection<Guid> Genres,
    Guid AuthorId,
    int AvailableCount,
    string ISBN,
    int YearPublished);

public static class BooksEndpoint
{
    public static void MapBooksEndpoint(this WebApplication app)
    {
        var endpoint = app.MapGroup("/api/books");
        endpoint.MapGet("/", GetAllBooksAsync);
        endpoint.MapGet("/{id:guid}", GetBookByIdAsync);
        endpoint.MapPost("/", AddBookAsync);
        endpoint.MapDelete("/{id:guid}", DeleteBookAsync);
        endpoint.MapPost("/{id:guid}/replenish", ReplenishBooksAsync);
    }

    private static async Task<IResult> GetAllBooksAsync(LibraryDbContext context, IMapper mapper,
        [FromQuery] string? search, [FromQuery] Guid? authorId, [FromQuery] Guid? genreId)
    {
        var books = context.Books 
            .Include(x => x.Author)
            .Include(x => x.Genres)
            .AsNoTracking();

        if (authorId is not null)
            books = books.Where(x => x.AuthorId == authorId);

        if (genreId is not null)
        {
            var genre = await context.Genres.FindAsync(genreId);
            if (genre is not null)
                books = books.Where(x => x.Genres.Contains(genre));
        }

        return search is not null
            ? Results.Ok(mapper.Map<List<BookDto>>(await books.Where(x => EF.Functions.Like(x.Title, $"%{search}%"))
                .AsNoTracking().ToListAsync()))
            : Results.Ok(mapper.Map<List<BookDto>>(await books.AsNoTracking().ToListAsync()));
    }

    private static async Task<IResult> GetBookByIdAsync(LibraryDbContext context, Guid id, IMapper mapper)
    {
        var book = await context.Books
            .Include(x => x.Author)
            .Include(x => x.Genres)
            .FirstOrDefaultAsync(x => x.Id == id);

        var dto = mapper.Map<BookDto>(book);
        dto.BorrowedBooks = context.Borrowings.Where(x => x.BookId == id && x.ReturnedDate == null).AsNoTracking()
            .Count();
        return book is null
            ? Results.NotFound(new { Message = "Книга не найдена" })
            : Results.Ok(dto);
    }

    private static async Task<IResult> DeleteBookAsync(LibraryDbContext context, Guid id)
    {
        var rows = await context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();
        return rows == 0 ? Results.NotFound() : Results.NoContent();
    }

    private static async Task<IResult> AddBookAsync(LibraryDbContext context, CreateBookRequest request, IMapper mapper)
    {
        var genres = new List<Genre>();
        foreach (var genreId in request.Genres)
        {
            var genre = await context.Genres.FindAsync(genreId);
            if (genre is null)
                continue;
            genres.Add(genre);
        }

        var author = await context.Authors.FindAsync(request.AuthorId);
        if (author is null)
        {
            return Results.BadRequest(new { Message = "Автор не найден" });
        }

        var newBook = new Book
        {
            Title = request.Title,
            Description = request.Description,
            Genres = genres,
            ISBN = request.ISBN,
            AvailableCount = request.AvailableCount,
            YearPublished = request.YearPublished,
            Author = author
        };
        var entity = await context.Books.AddAsync(newBook);
        await context.SaveChangesAsync();
        return Results.Created($"api/books/{entity.Entity.Id}", mapper.Map<BookDto>(entity.Entity));
    }

    private static async Task<IResult> ReplenishBooksAsync(LibraryDbContext context, Guid id, [FromQuery] int qty)
    {
        var book = await context.Books.FindAsync(id);
        if (book is null)
        {
            return Results.BadRequest(new { Message = "Книга не найдена" });
        }

        await context.Books.ExecuteUpdateAsync(s => s.SetProperty(e => e.AvailableCount, e => e.AvailableCount + qty));

        return Results.Ok(new { AvailableCount = book.AvailableCount + qty });
    }
}