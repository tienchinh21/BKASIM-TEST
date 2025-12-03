using AutoMapper;
using MiniAppGIBA.Entities.Commons;
using MiniAppGIBA.Entities.Fields;
using MiniAppGIBA.Entities.Memberships;
using MiniAppGIBA.Entities.Groups;
using MiniAppGIBA.Entities.Events;
using MiniAppGIBA.Entities.Sponsors;
using MiniAppGIBA.Entities.Articles;
using MiniAppGIBA.Models.DTOs.Fields;
using MiniAppGIBA.Models.DTOs.Memberships;
using MiniAppGIBA.Models.Request.Fields;
using MiniAppGIBA.Models.Request.Memberships;
using MiniAppGIBA.Models.Request.Articles;
using MiniAppGIBA.Models.Response.Articles;

namespace MiniAppGIBA.Base.Common
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            #region Membership
            CreateMap<Membership, Membership>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());

            CreateMap<Membership, MembershipDTO>();
            CreateMap<UpdateProfileRequest, Membership>();
            #endregion

            #region Field
            CreateMap<Field, Field>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());

            CreateMap<CreateFieldRequest, Field>();
            CreateMap<UpdateFieldRequest, Field>();
            CreateMap<Field, FieldDTO>();
            #endregion

            #region Group
            CreateMap<Group, Group>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region Event
            CreateMap<Event, Event>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region EventRegistration
            CreateMap<EventRegistration, EventRegistration>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region EventGuest
            CreateMap<EventGuest, EventGuest>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region GuestList
            CreateMap<GuestList, GuestList>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region EventGift
            CreateMap<EventGift, EventGift>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region Sponsor
            CreateMap<Sponsor, Sponsor>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region SponsorshipTier
            CreateMap<SponsorshipTier, SponsorshipTier>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region EventSponsor
            CreateMap<EventSponsor, EventSponsor>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region Ref
            CreateMap<Ref, Ref>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
            #endregion

            #region Article
            CreateMap<Article, Article>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());

            CreateMap<ArticleRequest, Article>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore())
                .ForMember(x => x.BannerImage, opt => opt.Ignore()) // Will be set manually after file upload
                .ForMember(x => x.Images, opt => opt.Ignore()) // Will be set manually after file upload
                .ForMember(x => x.Author, opt => opt.MapFrom(src => src.Author ?? string.Empty));

            CreateMap<Article, ArticleResponse>()
                .ForMember(dest => dest.Images, opt => opt.Ignore()) // Will be set manually in ConvertArticle
                .ForMember(dest => dest.BannerImage, opt => opt.Ignore()) // Will be set manually in ConvertArticle
                .ForMember(dest => dest.SummarizeContent, opt => opt.Ignore()); // Will be set manually in ConvertArticle
            #endregion
        }
    }
}