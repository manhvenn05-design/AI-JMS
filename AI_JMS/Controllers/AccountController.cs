using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AI_JMS.Models;
using AI_JMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AI_JMS.Controllers;

public class AccountController : Controller
{
     private readonly ApplicationDbContext _context;

    // Đây là "Dependency Injection": Lấy trạm trung chuyển DB đã đăng ký ở Program.cs
    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
public async Task<IActionResult> Login(string email, string password)
{
    // 1. Tìm User trong DB
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    if (user != null)
    {
        // 2. Kiểm tra mật khẩu (Sử dụng BCrypt để so khớp hash)
        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (isValid)
        {
            // 3. Tạo danh sách "lời khẳng định" (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("LastRoleId", user.LastSelectedRoleId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            // 4. "Đóng dấu" Cookie lên trình duyệt
            await HttpContext.SignInAsync("CookieAuth", principal);

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
    }

    ViewBag.Error = "Email hoặc mật khẩu không đúng!";
    return View();
}
}
