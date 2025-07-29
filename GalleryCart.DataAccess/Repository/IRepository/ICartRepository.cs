using GalleryCart.Models.Models;
using System.Linq.Expressions;

public interface ICartRepository
{
    Task<Cart?> GetAsync(Expression<Func<Cart, bool>> predicate);
    Task<bool> AddAsync(Cart entity);
    Task<bool> UpdateAsync(Cart entity);
    Task<bool> DeleteAsync(Guid cartId);
    Task<bool> ExistsAsync(Expression<Func<Cart, bool>> predicate);
}