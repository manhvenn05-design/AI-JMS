using AI_JMS.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_JMS.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // --- 1. Danh sách các bảng (DbSets) ---
    public DbSet<tblUsers> Users { get; set; }
    public DbSet<tblRoles> Roles { get; set; }
    public DbSet<tblUserRoles> UserRoles { get; set; }
    public DbSet<tblReviewerProfiles> ReviewerProfiles { get; set; }
    public DbSet<tblUsersExpertise> UsersExpertises { get; set; }
    public DbSet<tblSystemconfigs> Systemconfigs { get; set; }
    public DbSet<tblAnnouncements> Announcements { get; set; }
    public DbSet<tblMenus> Menus { get; set; }

    // --- 2. Cấu hình Mapping & Quan hệ (Fluent API) ---
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // A. Ánh xạ tên bảng (Mapping Table Names)
        // Lưu ý: Tên trong ToTable phải khớp 100% với tên bảng trong SQL Server Management Studio
        modelBuilder.Entity<tblUsers>().ToTable("Users");
        modelBuilder.Entity<tblRoles>().ToTable("Roles");
        modelBuilder.Entity<tblUserRoles>().ToTable("UserRoles");
        modelBuilder.Entity<tblReviewerProfiles>().ToTable("ReviewerProfiles");
        modelBuilder.Entity<tblUsersExpertise>().ToTable("UserExpertise"); // Khớp với sơ đồ image_547487

        // B. Cấu hình khóa chính phức hợp (Composite Key)
        modelBuilder.Entity<tblUserRoles>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // C. Cấu hình quan hệ 1-1 (Users <-> ReviewerProfiles)
        modelBuilder.Entity<tblUsers>()
            .HasOne(u => u.ReviewerProfile)
            .WithOne()
            .HasForeignKey<tblReviewerProfiles>(p => p.UserId);

        // D. Cấu hình quan hệ 1-N (Users <-> UserExpertise)
        modelBuilder.Entity<tblUsersExpertise>()
            .HasOne<tblUsers>()
            .WithMany(u => u.UserExpertises)
            .HasForeignKey(p => p.UserId);
    }
}