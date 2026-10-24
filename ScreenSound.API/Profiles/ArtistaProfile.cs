using AutoMapper;
using ScreenSound.API.Response;
using ScreenSound.Modelos;

namespace ScreenSound.API.Profiles;

public class ArtistaProfile : Profile
{
    public ArtistaProfile()
    {
        CreateMap<Artista, ArtistaResponse>()
            .ForMember(
                dest => dest.Classificacao, 
                opt => opt.MapFrom(src => 
                    src.Notas.Select(a => a.Nota).DefaultIfEmpty(0).Average()
                )
            );
    }
}