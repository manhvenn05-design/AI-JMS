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

        // 1. Ánh xạ tên bảng (Giữ nguyên của Mạnh)
        modelBuilder.Entity<tblUsers>().ToTable("Users");
        modelBuilder.Entity<tblRoles>().ToTable("Roles");
        modelBuilder.Entity<tblUserRoles>().ToTable("UserRoles");
        modelBuilder.Entity<tblReviewerProfiles>().ToTable("ReviewerProfiles");
        modelBuilder.Entity<tblUsersExpertise>().ToTable("UserExpertise");

        // 2. Khóa chính phức hợp cho UserRoles
        modelBuilder.Entity<tblUserRoles>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // 3. FIX LỖI UserId1: Cấu hình quan hệ 1-1 (Users <-> ReviewerProfiles)
        modelBuilder.Entity<tblUsers>()
            .HasOne(u => u.ReviewerProfile)      // Một User có một Profile
            .WithOne(p => p.User)               // Một Profile thuộc về một User (Chỉ rõ p.User ở đây)
            .HasForeignKey<tblReviewerProfiles>(p => p.UserId) // Dùng chung cột UserId làm khóa ngoại
            .OnDelete(DeleteBehavior.Cascade);

        // 4. Cấu hình quan hệ 1-N (Users <-> UserExpertise)
        modelBuilder.Entity<tblUsersExpertise>()
            .HasOne(p => p.User)                // Một Expertise thuộc về một User
            .WithMany(u => u.UserExpertises)    // Một User có nhiều Expertise
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}