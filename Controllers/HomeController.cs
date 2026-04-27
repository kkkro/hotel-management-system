using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QuanLyKhachSanContext _context;
        private static readonly string[] TrangThaiPhongTrongVariants = { "Trống", "Tr?ng", "Trá»‘ng" };
        private static readonly string[] TrangThaiCoKhachVariants = { "Đang sử dụng", "Có khách", "CÃ³ khÃ¡ch", "Co khach" };
        private static readonly string[] TrangThaiBaoTriVariants = { "Bảo trì", "B?o trì", "Báº£o trÃ¬" };
        private static readonly string[] TrangThaiDaDatVariants = { "Đã đặt", "Ðã d?t", "ÄÃ£ Ä‘áº·t" };

        public HomeController(ILogger<HomeController> logger, QuanLyKhachSanContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Public booking page - no login required
        [AllowAnonymous]
        public async Task<IActionResult> BookingPublic()
        {
            var danhSachLoaiPhong = await _context.LoaiPhongs
                .Include(l => l.GiaPhongs)
                .OrderBy(l => l.TenLoaiPhong)
                .ToListAsync();

            return View(danhSachLoaiPhong);
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê phòng
            var tongPhong = await _context.Phongs.CountAsync();
            var phongTrong = await _context.Phongs.Where(p => p.TrangThai != null && TrangThaiPhongTrongVariants.Contains(p.TrangThai)).CountAsync();
            var phongCoKhach = await _context.Phongs.Where(p => p.TrangThai != null && TrangThaiCoKhachVariants.Contains(p.TrangThai)).CountAsync();
            var phongBaoTri = await _context.Phongs.Where(p => p.TrangThai != null && TrangThaiBaoTriVariants.Contains(p.TrangThai)).CountAsync();
            var phongDaDat = await _context.Phongs.Where(p => p.TrangThai != null && TrangThaiDaDatVariants.Contains(p.TrangThai)).CountAsync();

            // Lấy danh sách phòng với thông tin chi tiết
            var danhSachPhong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .Include(p => p.CtthuePhongs)
                    .ThenInclude(ct => ct.MaThuePhongNavigation)
                        .ThenInclude(tp => tp.MaKhachHangNavigation)
                .OrderBy(p => p.SoPhong)
                .ToListAsync();

            // Lấy danh sách lượt nhận phòng sắp tới (trong 24 giờ)
            var ngayMai = DateTime.Now.AddHours(24);
            var lichNhanPhong = await _context.ThuePhongs
                .Include(tp => tp.MaKhachHangNavigation)
                .Include(tp => tp.CtthuePhongs)
                    .ThenInclude(pt => pt.MaPhongNavigation)
                        .ThenInclude(p => p.MaLoaiPhongNavigation)
                .Where(tp => tp.NgayNhan > DateTime.Now && tp.NgayNhan <= ngayMai)
                .OrderBy(tp => tp.NgayNhan)
                .Take(5)
                .ToListAsync();

            // Lấy danh sách loại phòng
            var danhSachLoaiPhong = await _context.LoaiPhongs
                .Include(l => l.GiaPhongs)
                .Include(l => l.Phongs)
                .OrderBy(l => l.MaLoaiPhong)
                .ToListAsync();

            // Thống kê khách hàng
            var tongKhachHang = await _context.KhachHangs.CountAsync();

            ViewBag.TongPhong = tongPhong;
            ViewBag.PhongTrong = phongTrong;
            ViewBag.PhongCoKhach = phongCoKhach;
            ViewBag.PhongBaoTri = phongBaoTri;
            ViewBag.PhongDaDat = phongDaDat;
            ViewBag.TongKhachHang = tongKhachHang;
            ViewBag.DanhSachPhong = danhSachPhong;
            ViewBag.LichNhanPhong = lichNhanPhong;
            ViewBag.DanhSachLoaiPhong = danhSachLoaiPhong;

            _logger.LogInformation("Người dùng {0} xem dashboard", User.Identity?.Name);

            return View(danhSachPhong);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
