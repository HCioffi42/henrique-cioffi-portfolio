using AutoMapper;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Domain;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Renamed from UpdateArtigoCommand to UpdateArticleCommand
        CreateMap<UpdateArticleCommand, Article>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationDate, opt => opt.Ignore());
    }
}