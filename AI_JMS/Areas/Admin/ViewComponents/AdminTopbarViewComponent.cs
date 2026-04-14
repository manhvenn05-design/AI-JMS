using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_JMS.Data;
using System.Security.Claims;
using AI_JMS.Models;

namespace AI_JMS.Areas.Admin.ViewComponents;

public class AdminTopbarViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public AdminTopbarViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

public async Task<IViewComponentResult> InvokeAsync()
{
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null) return View();

    int userId = int.Parse(userIdClaim.Value);

    // Dùng Include để nạp bảng trung gian và ThenInclude để nạp tên Role
    var user = await _context.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role) 
        .FirstOrDefaultAsync(u => u.UserId == userId);

    if (user != null)
    {
        // Lấy tên vai trò hiện tại để hiển thị caption
        var currentRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == user.LastSelectedRoleId);
        ViewBag.CurrentRoleName = currentRole?.Role.RoleName;
    }

    return View(user);
}
}