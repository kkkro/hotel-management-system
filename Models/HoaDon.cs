using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class HoaDon
    {
        public HoaDon()
        {
            CthoaDons = new HashSet<CthoaDon>();
        }

        public string MaHoaDon { get; set; } = null!;
        public string? MaNhanVien { get; set; }
        public DateTime? NgayLap { get; set; }
        public double? TongTien { get; set; }

        public virtual NhanVien? MaNhanVienNavigation { get; set; }
        public virtual ICollection<CthoaDon> CthoaDons { get; set; }
    }
}
