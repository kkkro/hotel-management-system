using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    [Authorize]
    public class KhachHangController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<KhachHangController> _logger;

        public KhachHangController(QuanLyKhachSanContext context, ILogger<KhachHangController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Nguoi dung {User} xem danh sach khach hang", User.Identity?.Name);

            var khachHangs = await _context.KhachHangs
                .Include(k => k.ThuePhongs)
                .Include(k => k.DatPhongs)
                .OrderBy(k => k.MaKhachHang)
                .ToListAsync();

            return View(khachHangs);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs
                .Include(k => k.ThuePhongs)
                    .ThenInclude(tp => tp.CtthuePhongs)
                        .ThenInclude(ct => ct.MaPhongNavigation)
                            .ThenInclude(p => p.MaLoaiPhongNavigation)
                .Include(k => k.DatPhongs)
                .FirstOrDefaultAsync(m => m.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Nguoi dung {User} xem chi tiet khach hang: {Id}", User.Identity?.Name, id);

            return View(khachHang);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKhachHang,TenKhachHang,DienThoai,DiaChi,Cccd,Email")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                if (await _context.KhachHangs.AnyAsync(k => k.MaKhachHang == khachHang.MaKhachHang))
                {
                    ModelState.AddModelError("MaKhachHang", "Ma khach hang da ton tai");
                    return View(khachHang);
                }

                if (!string.IsNullOrEmpty(khachHang.Cccd) &&
                    await _context.KhachHangs.AnyAsync(k => k.Cccd == khachHang.Cccd))
                {
                    ModelState.AddModelError("Cccd", "CCCD nay da ton tai trong he thong");
                    return View(khachHang);
                }

                _context.Add(khachHang);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nguoi dung {User} them khach hang moi: {MaKhachHang} - {TenKhachHang}",
                    User.Identity?.Name, khachHang.MaKhachHang, khachHang.TenKhachHang);

                TempData["Success"] = "Them khach hang thanh cong";
                return RedirectToAction(nameof(Index));
            }

            return View(khachHang);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaKhachHang,TenKhachHang,DienThoai,DiaChi,Cccd,Email")] KhachHang khachHang)
        {
            if (id != khachHang.MaKhachHang)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(khachHang.Cccd) &&
                    await _context.KhachHangs.AnyAsync(k => k.Cccd == khachHang.Cccd && k.MaKhachHang != id))
                {
                    ModelState.AddModelError("Cccd", "CCCD nay da ton tai trong he thong");
                    return View(khachHang);
                }

                try
                {
                    _context.Update(khachHang);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Nguoi dung {User} chinh sua khach hang: {Id}", User.Identity?.Name, id);

                    TempData["Success"] = "Cap nhat khach hang thanh cong";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await KhachHangExists(khachHang.MaKhachHang))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            return View(khachHang);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs
                .Include(k => k.ThuePhongs)
                .Include(k => k.DatPhongs)
                .FirstOrDefaultAsync(m => m.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            if (khachHang.DatPhongs.Any() || khachHang.ThuePhongs.Any())
            {
                TempData["Error"] = "Khong the xoa khach hang nay vi co don dat phong hoac lich su thue phong";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.KhachHangs.Remove(khachHang);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nguoi dung {User} xoa khach hang: {Id}", User.Identity?.Name, id);

            TempData["Success"] = "Xoa khach hang thanh cong";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.KhachHangs
                .OrderBy(k => k.TenKhachHang)
                .Select(k => new
                {
                    maKhachHang = k.MaKhachHang,
                    tenKhachHang = k.TenKhachHang,
                    dienthoai = k.DienThoai,
                    diachi = k.DiaChi,
                    email = k.Email
                })
                .ToListAsync();

            return Json(customers);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TenKhachHang) || string.IsNullOrEmpty(request.DienThoai))
                {
                    return Json(new { success = false, message = "Thong tin khong hop le" });
                }

                if (!string.IsNullOrEmpty(request.Cccd))
                {
                    var existingByCccd = await _context.KhachHangs
                        .FirstOrDefaultAsync(k => k.Cccd == request.Cccd);

                    if (existingByCccd != null)
                    {
                        return Json(new { success = true, maKhachHang = existingByCccd.MaKhachHang, message = "Su dung khach hang hien co" });
                    }
                }

                var existingByPhone = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.DienThoai == request.DienThoai);

                if (existingByPhone != null)
                {
                    return Json(new { success = true, maKhachHang = existingByPhone.MaKhachHang, message = "Su dung khach hang hien co" });
                }

                string maKhachHang = string.Empty;

                for (var attempt = 0; attempt < 5; attempt++)
                {
                    maKhachHang = await GenerateNextCustomerCodeAsync();

                    var khachHang = new KhachHang
                    {
                        MaKhachHang = maKhachHang,
                        TenKhachHang = request.TenKhachHang,
                        DienThoai = request.DienThoai,
                        DiaChi = request.DiaChi,
                        Cccd = request.Cccd,
                        Email = request.Email
                    };

                    _context.KhachHangs.Add(khachHang);

                    try
                    {
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Tao khach hang moi tu booking cong khai: {MaKhachHang} - {TenKhachHang}",
                            maKhachHang, request.TenKhachHang);

                        return Json(new { success = true, maKhachHang });
                    }
                    catch (DbUpdateException ex) when (IsDuplicateCustomerKey(ex))
                    {
                        _context.Entry(khachHang).State = EntityState.Detached;
                    }
                }

                return Json(new { success = false, message = "Khong the tao ma khach hang moi. Vui long thu lai." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi khi tao khach hang");
                return Json(new { success = false, message = "Loi may chu" });
            }
        }

        private async Task<bool> KhachHangExists(string id)
        {
            return await _context.KhachHangs.AnyAsync(e => e.MaKhachHang == id);
        }

        private async Task<string> GenerateNextCustomerCodeAsync()
        {
            var ids = await _context.KhachHangs
                .Select(k => k.MaKhachHang)
                .Where(id => id != null && id.StartsWith("KH"))
                .ToListAsync();

            var max = ids.Select(id =>
            {
                var digits = new string(id.Skip(2).Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out var number) ? number : 0;
            }).DefaultIfEmpty(0).Max();

            return $"KH{max + 1:D6}";
        }

        private static bool IsDuplicateCustomerKey(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601);
        }
    }

    public class CreateCustomerRequest
    {
        public string TenKhachHang { get; set; } = string.Empty;
        public string DienThoai { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string? Cccd { get; set; }
        public string? Email { get; set; }
    }
}
