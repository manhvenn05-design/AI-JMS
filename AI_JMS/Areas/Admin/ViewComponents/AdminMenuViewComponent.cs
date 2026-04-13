using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_JMS.Data;

namespace AI_JMS.Areas.Admin.ViewComponents;

public class AdminMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public AdminMenuViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 1. Giả lập UserId = 1 đang đăng nhập
        int currentUserId = 1; 

        // 2. Lấy thông tin User để biết họ đang chọn vai trò nào (LastSelectedRoleId)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        // 3. Lấy RoleId hiện tại từ DB (nếu null thì mặc định là 1 - Author chẳng hạn)
        int currentRoleId = user?.LastSelectedRoleId ?? 1;

        // 4. Lọc Menu dựa trên vai trò đang hoạt động
        var menus = await _context.Menus
            .Where(m => m.RequiredRoleId == currentRoleId || m.RequiredRoleId == null)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        return View(menus);
    }
}