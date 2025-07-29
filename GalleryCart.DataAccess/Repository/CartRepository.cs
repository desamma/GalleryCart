using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;

        public CartRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<Cart?> GetAsync(Expression<Func<Cart, bool>> predicate)
        {
            return await _db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Post)
                        .ThenInclude(p => p.User) // 🔥 THÊM DÒNG NÀY
                .FirstOrDefaultAsync(predicate);
        }





        public async Task<bool> AddAsync(Cart entity)
        {
            _db.Carts.Add(entity);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Cart entity)
{
    return await _db.SaveChangesAsync() > 0; // không gọi Update(), EF đang theo dõi cart và CartItems
}



        public async Task<bool> DeleteAsync(Guid cartId)
        {
            var cart = await _db.Carts.FindAsync(cartId);
            if (cart == null) return false;
            _db.Carts.Remove(cart);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsAsync(Expression<Func<Cart, bool>> predicate)
        {
            return await _db.Carts.AnyAsync(predicate);
        }
    }
}