using AutoMapper;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Application.Articles.Queries;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateArticleCommand, Article>();

        // Specifically ignores Id and CreatedAt to prevent overwriting metadata during updates.
        // Also ignores legacy fields as they are handled manually in the Command Handler for English sync.
        CreateMap<UpdateArticleCommand, Article>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.Ignore())
            .ForMember(dest => dest.Content, opt => opt.Ignore())
            .ForMember(dest => dest.Summary, opt => opt.Ignore());

        CreateMap<Article, ArticleResponse>();
        CreateMap<Article, ArticleSummaryDto>();
    }
}