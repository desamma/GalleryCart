using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class FavouritePostRepository : IFavouritePostRepository
    {
        private readonly ApplicationDbContext _db;

        public FavouritePostRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<FavouritePost> GetAllQueryable(Expression<Func<FavouritePost, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<FavouritePost> query = _db.FavouritePosts;
            if (asNoTracking)
            {
                query = query.AsNoTracking(); // Use AsNoTracking for read-only queries
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return query;
        }

        public async Task<FavouritePost?> GetAsync(Expression<Func<FavouritePost, bool>> predicate)
        {
            return await _db.FavouritePosts
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching favouritePost or null if none found
        }

        public async Task<bool> AddAsync(FavouritePost entity)
        {
            _db.FavouritePosts.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(FavouritePost entity)
        {
            _db.FavouritePosts.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid favouritePostId)
        {
            var favouritePost = await _db.FavouritePosts.FindAsync(favouritePostId);
            if (favouritePost == null)
            {
                return false; // FavouritePost not found
            }

            _db.FavouritePosts.Remove(favouritePost);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<FavouritePost, bool>> predicate)
        {
            return await _db.FavouritePosts.AnyAsync(predicate);
        }
    }
}
