using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface IHistoryRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<History> GetAllQueryable(Expression<Func<History, bool>>? predicate = null, bool asNoTracking = true);
        Task<History?> GetAsync(Expression<Func<History, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(History entity);
        Task<bool> UpdateAsync(History entity);
        Task<bool> DeleteAsync(Guid historyId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<History, bool>> predicate);
    }
}
