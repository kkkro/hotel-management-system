using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class ThuePhong
    {
        public ThuePhong()
        {
            CtthuePhongs = new HashSet<CtthuePhong>();
            HoaDons = new HashSet<HoaDon>();
        }

        public string MaThuePhong { get; set; } = null!;
        public string? MaKhachHang { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayNhan { get; set; }
        public DateTime? NgayTra { get; set; }

        public virtual KhachHang? MaKhachHangNavigation { get; set; }
        public virtual ICollection<CtthuePhong> CtthuePhongs { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}
