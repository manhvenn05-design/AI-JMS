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

    #region 1. XỬ LÝ XÁC THỰC (LOGIN - REGISTER - LOGOUT)

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        // Chuẩn hóa email đầu vào
        var normalizedEmail = email?.Trim().ToLower();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = _localizer["PasswordRequired"];
            return View();
        }

        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            if (!user.IsActive)
            {
                ViewBag.Error = _localizer["AccountLocked"];
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("LastRoleId", user.LastSelectedRoleId?.ToString() ?? "1")
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        ViewBag.Error = _localizer["InvalidLogin"];
        return View();
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string FullName, string Email, string PasswordHash, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(PasswordHash))
        {
            ViewBag.Error = _localizer["FillAllFields"];
            return View();
        }

        if (PasswordHash.Length < 6)
        {
            ViewBag.Error = _localizer["PasswordTooShort"];
            return View();
        }

        if (PasswordHash != confirmPassword)
        {
            ViewBag.Error = _localizer["PasswordsDoNotMatch"];
            return View();
        }

        if (await _context.Users.AnyAsync(u => u.Email == Email.Trim().ToLower()))
        {
            ViewBag.Error = _localizer["EmailAlreadyExists"];
            return View();
        }

        var user = new tblUsers
        {
            FullName = FullName.Trim(),
            Email = Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordHash),
            IsActive = true,
            CreatedAt = DateTime.Now,
            LastSelectedRoleId = 1
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new tblUserRoles { UserId = user.UserId, RoleId = 1 });
        await _context.SaveChangesAsync();

        // THÊM DÒNG NÀY: Để hiện Alert khi nhảy về trang Login
        TempData["RegisterSuccess"] = _localizer["RegisterSuccessMessage"].Value;
        return RedirectToAction("Login");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Login");
    }

    #endregion

    #region 2. QUẢN LÝ HỒ SƠ (PROFILE)

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var vm = await GetProfileData();
        if (vm == null) return RedirectToAction("Login");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(UserProfileViewModel vm, IFormFile? avatarFile)
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users
            .Include(u => u.ReviewerProfile)
            .Include(u => u.UserExpertises)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return RedirectToAction("Login");

        // --- LOGIC: CHỈ CẬP NHẬT NẾU FORM CÓ GỬI DỮ LIỆU (Tránh ghi đè Null) ---

        // 1. Kiểm tra nếu Form gửi lên có FullName -> Người dùng đang lưu ở Tab 1
        if (!string.IsNullOrEmpty(vm.FullName))
        {
            user.FullName = vm.FullName;
            user.Phone = vm.Phone;
            user.Gender = vm.Gender;

            // Xử lý Avatar (Giữ nguyên logic cũ của Mạnh)
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/avatars", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create)) { await avatarFile.CopyToAsync(stream); }
                user.Avatar = fileName;
            }
        }

        // 2. Kiểm tra nếu Form gửi lên có AcademicTitle hoặc Affiliation -> Người dùng đang lưu ở Tab 2
        if (vm.AcademicTitle != null || vm.Affiliation != null)
        {
            if (user.ReviewerProfile == null)
            {
                user.ReviewerProfile = new tblReviewerProfiles { UserId = userId };
                _context.ReviewerProfiles.Add(user.ReviewerProfile);
            }

            // CHỈ CẬP NHẬT NẾU GIÁ TRỊ GỬI LÊN KHÔNG NULL
            if (vm.AcademicTitle != null) user.ReviewerProfile.AcademicTitle = vm.AcademicTitle;
            if (vm.Affiliation != null) user.ReviewerProfile.Affiliation = vm.Affiliation;

            // Xử lý Chuyên môn (Keywords)
            if (vm.ExpertiseKeywords != null)
            {
                _context.UsersExpertises.RemoveRange(user.UserExpertises);
                var keywords = vm.ExpertiseKeywords.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k));
                foreach (var k in keywords)
                    _context.UsersExpertises.Add(new tblUsersExpertise { UserId = userId, Keyword = k });
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = _localizer["UpdateProfileSuccess"].Value;
        return RedirectToAction("Profile");
    }

    private async Task<UserProfileViewModel?> GetProfileData()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users
            .AsNoTracking() // Dùng AsNoTracking để lấy dữ liệu mới nhất từ DB
            .Include(u => u.ReviewerProfile)
            .Include(u => u.UserExpertises)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return null;

        return new UserProfileViewModel
        {
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            Phone = user.Phone ?? "",
            Gender = user.Gender ?? "Nam",
            Avatar = user.Avatar,
            AcademicTitle = user.ReviewerProfile?.AcademicTitle ?? "Không",
            Affiliation = user.ReviewerProfile?.Affiliation ?? "",
            ExpertiseKeywords = user.UserExpertises != null
                ? string.Join(", ", user.UserExpertises.Select(e => e.Keyword))
                : ""
        };
    }

    #endregion

    #region 3. BẢO MẬT (CHANGE PASSWORD)

    // --- TRONG HÀM CHANGEPASSWORD ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        // ModelState.IsValid sẽ tự check độ dài 6 ký tự từ ViewModel, bạn không cần code tay lại
        if (!ModelState.IsValid) return View("Profile", await GetProfileData());

        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // KIỂM TRA MẬT KHẨU HIỆN TẠI
        if (!BCrypt.Net.BCrypt.Verify(vm.OldPassword, user.PasswordHash))
        {
            ModelState.AddModelError("OldPassword", _localizer["WrongOldPassword"]);
            return View("Profile", await GetProfileData());
        }

        // Hash mật khẩu mới
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["ChangePasswordSuccess"].Value;
        return RedirectToAction("Profile");
    }

    #endregion

    #region 4. HÀM TRỢ GIÚP (HELPERS)

    private int GetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out int userId) ? userId : 0;
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

    #endregion
}