using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebKhachSan.Models;
using WebKhachSan.ViewModels;

namespace WebKhachSan.Controllers
{
    [Authorize]
    public class DatPhongController : Controller
    {
        private readonly QuanLyKhachSanContext _context;
        private readonly ILogger<DatPhongController> _logger;
        private static readonly string[] TrangThaiPhongTrongVariants = { "Trống", "Tr?ng", "Trá»‘ng" };
        private static readonly string[] TrangThaiDatPhongChoNhanPhongVariants =
        {
            "Chưa nhận phòng",
            "Chờ xác nhận",
            "Đã xác nhận"
        };

        public DatPhongController(QuanLyKhachSanContext context, ILogger<DatPhongController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Người dùng {0} xem danh sách đặt phòng", User.Identity?.Name);

            var datPhongs = await _context.DatPhongs
                .Include(dp => dp.MaKhachHangNavigation)
                .Include(dp => dp.CtdatPhongs)
                    .ThenInclude(ct => ct.MaLoaiPhongNavigation)
                .OrderByDescending(dp => dp.NgayDat)
                .ToListAsync();

            return View(datPhongs);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var datPhong = await _context.DatPhongs
                .Include(dp => dp.MaKhachHangNavigation)
                .Include(dp => dp.CtdatPhongs)
                    .ThenInclude(ct => ct.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(m => m.MaDatPhong == id);

            if (datPhong == null)
            {
                return NotFound();
            }

            return View(datPhong);
        }

        public async Task<IActionResult> CreateModern()
        {
            _logger.LogInformation("Người dùng {0} mở form đặt phòng", User.Identity?.Name);

            var loaiPhongs = await _context.LoaiPhongs
                .OrderBy(lp => lp.TenLoaiPhong)
                .Select(lp => new { lp.MaLoaiPhong, lp.TenLoaiPhong })
                .ToListAsync();

            ViewBag.LoaiPhong = loaiPhongs
                .Select(lp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = lp.MaLoaiPhong,
                    Text = lp.TenLoaiPhong
                })
                .ToList();

            return View("CreateModern");
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Người dùng {0} mở form đặt phòng cơ bản", User.Identity?.Name);

            var loaiPhongs = await _context.LoaiPhongs
                .OrderBy(lp => lp.TenLoaiPhong)
                .Select(lp => new { lp.MaLoaiPhong, lp.TenLoaiPhong })
                .ToListAsync();

            ViewBag.LoaiPhong = loaiPhongs
                .Select(lp => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = lp.MaLoaiPhong,
                    Text = lp.TenLoaiPhong
                })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDatPhong,MaKhachHang,NgayDat,NgayNhanDuKien,NgayTraDuKien,MaLoaiPhong,SoLuong,GhiChu")] DatPhong datPhong, string MaLoaiPhong, int SoLuong, string GhiChu)
        {
            try
            {
                var khachHang = await _context.KhachHangs.FindAsync(datPhong.MaKhachHang);
                if (khachHang == null)
                {
                    _logger.LogWarning("Khách hàng {0} không tồn tại", datPhong.MaKhachHang);
                    ViewBag.ErrorMessage = "Khách hàng không tồn tại";
                    return RedirectToAction(nameof(CreateModern));
                }

                var loaiPhong = await _context.LoaiPhongs.FindAsync(MaLoaiPhong);
                if (loaiPhong == null)
                {
                    _logger.LogWarning("Loại phòng {0} không tồn tại", MaLoaiPhong);
                    ViewBag.ErrorMessage = "Loại phòng không tồn tại";
                    return RedirectToAction(nameof(CreateModern));
                }

                datPhong.MaDatPhong = GenerateMaDatPhong();
                datPhong.NgayDat = DateTime.Now;
                datPhong.TrangThai = "Chưa nhận phòng";

                _context.Add(datPhong);
                await _context.SaveChangesAsync();

                var giaDatPhong = await _context.GiaPhongs
                    .Where(gp => gp.MaLoaiPhong == MaLoaiPhong && gp.NgayBatDau <= DateTime.Now && gp.NgayKetThuc >= DateTime.Now)
                    .FirstOrDefaultAsync();

                double giaTinh = giaDatPhong?.Gia ?? 0.0;
                int soNgay = (int)(((datPhong.NgayTraDuKien ?? DateTime.Now) - (datPhong.NgayNhanDuKien ?? DateTime.Now)).TotalDays);

                for (int i = 0; i < SoLuong; i++)
                {
                    _context.Add(new CtdatPhong
                    {
                        MaDatPhong = datPhong.MaDatPhong,
                        MaLoaiPhong = MaLoaiPhong,
                        SoLuong = 1,
                        GiaTamTinh = giaTinh * soNgay
                    });
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Người dùng {0} tạo đơn đặt phòng: {1} cho khách {2}",
                    User.Identity?.Name, datPhong.MaDatPhong, khachHang.TenKhachHang);

                return RedirectToAction(nameof(Details), new { id = datPhong.MaDatPhong });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn đặt phòng");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tạo đơn đặt phòng";
                return RedirectToAction(nameof(CreateModern));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateFromPublic([FromBody] PublicBookingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MaKhachHang) || request.SelectedRooms == null || !request.SelectedRooms.Any())
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ" });
                }

