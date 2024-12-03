using AutoMapper;
using LibraryWorkstation.Data;
using LibraryWorkstation.Data.Dtos;
using LibraryWorkstation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWorkstation.Endpoints;

public record CreateReaderRequest(string FirstName, string LastName, string? Patronymic, string Phone);

public static class ReadersEndpoint
{
    public static void MapReadersEndpoint(this WebApplication app)
    {
        var endpoint = app.MapGroup("/api/readers");
        endpoint.MapGet("/", GetAllReadersAsync);
        endpoint.MapGet("/{id:guid}", GetReaderByIdAsync);
        endpoint.MapPost("/", AddReaderAsync);
        endpoint.MapGet("/{id:guid}/borrowings", GetReaderBorrowingsAsync);
    }

    public static async Task<IResult> GetAllReadersAsync(LibraryDbContext context, IMapper mapper,
        [FromQuery] string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Results.Ok(mapper.Map<List<ReaderDto>>(await context.Readers.ToListAsync()));
        
        if (Guid.TryParse(search, out var id))
        {
            return Results.Ok(mapper.Map<List<ReaderDto>>(context.Readers.Where(x => x.Id == id)));
        }
        var readers = context.Readers.Where(x =>
            x.FirstName.Contains(search) || x.LastName.Contains(search));
        return Results.Ok(mapper.Map<List<ReaderDto>>(await readers.ToListAsync()));
    }

    public static async Task<IResult> GetReaderByIdAsync(LibraryDbContext context, IMapper mapper, Guid id)
    {
        return Results.Ok(mapper.Map<ReaderDto>(await context.Readers.FindAsync(id)));
    }

    public static async Task<IResult> AddReaderAsync(LibraryDbContext context, IMapper mapper,
        CreateReaderRequest request)
    {
        if (await context.Readers.FirstOrDefaultAsync(x => x.Phone == request.Phone) is not null)
        {
            return Results.Conflict(new { Message = "Читатель с таким телефоном уже существует" });
        }

        var newReader = new Reader
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Patronymic = request.Patronymic,
            Phone = request.Phone
        };

        var entity = await context.Readers.AddAsync(newReader);
        await context.SaveChangesAsync();

        return Results.Created($"/api/readers/{entity.Entity.Id}", mapper.Map<ReaderDto>(entity.Entity));
    }

    public static async Task<IResult> GetReaderBorrowingsAsync(LibraryDbContext context, IMapper mapper, Guid id)
    {
        var reader = await context.Readers
            .Include(x => x.Borrowings)
            .ThenInclude(x => x.Book)
            .ThenInclude(x => x.Genres)
            .Include(x => x.Borrowings)
            .ThenInclude(x => x.Book.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return reader is null
            ? Results.NotFound(new { Message = "Читатель не найден" })
            : Results.Ok(mapper.Map<List<BorrowingDto>>(reader.Borrowings));
    }
}