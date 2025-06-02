using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface IFavouritePostRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<FavouritePost> GetAllQueryable(Expression<Func<FavouritePost, bool>>? predicate = null, bool asNoTracking = true);
        Task<FavouritePost?> GetAsync(Expression<Func<FavouritePost, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(FavouritePost entity);
        Task<bool> UpdateAsync(FavouritePost entity);
        Task<bool> DeleteAsync(Guid favouritePostId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<FavouritePost, bool>> predicate);
    }
}
