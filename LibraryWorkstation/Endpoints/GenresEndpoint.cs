using AutoMapper;
using LibraryWorkstation.Data;
using LibraryWorkstation.Data.Dtos;
using LibraryWorkstation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LibraryWorkstation.Endpoints;

public record CreateGenreRequest(string Name);

public static class GenresEndpoint
{
    public static void MapGenresEndpoint(this WebApplication app)
    {
        var endpoint = app.MapGroup("/api/genres");
        endpoint.MapGet("/", GetAllGenresAsync);
        endpoint.MapGet("/{id:guid}", GetGenreByIdAsync);
        endpoint.MapPost("/", AddGenreAsync);
        endpoint.MapDelete("/{id:guid}", DeleteGenreByIdAsync);
    }

    public static async Task<IResult> GetAllGenresAsync(LibraryDbContext context, IMapper mapper, [FromQuery] string? search)
    {
        var genres = context.Genres.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            genres = genres.Where(x => x.Name.Contains(search!));
        
        return Results.Ok(mapper.Map<List<GenreDto>>(await genres.ToListAsync()));
    }

    public static async Task<IResult> GetGenreByIdAsync(LibraryDbContext context, IMapper mapper, Guid id)
    {
        return Results.Ok(mapper.Map<GenreDto>(await context.Genres.FirstOrDefaultAsync(x => x.Id == id)));
    }
    
    public static async Task<IResult> DeleteGenreByIdAsync(LibraryDbContext context, IMapper mapper, Guid id)
    {
        var rows = await context.Genres.Where(x => x.Id == id).ExecuteDeleteAsync();
        return rows == 0 ? Results.NotFound() : Results.NoContent();
    }
    
    public static async Task<IResult> AddGenreAsync(LibraryDbContext context, IMapper mapper, CreateGenreRequest request)
    {
        if (await context.Genres.FirstOrDefaultAsync(x => 
                EF.Functions.Like(x.Name, $"%{request.Name}%")) is not null)
        {
            return Results.Conflict(new { Message = "Такой жанр уже существует" });
        }
        var newGenre = new Genre
        {
            Name = request.Name
        };
        var entity = await context.AddAsync(newGenre);
        await context.SaveChangesAsync();
        return Results.Created($"/api/genres/{entity.Entity.Id}", mapper.Map<GenreDto>(entity.Entity));
    }
}