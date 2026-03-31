using System;
using System.Collections.Generic;

namespace WebKhachSan.Models
{
    public partial class TaiKhoan
    {
        public TaiKhoan()
        {
            NhanViens = new HashSet<NhanVien>();
        }

        public int MaTaiKhoan { get; set; }
        public string? TenDangNhap { get; set; }
        public string? MatKhau { get; set; }
        public string? VaiTro { get; set; }

        public virtual ICollection<NhanVien> NhanViens { get; set; }
    }
}
