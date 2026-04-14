using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AI_JMS.Data;
using System.Security.Claims;
using AI_JMS.Models;

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
        // 1. Lấy ID người dùng từ Cookie
        var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        // Lưu ý: Đổi tblMenus thành Menus cho khớp với SQL
        if (userIdClaim == null) return View(new List<tblMenus>()); 

        int userId = int.Parse(userIdClaim.Value);

        // 2. Lấy User để biết vai trò hiện tại
        // Kiểm tra lại trong DbContext là _context.Users hay _context.tblUsers nhé
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return View(new List<tblMenus>());

        // Xử lý Nullable: Nếu LastSelectedRoleId null thì mặc định là 1 (Tác giả)
        int currentRoleId = user.LastSelectedRoleId ?? 1;

        // 3. Lấy danh sách Menu dựa trên cấu trúc SQL của bạn
        var menus = await _context.Menus
            .Where(m => m.RequiredRoleId == currentRoleId) // Dùng cột RequiredRoleId có sẵn trong bảng Menus
            .OrderBy(m => m.DisplayOrder) // Dùng DisplayOrder thay vì Order cho khớp SQL
            .ToListAsync();

        return View(menus);
    }
}