using AI_JMS.Models;

using Microsoft.EntityFrameworkCore;
namespace AI_JMS.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

        public DbSet<tblUsers> Users { get; set; }
        public DbSet<tblRoles> Roles { get; set; }
        public DbSet<tblUserRoles> UserRoles { get; set; }
        public DbSet<tblReviewerProfiles> ReviewerProfiles { get; set; }    
        public DbSet<tblUsersExpertise> UsersExpertise { get; set; }
        public DbSet<tblSystemconfigs> Systemconfigs { get; set; }
        public DbSet<tblAnnouncements> Announcements { get; set; }
        public DbSet<tblMenus> Menus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cấu hình khóa chính phức hợp cho bảng trung gian UserRoles
        modelBuilder.Entity<tblUserRoles>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        base.OnModelCreating(modelBuilder);
    }
}