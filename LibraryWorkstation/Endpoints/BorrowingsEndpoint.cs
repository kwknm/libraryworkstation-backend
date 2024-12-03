using AutoMapper;
using LibraryWorkstation.Data;
using LibraryWorkstation.Data.Dtos;
using LibraryWorkstation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWorkstation.Endpoints;

public record EditBorrowingRequest(DateOnly? Deadline, DateOnly? ReturnedDate);

public record NewBorrowingRequest(Guid ReaderId, Guid BookId, string Deadline);

public static class BorrowingsEndpoint
{
    public static void MapBorrowingsEndpoint(this WebApplication app)
    {
        var endpoint = app.MapGroup("/api/borrowings");
        endpoint.MapGet("/", GetAllBorrowingsAsync);
        endpoint.MapPost("/", AddBorrowingAsync);
        endpoint.MapGet("/{id:guid}", GetBorrowingByIdAsync);
        endpoint.MapPost("/{id:guid}/return", ReturnBorrowedBookAsync);
        endpoint.MapPatch("/{id:guid}", EditBorrowingAsync);
        endpoint.MapDelete("/{id:guid}", DeleteBorrowingAsync);
    }

    public static async Task<IResult> GetAllBorrowingsAsync(LibraryDbContext context, IMapper mapper,
        [FromQuery] Guid? readerId, [FromQuery] string? type = "all")
    {
        var borrowings = context.Borrowings
            .Include(x => x.Book)
            .Include(x => x.Book.Author)
            .Include(x => x.Book.Genres)
            .Include(x => x.Reader)
            .AsNoTracking();

        switch (type)
        {
            case "open":
                borrowings = borrowings.Where(x => x.ReturnedDate == null);
                break;
            case "close":
                borrowings = borrowings.Where(x => x.ReturnedDate != null);
                break;
            case "overdue":
                borrowings = borrowings.Where(x =>
                    x.Deadline < DateOnly.FromDateTime(DateTime.Now) && x.ReturnedDate == null);
                break;
        }

        if (readerId is not null)
        {
            borrowings = borrowings.Where(x => x.ReaderId == readerId);
        }

        return Results.Ok(mapper.Map<List<BorrowingDto>>(await borrowings.ToListAsync()));
    }

    public static async Task<IResult> GetBorrowingByIdAsync(LibraryDbContext context, IMapper mapper, Guid id)
    {
        return Results.Ok(mapper.Map<BorrowingDto>(await context.Borrowings
            .Include(x => x.Book)
            .Include(x => x.Book.Author)
            .Include(x => x.Book.Genres)
            .Include(x => x.Reader)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)));
    }

    public static async Task<IResult> ReturnBorrowedBookAsync(LibraryDbContext context, IMapper mapper, Guid id)
    {
        var borrowing = await context.Borrowings
            .Include(x => x.Book)
            .Include(x => x.Book.Author)
            .Include(x => x.Book.Genres)
            .Include(x => x.Reader)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (borrowing is null)
        {
            return Results.NotFound(new { Message = "Не найдено" });
        }

        var book = await context.Books.FindAsync(borrowing.BookId);
        if (book is not null)
            book.AvailableCount++;

        borrowing.ReturnedDate = DateOnly.FromDateTime(DateTime.Now);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }

    public static async Task<IResult> EditBorrowingAsync(LibraryDbContext context, IMapper mapper, Guid id,
        EditBorrowingRequest request)
    {
        var borrowing = await context.Borrowings
            .Include(x => x.Book)
            .Include(x => x.Book.Author)
            .Include(x => x.Book.Genres)
            .Include(x => x.Reader)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (borrowing is null)
        {
            return Results.NotFound(new { Message = "Не найдено" });
        }

        if (request.Deadline is not null)
            borrowing.Deadline = (DateOnly)request.Deadline;

        if (request.ReturnedDate is not null)
            borrowing.ReturnedDate = (DateOnly)request.ReturnedDate;
        await context.SaveChangesAsync();
        return Results.NoContent();
    }

    public static async Task<IResult> AddBorrowingAsync(LibraryDbContext context, IMapper mapper,
        NewBorrowingRequest request)
    {
        var reader = await context.Readers.FindAsync(request.ReaderId);
        if (reader is null)
            return Results.BadRequest(new { Message = "Читатель не найден" });

        var book = await context.Books.FindAsync(request.BookId);
        if (book is null)
            return Results.BadRequest(new { Message = "Книга не найдена" });
        if (book.AvailableCount == 0)
        {
            return Results.BadRequest(new { Message = "На складе не осталось доступных книг" });
        }

        var deadline = DateOnly.Parse(request.Deadline);

        if (deadline < DateOnly.FromDateTime(DateTime.Now))
            return Results.BadRequest(new { Message = "Неверный срок сдачи" });

        var newBorrowing = new Borrowing
        {
            BookId = book.Id,
            ReaderId = reader.Id,
            Deadline = deadline
        };
        var entity = await context.Borrowings.AddAsync(newBorrowing);
        book.AvailableCount--;
        await context.SaveChangesAsync();

        var fetchedBorrowing = await context.Borrowings
            .Include(x => x.Book)
            .ThenInclude(x => x.Author)
            .Include(x => x.Book.Genres)
            .Include(x => x.Reader)
            .FirstOrDefaultAsync(x => x.Id == entity.Entity.Id);

        return Results.Ok(mapper.Map<BorrowingDto>(fetchedBorrowing));
    }

    public static async Task<IResult> DeleteBorrowingAsync(LibraryDbContext context, IMapper mapper, Guid id)
    {
        var rows = await context.Borrowings.Where(x => x.Id == id).ExecuteDeleteAsync();
        return rows == 0 ? Results.NotFound() : Results.NoContent();
    }
}