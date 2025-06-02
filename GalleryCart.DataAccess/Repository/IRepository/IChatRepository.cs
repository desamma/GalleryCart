using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface IChatRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<Chat> GetAllQueryable(Expression<Func<Chat, bool>>? predicate = null, bool asNoTracking = true);
        Task<Chat?> GetAsync(Expression<Func<Chat, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(Chat entity);
        Task<bool> UpdateAsync(Chat entity);
        Task<bool> DeleteAsync(Guid chatId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<Chat, bool>> predicate);

    }
}
