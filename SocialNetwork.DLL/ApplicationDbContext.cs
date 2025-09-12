using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.DLL.Configurations;
using SocialNetwork.DLL.Entities;

namespace SocialNetwork.DLL;

public class ApplicationDbContext : IdentityDbContext<UserEntity>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new FriendConfiguration());
        builder.ApplyConfiguration(new MessageConfuiguration());

        // Конфигурация для UserEntity
        builder.Entity<UserEntity>()
            .Property(u => u.BirthDate)
            .HasColumnType("timestamp without time zone");
    }
}