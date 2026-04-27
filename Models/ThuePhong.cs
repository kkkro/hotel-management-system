using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class ThuePhong
    {
        public ThuePhong()
        {
            CthoaDons = new HashSet<CthoaDon>();
            CtthuePhongs = new HashSet<CtthuePhong>();
        }

        public string MaThuePhong { get; set; } = null!;
        public string? MaKhachHang { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayNhan { get; set; }
        public DateTime? NgayTra { get; set; }

        public virtual KhachHang? MaKhachHangNavigation { get; set; }
        public virtual ICollection<CthoaDon> CthoaDons { get; set; }
        public virtual ICollection<CtthuePhong> CtthuePhongs { get; set; }
    }
}
