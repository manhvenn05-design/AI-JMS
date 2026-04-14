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

    // CHỈ GIỮ LẠI MỘT HÀM INDEX NÀY (Đã tích hợp phân trang)
    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 10; 
        var totalUsers = await _context.Users.CountAsync();
        
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Roles = await _context.Roles.ToListAsync();
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
        ViewBag.CurrentPage = page;

        return View(users);
    }

    // Hàm cấp quyền
    [HttpPost]
    public async Task<IActionResult> AssignRole(int userId, int roleId)
    {
        var exists = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (!exists)
        {
            var userRole = new tblUserRoles { UserId = userId, RoleId = roleId };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // Hàm đổi trạng thái kích hoạt (Khóa/Mở khóa)
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsActive = !user.IsActive; 
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}