using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;

namespace WebKhachSan.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class NhanVienController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<NhanVienController> _logger;

        public NhanVienController(QuanLyKhachSanContext context, ILogger<NhanVienController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Nguoi dung {User} xem danh sach nhan vien", User.Identity?.Name);

            var nhanViens = await _context.NhanViens
                .Include(nv => nv.MaTaiKhoanNavigation)
                .OrderBy(nv => nv.MaNhanVien)
                .ToListAsync();

            return View(nhanViens);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .Include(nv => nv.MaTaiKhoanNavigation)
                .Include(nv => nv.HoaDons)
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Nguoi dung {User} xem chi tiet nhan vien: {Id}", User.Identity?.Name, id);

            return View(nhanVien);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenNhanVien,NgaySinh,DienThoai,DiaChi,ChucVu")] NhanVien nhanVien)
        {
            ModelState.Remove(nameof(NhanVien.MaNhanVien));
            nhanVien.MaNhanVien = await GenerateNextEmployeeCodeAsync();

            if (!ModelState.IsValid)
            {
                return View(nhanVien);
            }

            var isLeTan = IsLeTanRole(nhanVien.ChucVu);
            TaiKhoan? taiKhoan = null;

            if (isLeTan)
            {
                if (!nhanVien.NgaySinh.HasValue)
                {
                    ModelState.AddModelError("NgaySinh", "Ngay sinh la bat buoc khi tao tai khoan cho le tan");
                    return View(nhanVien);
                }

                if (await _context.TaiKhoans.AnyAsync(tk => tk.TenDangNhap == nhanVien.MaNhanVien))
                {
                    ModelState.AddModelError("MaNhanVien", "Da ton tai tai khoan co ten dang nhap trung voi ma nhan vien nay");
                    return View(nhanVien);
                }

                var nextAccountId = await GenerateNextAccountIdAsync();
                taiKhoan = new TaiKhoan
                {
                    MaTaiKhoan = nextAccountId,
                    TenDangNhap = nhanVien.MaNhanVien,
                    MatKhau = nhanVien.NgaySinh.Value.ToString("ddMMyyyy"),
                    VaiTro = "nhanvien"
                };

                nhanVien.MaTaiKhoan = taiKhoan.MaTaiKhoan;
            }
            else
            {
                nhanVien.MaTaiKhoan = null;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (taiKhoan != null)
                {
                    _context.TaiKhoans.Add(taiKhoan);
                }

                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Nguoi dung {User} them nhan vien moi: {MaNhanVien} - {TenNhanVien}",
                    User.Identity?.Name, nhanVien.MaNhanVien, nhanVien.TenNhanVien);

                TempData["Success"] = isLeTan
                    ? "Them nhan vien thanh cong va da tao tai khoan le tan"
                    : "Them nhan vien thanh cong";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Loi khi them nhan vien {MaNhanVien}", nhanVien.MaNhanVien);
                ModelState.AddModelError(string.Empty, "Khong the them nhan vien. Vui long thu lai.");
                return View(nhanVien);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .Include(nv => nv.HoaDons)
                .FirstOrDefaultAsync(nv => nv.MaNhanVien == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaNhanVien,TenNhanVien,NgaySinh,DienThoai,DiaChi,ChucVu,MaTaiKhoan")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNhanVien)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Nguoi dung {User} cap nhat nhan vien: {MaNhanVien}",
                        User.Identity?.Name, nhanVien.MaNhanVien);

                    TempData["Success"] = "Cap nhat nhan vien thanh cong";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNhanVien))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(nhanVien);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .Include(nv => nv.HoaDons)
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            if (nhanVien.HoaDons.Any())
            {
                TempData["Error"] = "Khong the xoa nhan vien nay vi co lien quan den hoa don";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nguoi dung {User} xoa nhan vien: {Id}", User.Identity?.Name, id);

            TempData["Success"] = "Xoa nhan vien thanh cong";
            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(string id)
        {
            return _context.NhanViens.Any(e => e.MaNhanVien == id);
        }

        private async Task<int> GenerateNextAccountIdAsync()
        {
            var currentMax = await _context.TaiKhoans
                .Select(tk => (int?)tk.MaTaiKhoan)
                .MaxAsync();

            return (currentMax ?? 0) + 1;
        }

        private async Task<string> GenerateNextEmployeeCodeAsync()
        {
            var ids = await _context.NhanViens
                .Select(nv => nv.MaNhanVien)
                .Where(id => id != null && id.StartsWith("NV"))
                .ToListAsync();

            var max = ids.Select(id =>
            {
                var digits = new string(id.Skip(2).Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out var number) ? number : 0;
            }).DefaultIfEmpty(0).Max();

            return $"NV{max + 1:D3}";
        }

        private static bool IsLeTanRole(string? chucVu)
        {
            return NormalizeText(chucVu) == "le tan";
        }

        private static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(char.ToLowerInvariant(c));
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
