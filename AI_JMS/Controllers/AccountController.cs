using Microsoft.AspNetCore.Mvc;
using AI_JMS.Models;
using AI_JMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization; // Thêm thư viện này

namespace AI_JMS.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer; // Khai báo Localizer ở đây

    // Inject cả DbContext và Localizer vào Constructor
    public AccountController(ApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken] // Thêm để bảo mật
    public async Task<IActionResult> Login(string email, string password)
    {
        // Kiểm tra xem tên bảng là Users hay tblUsers (giả định là tblUsers theo model của bạn)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user != null)
        {
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("LastRoleId", user.LastSelectedRoleId.ToString()  ?? "1")
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal);

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        ViewBag.Error = _localizer["InvalidLogin"]; // Dùng _localizer đã inject
        return View();
    }

    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return LocalRedirect(returnUrl);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(tblUsers model, string confirmPassword)
    {
        if (ModelState.IsValid)
        {
            // 1. Kiểm tra mật khẩu xác nhận
            // Lưu ý: PasswordHash ở đây đang chứa mật khẩu chưa băm từ form gửi lên
            if (model.PasswordHash != confirmPassword)
            {
                ViewBag.Error = _localizer["PasswordMismatch"];
                return View(model);
            }

            // 2. Kiểm tra Email (Dùng đúng tên bảng tblUsers)
            var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (existingUser)
            {
                ViewBag.Error = _localizer["EmailExists"];
                return View(model);
            }

            // 3. Băm mật khẩu
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
            model.LastSelectedRoleId = 1;

            _context.Users.Add(model);
            await _context.SaveChangesAsync();



            // 4. Gán vai trò (Dùng đúng tên bảng tblUserRoles)
            var userRole = new tblUserRoles
            {
                UserId = model.UserId,
                RoleId = 1
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();



            TempData["RegisterSuccess"] = "Chào mừng " + model.FullName + "! Tài khoản của bạn đã được tạo.";



            return RedirectToAction("Login");
        }
        return View(model);
    }
    //logout
    [HttpPost] // Dùng Post để bảo mật hơn
    public async Task<IActionResult> Logout()
    {
        // Xóa sạch Cookie xác thực
        await HttpContext.SignOutAsync("CookieAuth");

        // Quay về trang chủ hoặc trang Login
        return RedirectToAction("Login");
    }

    //profile
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        // Lấy ID từ Cookie người dùng đang đăng nhập
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

        int userId = int.Parse(userIdStr);
        var user = await _context.Users.FindAsync(userId);

        if (user == null) return NotFound();

        return View(user); // Trả về View Profile.cshtml mà bạn đã tạo
    }
    
    [HttpPost]
    public async Task<IActionResult> UpdateProfile(tblUsers updatedUser)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user = await _context.Users.FindAsync(userId);

        if (user != null)
        {
            user.FullName = updatedUser.FullName;
            // Bạn có thể cập nhật thêm Phone, Bio... tùy ý
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật hồ sơ thành công!";
        }
        return RedirectToAction("Profile");
    }

    
}