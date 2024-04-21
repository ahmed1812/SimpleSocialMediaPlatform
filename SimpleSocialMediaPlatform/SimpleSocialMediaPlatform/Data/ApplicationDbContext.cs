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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring the relationship between ApplicationUser and Comments
            modelBuilder.Entity<Comments>()
                .HasOne<ApplicationUser>(s => s.User)   // Define the navigation property
                .WithMany(g => g.Comments)              // Define the collection property in ApplicationUser
                .HasForeignKey("UserId");               // Specify the foreign key

 

            // You can put additional model configuration here
        }
        public DbSet<Post>? Posts { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<UserInfo> userInfos { get; set; }
        //public object ApplicationUser { get; internal set; }
        //public DbSet<ApplicationUser> applicationUsers { get; set; }
    }
}
