using AutoMapper;
using LibraryWorkstation.Data;
using LibraryWorkstation.Data.Dtos;
using LibraryWorkstation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryWorkstation.Endpoints;

public record CreateAuthorRequest(string FirstName, string LastName, string? Patronymic);

public static class AuthorsEndpoint
{
    public static void MapAuthorsEndpoint(this WebApplication app)
    {
        var endpoint = app.MapGroup("/api/authors");
        endpoint.MapGet("/", GetAllAuthorsAsync);
        endpoint.MapPost("/", AddAuthorAsync);
        endpoint.MapGet("/{id:guid}", GetAuthorByIdAsync);
        endpoint.MapDelete("/{id:guid}", DeleteAuthorAsync);
        endpoint.MapGet("/{id:guid}/books", GetAuthorBooksAsync);
    }

    private static async Task<IResult> GetAllAuthorsAsync(LibraryDbContext context, IMapper mapper, [FromQuery] string? search)
    {
        
        return search is not null
            ? Results.Ok(mapper.Map<List<AuthorDto>>(await context.Authors.Where(x => 
                x.FirstName.Contains(search) || x.LastName.Contains(search)).ToListAsync()))
            : Results.Ok(mapper.Map<List<AuthorDto>>(await context.Authors.AsNoTracking().ToListAsync()));
    }

    private static async Task<IResult> AddAuthorAsync(LibraryDbContext context, CreateAuthorRequest request, IMapper mapper)
    {
        var newAuthor = new Author
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Patronymic = request.Patronymic
        };
        var entity = await context.Authors.AddAsync(newAuthor);
        await context.SaveChangesAsync();
        return Results.Created($"/api/authors/{entity.Entity.Id}", mapper.Map<AuthorDto>(entity.Entity));
    }

    private static async Task<IResult> GetAuthorByIdAsync(LibraryDbContext context, Guid id, IMapper mapper)
    {
        var author = await context.Authors.FindAsync(id);
        return author is null
            ? Results.NotFound(new { Message = "Автор не найден" })
            : Results.Ok(mapper.Map<AuthorDto>(author));
    }

    private static async Task<IResult> DeleteAuthorAsync(LibraryDbContext context, Guid id)
    {
        var rows = await context.Authors.Where(x => x.Id == id).ExecuteDeleteAsync();
        return rows == 0 ? Results.NotFound() : Results.NoContent();
    }

    private static async Task<IResult> GetAuthorBooksAsync(LibraryDbContext context, IMapper mapper, Guid id)
    {
        var author = await context.Authors
            .Include(x => x.Books)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return author is null
            ? Results.NotFound(new { Message = "Автор не найден" })
            : Results.Ok(mapper.Map<BookDto>(author.Books));
    }
}