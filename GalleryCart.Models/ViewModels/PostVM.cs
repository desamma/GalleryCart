using System.ComponentModel.DataAnnotations;
using GalleryCart.Models.Models;

namespace GalleryCart.Models.ViewModels;

public class PostVM
{
    public Guid PostId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; }

    // The path to the image or video file (use cloudinary)
    public string Path { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? PostDate { get; set; }

    public int LikeCount { get; set; } = 0;

    public int DislikeCount { get; set; } = 0;

    public int SaleCount { get; set; } = 0;

    public bool IsPorfolio { get; set; } // true: show in portfolio

    public bool IsMature { get; set; } // true: mature content

    public bool IsImage { get; set; } // true: image, false: video
    
    public IEnumerable<Tag> PostTags { get; set; } = new List<Tag>();
}