using AutoMapper;
using LibraryWorkstation.Data.Dtos;
using LibraryWorkstation.Data.Entities;

namespace LibraryWorkstation.Profiles;

public class DefaultProfile : Profile
{
    public DefaultProfile()
    {
        CreateMap<Author, AuthorDto>();
        CreateMap<Book, BookDto>()
            .ForMember(x => x.Genres,
                opt => opt.MapFrom(src => src.Genres.Select(x => x.Name)));
        CreateMap<Genre, GenreDto>();
        CreateMap<Reader, ReaderDto>();
        CreateMap<Borrowing, BorrowingDto>();
    }
}