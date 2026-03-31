using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    [Authorize]
    public class LoaiPhongController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<LoaiPhongController> _logger;

        public LoaiPhongController(QuanLyKhachSanContext context, ILogger<LoaiPhongController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: LoaiPhong
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Người dùng {0} xem danh sách loại phòng", User.Identity?.Name);
            
            var loaiPhongs = await _context.LoaiPhongs
                .Include(l => l.GiaPhongs)
                .OrderBy(l => l.MaLoaiPhong)
                .ToListAsync();
            
            return View(loaiPhongs);
        }

        // GET: LoaiPhong/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var loaiPhong = await _context.LoaiPhongs
                .Include(l => l.GiaPhongs)
                .Include(l => l.Phongs)
                .FirstOrDefaultAsync(m => m.MaLoaiPhong == id);

            if (loaiPhong == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Xem chi tiết loại phòng: {0}", id);
            return View(loaiPhong);
        }

        // GET: LoaiPhong/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoaiPhong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLoaiPhong,TenLoaiPhong,MoTa,SoNguoiToiDa")] LoaiPhong loaiPhong)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã loại phòng đã tồn tại
                if (await _context.LoaiPhongs.AnyAsync(l => l.MaLoaiPhong == loaiPhong.MaLoaiPhong))
                {
                    ModelState.AddModelError("MaLoaiPhong", "Mã loại phòng đã tồn tại");
                    return View(loaiPhong);
                }

                _context.Add(loaiPhong);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Người dùng {0} tạo loại phòng mới: {1} - {2}", 
                    User.Identity?.Name, loaiPhong.MaLoaiPhong, loaiPhong.TenLoaiPhong);
                
                TempData["Success"] = "Thêm loại phòng thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(loaiPhong);
        }

        // GET: LoaiPhong/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var loaiPhong = await _context.LoaiPhongs.FindAsync(id);
            if (loaiPhong == null)
            {
                return NotFound();
            }
            
            return View(loaiPhong);
        }

        // POST: LoaiPhong/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaLoaiPhong,TenLoaiPhong,MoTa,SoNguoiToiDa")] LoaiPhong loaiPhong)
        {
            if (id != loaiPhong.MaLoaiPhong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiPhong);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Người dùng {0} chỉnh sửa loại phòng: {1}", 
                        User.Identity?.Name, id);
                    
                    TempData["Success"] = "Cập nhật loại phòng thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await LoaiPhongExists(loaiPhong.MaLoaiPhong))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(loaiPhong);
        }

        // GET: LoaiPhong/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var loaiPhong = await _context.LoaiPhongs
                .Include(l => l.Phongs)
                .FirstOrDefaultAsync(m => m.MaLoaiPhong == id);

            if (loaiPhong == null)
            {
                return NotFound();
            }

            return View(loaiPhong);
        }

        // POST: LoaiPhong/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var loaiPhong = await _context.LoaiPhongs.FindAsync(id);
            if (loaiPhong == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có phòng nào đang sử dụng loại phòng này
            var phongCount = await _context.Phongs.CountAsync(p => p.MaLoaiPhong == id);
            if (phongCount > 0)
            {
                TempData["Error"] = $"Không thể xóa loại phòng này vì còn {phongCount} phòng đang sử dụng";
                return RedirectToAction(nameof(Index));
            }

            _context.LoaiPhongs.Remove(loaiPhong);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Người dùng {0} xóa loại phòng: {1}", 
                User.Identity?.Name, id);
            
            TempData["Success"] = "Xóa loại phòng thành công";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> LoaiPhongExists(string id)
        {
            return await _context.LoaiPhongs.AnyAsync(e => e.MaLoaiPhong == id);
        }

        // API: Lấy giá hiện tại của loại phòng
        [HttpGet]
        public async Task<JsonResult> GetGiaHienTai(string maLoaiPhong)
        {
            var giaHienTai = await _context.GiaPhongs
                .Where(g => g.MaLoaiPhong == maLoaiPhong &&
                            g.NgayBatDau <= DateTime.Now &&
                            (g.NgayKetThuc == null || g.NgayKetThuc >= DateTime.Now))
                .OrderByDescending(g => g.NgayBatDau)
                .FirstOrDefaultAsync();

            if (giaHienTai != null)
            {
                return Json(new { success = true, gia = giaHienTai.Gia, maGia = giaHienTai.MaGia });
            }

            return Json(new { success = false, gia = 0, message = "Chưa có giá được thiết lập" });
        }

        // API: Lấy danh sách giá của loại phòng
        [HttpGet]
        public async Task<JsonResult> GetDanhSachGia(string maLoaiPhong)
        {
            var danhSachGia = await _context.GiaPhongs
                .Where(g => g.MaLoaiPhong == maLoaiPhong)
                .OrderByDescending(g => g.NgayBatDau)
                .Select(g => new
                {
                    maGia = g.MaGia,
                    gia = g.Gia,
                    ngayBatDau = g.NgayBatDau.HasValue ? g.NgayBatDau.Value.ToString("dd/MM/yyyy") : "",
                    ngayKetThuc = g.NgayKetThuc.HasValue ? g.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "(Không giới hạn)",
                    trangThai = (g.NgayBatDau <= DateTime.Now && (g.NgayKetThuc == null || g.NgayKetThuc >= DateTime.Now)) ? "Đang áp dụng" : "Hết hạn"
                })
                .ToListAsync();

            return Json(danhSachGia);
        }
    }
}