                DateTime ngayNhan = DateTime.Parse(request.NgayNhanDuKien);
                DateTime ngayTra = DateTime.Parse(request.NgayTraDuKien);
                if (ngayTra <= ngayNhan)
                {
                    return Json(new { success = false, message = "Ngày trả phòng phải sau ngày nhận phòng" });
                }

                var selectedRoomIds = request.SelectedRooms
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                if (!selectedRoomIds.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một phòng" });
                }

                var bookedRoomIds = await GetBookedRoomIdsAsync(ngayNhan, ngayTra);

                var selectedRooms = await _context.Phongs
                    .Include(p => p.MaLoaiPhongNavigation)
                    .Where(p => selectedRoomIds.Contains(p.MaPhong) && !bookedRoomIds.Contains(p.MaPhong))
                    .ToListAsync();

                if (selectedRooms.Count != selectedRoomIds.Count)
                {
                    return Json(new { success = false, message = "Một hoặc nhiều phòng đã không còn trống" });
                }

                var datPhong = new DatPhong
                {
                    MaDatPhong = GenerateMaDatPhong(),
                    MaKhachHang = request.MaKhachHang,
                    NgayDat = DateTime.Now,
                    NgayNhanDuKien = ngayNhan,
                    NgayTraDuKien = ngayTra,
                    TrangThai = "Chưa nhận phòng"
                };

                _context.Add(datPhong);

                int soNgay = Math.Max(1, (ngayTra.Date - ngayNhan.Date).Days);
                var roomGroups = selectedRooms
                    .GroupBy(p => p.MaLoaiPhong)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .ToList();

                foreach (var roomGroup in roomGroups)
                {
                    string maLoaiPhong = roomGroup.Key!;
                    double gia = await GetCurrentPriceValueAsync(maLoaiPhong);

                    _context.Add(new CtdatPhong
                    {
                        MaDatPhong = datPhong.MaDatPhong,
                        MaLoaiPhong = maLoaiPhong,
                        SoLuong = roomGroup.Count(),
                        GiaTamTinh = gia * soNgay * roomGroup.Count()
                    });
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Khách hàng công khai {0} tạo đơn đặt phòng: {1}",
                    request.MaKhachHang, datPhong.MaDatPhong);

