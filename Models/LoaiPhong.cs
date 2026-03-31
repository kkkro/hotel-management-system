using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class LoaiPhong
    {
        public LoaiPhong()
        {
            CtdatPhongs = new HashSet<CtdatPhong>();
            GiaPhongs = new HashSet<GiaPhong>();
            Phongs = new HashSet<Phong>();
        }

        public string MaLoaiPhong { get; set; } = null!;
        public string? TenLoaiPhong { get; set; }
        public string? MoTa { get; set; }
        public int? SoNguoiToiDa { get; set; }

        public virtual ICollection<CtdatPhong> CtdatPhongs { get; set; }
        public virtual ICollection<GiaPhong> GiaPhongs { get; set; }
        public virtual ICollection<Phong> Phongs { get; set; }
    }
}
