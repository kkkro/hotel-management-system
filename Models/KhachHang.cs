using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class KhachHang
    {
        public KhachHang()
        {
            DatPhongs = new HashSet<DatPhong>();
            ThuePhongs = new HashSet<ThuePhong>();
        }

        public string MaKhachHang { get; set; } = null!;
        public string? TenKhachHang { get; set; }
        public string? DienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string? Cccd { get; set; }
        public string? Email { get; set; }
        public virtual ICollection<DatPhong> DatPhongs { get; set; }
        public virtual ICollection<ThuePhong> ThuePhongs { get; set; }
    }
}