                return Json(new
                {
                    success = true,
                    maDatPhong = datPhong.MaDatPhong,
                    soPhongDaChon = selectedRooms.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn đặt phòng từ công khai");
                return Json(new { success = false, message = "Lỗi máy chủ" });
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var datPhong = await _context.DatPhongs.FindAsync(id);
            if (datPhong == null)
            {
                return NotFound();
            }

            return View(datPhong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaDatPhong,MaKhachHang,NgayDat,NgayNhanDuKien,NgayTraDuKien,TrangThai")] DatPhong datPhong)
        {
            if (id != datPhong.MaDatPhong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(datPhong);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Người dùng {0} cập nhật đơn đặt phòng: {1}", User.Identity?.Name, datPhong.MaDatPhong);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatPhongExists(datPhong.MaDatPhong))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(datPhong);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var datPhong = await _context.DatPhongs
                .Include(dp => dp.MaKhachHangNavigation)
                .Include(dp => dp.CtdatPhongs)
                .FirstOrDefaultAsync(m => m.MaDatPhong == id);

            if (datPhong == null)
            {
                return NotFound();
            }

            return View(datPhong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var datPhong = await _context.DatPhongs.FindAsync(id);
            if (datPhong == null)
            {
                return NotFound();
            }

            if (datPhong.TrangThai == "Chờ xác nhận")
            {
                datPhong.TrangThai = "Chưa nhận phòng";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Người dùng {0} xác nhận đơn đặt phòng: {1}",
                    User.Identity?.Name, datPhong.MaDatPhong);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CheckIn(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Content("<div class='alert alert-danger mb-0'>Don dat phong khong hop le.</div>", "text/html");
            }

            var datPhong = await _context.DatPhongs
                .Include(dp => dp.MaKhachHangNavigation)
                .Include(dp => dp.CtdatPhongs)
                    .ThenInclude(ct => ct.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == id);

            if (datPhong == null)
            {
                return Content("<div class='alert alert-danger mb-0'>Khong tim thay don dat phong.</div>", "text/html");
            }

            if (!TrangThaiDatPhongChoNhanPhongVariants.Contains(datPhong.TrangThai ?? string.Empty))
            {
                return Content("<div class='alert alert-warning mb-0'>Chi don chua nhan phong moi co the checkin.</div>", "text/html");
            }

            var model = await BuildCheckInViewModelAsync(datPhong);
            return PartialView("_CheckInModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(DatPhongCheckInViewModel model)
        {
            var datPhong = await _context.DatPhongs
                .Include(dp => dp.MaKhachHangNavigation)
                .Include(dp => dp.CtdatPhongs)
                    .ThenInclude(ct => ct.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(dp => dp.MaDatPhong == model.MaDatPhong);

            if (datPhong == null)
            {
                Response.StatusCode = 404;
                return Content("<div class='alert alert-danger mb-0'>Khong tim thay don dat phong.</div>", "text/html");
            }

            if (!TrangThaiDatPhongChoNhanPhongVariants.Contains(datPhong.TrangThai ?? string.Empty))
            {
                Response.StatusCode = 400;
                return Content("<div class='alert alert-warning mb-0'>Don dat phong nay khong o trang thai co the checkin.</div>", "text/html");
            }

            var expectedGroups = datPhong.CtdatPhongs
                .GroupBy(ct => new { ct.MaLoaiPhong, TenLoaiPhong = ct.MaLoaiPhongNavigation != null ? ct.MaLoaiPhongNavigation.TenLoaiPhong : ct.MaLoaiPhong })
                .Select(g => new
                {
                    MaLoaiPhong = g.Key.MaLoaiPhong,
                    TenLoaiPhong = g.Key.TenLoaiPhong ?? g.Key.MaLoaiPhong ?? string.Empty,
                    SoLuong = g.Sum(x => x.SoLuong ?? 0)
                })
                .ToList();

            model = await BuildCheckInViewModelAsync(datPhong, model);

            foreach (var expected in expectedGroups)
            {
                var submitted = model.LoaiPhongs.FirstOrDefault(lp => lp.MaLoaiPhong == expected.MaLoaiPhong);
                var selectedCount = submitted?.SelectedPhongIds?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count() ?? 0;

                if (selectedCount != expected.SoLuong)
                {
                    ModelState.AddModelError(string.Empty, $"Loai phong '{expected.TenLoaiPhong}' can chon dung {expected.SoLuong} phong.");
                }
            }

            var allSelectedRoomIds = model.LoaiPhongs
                .SelectMany(lp => lp.SelectedPhongIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();

            if (allSelectedRoomIds.Count != allSelectedRoomIds.Distinct().Count())
            {
                ModelState.AddModelError(string.Empty, "Khong duoc chon trung mot phong cho nhieu loai.");
            }

            var selectedRooms = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .Where(p => allSelectedRoomIds.Contains(p.MaPhong))
                .ToListAsync();

            if (selectedRooms.Count != allSelectedRoomIds.Distinct().Count())
            {
                ModelState.AddModelError(string.Empty, "Mot hoac nhieu phong duoc chon khong ton tai.");
            }

            foreach (var room in selectedRooms)
            {
                if (room.TrangThai == null || !TrangThaiPhongTrongVariants.Contains(room.TrangThai))
                {
                    ModelState.AddModelError(string.Empty, $"Phong {room.SoPhong ?? room.MaPhong} khong con trong.");
                }
            }

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return PartialView("_CheckInModal", model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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

                foreach (var room in selectedRooms)
                {
                    var maThuePhong = $"TP{nextRentalNumber:D3}";
                    nextRentalNumber++;
                    var gia = await GetCurrentPriceValueAsync(room.MaLoaiPhong);

                    _context.ThuePhongs.Add(new ThuePhong
                    {
                        MaThuePhong = maThuePhong,
                        MaKhachHang = datPhong.MaKhachHang,
                        TrangThai = "Dang thue",
                        NgayNhan = datPhong.NgayNhanDuKien ?? DateTime.Today,
                        NgayTra = datPhong.NgayTraDuKien
                    });

                    _context.CtthuePhongs.Add(new CtthuePhong
                    {
                        MaThuePhong = maThuePhong,
                        MaPhong = room.MaPhong,
                        GiaThueTaiThoiDiem = gia
                    });

                    room.TrangThai = "Đang sử dụng";
                }

                datPhong.TrangThai = "Đã nhận phòng";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Nguoi dung {User} checkin don dat phong {MaDatPhong} thanh {Count} phieu thue",
                    User.Identity?.Name, datPhong.MaDatPhong, selectedRooms.Count);

                return Json(new { success = true, message = $"Da checkin thanh cong cho don {datPhong.MaDatPhong}." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Loi khi checkin don dat phong {MaDatPhong}", datPhong.MaDatPhong);
                Response.StatusCode = 500;
                return PartialView("_CheckInModal", model);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var datPhong = await _context.DatPhongs
                .Include(dp => dp.CtdatPhongs)
                .FirstOrDefaultAsync(m => m.MaDatPhong == id);

            if (datPhong != null)
            {
                _context.CtdatPhongs.RemoveRange(datPhong.CtdatPhongs);
                _context.DatPhongs.Remove(datPhong);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Người dùng {0} xóa đơn đặt phòng: {1}",
                    User.Identity?.Name, datPhong.MaDatPhong);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableRooms([FromBody] RoomSearchRequest request)
        {
            try
            {
                DateTime ngayNhan = request.NgayNhan;
                DateTime ngayTra = request.NgayTra;

                if (ngayNhan == default || ngayTra == default)
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ" });
                }

                if (ngayTra <= ngayNhan)
                {
                    return Json(new { success = false, message = "Ngày trả phòng phải sau ngày nhận phòng" });
                }

                var bookedRoomIds = await GetBookedRoomIdsAsync(ngayNhan, ngayTra);

                var availableRooms = await _context.Phongs
                    .Include(p => p.MaLoaiPhongNavigation)
                    .Where(p => !bookedRoomIds.Contains(p.MaPhong))
                    .Where(p => p.TrangThai == null || TrangThaiPhongTrongVariants.Contains(p.TrangThai))
                    .Select(p => new AvailableRoomDto
                    {
                        MaPhong = p.MaPhong,
                        SoPhong = p.SoPhong,
                        MaLoaiPhong = p.MaLoaiPhong,
                        TenLoaiPhong = p.MaLoaiPhongNavigation != null ? p.MaLoaiPhongNavigation.TenLoaiPhong : "Khác",
                        SoNguoiToiDa = p.MaLoaiPhongNavigation != null ? p.MaLoaiPhongNavigation.SoNguoiToiDa : null
                    })
                    .ToListAsync();

                int soNgay = Math.Max(1, (ngayTra.Date - ngayNhan.Date).Days);

                if (!string.IsNullOrWhiteSpace(request.MaLoaiPhong))
                {
                    var roomsByType = availableRooms
                        .Where(room => room.MaLoaiPhong == request.MaLoaiPhong)
                        .Select(room => new
                        {
                            maphong = room.MaPhong,
                            maPhong = room.MaPhong,
                            maLoaiPhong = room.MaLoaiPhong,
                            tenLoaiPhong = room.TenLoaiPhong
                        })
                        .ToList();

                    double gia = await GetCurrentPriceValueAsync(request.MaLoaiPhong);

                    return Json(new
                    {
                        success = roomsByType.Count > 0,
                        phongTrongs = roomsByType,
                        gia = gia,
                        soNgay = soNgay,
                        giaTinh = gia * soNgay,
                        message = roomsByType.Count > 0 ? $"Có {roomsByType.Count} phòng trống" : "Không có phòng trống"
                    });
                }

                var priceLookup = await _context.GiaPhongs
                    .Where(gp => gp.NgayBatDau <= DateTime.Now && gp.NgayKetThuc >= DateTime.Now)
                    .GroupBy(gp => gp.MaLoaiPhong)
                    .Select(g => new
                    {
                        MaLoaiPhong = g.Key,
                        Gia = g.OrderByDescending(x => x.NgayBatDau).Select(x => x.Gia).FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.MaLoaiPhong, x => x.Gia ?? 0.0);

                var groupedRooms = availableRooms
                    .GroupBy(room => new { room.MaLoaiPhong, room.TenLoaiPhong, room.SoNguoiToiDa })
                    .Select(group => new
                    {
                        maLoaiPhong = group.Key.MaLoaiPhong,
                        tenLoaiPhong = group.Key.TenLoaiPhong,
                        soNguoiToiDa = group.Key.SoNguoiToiDa,
                        soLuongPhongTrong = group.Count(),
                        gia = group.Key.MaLoaiPhong != null && priceLookup.ContainsKey(group.Key.MaLoaiPhong)
                            ? priceLookup[group.Key.MaLoaiPhong]
                            : 0.0,
                        phongTrongs = group
                            .OrderBy(room => room.SoPhong)
                            .Select(room => new
                            {
                                maPhong = room.MaPhong,
                                soPhong = room.SoPhong,
                                maLoaiPhong = room.MaLoaiPhong
                            })
                            .ToList()
                    })
                    .OrderBy(group => group.tenLoaiPhong)
                    .ToList();

                return Json(new
                {
                    success = groupedRooms.Count > 0,
                    nhomPhongTrongs = groupedRooms,
                    soNgay = soNgay,
                    tongSoPhongTrong = availableRooms.Count,
                    message = groupedRooms.Count > 0 ? $"Có {availableRooms.Count} phòng trống" : "Không có phòng trống"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm phòng trống");
                return Json(new { success = false, message = "Lỗi máy chủ" });
            }
        }

        private async Task<List<string>> GetBookedRoomIdsAsync(DateTime ngayNhan, DateTime ngayTra)
        {
            return await _context.ThuePhongs
                .Include(tp => tp.CtthuePhongs)
                .Where(tp => tp.NgayNhan < ngayTra && tp.NgayTra > ngayNhan)
                .SelectMany(tp => tp.CtthuePhongs)
                .Select(ct => ct.MaPhong)
                .Distinct()
                .ToListAsync();
        }

        private async Task<double> GetCurrentPriceValueAsync(string? maLoaiPhong)
        {
            if (string.IsNullOrWhiteSpace(maLoaiPhong))
            {
                return 0.0;
            }

            return await _context.GiaPhongs
                .Where(gp => gp.MaLoaiPhong == maLoaiPhong && gp.NgayBatDau <= DateTime.Now && gp.NgayKetThuc >= DateTime.Now)
                .OrderByDescending(gp => gp.NgayBatDau)
                .Select(gp => gp.Gia ?? 0.0)
                .FirstOrDefaultAsync();
        }

        private async Task<DatPhongCheckInViewModel> BuildCheckInViewModelAsync(DatPhong datPhong, DatPhongCheckInViewModel? postedModel = null)
        {
            var groupedBookedTypes = datPhong.CtdatPhongs
                .GroupBy(ct => new { ct.MaLoaiPhong, TenLoaiPhong = ct.MaLoaiPhongNavigation != null ? ct.MaLoaiPhongNavigation.TenLoaiPhong : ct.MaLoaiPhong })
                .Select(g => new
                {
                    MaLoaiPhong = g.Key.MaLoaiPhong ?? string.Empty,
                    TenLoaiPhong = g.Key.TenLoaiPhong ?? g.Key.MaLoaiPhong ?? string.Empty,
                    SoLuong = g.Sum(x => x.SoLuong ?? 0)
                })
                .ToList();

            var model = new DatPhongCheckInViewModel
            {
                MaDatPhong = datPhong.MaDatPhong,
                MaKhachHang = datPhong.MaKhachHang,
                TenKhachHang = datPhong.MaKhachHangNavigation?.TenKhachHang ?? string.Empty,
                DienThoai = datPhong.MaKhachHangNavigation?.DienThoai,
                NgayNhan = datPhong.NgayNhanDuKien,
                NgayTra = datPhong.NgayTraDuKien
            };

            foreach (var bookedType in groupedBookedTypes)
            {
                var postedType = postedModel?.LoaiPhongs.FirstOrDefault(x => x.MaLoaiPhong == bookedType.MaLoaiPhong);
                var availableRooms = await _context.Phongs
                    .Where(p => p.MaLoaiPhong == bookedType.MaLoaiPhong && p.TrangThai != null && TrangThaiPhongTrongVariants.Contains(p.TrangThai))
                    .OrderBy(p => p.SoPhong)
                    .Select(p => new PhongTrongItemViewModel
                    {
                        MaPhong = p.MaPhong,
                        SoPhong = p.SoPhong,
                        MaLoaiPhong = p.MaLoaiPhong,
                        TenLoaiPhong = bookedType.TenLoaiPhong,
                        GiaHienTai = null
                    })
                    .ToListAsync();

                model.LoaiPhongs.Add(new DatPhongCheckInLoaiPhongViewModel
                {
                    MaLoaiPhong = bookedType.MaLoaiPhong,
                    TenLoaiPhong = bookedType.TenLoaiPhong,
                    SoLuongCanChon = bookedType.SoLuong,
                    SelectedPhongIds = postedType?.SelectedPhongIds ?? new List<string>(),
                    PhongTrong = availableRooms
                });
            }

            return model;
        }

        private bool DatPhongExists(string id)
        {
            return _context.DatPhongs.Any(e => e.MaDatPhong == id);
        }

        private string GenerateMaDatPhong()
        {
            var lastId = _context.DatPhongs
                .OrderByDescending(dp => dp.MaDatPhong)
                .Select(dp => dp.MaDatPhong)
                .FirstOrDefault() ?? "DP000000";

            int number = int.Parse(lastId.Substring(2)) + 1;
            return "DP" + number.ToString("D6");
        }

        private sealed class AvailableRoomDto
        {
            public string MaPhong { get; set; } = null!;
            public string? SoPhong { get; set; }
            public string? MaLoaiPhong { get; set; }
            public string? TenLoaiPhong { get; set; }
            public int? SoNguoiToiDa { get; set; }
        }
    }

    public class RoomSearchRequest
    {
        public string? MaLoaiPhong { get; set; }
        public DateTime NgayNhan { get; set; }
        public DateTime NgayTra { get; set; }
    }

    public class PublicBookingRequest
    {
        public string MaKhachHang { get; set; } = string.Empty;
        public string NgayNhanDuKien { get; set; } = string.Empty;
        public string NgayTraDuKien { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public List<string> SelectedRooms { get; set; } = new();
    }
}
