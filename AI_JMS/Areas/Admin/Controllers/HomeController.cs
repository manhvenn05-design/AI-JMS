using Microsoft.AspNetCore.Mvc;
using AI_JMS.Data; // Đảm bảo đúng namespace chứa DbContext của bạn
using AI_JMS.Models;
using System.Security.Claims;

namespace AI_JMS.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    // Đây là "Dependency Injection": Lấy trạm trung chuyển DB đã đăng ký ở Program.cs
    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SwitchRole(int roleId)
    {
       // 1. Lấy ID của người dùng đang đăng nhập từ Cookie
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account", new { area = "" });

        int userId = int.Parse(userIdStr);

        // 2. Cập nhật lại vai trò đang chọn vào Database
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastSelectedRoleId = roleId;
            await _context.SaveChangesAsync();
        }

        // Sau khi lưu xong, quay lại trang Index. 
        // Lúc này Sidebar và Topbar sẽ tự động đọc giá trị mới từ DB và thay đổi theo.
        return RedirectToAction("Index");
    }
}