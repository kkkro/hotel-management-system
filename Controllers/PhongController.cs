using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    [Authorize]
    public class PhongController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<PhongController> _logger;

        public PhongController(QuanLyKhachSanContext context, ILogger<PhongController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Phong
        public async Task<IActionResult> Index(string? trangThai)
        {
            _logger.LogInformation("Người dùng {0} xem danh sách phòng (bộ lọc: {1})", 
                User.Identity?.Name, trangThai ?? "Tất cả");

            IQueryable<Phong> phongs = _context.Phongs.Include(p => p.MaLoaiPhongNavigation);

            // Bộ lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                phongs = phongs.Where(p => p.TrangThai == trangThai);
            }

            var result = await phongs.OrderBy(p => p.SoPhong).ToListAsync();

            // Truyền danh sách trạng thái cho View
            ViewBag.TrangThaiList = new[] { "Trống", "Có khách", "Bảo trì", "Đã đặt" };
            ViewBag.CurrentFilter = trangThai;

            return View(result);
        }

        // GET: Phong/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                    .ThenInclude(l => l.GiaPhongs)
                .Include(p => p.CtthuePhongs)
                    .ThenInclude(ct => ct.MaThuePhongNavigation)
                        .ThenInclude(tp => tp.MaKhachHangNavigation)
                .FirstOrDefaultAsync(m => m.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Xem chi tiết phòng: {0}", id);
            return View(phong);
        }

        // GET: Phong/Create
        public async Task<IActionResult> Create()
        {
            // Truyền danh sách loại phòng cho dropdown
            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            ViewBag.TrangThaiList = new[] { "Trống", "Có khách", "Bảo trì", "Đã đặt" };
            return View();
        }

        // POST: Phong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhong,SoPhong,TrangThai,MaLoaiPhong")] Phong phong)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã phòng đã tồn tại
                if (await _context.Phongs.AnyAsync(p => p.MaPhong == phong.MaPhong))
                {
                    ModelState.AddModelError("MaPhong", "Mã phòng đã tồn tại");
                    ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
                    ViewBag.TrangThaiList = new[] { "Trống", "Có khách", "Bảo trì", "Đã đặt" };
                    return View(phong);
                }

                // Mặc định trạng thái là "Trống" nếu không có
                if (string.IsNullOrEmpty(phong.TrangThai))
                {
                    phong.TrangThai = "Trống";
                }

                _context.Add(phong);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Người dùng {0} tạo phòng mới: {1} (Loại: {2})",
                    User.Identity?.Name, phong.MaPhong, phong.MaLoaiPhong);

                TempData["Success"] = "Thêm phòng thành công";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            ViewBag.TrangThaiList = new[] { "Trống", "Có khách", "Bảo trì", "Đã đặt" };
            return View(phong);
        }

        // GET: Phong/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null)
            {
                return NotFound();
            }

            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            ViewBag.TrangThaiList = new[] { "Trống", "Có khách", "Bảo trì", "Đã đặt" };
            return View(phong);
        }

        // POST: Phong/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaPhong,SoPhong,TrangThai,MaLoaiPhong")] Phong phong)
        {
            if (id != phong.MaPhong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phong);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Người dùng {0} chỉnh sửa phòng: {1}",
                        User.Identity?.Name, id);

                    TempData["Success"] = "Cập nhật phòng thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PhongExists(phong.MaPhong))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            ViewBag.TrangThaiList = new[] { "Trống", "Có khách", "Bảo trì", "Đã đặt" };
            return View(phong);
        }

        // GET: Phong/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(m => m.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            return View(phong);
        }

        // POST: Phong/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null)
            {
                return NotFound();
            }

            _context.Phongs.Remove(phong);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Người dùng {0} xóa phòng: {1}",
                User.Identity?.Name, id);

            TempData["Success"] = "Xóa phòng thành công";
            return RedirectToAction(nameof(Index));
        }

        // API: Lấy danh sách phòng theo loại phòng (cho AJAX)
        [HttpGet]
        public async Task<JsonResult> GetPhongByLoaiPhong(string maLoaiPhong)
        {
            var phongs = await _context.Phongs
                .Where(p => p.MaLoaiPhong == maLoaiPhong && p.TrangThai == "Trống")
                .Select(p => new { id = p.MaPhong, text = p.SoPhong })
                .ToListAsync();

            return Json(phongs);
        }

        // API: Cập nhật trạng thái phòng (cho AJAX)
        [HttpPost]
        public async Task<JsonResult> UpdateTrangThai(string maPhong, string trangThai)
        {
            var phong = await _context.Phongs.FindAsync(maPhong);
            if (phong == null)
            {
                return Json(new { success = false, message = "Phòng không tồn tại" });
            }

            phong.TrangThai = trangThai;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cập nhật trạng thái phòng {0} thành: {1}",
                maPhong, trangThai);

            return Json(new { success = true, message = "Cập nhật thành công" });
        }

        // API: Lấy lịch sử thuê phòng chi tiết (cho AJAX)
        [HttpGet]
        public async Task<JsonResult> GetLichSuThuePhong(string maPhong)
        {
            var lichSu = await _context.CtthuePhongs
                .Where(ct => ct.MaPhong == maPhong)
                .Include(ct => ct.MaThuePhongNavigation)
                    .ThenInclude(tp => tp.MaKhachHangNavigation)
                .OrderByDescending(ct => ct.MaThuePhongNavigation.NgayNhan)
                .Select(ct => new
                {
                    maThuePhong = ct.MaThuePhong,
                    khachHang = ct.MaThuePhongNavigation.MaKhachHangNavigation.TenKhachHang,
                    ngayNhan = ct.MaThuePhongNavigation.NgayNhan.HasValue ? ct.MaThuePhongNavigation.NgayNhan.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    ngayTra = ct.MaThuePhongNavigation.NgayTra.HasValue ? ct.MaThuePhongNavigation.NgayTra.Value.ToString("dd/MM/yyyy HH:mm") : "Còn lưu trú",
                    giaThueTaiThoiDiem = ct.GiaThueTaiThoiDiem ?? 0,
                    trangThai = ct.MaThuePhongNavigation.NgayTra.HasValue ? "Đã hoàn thành" : "Đang lưu trú"
                })
                .ToListAsync();

            return Json(lichSu);
        }

        private async Task<bool> PhongExists(string id)
        {
            return await _context.Phongs.AnyAsync(e => e.MaPhong == id);
        }
    }
}
