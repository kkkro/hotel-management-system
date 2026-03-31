using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class DatPhong
    {
        public DatPhong()
        {
            CtdatPhongs = new HashSet<CtdatPhong>();
        }

        public string MaDatPhong { get; set; } = null!;
        public string? MaKhachHang { get; set; }
        public DateTime? NgayDat { get; set; }
        public DateTime? NgayNhanDuKien { get; set; }
        public DateTime? NgayTraDuKien { get; set; }
        public string? TrangThai { get; set; }

        public virtual KhachHang? MaKhachHangNavigation { get; set; }
        public virtual ICollection<CtdatPhong> CtdatPhongs { get; set; }
    }
}
