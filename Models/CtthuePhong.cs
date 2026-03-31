using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class CtthuePhong
    {
        public string MaThuePhong { get; set; } = null!;
        public string MaPhong { get; set; } = null!;
        public double? GiaThueTaiThoiDiem { get; set; }

        public virtual Phong MaPhongNavigation { get; set; } = null!;
        public virtual ThuePhong MaThuePhongNavigation { get; set; } = null!;
    }
}
