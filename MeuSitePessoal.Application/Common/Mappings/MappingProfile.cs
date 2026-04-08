using AutoMapper;
using MeuSitePessoal.Application.Articles.Commands.CreateArticle;
using MeuSitePessoal.Application.Articles.Commands.UpdateArticle;
using MeuSitePessoal.Application.Articles.Queries;
using MeuSitePessoal.Application.Articles.Queries.GetArticles;
using MeuSitePessoal.Domain;
using MeuSitePessoal.Domain.Entities;

namespace MeuSitePessoal.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateArticleCommand, Article>();

        // Specifically ignores Id and CreatedAt to prevent overwriting metadata during updates.
        CreateMap<UpdateArticleCommand, Article>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        CreateMap<Article, ArticleResponse>();
        CreateMap<Article, ArticleSummaryDto>();
    }
}