using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;
using WebKhachSan.ViewModels;

namespace WebKhachSan.Controllers
{
    [Authorize(Policy = "StaffAndAbove")]
    public class ThuePhongController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<ThuePhongController> _logger;
        private static readonly string[] TrangThaiPhongTrongVariants = { "Trống", "Tr?ng", "Trá»‘ng" };

        public ThuePhongController(QuanLyKhachSanContext context, ILogger<ThuePhongController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var thuePhongs = await _context.ThuePhongs
                .Include(tp => tp.MaKhachHangNavigation)
                .Include(tp => tp.CtthuePhongs)
                    .ThenInclude(ct => ct.MaPhongNavigation)
                        .ThenInclude(p => p.MaLoaiPhongNavigation)
                .OrderByDescending(tp => tp.NgayNhan)
                .ToListAsync();

            return View(thuePhongs);
        }

        [HttpGet]
        public async Task<IActionResult> AvailableRoomsModal(string? maPhong, string? soPhong, string? maLoaiPhong)
        {
            var model = new DanhSachPhongTrongViewModel
            {
                MaPhong = maPhong,
                SoPhong = soPhong,
                MaLoaiPhong = maLoaiPhong,
                LoaiPhongs = await _context.LoaiPhongs.OrderBy(lp => lp.TenLoaiPhong).ToListAsync(),
                Phongs = await GetAvailableRoomsAsync(maPhong, soPhong, maLoaiPhong)
            };

            return PartialView("_AvailableRoomsModalBody", model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOffline(string maPhong)
        {
            var model = await BuildOfflineRentalViewModelAsync(maPhong);
            if (model == null)
            {
                return Content("<div class='alert alert-danger mb-0'>Khong tim thay phong trong hop le.</div>", "text/html");
            }

            return PartialView("_CreateOfflineRentalForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOffline(ThuePhongOfflineViewModel model)
        {
            await PopulateOfflineRentalDisplayDataAsync(model);
            await ValidateOfflineRentalAsync(model);

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("_CreateOfflineRentalForm", model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var phong = await _context.Phongs
                    .Include(p => p.MaLoaiPhongNavigation)
                    .FirstAsync(p => p.MaPhong == model.MaPhong);

                var giaHienTai = await GetCurrentPriceAsync(phong.MaLoaiPhong);
                if (giaHienTai == null)
                {
                    ModelState.AddModelError(string.Empty, "Phong nay chua co gia hien tai, khong the lap phieu thue.");
                    Response.StatusCode = 400;
                    return PartialView("_CreateOfflineRentalForm", model);
                }

                var khachHang = await ResolveCustomerAsync(model);
                var maThuePhong = await GenerateNextCodeAsync(_context.ThuePhongs.Select(tp => tp.MaThuePhong), "TP");

                var thuePhong = new ThuePhong
                {
                    MaThuePhong = maThuePhong,
                    MaKhachHang = khachHang.MaKhachHang,
                    TrangThai = "Dang thue",
                    NgayNhan = model.NgayNhan?.Date ?? DateTime.Today,
                    NgayTra = model.NgayTra?.Date
                };

                var chiTiet = new CtthuePhong
                {
                    MaThuePhong = maThuePhong,
                    MaPhong = phong.MaPhong,
                    GiaThueTaiThoiDiem = giaHienTai.Gia
                };

                phong.TrangThai = "Đang sử dụng";

                _context.ThuePhongs.Add(thuePhong);
                _context.CtthuePhongs.Add(chiTiet);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Nguoi dung {User} tao phieu thue {MaThuePhong} cho phong {MaPhong}",
                    User.Identity?.Name, maThuePhong, phong.MaPhong);

                TempData["Success"] = $"Da thue phong {phong.SoPhong} cho khach {khachHang.TenKhachHang}.";
                return Json(new
                {
                    success = true,
                    message = $"Da xac nhan thue phong {phong.SoPhong} thanh cong."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Loi khi tao phieu thue offline cho phong {MaPhong}", model.MaPhong);
                ModelState.AddModelError(string.Empty, "Khong the xac nhan thue phong. Vui long thu lai.");
                Response.StatusCode = 500;
                return PartialView("_CreateOfflineRentalForm", model);
            }
        }

        [HttpGet]
        public IActionResult CreateStep1()
        {
            return PartialView("_CreateStep1");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep1(ThuePhongStep1ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("_CreateStep1", model);
            }

            var phongs = await GetAvailableRoomsAsync(null, null, null);
            var step2 = new ThuePhongStep2ViewModel
            {
                TenKhachHang = model.TenKhachHang,
                DienThoai = model.DienThoai,
                DiaChi = model.DiaChi,
                Cccd = model.Cccd,
                NgayNhan = DateTime.Today,
                NgayTra = null,
                Phongs = phongs
            };

            return PartialView("_CreateStep2", step2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep2(ThuePhongStep2ViewModel model)
        {
            if (model.SelectedPhongs == null || !model.SelectedPhongs.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui long chon it nhat mot phong.");
            }

            if (!model.NgayNhan.HasValue)
            {
                ModelState.AddModelError(nameof(model.NgayNhan), "Vui long chon ngay nhan phong.");
            }

            if (model.NgayTra.HasValue && model.NgayNhan.HasValue && model.NgayTra.Value.Date < model.NgayNhan.Value.Date)
            {
                ModelState.AddModelError(nameof(model.NgayTra), "Ngay tra phong khong duoc nho hon ngay nhan phong.");
            }

            if (!ModelState.IsValid)
            {
                model.Phongs = await GetAvailableRoomsAsync(null, null, null);
                Response.StatusCode = 400;
                return PartialView("_CreateStep2", model);
            }

            var phongsChon = new List<PhongChonViewModel>();
            foreach (var maPhong in model.SelectedPhongs)
            {
                var phong = await _context.Phongs
                    .Include(p => p.MaLoaiPhongNavigation)
                    .FirstOrDefaultAsync(p => p.MaPhong == maPhong);

                if (phong != null)
                {
                    var gia = await GetCurrentPriceAsync(phong.MaLoaiPhong);
                    phongsChon.Add(new PhongChonViewModel
                    {
                        MaPhong = phong.MaPhong,
                        SoPhong = phong.SoPhong,
                        TenLoaiPhong = phong.MaLoaiPhongNavigation?.TenLoaiPhong,
                        GiaThue = gia?.Gia ?? 0
                    });
                }
            }

            var confirm = new ThuePhongConfirmViewModel
            {
                Cccd = model.Cccd,
                TenKhachHang = model.TenKhachHang,
                DienThoai = model.DienThoai,
                DiaChi = model.DiaChi,
                NgayNhan = model.NgayNhan ?? DateTime.Today,
                NgayTra = model.NgayTra,
                PhongsChon = phongsChon,
                TongTien = phongsChon.Sum(p => p.GiaThue ?? 0)
            };

            return PartialView("_CreateConfirm", confirm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFinal(ThuePhongConfirmViewModel model)
        {
            if (model.PhongsChon == null || !model.PhongsChon.Any())
            {
                return Json(new { success = false, message = "Khong co phong nao duoc chon." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var khachHang = await ResolveCustomerAsync(model.Cccd, model.TenKhachHang, model.DienThoai, model.DiaChi);
                var existingRentalIds = await _context.ThuePhongs
                    .Select(tp => tp.MaThuePhong)
                    .ToListAsync();
                var nextRentalNumber = existingRentalIds
                    .Select(id =>
                    {
                        var digits = new string((id ?? string.Empty).Skip(2).Where(char.IsDigit).ToArray());
                        return int.TryParse(digits, out var number) ? number : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                foreach (var phongChon in model.PhongsChon)
                {
                    var maThuePhong = $"TP{nextRentalNumber:D3}";
                    nextRentalNumber++;
                    var gia = await GetCurrentPriceFromMaLoaiPhongAsync(phongChon.MaPhong);

                    var thuePhong = new ThuePhong
                    {
                    MaThuePhong = maThuePhong,
                    MaKhachHang = khachHang.MaKhachHang,
                    TrangThai = "Dang thue",
                    NgayNhan = model.NgayNhan,
                    NgayTra = model.NgayTra
                };

                    var chiTiet = new CtthuePhong
                    {
                        MaThuePhong = maThuePhong,
                        MaPhong = phongChon.MaPhong,
                        GiaThueTaiThoiDiem = gia
                    };

                    var phong = await _context.Phongs.FindAsync(phongChon.MaPhong);
                    if (phong != null)
                    {
                        phong.TrangThai = "Đang sử dụng";
                    }

                    _context.ThuePhongs.Add(thuePhong);
                    _context.CtthuePhongs.Add(chiTiet);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Nguoi dung {User} tao phieu thue cho {Count} phong, khach hang {TenKhachHang}",
                    User.Identity?.Name, model.PhongsChon.Count, model.TenKhachHang);

                return Json(new { success = true, message = $"Da tao phieu thue cho {model.PhongsChon.Count} phong thanh cong." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Loi khi tao phieu thue");
                return Json(new { success = false, message = "Khong the tao phieu thue. Vui long thu lai." });
            }
        }

        private async Task<KhachHang> ResolveCustomerAsync(string? cccd, string tenKhachHang, string? dienThoai, string? diaChi)
        {
            KhachHang? khachHang = null;

            if (!string.IsNullOrWhiteSpace(cccd))
            {
                khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Cccd == cccd);
            }

            if (khachHang == null)
            {
                var maKhachHang = await GenerateNextCodeAsync(_context.KhachHangs.Select(k => k.MaKhachHang), "KH");
                khachHang = new KhachHang { MaKhachHang = maKhachHang };
                _context.KhachHangs.Add(khachHang);
            }

            khachHang.TenKhachHang = tenKhachHang.Trim();
            khachHang.DienThoai = dienThoai?.Trim();
            khachHang.DiaChi = diaChi?.Trim();
            khachHang.Cccd = string.IsNullOrWhiteSpace(cccd) ? null : cccd.Trim();

            await _context.SaveChangesAsync();
            return khachHang;
        }

        private async Task<double?> GetCurrentPriceFromMaLoaiPhongAsync(string maPhong)
        {
            var phong = await _context.Phongs.FindAsync(maPhong);
            if (phong == null) return null;
            var gia = await GetCurrentPriceAsync(phong.MaLoaiPhong);
            return gia?.Gia;
        }

        [HttpGet]
        public async Task<IActionResult> TraPhong(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("TraPhong called with empty id");
                    return BadRequest("ID is required");
                }

                var thuePhong = await _context.ThuePhongs
                    .Include(tp => tp.MaKhachHangNavigation)
                    .Include(tp => tp.CtthuePhongs)
                        .ThenInclude(ct => ct.MaPhongNavigation)
                            .ThenInclude(p => p.MaLoaiPhongNavigation)
                    .FirstOrDefaultAsync(tp => tp.MaThuePhong == id);

                if (thuePhong == null)
                {
                    _logger.LogWarning($"TraPhong not found with id: {id}");
                    return NotFound();
                }

                var model = new TraPhongViewModel
                {
                    MaThuePhong = thuePhong.MaThuePhong,
                    MaKhachHang = thuePhong.MaKhachHang,
                    TenKhachHang = thuePhong.MaKhachHangNavigation?.TenKhachHang,
                    SoPhong = thuePhong.CtthuePhongs.FirstOrDefault()?.MaPhongNavigation?.SoPhong,
                    NgayNhan = thuePhong.NgayNhan,
                    NgayTra = DateTime.Today
                };

                // Fetch all rentals for customer, then filter on client side
                // because EF Core cannot translate IsReturnedRentalStatus method to SQL
                var allRentals = await _context.ThuePhongs
                    .Include(tp => tp.CtthuePhongs)
                        .ThenInclude(ct => ct.MaPhongNavigation)
                            .ThenInclude(p => p.MaLoaiPhongNavigation)
                    .Where(tp => tp.MaKhachHang == thuePhong.MaKhachHang)
                    .OrderBy(tp => tp.MaThuePhong)
                    .ToListAsync();
                var activeRentals = allRentals
                    .Where(tp => !IsReturnedRentalStatus(tp.TrangThai))
                    .ToList();

                model.SelectedRentalIds = new List<string> { thuePhong.MaThuePhong };
                model.RentalOptions = activeRentals.Select(tp =>
                {
                    var chiTiet = tp.CtthuePhongs.FirstOrDefault();
                    return new TraPhongRentalItemViewModel
                    {
                        MaThuePhong = tp.MaThuePhong,
                        SoPhong = chiTiet?.MaPhongNavigation?.SoPhong ?? chiTiet?.MaPhong,
                        TenLoaiPhong = chiTiet?.MaPhongNavigation?.MaLoaiPhongNavigation?.TenLoaiPhong,
                        NgayNhan = tp.NgayNhan,
                        GiaThue = chiTiet?.GiaThueTaiThoiDiem,
                        IsSelected = tp.MaThuePhong == thuePhong.MaThuePhong
                    };
                }).ToList();

                return PartialView("_TraPhongModal", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in TraPhong GET action with id: {id}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraPhong(TraPhongViewModel model)
        {
            model.SelectedRentalIds = model.SelectedRentalIds?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList() ?? new List<string>();

            if (!model.SelectedRentalIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui long chon it nhat mot phieu thue de tra phong.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateTraPhongOptionsAsync(model);
                Response.StatusCode = 400;
                return PartialView("_TraPhongModal", model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var selectedRentals = await _context.ThuePhongs
                    .Include(tp => tp.CtthuePhongs)
                        .ThenInclude(ct => ct.MaPhongNavigation)
                            .ThenInclude(p => p.MaLoaiPhongNavigation)
                    .Where(tp => model.SelectedRentalIds.Contains(tp.MaThuePhong))
                    .ToListAsync();

                if (!selectedRentals.Any())
                {
                    return NotFound();
                }

                var customerIds = selectedRentals
                    .Select(tp => tp.MaKhachHang)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                if (customerIds.Count > 1)
                {
                    ModelState.AddModelError(string.Empty, "Chi duoc gop cac phieu thue cua cung mot khach hang.");
                    await PopulateTraPhongOptionsAsync(model);
                    Response.StatusCode = 400;
                    return PartialView("_TraPhongModal", model);
                }

                var alreadyInvoicedIds = await GetAlreadyInvoicedRentalIdsAsync(model.SelectedRentalIds);
                if (alreadyInvoicedIds.Any())
                {
                    ModelState.AddModelError(string.Empty, $"Cac phieu thue sau da co hoa don: {string.Join(", ", alreadyInvoicedIds)}.");
                    await PopulateTraPhongOptionsAsync(model);
                    Response.StatusCode = 400;
                    return PartialView("_TraPhongModal", model);
                }

                foreach (var thuePhong in selectedRentals)
                {
                    thuePhong.NgayTra = model.NgayTra;
                    thuePhong.TrangThai = "Da tra";

                    foreach (var ct in thuePhong.CtthuePhongs)
                    {
                        var phong = await _context.Phongs.FindAsync(ct.MaPhong);
                        if (phong != null)
                        {
                            phong.TrangThai = "Trống";
                        }
                    }
                }

                await CreateCombinedInvoiceForReturnedRentalsAsync(selectedRentals);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Nguoi dung {User} tra phong cho {Count} phieu thue cua khach {MaKhachHang}, NgayTra: {NgayTra}",
                    User.Identity?.Name, selectedRentals.Count, customerIds.FirstOrDefault(), model.NgayTra);

                TempData["Success"] = $"Da tra phong thanh cong.";
                return Json(new { success = true, message = "Da tra phong va tao hoa don thanh cong." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Loi khi tra phong {MaThuePhong}", model.MaThuePhong);
                ModelState.AddModelError(string.Empty, "Khong the tra phong. Vui long thu lai.");
                await PopulateTraPhongOptionsAsync(model);
                Response.StatusCode = 500;
                return PartialView("_TraPhongModal", model);
            }
        }

        private async Task<List<PhongTrongItemViewModel>> GetAvailableRoomsAsync(string? maPhong, string? soPhong, string? maLoaiPhong)
        {
            var query = _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .Where(p => p.TrangThai != null && TrangThaiPhongTrongVariants.Contains(p.TrangThai));

            if (!string.IsNullOrWhiteSpace(maPhong))
            {
                query = query.Where(p => p.MaPhong.Contains(maPhong));
            }

            if (!string.IsNullOrWhiteSpace(soPhong))
            {
                query = query.Where(p => p.SoPhong != null && p.SoPhong.Contains(soPhong));
            }

            if (!string.IsNullOrWhiteSpace(maLoaiPhong))
            {
                query = query.Where(p => p.MaLoaiPhong == maLoaiPhong);
            }

            var phongs = await query.OrderBy(p => p.SoPhong).ToListAsync();
            var roomTypeIds = phongs.Select(p => p.MaLoaiPhong).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            var today = DateTime.Today;

            var priceMap = await _context.GiaPhongs
                .Where(g => g.MaLoaiPhong != null
                    && roomTypeIds.Contains(g.MaLoaiPhong)
                    && g.NgayBatDau <= today
                    && (g.NgayKetThuc == null || g.NgayKetThuc >= today))
                .OrderByDescending(g => g.NgayBatDau)
                .ToListAsync();

            return phongs.Select(p => new PhongTrongItemViewModel
            {
                MaPhong = p.MaPhong,
                SoPhong = p.SoPhong,
                MaLoaiPhong = p.MaLoaiPhong,
                TenLoaiPhong = p.MaLoaiPhongNavigation?.TenLoaiPhong,
                SoNguoiToiDa = p.MaLoaiPhongNavigation?.SoNguoiToiDa,
                GiaHienTai = priceMap.FirstOrDefault(g => g.MaLoaiPhong == p.MaLoaiPhong)?.Gia
            }).ToList();
        }

        private async Task<ThuePhongOfflineViewModel?> BuildOfflineRentalViewModelAsync(string maPhong)
        {
            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong && p.TrangThai != null && TrangThaiPhongTrongVariants.Contains(p.TrangThai));

            if (phong == null)
            {
                return null;
            }

            var giaHienTai = await GetCurrentPriceAsync(phong.MaLoaiPhong);

            return new ThuePhongOfflineViewModel
            {
                MaPhong = phong.MaPhong,
                SoPhong = phong.SoPhong,
                TenLoaiPhong = phong.MaLoaiPhongNavigation?.TenLoaiPhong,
                GiaHienTai = giaHienTai?.Gia,
                NgayNhan = DateTime.Today,
                NgayTra = null
            };
        }

        private async Task PopulateOfflineRentalDisplayDataAsync(ThuePhongOfflineViewModel model)
        {
            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(p => p.MaPhong == model.MaPhong);

            if (phong == null)
            {
                return;
            }

            model.SoPhong = phong.SoPhong;
            model.TenLoaiPhong = phong.MaLoaiPhongNavigation?.TenLoaiPhong;
            model.GiaHienTai = (await GetCurrentPriceAsync(phong.MaLoaiPhong))?.Gia;
        }

        private async Task ValidateOfflineRentalAsync(ThuePhongOfflineViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MaPhong))
            {
                ModelState.AddModelError(nameof(model.MaPhong), "Phong khong hop le.");
                return;
            }

            if (!model.NgayNhan.HasValue)
            {
                ModelState.AddModelError(nameof(model.NgayNhan), "Vui long chon ngay nhan phong.");
            }

            if (model.NgayTra.HasValue && model.NgayNhan.HasValue && model.NgayTra.Value.Date < model.NgayNhan.Value.Date)
            {
                ModelState.AddModelError(nameof(model.NgayTra), "Ngay tra phong phai lon hon hoac bang ngay nhan phong.");
            }

            var phong = await _context.Phongs.FirstOrDefaultAsync(p => p.MaPhong == model.MaPhong);
            if (phong == null)
            {
                ModelState.AddModelError(string.Empty, "Phong khong ton tai.");
                return;
            }

            if (!string.Equals(NormalizeTrangThai(phong.TrangThai), "Trống", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Phong nay khong con trong de cho thue.");
            }

            var hasActiveRental = await _context.CtthuePhongs
                .Include(ct => ct.MaThuePhongNavigation)
                .AnyAsync(ct => ct.MaPhong == model.MaPhong && ct.MaThuePhongNavigation.NgayTra == null);

            if (hasActiveRental)
            {
                ModelState.AddModelError(string.Empty, "Phong nay dang co phieu thue chua ket thuc.");
            }

            if (await GetCurrentPriceAsync(phong.MaLoaiPhong) == null)
            {
                ModelState.AddModelError(string.Empty, "Loai phong nay chua co gia hien tai.");
            }
        }

        private async Task<GiaPhong?> GetCurrentPriceAsync(string? maLoaiPhong)
        {
            if (string.IsNullOrWhiteSpace(maLoaiPhong))
            {
                return null;
            }

            var today = DateTime.Today;
            return await _context.GiaPhongs
                .Where(g => g.MaLoaiPhong == maLoaiPhong
                    && g.NgayBatDau <= today
                    && (g.NgayKetThuc == null || g.NgayKetThuc >= today))
                .OrderByDescending(g => g.NgayBatDau)
                .FirstOrDefaultAsync();
        }

        private async Task<KhachHang> ResolveCustomerAsync(ThuePhongOfflineViewModel model)
        {
            KhachHang? khachHang = null;

            if (!string.IsNullOrWhiteSpace(model.Cccd))
            {
                khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Cccd == model.Cccd);
            }

            if (khachHang == null)
            {
                var maKhachHang = await GenerateNextCodeAsync(_context.KhachHangs.Select(k => k.MaKhachHang), "KH");
                khachHang = new KhachHang
                {
                    MaKhachHang = maKhachHang
                };
                _context.KhachHangs.Add(khachHang);
            }

            khachHang.TenKhachHang = model.TenKhachHang?.Trim();
            khachHang.DienThoai = model.DienThoai?.Trim();
            khachHang.DiaChi = model.DiaChi?.Trim();
            khachHang.Cccd = string.IsNullOrWhiteSpace(model.Cccd) ? null : model.Cccd.Trim();

            await _context.SaveChangesAsync();
            return khachHang;
        }

        private async Task<string> GenerateNextCodeAsync(IQueryable<string> source, string prefix)
        {
            var ids = await source.Where(id => id != null && id.StartsWith(prefix)).ToListAsync();
            var max = ids.Select(id =>
            {
                var digits = new string(id.Skip(prefix.Length).Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out var number) ? number : 0;
            }).DefaultIfEmpty(0).Max();

            return $"{prefix}{max + 1:D3}";
        }

        private async Task CreateCombinedInvoiceForReturnedRentalsAsync(List<ThuePhong> rentals)
        {
            if (!rentals.Any())
            {
                return;
            }

            var maHoaDon = await GenerateNextCodeAsync(_context.HoaDons.Select(hd => hd.MaHoaDon), "HD");
            var chiTietHoaDon = new List<CthoaDon>();
            var existingDetailIds = await _context.CthoaDons.Select(x => x.MaCthd).ToListAsync();
            var nextDetailNumber = existingDetailIds
                .Select(id =>
                {
                    var digits = new string((id ?? string.Empty).Skip(4).Where(char.IsDigit).ToArray());
                    return int.TryParse(digits, out var number) ? number : 0;
                })
                .DefaultIfEmpty(0)
                .Max() + 1;
            double tongTien = 0;

            foreach (var rental in rentals.OrderBy(tp => tp.MaThuePhong))
            {
                var ngayNhan = rental.NgayNhan?.Date ?? DateTime.Today;
                var ngayTra = rental.NgayTra?.Date ?? DateTime.Today;
                var soNgay = Math.Max(1, (ngayTra - ngayNhan).Days);
                var roomDescriptions = rental.CtthuePhongs.Select(ct =>
                {
                    var soPhong = ct.MaPhongNavigation?.SoPhong ?? ct.MaPhong;
                    var tenLoaiPhong = ct.MaPhongNavigation?.MaLoaiPhongNavigation?.TenLoaiPhong ?? "Phong";
                    return $"{tenLoaiPhong} {soPhong}";
                }).ToList();
                var lineTotal = rental.CtthuePhongs.Sum(ct => (ct.GiaThueTaiThoiDiem ?? 0) * soNgay);
                var maCthd = $"CTHD{nextDetailNumber:D3}";
                nextDetailNumber++;

                chiTietHoaDon.Add(new CthoaDon
                {
                    MaCthd = maCthd,
                    MaHoaDon = maHoaDon,
                    MaThuePhong = rental.MaThuePhong,
                    NoiDung = $"[{rental.MaThuePhong}] {string.Join(", ", roomDescriptions)} - {soNgay} ngay",
                    SoTien = lineTotal
                });

                tongTien += lineTotal;
            }

            var hoaDon = new HoaDon
            {
                MaHoaDon = maHoaDon,
                MaNhanVien = null,
                NgayLap = DateTime.Now,
                TongTien = tongTien
            };

            _context.HoaDons.Add(hoaDon);
            _context.CthoaDons.AddRange(chiTietHoaDon);
        }

        private async Task PopulateTraPhongOptionsAsync(TraPhongViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.MaThuePhong))
                {
                    _logger.LogWarning("PopulateTraPhongOptionsAsync called with empty MaThuePhong");
                    return;
                }

                var anchorRental = await _context.ThuePhongs
                    .Include(tp => tp.MaKhachHangNavigation)
                    .Include(tp => tp.CtthuePhongs)
                        .ThenInclude(ct => ct.MaPhongNavigation)
                            .ThenInclude(p => p.MaLoaiPhongNavigation)
                    .FirstOrDefaultAsync(tp => tp.MaThuePhong == model.MaThuePhong);

                if (anchorRental == null)
                {
                    _logger.LogWarning($"PopulateTraPhongOptionsAsync: Rental not found with MaThuePhong: {model.MaThuePhong}");
                    return;
                }

                model.MaKhachHang = anchorRental.MaKhachHang;
                model.TenKhachHang = anchorRental.MaKhachHangNavigation?.TenKhachHang;
                model.SoPhong = anchorRental.CtthuePhongs.FirstOrDefault()?.MaPhongNavigation?.SoPhong;
                model.NgayNhan = anchorRental.NgayNhan;

                // Fetch all rentals for customer, then filter on client side
                // because EF Core cannot translate IsReturnedRentalStatus method to SQL
                var allRentals = await _context.ThuePhongs
                    .Include(tp => tp.CtthuePhongs)
                        .ThenInclude(ct => ct.MaPhongNavigation)
                            .ThenInclude(p => p.MaLoaiPhongNavigation)
                    .Where(tp => tp.MaKhachHang == anchorRental.MaKhachHang)
                    .OrderBy(tp => tp.MaThuePhong)
                    .ToListAsync();
                var activeRentals = allRentals
                    .Where(tp => !IsReturnedRentalStatus(tp.TrangThai))
                    .ToList();

                model.RentalOptions = activeRentals.Select(tp =>
                {
                    var chiTiet = tp.CtthuePhongs.FirstOrDefault();
                    return new TraPhongRentalItemViewModel
                    {
                        MaThuePhong = tp.MaThuePhong,
                        SoPhong = chiTiet?.MaPhongNavigation?.SoPhong ?? chiTiet?.MaPhong,
                        TenLoaiPhong = chiTiet?.MaPhongNavigation?.MaLoaiPhongNavigation?.TenLoaiPhong,
                        NgayNhan = tp.NgayNhan,
                        GiaThue = chiTiet?.GiaThueTaiThoiDiem,
                        IsSelected = model.SelectedRentalIds.Contains(tp.MaThuePhong)
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in PopulateTraPhongOptionsAsync for MaThuePhong: {model?.MaThuePhong}");
                // Don't rethrow - allow the operation to continue with empty options
            }
        }

        private async Task<List<string>> GetAlreadyInvoicedRentalIdsAsync(IEnumerable<string> rentalIds)
        {
            var ids = rentalIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!ids.Any())
            {
                return new List<string>();
            }

            var directIds = await _context.CthoaDons
                .Where(ct => ct.MaThuePhong != null && ids.Contains(ct.MaThuePhong))
                .Select(ct => ct.MaThuePhong!)
                .ToListAsync();

            var taggedDetails = await _context.CthoaDons
                .Where(ct => ct.NoiDung != null)
                .Select(ct => ct.NoiDung!)
                .ToListAsync();

            var detailIds = ids
                .Where(id => taggedDetails.Any(detail => detail.StartsWith($"[{id}]")))
                .ToList();

            return directIds.Concat(detailIds).Distinct().ToList();
        }

        private static bool IsReturnedRentalStatus(string? trangThai)
        {
            var normalized = (trangThai ?? string.Empty).Trim();
            return normalized == "Da tra" || normalized == "Đã trả";
        }

        private static string NormalizeTrangThai(string? trangThai)
        {
            var normalized = (trangThai ?? string.Empty).Trim();

            return normalized switch
            {
                "Tr?ng" or "Trá»‘ng" or "Trống" => "Trống",
                "Đang sử dụng" or "CÃ³ khÃ¡ch" or "Có khách" => "Đang sử dụng",
                "B?o trì" or "Báº£o trÃ¬" or "Bảo trì" => "Bảo trì",
                "Ðã d?t" or "ÄÃ£ Ä‘áº·t" or "Đã đặt" => "Đã đặt",
                _ => normalized
            };
        }
    }
}
