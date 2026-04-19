using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    [Authorize]
    public class PhongController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<PhongController> _logger;
        private static readonly string[] TrangThaiPhongTrongVariants = { "Trống", "Tr?ng", "Trá»‘ng" };
        private static readonly string[] TrangThaiCoKhachVariants = { "Có khách", "CÃ³ khÃ¡ch", "Co khach" };
        private static readonly string[] TrangThaiBaoTriVariants = { "Bảo trì", "B?o trì", "Báº£o trÃ¬" };
        private static readonly string[] TrangThaiDaDatVariants = { "Đã đặt", "Ðã d?t", "ÄÃ£ Ä‘áº·t" };

        private static readonly string[] TrangThaiPhongList =
        {
            "Tr\u1ed1ng",
            "C\u00f3 kh\u00e1ch",
            "B\u1ea3o tr\u00ec",
            "\u0110\u00e3 \u0111\u1eb7t"
        };

        public PhongController(QuanLyKhachSanContext context, ILogger<PhongController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Phong
        public async Task<IActionResult> Index(string? trangThai)
        {
            _logger.LogInformation("Nguoi dung {User} xem danh sach phong (bo loc: {Filter})",
                User.Identity?.Name, trangThai ?? "Tat ca");

            IQueryable<Phong> phongs = _context.Phongs.Include(p => p.MaLoaiPhongNavigation);

            if (!string.IsNullOrEmpty(trangThai))
            {
                var trangThaiVariants = GetTrangThaiVariants(trangThai);
                phongs = phongs.Where(p => p.TrangThai != null && trangThaiVariants.Contains(p.TrangThai));
            }

            var result = await phongs.OrderBy(p => p.SoPhong).ToListAsync();

            ViewBag.TrangThaiList = TrangThaiPhongList;
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

            _logger.LogInformation("Xem chi tiet phong: {Id}", id);
            return View(phong);
        }

        // GET: Phong/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            ViewBag.TrangThaiList = TrangThaiPhongList;
            return View();
        }

        // POST: Phong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhong,SoPhong,TrangThai,MaLoaiPhong")] Phong phong)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Phongs.AnyAsync(p => p.MaPhong == phong.MaPhong))
                {
                    ModelState.AddModelError("MaPhong", "Ma phong da ton tai");
                    ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
                    ViewBag.TrangThaiList = TrangThaiPhongList;
                    return View(phong);
                }

                if (string.IsNullOrEmpty(phong.TrangThai))
                {
                    phong.TrangThai = "Tr\u1ed1ng";
                }

                _context.Add(phong);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nguoi dung {User} tao phong moi: {MaPhong} (Loai: {MaLoaiPhong})",
                    User.Identity?.Name, phong.MaPhong, phong.MaLoaiPhong);

                TempData["Success"] = "Them phong thanh cong";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            ViewBag.TrangThaiList = TrangThaiPhongList;
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
            ViewBag.TrangThaiList = TrangThaiPhongList;
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

                    _logger.LogInformation("Nguoi dung {User} chinh sua phong: {Id}",
                        User.Identity?.Name, id);

                    TempData["Success"] = "Cap nhat phong thanh cong";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PhongExists(phong.MaPhong))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            ViewBag.TrangThaiList = TrangThaiPhongList;
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

            _logger.LogInformation("Nguoi dung {User} xoa phong: {Id}",
                User.Identity?.Name, id);

            TempData["Success"] = "Xoa phong thanh cong";
            return RedirectToAction(nameof(Index));
        }

        // API: Lay danh sach phong theo loai phong
        [HttpGet]
        public async Task<JsonResult> GetPhongByLoaiPhong(string maLoaiPhong)
        {
            var phongs = await _context.Phongs
                .Where(p => p.MaLoaiPhong == maLoaiPhong && p.TrangThai != null && TrangThaiPhongTrongVariants.Contains(p.TrangThai))
                .Select(p => new { id = p.MaPhong, text = p.SoPhong })
                .ToListAsync();

            return Json(phongs);
        }

        // API: Cap nhat trang thai phong
        [HttpPost]
        public async Task<JsonResult> UpdateTrangThai(string maPhong, string trangThai)
        {
            var phong = await _context.Phongs.FindAsync(maPhong);
            if (phong == null)
            {
                return Json(new { success = false, message = "Phong khong ton tai" });
            }

            phong.TrangThai = trangThai;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cap nhat trang thai phong {MaPhong} thanh: {TrangThai}",
                maPhong, trangThai);

            return Json(new { success = true, message = "Cap nhat thanh cong" });
        }

        // API: Lay lich su thue phong chi tiet
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
                    ngayTra = ct.MaThuePhongNavigation.NgayTra.HasValue ? ct.MaThuePhongNavigation.NgayTra.Value.ToString("dd/MM/yyyy HH:mm") : "Con luu tru",
                    giaThueTaiThoiDiem = ct.GiaThueTaiThoiDiem ?? 0,
                    trangThai = ct.MaThuePhongNavigation.NgayTra.HasValue ? "Da hoan thanh" : "Dang luu tru"
                })
                .ToListAsync();

            return Json(lichSu);
        }

        private async Task<bool> PhongExists(string id)
        {
            return await _context.Phongs.AnyAsync(e => e.MaPhong == id);
        }

        private static string[] GetTrangThaiVariants(string trangThai)
        {
            return trangThai switch
            {
                "Trống" or "Tr?ng" or "Trá»‘ng" => TrangThaiPhongTrongVariants,
                "Có khách" or "CÃ³ khÃ¡ch" or "Co khach" => TrangThaiCoKhachVariants,
                "Bảo trì" or "B?o trì" or "Báº£o trÃ¬" => TrangThaiBaoTriVariants,
                "Đã đặt" or "Ðã d?t" or "ÄÃ£ Ä‘áº·t" => TrangThaiDaDatVariants,
                _ => new[] { trangThai }
            };
        }
    }
}
