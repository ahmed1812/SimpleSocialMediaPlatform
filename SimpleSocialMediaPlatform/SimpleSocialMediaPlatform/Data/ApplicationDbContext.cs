using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMediaPlatform.Models;

namespace SimpleSocialMediaPlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Post>? Posts { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<UserInfo> userInfos { get; set; }
        public object ApplicationUser { get; internal set; }
        //public DbSet<ApplicationUser> applicationUsers { get; set; }
    }
}
