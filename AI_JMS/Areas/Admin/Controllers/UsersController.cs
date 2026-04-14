using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_JMS.Data;
using AI_JMS.Models;

namespace AI_JMS.Areas.Admin.Controllers;

[Area("Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Trang danh sách người dùng
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.ToListAsync();
        ViewBag.Roles = await _context.Roles.ToListAsync(); // Lấy danh sách Role để hiện vào Dropdown
        return View(users);
    }

    // Hàm cấp quyền (Logic quan trọng nhất)
    [HttpPost]
    public async Task<IActionResult> AssignRole(int userId, int roleId)
    {
        // 1. Kiểm tra xem User này đã có Role này chưa
        var exists = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (!exists)
        {
            // 2. Nếu chưa có thì thêm mới vào bảng trung gian
            var userRole = new tblUserRoles { UserId = userId, RoleId = roleId };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}