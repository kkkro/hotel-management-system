using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class Phong
    {
        public Phong()
        {
            CtthuePhongs = new HashSet<CtthuePhong>();
        }

        public string MaPhong { get; set; } = null!;
        public string? SoPhong { get; set; }
        public string? TrangThai { get; set; }
        public string? MaLoaiPhong { get; set; }

        public virtual LoaiPhong? MaLoaiPhongNavigation { get; set; }
        public virtual ICollection<CtthuePhong> CtthuePhongs { get; set; }
    }
}
