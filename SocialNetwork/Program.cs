using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.BLL.Services;
using SocialNetwork.DLL;
using SocialNetwork.DLL.Entities;
using SocialNetwork.DLL.Interfaces;
using SocialNetwork.DLL.Repositories;
using SocialNetwork.DLL.UoW;

namespace SocialNetwork;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string connection = builder.Configuration.GetConnectionString("DefaultConnection");
        
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connection), ServiceLifetime.Scoped);
        builder.Services.AddIdentity<UserEntity, IdentityRole>(opts =>
        { 
            opts.Password.RequiredLength = 5;
            opts.Password.RequireNonAlphanumeric = true;
            opts.Password.RequireLowercase = true;
            opts.Password.RequireUppercase = true;
            opts.Password.RequireDigit = true;
        })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders(); 

        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<FriendService>();
        builder.Services.AddScoped<MessageService>();
        builder.Services.AddScoped<IRepository<FriendEntity>, FriendsRepository>();
        builder.Services.AddScoped<IRepository<MessageEntity>, MessageRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddAutoMapper((v) => v.AddProfile(new MappingProfile()));

        // Add services to the container.
        builder.Services.AddControllersWithViews();

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

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}