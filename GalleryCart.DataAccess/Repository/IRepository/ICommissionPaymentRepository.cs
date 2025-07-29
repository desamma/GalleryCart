using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface ICommissionPaymentRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<CommissionPayment> GetAllQueryable(Expression<Func<CommissionPayment, bool>>? predicate = null, bool asNoTracking = true);
        Task<CommissionPayment?> GetAsync(Expression<Func<CommissionPayment, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(CommissionPayment entity);
        Task<bool> UpdateAsync(CommissionPayment entity);
        Task<bool> DeleteAsync(Guid commissionPaymentId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<CommissionPayment, bool>> predicate);
    }
}
