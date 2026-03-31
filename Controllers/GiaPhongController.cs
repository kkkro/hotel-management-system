using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    [Authorize]
    public class GiaPhongController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<GiaPhongController> _logger;

        public GiaPhongController(QuanLyKhachSanContext context, ILogger<GiaPhongController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: GiaPhong
        public async Task<IActionResult> Index(string? maLoaiPhong)
        {
            _logger.LogInformation("Người dùng {0} xem danh sách giá phòng", User.Identity?.Name);

            IQueryable<GiaPhong> giaPhongs = _context.GiaPhongs
                .Include(g => g.MaLoaiPhongNavigation)
                .OrderByDescending(g => g.NgayBatDau);

            if (!string.IsNullOrEmpty(maLoaiPhong))
            {
                giaPhongs = giaPhongs.Where(g => g.MaLoaiPhong == maLoaiPhong);
                ViewBag.MaLoaiPhong = maLoaiPhong;
                ViewBag.TenLoaiPhong = await _context.LoaiPhongs
                    .Where(l => l.MaLoaiPhong == maLoaiPhong)
                    .Select(l => l.TenLoaiPhong)
                    .FirstOrDefaultAsync();
            }

            var result = await giaPhongs.ToListAsync();
            return View(result);
        }

        // GET: GiaPhong/Create
        public async Task<IActionResult> Create(string? maLoaiPhong)
        {
            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            
            if (!string.IsNullOrEmpty(maLoaiPhong))
            {
                ViewBag.MaLoaiPhong = maLoaiPhong;
            }
            
            return View();
        }

        // POST: GiaPhong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaGia,MaLoaiPhong,Gia,NgayBatDau,NgayKetThuc")] GiaPhong giaPhong)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã giá đã tồn tại
                if (await _context.GiaPhongs.AnyAsync(g => g.MaGia == giaPhong.MaGia))
                {
                    ModelState.AddModelError("MaGia", "Mã giá đã tồn tại");
                    ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
                    return View(giaPhong);
                }

                // Kiểm tra loại phòng tồn tại
                if (!await _context.LoaiPhongs.AnyAsync(l => l.MaLoaiPhong == giaPhong.MaLoaiPhong))
                {
                    ModelState.AddModelError("MaLoaiPhong", "Loại phòng không tồn tại");
                    ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
                    return View(giaPhong);
                }

                // Kiểm tra ngày bắt đầu <= ngày kết thúc
                if (giaPhong.NgayKetThuc.HasValue && giaPhong.NgayBatDau > giaPhong.NgayKetThuc)
                {
                    ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu");
                    ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
                    return View(giaPhong);
                }

                _context.Add(giaPhong);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Người dùng {0} thêm giá phòng mới: {1} - {2} VND/đêm",
                    User.Identity?.Name, giaPhong.MaGia, giaPhong.Gia);

                TempData["Success"] = "Thêm giá phòng thành công";
                
                if (!string.IsNullOrEmpty(giaPhong.MaLoaiPhong))
                {
                    return RedirectToAction("Index", new { maLoaiPhong = giaPhong.MaLoaiPhong });
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            return View(giaPhong);
        }

        // GET: GiaPhong/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var giaPhong = await _context.GiaPhongs
                .Include(g => g.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(g => g.MaGia == id);
            
            if (giaPhong == null)
            {
                return NotFound();
            }

            ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
            return View(giaPhong);
        }

        // POST: GiaPhong/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaGia,MaLoaiPhong,Gia,NgayBatDau,NgayKetThuc")] GiaPhong giaPhong)
        {
            if (id != giaPhong.MaGia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra ngày bắt đầu <= ngày kết thúc
                if (giaPhong.NgayKetThuc.HasValue && giaPhong.NgayBatDau > giaPhong.NgayKetThuc)
                {
                    ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu");
                    ViewBag.LoaiPhongs = await _context.LoaiPhongs.ToListAsync();
                    return View(giaPhong);
                }

                try
                {
                    _context.Update(giaPhong);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Người dùng {0} chỉnh sửa giá phòng: {1}",
                        User.Identity?.Name, id);

                    TempData["Success"] = "Cập nhật giá phòng thành công";
                    return RedirectToAction(nameof(Index), new { maLoaiPhong = giaPhong.MaLoaiPhong });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await GiaPhongExists(giaPhong.MaGia))
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
            return View(giaPhong);
        }

        // GET: GiaPhong/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var giaPhong = await _context.GiaPhongs
                .Include(g => g.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(m => m.MaGia == id);

            if (giaPhong == null)
            {
                return NotFound();
            }

            return View(giaPhong);
        }

        // POST: GiaPhong/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var giaPhong = await _context.GiaPhongs.FindAsync(id);
            if (giaPhong == null)
            {
                return NotFound();
            }

            var maLoaiPhong = giaPhong.MaLoaiPhong;
            _context.GiaPhongs.Remove(giaPhong);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Người dùng {0} xóa giá phòng: {1}",
                User.Identity?.Name, id);

            TempData["Success"] = "Xóa giá phòng thành công";
            return RedirectToAction(nameof(Index), new { maLoaiPhong = maLoaiPhong });
        }

        private async Task<bool> GiaPhongExists(string id)
        {
            return await _context.GiaPhongs.AnyAsync(e => e.MaGia == id);
        }
    }
}
