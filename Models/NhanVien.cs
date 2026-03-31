using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class NhanVien
    {
        public NhanVien()
        {
            HoaDons = new HashSet<HoaDon>();
        }

        public string MaNhanVien { get; set; } = null!;
        public string? TenNhanVien { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string? ChucVu { get; set; }
        public int? MaTaiKhoan { get; set; }

        public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}
