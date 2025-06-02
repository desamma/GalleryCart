using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface IPostRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<Post> GetAllQueryable(Expression<Func<Post, bool>>? predicate = null, bool asNoTracking = true);
        Task<Post?> GetAsync(Expression<Func<Post, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(Post entity);
        Task<bool> UpdateAsync(Post entity);
        Task<bool> DeleteAsync(Guid postId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<Post, bool>> predicate);
    }
}
