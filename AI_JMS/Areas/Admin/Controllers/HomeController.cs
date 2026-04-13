using Microsoft.AspNetCore.Mvc;
using AI_JMS.Data; // Đảm bảo đúng namespace chứa DbContext của bạn
using AI_JMS.Models;

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
        // Giả lập UserId = 1 đang đăng nhập
        int currentUserId = 1; 
        
        var user = await _context.Users.FindAsync(currentUserId);
        
        if (user != null)
        {
            // Cập nhật vai trò người dùng vừa chọn vào cột LastSelectedRoleId
            user.LastSelectedRoleId = roleId; 
            await _context.SaveChangesAsync();
        }

        // Sau khi lưu xong, quay lại trang Index. 
        // Lúc này Sidebar và Topbar sẽ tự động đọc giá trị mới từ DB và thay đổi theo.
        return RedirectToAction("Index");
    }
}