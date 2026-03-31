using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class CthoaDon
    {
        public string MaCthd { get; set; } = null!;
        public string? MaHoaDon { get; set; }
        public string? NoiDung { get; set; }
        public double? SoTien { get; set; }

        public virtual HoaDon? MaHoaDonNavigation { get; set; }
    }
}
