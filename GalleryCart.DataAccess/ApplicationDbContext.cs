using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GalleryCart.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Chat> Chats { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<Commission> Commissions { get; set; } = null!;
        public virtual DbSet<FavouritePost> FavouritePosts { get; set; } = null!;
        public virtual DbSet<History> Histories { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<Tag> Tags { get; set; } = null!;
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }
        // No need for this
        //public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Enable sensitive data logging
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique constraints
            builder.Entity<User>().HasIndex(u => u.UserName).IsUnique(); 
            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            builder.Entity<Tag>().HasIndex(t => t.TagName).IsUnique();

            // Primary keys
            builder.Entity<Chat>().HasKey(c => c.ChatId);
            builder.Entity<Comment>().HasKey(c => c.CommentId);
            builder.Entity<Commission>().HasKey(c => c.CommissionId);
            builder.Entity<FavouritePost>().HasKey(fp => new { fp.UserId, fp.PostId });
            builder.Entity<History>().HasKey(h => h.HistoryId);
            builder.Entity<Post>().HasKey(p => p.PostId);
            builder.Entity<Tag>().HasKey(t => t.TagId);
            builder.Entity<User>().HasKey(u => u.Id);
            builder.Entity<Cart>().HasKey(c => c.CartId);
            builder.Entity<CartItem>().HasKey(ci => ci.CartItemId);

            // Relationships

            // Chat to User (Sender)
            // Use ClientSetNull to allow deletion of User without deleting Chats and no conflict in dumb SQL
            builder.Entity<Chat>()
                .HasOne(c => c.Sender)
                .WithMany(u => u.MessageSent)
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Chat to User (Receiver)
            // Use ClientSetNull to allow deletion of User without deleting Chats and no conflict in dumb SQL
            builder.Entity<Chat>()
                .HasOne(c => c.Receiver)
                .WithMany(u => u.MessageReceived)
                .HasForeignKey(c => c.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Comment to User
            // Use ClientSetNull to allow deletion of User without deleting Comments and no conflict in dumb SQL
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Comment to Post
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Commission to User (Commissioner)
            // User cannot delete their profile if they have commissions
            builder.Entity<Commission>()
                .HasOne(c => c.Commissioner)
                .WithMany(u => u.CommissionsRequested)
                .HasForeignKey(c => c.CommissionerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Commission to User (Artist)
            // User cannot delete their profile if they have commissions
            builder.Entity<Commission>()
                .HasOne(c => c.Artist)
                .WithMany(u => u.CommissionsReceived)
                .HasForeignKey(c => c.ArtistId)
                .OnDelete(DeleteBehavior.Restrict);

            // FavouritePost to User
            builder.Entity<FavouritePost>()
                .HasOne(fp => fp.User)
                .WithMany(u => u.FavouritePosts)
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // FavouritePost to Post (no need but keeping for future if needed)
            builder.Entity<FavouritePost>()
                .HasOne(fp => fp.Post)
                .WithMany(p => p.FavouritePosts)
                .HasForeignKey(fp => fp.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            // History to User
            builder.Entity<History>()
                .HasOne(h => h.User)
                .WithMany(u => u.Histories)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // History to Post (many to many)
            builder.Entity<History>()
                .HasOne(h => h.Post)
                .WithMany(p => p.Histories)
                .HasForeignKey(h => h.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post to User
            // SetNull to avoid constrain error
            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Post to Tag (many to many)
            builder.Entity<Post>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Posts)
                .UsingEntity(j => j.ToTable("PostTags"));

            // Cart to User (one-to-many)
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart to CartItems (one-to-many)
            builder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem to Commission (many-to-one)
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Commission)
                .WithMany()
                .HasForeignKey(ci => ci.CommissionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add precision for decimal fields
            builder.Entity<Post>()
                .Property(p => p.Price)
                .HasPrecision(22, 2);

            builder.Entity<Commission>()
                .Property(c => c.Price)
                .HasPrecision(22, 2);

            builder.Entity<History>()
                .Property(h => h.TotalPrice)
                .HasPrecision(22, 2);
        }
    }
}
