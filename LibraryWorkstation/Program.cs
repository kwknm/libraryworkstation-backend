using LibraryWorkstation.Data;
using LibraryWorkstation.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(x =>
{
    x.AddPolicy("ApiCorsPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Database Configuration
builder.Services.AddDbContext<LibraryDbContext>(x =>
    x.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ApiCorsPolicy");

app.UseHttpsRedirection();

app.MapGet("/", () => "API проекта по практике \"Система управления библиотекой\"");

app.MapAuthorsEndpoint();
app.MapBooksEndpoint();
app.MapReadersEndpoint();
app.MapBorrowingsEndpoint();
app.MapGenresEndpoint();

app.Run();