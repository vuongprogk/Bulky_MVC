using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!)
);
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));


// To using roles in Identity, it nessesary to add custom Identity rather than Default Identity

//builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();
// By default TokenProvider is added by default for AddDefaultIdentity but when add custom Identity its not automatically added.

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// Application Cookie cannot be added before Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddRazorPages(); // add services to add razor page

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();

// authentication action must be call before authorization()
app.UseAuthentication();

app.UseAuthorization();
app.MapRazorPages(); // add to map razor page
app.MapControllerRoute(name: "default", pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(name: "default", pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

// Place this before run to apply update migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
app.Run();
