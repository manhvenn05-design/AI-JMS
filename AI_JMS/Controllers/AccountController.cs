using Microsoft.AspNetCore.Mvc;
using AI_JMS.Models;
using AI_JMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using AI_JMS.Models.ViewModels;

namespace AI_JMS.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AccountController(ApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user != null)
        {
            // 1. Kiểm tra mật khẩu trước
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // 2. PHẢI kiểm tra IsActive ngay tại đây
                // Sửa từ chuỗi tiếng Việt trực tiếp sang dùng key
                if (!user.IsActive)
                {
                    ViewBag.Error = _localizer["AccountLocked"]; 
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("LastRoleId", user.LastSelectedRoleId.ToString() ?? "1")
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal);
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
        }

        ViewBag.Error = _localizer["InvalidLogin"];
        return View();
    }

    // --- PHẦN PROFILE NÂNG CẤP (Xử lý 3 bảng) ---

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

        int userId = int.Parse(userIdStr);

        // Nạp kèm dữ liệu từ ReviewerProfiles và UserExpertises
        var user = await _context.Users
            .Include(u => u.ReviewerProfile)
            .Include(u => u.UserExpertises)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        // Đổ dữ liệu vào ViewModel để hiển thị ra View
        var vm = new UserProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Gender = user.Gender,
            Avatar = user.Avatar,
            AcademicTitle = user.ReviewerProfile?.AcademicTitle,
            Affiliation = user.ReviewerProfile?.Affiliation,
            ExpertiseKeywords = string.Join(", ", user.UserExpertises.Select(e => e.Keyword))
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(UserProfileViewModel vm, IFormFile? avatarFile)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var user = await _context.Users
            .Include(u => u.ReviewerProfile)
            .Include(u => u.UserExpertises)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        // 1. Cập nhật bảng tblUsers
        user.FullName = vm.FullName ?? user.FullName;
        user.Email = vm.Email ?? user.Email;
        user.Phone = vm.Phone;
        user.Gender = vm.Gender;

        // Xử lý Upload Avatar (nếu có chọn file mới)
        if (avatarFile != null && avatarFile.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/avatars", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }
            user.Avatar = fileName;
        }

        // 2. Cập nhật bảng ReviewerProfiles
        if (user.ReviewerProfile == null)
        {
            user.ReviewerProfile = new tblReviewerProfiles { UserId = userId };
            _context.ReviewerProfiles.Add(user.ReviewerProfile);
        }
        user.ReviewerProfile.AcademicTitle = vm.AcademicTitle ?? user.ReviewerProfile.AcademicTitle;
        user.ReviewerProfile.Affiliation = vm.Affiliation ?? user.ReviewerProfile.Affiliation;

        // 3. Cập nhật bảng UserExpertise (Xóa hết cũ, add lại mới)
        _context.UsersExpertises.RemoveRange(user.UserExpertises);
        if (!string.IsNullOrEmpty(vm.ExpertiseKeywords))
        {
            var keywords = vm.ExpertiseKeywords.Split(',').Select(k => k.Trim());
            foreach (var k in keywords)
            {
                _context.UsersExpertises.Add(new tblUsersExpertise { UserId = userId, Keyword = k });
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật hồ sơ và hồ sơ khoa học thành công!";
        return RedirectToAction("Profile");
    }

    // --- CÁC HÀM KHÁC GIỮ NGUYÊN ---
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

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Login");
    }
}