using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(QuanLyKhachSanContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string tenDangNhap, string matKhau, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên đăng nhập và mật khẩu.");
                return View();
            }

            // Kiểm tra thông tin đăng nhập với cơ sở dữ liệu
            var taiKhoan = _context.TaiKhoans
                .FirstOrDefault(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan == null || taiKhoan.MatKhau != matKhau)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                _logger.LogWarning($"Lỗi đăng nhập: Thông tin không hợp lệ cho tài khoản {tenDangNhap}");
                return View();
            }

            // Tạo claims để lưu thông tin đăng nhập
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.MaTaiKhoan.ToString()),
                new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, taiKhoan.VaiTro ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(rememberMe ? 30 : 1)
            };

            // Xác thực người dùng
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation($"Tài khoản {tenDangNhap} (VaiTro: {taiKhoan.VaiTro}) đăng nhập thành công.");
            
            // Chuyển hướng dựa trên vai trò
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation($"Tài khoản {username} đã đăng xuất. Phiên làm việc kết thúc.");
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            _logger.LogWarning($"Tài khoản {User.Identity?.Name} cố gắng truy cập tài nguyên bị từ chối.");
            return View();
        }
    }
}
