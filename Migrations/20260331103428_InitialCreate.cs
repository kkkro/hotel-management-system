using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebKhachSan.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKhachHang = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenKhachHang = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DienThoai = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    DiaChi = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    CCCD = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KhachHan__88D2F0E5360820A4", x => x.MaKhachHang);
                });

            migrationBuilder.CreateTable(
                name: "LoaiPhong",
                columns: table => new
                {
                    MaLoaiPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenLoaiPhong = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    MoTa = table.Column<string>(type: "text", nullable: true),
                    SoNguoiToiDa = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoaiPhon__23021217BD3EC184", x => x.MaLoaiPhong);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    TenDangNhap = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MatKhau = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    VaiTro = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TaiKhoan__AD7C652953631F21", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "DatPhong",
                columns: table => new
                {
                    MaDatPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaKhachHang = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    NgayDat = table.Column<DateTime>(type: "date", nullable: true),
                    NgayNhanDuKien = table.Column<DateTime>(type: "date", nullable: true),
                    NgayTraDuKien = table.Column<DateTime>(type: "date", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DatPhong__6344ADEA8C00C577", x => x.MaDatPhong);
                    table.ForeignKey(
                        name: "FK_DatPhong_KhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang");
                });

            migrationBuilder.CreateTable(
                name: "ThuePhong",
                columns: table => new
                {
                    MaThuePhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaKhachHang = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayNhan = table.Column<DateTime>(type: "date", nullable: true),
                    NgayTra = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ThuePhon__4859A8781FEF241D", x => x.MaThuePhong);
                    table.ForeignKey(
                        name: "FK_ThuePhong_KhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang");
                });

            migrationBuilder.CreateTable(
                name: "GiaPhong",
                columns: table => new
                {
                    MaGia = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaLoaiPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Gia = table.Column<double>(type: "float", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "date", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GiaPhong__3CD3DE5E82ED75AC", x => x.MaGia);
                    table.ForeignKey(
                        name: "FK_GiaPhong_LoaiPhong",
                        column: x => x.MaLoaiPhong,
                        principalTable: "LoaiPhong",
                        principalColumn: "MaLoaiPhong");
                });

            migrationBuilder.CreateTable(
                name: "Phong",
                columns: table => new
                {
                    MaPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SoPhong = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    TrangThai = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MaLoaiPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Phong__20BD5E5B29621A33", x => x.MaPhong);
                    table.ForeignKey(
                        name: "FK__Phong__MaLoaiPho__3D5E1FD2",
                        column: x => x.MaLoaiPhong,
                        principalTable: "LoaiPhong",
                        principalColumn: "MaLoaiPhong");
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNhanVien = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenNhanVien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: true),
                    DienThoai = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NhanVien__77B2CA474981CB5D", x => x.MaNhanVien);
                    table.ForeignKey(
                        name: "FK_NhanVien_TaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "CTDatPhong",
                columns: table => new
                {
                    MaDatPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaLoaiPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    GiaTamTinh = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CTDatPho__01748CCB2826D9B7", x => new { x.MaDatPhong, x.MaLoaiPhong });
                    table.ForeignKey(
                        name: "FK_CTDP_DatPhong",
                        column: x => x.MaDatPhong,
                        principalTable: "DatPhong",
                        principalColumn: "MaDatPhong");
                    table.ForeignKey(
                        name: "FK_CTDP_LoaiPhong",
                        column: x => x.MaLoaiPhong,
                        principalTable: "LoaiPhong",
                        principalColumn: "MaLoaiPhong");
                });

            migrationBuilder.CreateTable(
                name: "CTThuePhong",
                columns: table => new
                {
                    MaThuePhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    GiaThueTaiThoiDiem = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CTThuePh__EA527D9DC71494CE", x => new { x.MaThuePhong, x.MaPhong });
                    table.ForeignKey(
                        name: "FK_CTTP_Phong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong");
                    table.ForeignKey(
                        name: "FK_CTTP_ThuePhong",
                        column: x => x.MaThuePhong,
                        principalTable: "ThuePhong",
                        principalColumn: "MaThuePhong");
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHoaDon = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaNhanVien = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    NgayLap = table.Column<DateTime>(type: "date", nullable: true),
                    TongTien = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HoaDon__835ED13B84974548", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDon_NhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "CTHoaDon",
                columns: table => new
                {
                    MaCTHD = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaHoaDon = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    MaThuePhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SoTien = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CTHoaDon__1E4FA771C49770B6", x => x.MaCTHD);
                    table.ForeignKey(
                        name: "FK_CTHD_HoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "MaHoaDon");
                    table.ForeignKey(
                        name: "FK_CTHD_ThuePhong",
                        column: x => x.MaThuePhong,
                        principalTable: "ThuePhong",
                        principalColumn: "MaThuePhong");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CTDatPhong_MaLoaiPhong",
                table: "CTDatPhong",
                column: "MaLoaiPhong");

            migrationBuilder.CreateIndex(
                name: "IX_CTHoaDon_MaHoaDon",
                table: "CTHoaDon",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_CTHoaDon_MaThuePhong",
                table: "CTHoaDon",
                column: "MaThuePhong");

            migrationBuilder.CreateIndex(
                name: "IX_CTThuePhong_MaPhong",
                table: "CTThuePhong",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_MaKhachHang",
                table: "DatPhong",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_GiaPhong_MaLoaiPhong",
                table: "GiaPhong",
                column: "MaLoaiPhong");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaNhanVien",
                table: "HoaDon",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_MaTaiKhoan",
                table: "NhanVien",
                column: "MaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaLoaiPhong",
                table: "Phong",
                column: "MaLoaiPhong");

            migrationBuilder.CreateIndex(
                name: "IX_ThuePhong_MaKhachHang",
                table: "ThuePhong",
                column: "MaKhachHang");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CTDatPhong");

            migrationBuilder.DropTable(
                name: "CTHoaDon");

            migrationBuilder.DropTable(
                name: "CTThuePhong");

            migrationBuilder.DropTable(
                name: "GiaPhong");

            migrationBuilder.DropTable(
                name: "DatPhong");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "Phong");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "ThuePhong");

            migrationBuilder.DropTable(
                name: "LoaiPhong");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "KhachHang");
        }
    }
}
