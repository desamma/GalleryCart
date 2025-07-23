using AutoMapper;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;

namespace GalleryCart.Utilities.Utils;

public class MyMapper : Profile
{
    public MyMapper()
    {
        CreateMap<User, ProfileInputModel>().ReverseMap();
    }
}