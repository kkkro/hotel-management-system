using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class GiaPhong
    {
        public string MaGia { get; set; } = null!;
        public string? MaLoaiPhong { get; set; }
        public double? Gia { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }

        public virtual LoaiPhong? MaLoaiPhongNavigation { get; set; }
    }
}
