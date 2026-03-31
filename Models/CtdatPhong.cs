using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class CtdatPhong
    {
        public string MaDatPhong { get; set; } = null!;
        public string MaLoaiPhong { get; set; } = null!;
        public int? SoLuong { get; set; }
        public double? GiaTamTinh { get; set; }

        public virtual DatPhong MaDatPhongNavigation { get; set; } = null!;
        public virtual LoaiPhong MaLoaiPhongNavigation { get; set; } = null!;
    }
}
