using GalleryCart.DataAccess;
using GalleryCart.DataAccess.Repository;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Utilities.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}, ServiceLifetime.Transient);

// Identity and roles
builder.Services
    .AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ àáảãạâầấẩẫậăằắẳẵặèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ()";
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//Configure .env config binding
builder.Configuration["EmailSettings:FromEmail"] = Environment.GetEnvironmentVariable("EMAILSETTINGS__FROMEMAIL");
builder.Configuration["EmailSettings:FromPassword"] = Environment.GetEnvironmentVariable("EMAILSETTINGS__FROMPASSWORD");
Console.WriteLine("EMAIL: " + builder.Configuration["EmailSettings:FromEmail"]);
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));


// Configure Repositories
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommissionRepository, CommissionRepository>();
builder.Services.AddScoped<IFavouritePostRepository, FavouritePostRepository>();
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddTransient<IEmailSender, EmailSender>();


// Configure default routes (This should be after configured the Identity)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Enable razor page
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Set session timeout
});

var app = builder.Build();

// Seed data for roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Initialize the database and seed data
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        // Log the error (uncomment ex variable name and write a log)
        Console.WriteLine($"An error occurred while seeding role in the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.MapRazorPages();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Guest}/{controller=Home}/{action=Index}/{id?}");

app.Run();
