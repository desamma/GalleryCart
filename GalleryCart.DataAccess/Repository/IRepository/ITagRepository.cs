using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface ITagRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<Tag> GetAllQueryable(Expression<Func<Tag, bool>>? predicate = null, bool asNoTracking = true);
        Task<Tag?> GetAsync(Expression<Func<Tag, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(Tag entity);
        Task<bool> UpdateAsync(Tag entity);
        Task<bool> DeleteAsync(Guid tagId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<Tag, bool>> predicate);
    }
}
