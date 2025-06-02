using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface ICommentRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<Comment> GetAllQueryable(Expression<Func<Comment, bool>>? predicate = null, bool asNoTracking = false);
        Task<Comment?> GetAsync(Expression<Func<Comment, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(Comment entity);
        Task<bool> UpdateAsync(Comment entity);
        Task<bool> DeleteAsync(Guid CommentId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<Comment, bool>> predicate);
    }
}
