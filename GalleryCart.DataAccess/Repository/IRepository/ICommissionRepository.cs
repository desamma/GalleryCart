using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface ICommissionRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<Commission> GetAllQueryable(Expression<Func<Commission, bool>>? predicate = null, bool asNoTracking = true);
        Task<Commission?> GetAsync(Expression<Func<Commission, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(Commission entity);
        Task<bool> UpdateAsync(Commission entity);
        Task<bool> DeleteAsync(Guid commissionId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<Commission, bool>> predicate);
    }
}
