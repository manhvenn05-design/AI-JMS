using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_JMS.Data;

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
    int currentUserId = 1; // Giả lập User số 1 đang đăng nhập

    // Lấy thông tin User kèm theo các Vai trò (Roles) của họ
    var user = await _context.Users
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.UserId == currentUserId);

    // Tìm tên vai trò hiện tại để hiển thị ở dòng dưới tên User
    var currentRole = user?.UserRoles
        .FirstOrDefault(ur => ur.RoleId == user.LastSelectedRoleId)?.Role?.RoleName 
        ?? "Chưa chọn vai trò";

    ViewBag.CurrentRoleName = currentRole;

    // QUAN TRỌNG: Trả về đối tượng 'user' (kiểu tblUsers)
    return View(user); 
}
}