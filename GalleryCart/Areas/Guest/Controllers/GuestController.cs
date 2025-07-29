using AutoMapper;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalleryCart.Areas.Guest.Controllers;

[Area("Guest")]
public class GuestController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPostRepository _postRepository;

    public GuestController(IPostRepository postRepository, IUserRepository userRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IActionResult> ArtistProfile(Guid? userId, string tab = "portfolio", int pageNumber = 1, int pageSize = 21)
    {
        if (userId == null) return NotFound();
        var targetUserId = userId;
        const bool isOwnProfile = false;

        var user = await _userRepository.GetAsync(u => u.Id == targetUserId);
        if (user == null) return NotFound();

        // Validate tab parameter - non-owners can only view portfolio
        if (tab != "portfolio")
        {
            return RedirectToAction(nameof(ArtistProfile), new { userId = targetUserId, tab = "portfolio", pageNumber = 1, pageSize });
        }

        var postsQuery = _postRepository.GetAllQueryable(
            p => p.UserId == targetUserId && p.IsPorfolio == true);

        var totalPostsCount = await postsQuery.CountAsync();

        var posts = await postsQuery
            .OrderByDescending(p => p.PostDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var postVMs = _mapper.Map<List<PostVM>>(posts);

        var viewModel = _mapper.Map<UserProfileVM>(user);
        viewModel.IsOwnProfile = isOwnProfile;
        viewModel.Posts = postVMs;
        viewModel.TotalPostsCount = totalPostsCount;
        viewModel.CurrentPage = pageNumber;
        viewModel.PageSize = pageSize;

        ViewBag.ActiveTab = tab;

        return View(viewModel);
    }
    public IActionResult AllArtist()
    {
        try
        {
            var artists = _userRepository.GetAllQueryable()
                .Where(u => u.IsArtits)
                .OrderByDescending(u => u.NormalizedUserName);

            var model = new AllArtistModel
            {
                Artist = artists
            };

            return View(model);
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred while fetching artists: {ex.Message}");
        }
    }
}