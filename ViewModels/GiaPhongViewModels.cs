using System.ComponentModel.DataAnnotations;
using WebKhachSan.Models;

namespace WebKhachSan.ViewModels
{
    public class GiaPhongDetailsViewModel
    {
        public GiaPhong GiaPhong { get; set; } = null!;
        public List<Phong> PhongsApDung { get; set; } = new();
    }

    public class GiaPhongBulkCreateViewModel
    {
        [Required(ErrorMessage = "Gia phong khong duoc de trong.")]
        [Range(1000, double.MaxValue, ErrorMessage = "Gia phong phai lon hon 0.")]
        public double? Gia { get; set; }

        [Required(ErrorMessage = "Ngay bat dau khong duoc de trong.")]
        [DataType(DataType.Date)]
        public DateTime? NgayBatDau { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayKetThuc { get; set; }

        public List<string> SelectedLoaiPhongIds { get; set; } = new();
        public List<LoaiPhongBulkSelectionItemViewModel> LoaiPhongApDung { get; set; } = new();
    }

    public class LoaiPhongBulkSelectionItemViewModel
    {
        public string MaLoaiPhong { get; set; } = string.Empty;
        public string? TenLoaiPhong { get; set; }
        public int SoLuongPhong { get; set; }
        public double? GiaHienTai { get; set; }
    }
}
