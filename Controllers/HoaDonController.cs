using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    [Authorize]
    public class HoaDonController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<HoaDonController> _logger;

        public HoaDonController(QuanLyKhachSanContext context, ILogger<HoaDonController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var hoaDons = await _context.HoaDons
                .Include(hd => hd.MaNhanVienNavigation)
                .Include(hd => hd.CthoaDons)
                    .ThenInclude(ct => ct.MaThuePhongNavigation)
                        .ThenInclude(tp => tp.MaKhachHangNavigation)
                .OrderByDescending(hd => hd.NgayLap)
                .ToListAsync();

            return View(hoaDons);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(hd => hd.MaNhanVienNavigation)
                .Include(hd => hd.CthoaDons)
                    .ThenInclude(ct => ct.MaThuePhongNavigation)
                        .ThenInclude(tp => tp.MaKhachHangNavigation)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.NhanViens = await _context.NhanViens
                .OrderBy(nv => nv.TenNhanVien)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaHoaDon,MaNhanVien,NgayLap,TongTien")] HoaDon hoaDon)
        {
            if (ModelState.IsValid)
            {
                if (await _context.HoaDons.AnyAsync(hd => hd.MaHoaDon == hoaDon.MaHoaDon))
                {
                    ModelState.AddModelError(nameof(HoaDon.MaHoaDon), "Ma hoa don da ton tai");
                }
                else if (!string.IsNullOrEmpty(hoaDon.MaNhanVien) && !await _context.NhanViens.AnyAsync(nv => nv.MaNhanVien == hoaDon.MaNhanVien))
                {
                    ModelState.AddModelError(nameof(HoaDon.MaNhanVien), "Ma nhan vien khong ton tai");
                }
                else
                {
                    hoaDon.NgayLap ??= DateTime.Today;
                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tao hoa don thanh cong";
                    return RedirectToAction(nameof(Details), new { id = hoaDon.MaHoaDon });
                }
            }

            ViewBag.NhanViens = await _context.NhanViens
                .OrderBy(nv => nv.TenNhanVien)
                .ToListAsync();

            return View(hoaDon);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            ViewBag.NhanViens = await _context.NhanViens
                .OrderBy(nv => nv.TenNhanVien)
                .ToListAsync();

            return View(hoaDon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaHoaDon,MaNhanVien,NgayLap,TongTien")] HoaDon hoaDon)
        {
            if (id != hoaDon.MaHoaDon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hoaDon);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cap nhat hoa don thanh cong";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoaDonExists(hoaDon.MaHoaDon))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            ViewBag.NhanViens = await _context.NhanViens
                .OrderBy(nv => nv.TenNhanVien)
                .ToListAsync();

            return View(hoaDon);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(hd => hd.MaNhanVienNavigation)
                .Include(hd => hd.CthoaDons)
                    .ThenInclude(ct => ct.MaThuePhongNavigation)
                        .ThenInclude(tp => tp.MaKhachHangNavigation)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var hoaDon = await _context.HoaDons
                .Include(hd => hd.CthoaDons)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            if (hoaDon.CthoaDons.Any())
            {
                _context.CthoaDons.RemoveRange(hoaDon.CthoaDons);
            }

            _context.HoaDons.Remove(hoaDon);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xoa hoa don thanh cong";
            return RedirectToAction(nameof(Index));
        }

        private bool HoaDonExists(string id)
        {
            return _context.HoaDons.Any(e => e.MaHoaDon == id);
        }
    }
}
