using AutoMapper;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;

namespace GalleryCart.Utilities.Utils;

public class MyMapper : Profile
{
    public MyMapper()
    {
        CreateMap<User, ProfileInputModel>().ReverseMap();
        // Post to PostVM mapping
        CreateMap<Post, PostVM>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.PostId))
            .ForMember(dest => dest.PostTags, opt => opt.MapFrom(src => src.Tags));
                
        // User to UserProfileVM mapping (partial)
        CreateMap<User, UserProfileVM>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FavoritePosts, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPostsCount, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
            .ForMember(dest => dest.PageSize, opt => opt.Ignore())
            .ForMember(dest => dest.IsOwnProfile, opt => opt.Ignore())
            .ForMember(dest => dest.HasMorePosts, opt => opt.Ignore());
    }
}