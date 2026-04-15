using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebKhachSan.Models
{
    public partial class QuanLyKhachSanContext : DbContext
    {
        public QuanLyKhachSanContext()
        {
        }

        public QuanLyKhachSanContext(DbContextOptions<QuanLyKhachSanContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CtdatPhong> CtdatPhongs { get; set; } = null!;
        public virtual DbSet<CthoaDon> CthoaDons { get; set; } = null!;
        public virtual DbSet<CtthuePhong> CtthuePhongs { get; set; } = null!;
        public virtual DbSet<DatPhong> DatPhongs { get; set; } = null!;
        public virtual DbSet<GiaPhong> GiaPhongs { get; set; } = null!;
        public virtual DbSet<HoaDon> HoaDons { get; set; } = null!;
        public virtual DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public virtual DbSet<LoaiPhong> LoaiPhongs { get; set; } = null!;
        public virtual DbSet<NhanVien> NhanViens { get; set; } = null!;
        public virtual DbSet<Phong> Phongs { get; set; } = null!;
        public virtual DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public virtual DbSet<ThuePhong> ThuePhongs { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True");

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CtdatPhong>(entity =>
            {
                entity.HasKey(e => new { e.MaDatPhong, e.MaLoaiPhong })
                    .HasName("PK__CTDatPho__01748CCB2826D9B7");

                entity.ToTable("CTDatPhong");

                entity.Property(e => e.MaDatPhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MaLoaiPhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.MaDatPhongNavigation)
                    .WithMany(p => p.CtdatPhongs)
                    .HasForeignKey(d => d.MaDatPhong)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CTDP_DatPhong");

                entity.HasOne(d => d.MaLoaiPhongNavigation)
                    .WithMany(p => p.CtdatPhongs)
                    .HasForeignKey(d => d.MaLoaiPhong)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CTDP_LoaiPhong");
            });

            modelBuilder.Entity<CthoaDon>(entity =>
            {
                entity.HasKey(e => e.MaCthd)
                    .HasName("PK__CTHoaDon__1E4FA771C49770B6");

                entity.ToTable("CTHoaDon");

                entity.Property(e => e.MaCthd)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("MaCTHD");

                entity.Property(e => e.MaHoaDon)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.NoiDung).HasMaxLength(255);

                entity.HasOne(d => d.MaHoaDonNavigation)
                    .WithMany(p => p.CthoaDons)
                    .HasForeignKey(d => d.MaHoaDon)
                    .HasConstraintName("FK_CTHD_HoaDon");
            });

            modelBuilder.Entity<CtthuePhong>(entity =>
            {
                entity.HasKey(e => new { e.MaThuePhong, e.MaPhong })
                    .HasName("PK__CTThuePh__EA527D9DC71494CE");

                entity.ToTable("CTThuePhong");

                entity.Property(e => e.MaThuePhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MaPhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.MaPhongNavigation)
                    .WithMany(p => p.CtthuePhongs)
                    .HasForeignKey(d => d.MaPhong)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CTTP_Phong");

                entity.HasOne(d => d.MaThuePhongNavigation)
                    .WithMany(p => p.CtthuePhongs)
                    .HasForeignKey(d => d.MaThuePhong)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CTTP_ThuePhong");
            });

            modelBuilder.Entity<DatPhong>(entity =>
            {
                entity.HasKey(e => e.MaDatPhong)
                    .HasName("PK__DatPhong__6344ADEA8C00C577");

                entity.ToTable("DatPhong");

                entity.Property(e => e.MaDatPhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MaKhachHang)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.NgayDat).HasColumnType("date");

                entity.Property(e => e.NgayNhanDuKien).HasColumnType("date");

                entity.Property(e => e.NgayTraDuKien).HasColumnType("date");

                entity.Property(e => e.TrangThai).HasMaxLength(50);

                entity.HasOne(d => d.MaKhachHangNavigation)
                    .WithMany(p => p.DatPhongs)
                    .HasForeignKey(d => d.MaKhachHang)
                    .HasConstraintName("FK_DatPhong_KhachHang");
            });

            modelBuilder.Entity<GiaPhong>(entity =>
            {
                entity.HasKey(e => e.MaGia)
                    .HasName("PK__GiaPhong__3CD3DE5E82ED75AC");

                entity.ToTable("GiaPhong");

                entity.Property(e => e.MaGia)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MaLoaiPhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.NgayBatDau).HasColumnType("date");

                entity.Property(e => e.NgayKetThuc).HasColumnType("date");

                entity.HasOne(d => d.MaLoaiPhongNavigation)
                    .WithMany(p => p.GiaPhongs)
                    .HasForeignKey(d => d.MaLoaiPhong)
                    .HasConstraintName("FK_GiaPhong_LoaiPhong");
            });

            modelBuilder.Entity<HoaDon>(entity =>
            {
                entity.HasKey(e => e.MaHoaDon)
                    .HasName("PK__HoaDon__835ED13B84974548");

                entity.ToTable("HoaDon");

                entity.Property(e => e.MaHoaDon)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MaNhanVien)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MaThuePhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.NgayLap).HasColumnType("date");

                entity.HasOne(d => d.MaNhanVienNavigation)
                    .WithMany(p => p.HoaDons)
                    .HasForeignKey(d => d.MaNhanVien)
                    .HasConstraintName("FK_HoaDon_NhanVien");

                entity.HasOne(d => d.MaThuePhongNavigation)
                    .WithMany(p => p.HoaDons)
                    .HasForeignKey(d => d.MaThuePhong)
                    .HasConstraintName("FK_HoaDon_ThuePhong");
            });

            modelBuilder.Entity<KhachHang>(entity =>
            {
                entity.HasKey(e => e.MaKhachHang)
                    .HasName("PK__KhachHan__88D2F0E5360820A4");

                entity.ToTable("KhachHang");

                entity.Property(e => e.MaKhachHang)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Cccd)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("CCCD");

                entity.Property(e => e.DiaChi)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.DienThoai)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.TenKhachHang)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LoaiPhong>(entity =>
            {
                entity.HasKey(e => e.MaLoaiPhong)
                    .HasName("PK__LoaiPhon__23021217BD3EC184");

                entity.ToTable("LoaiPhong");

                entity.Property(e => e.MaLoaiPhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MoTa).HasColumnType("text");

                entity.Property(e => e.TenLoaiPhong)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<NhanVien>(entity =>
            {
                entity.HasKey(e => e.MaNhanVien)
                    .HasName("PK__NhanVien__77B2CA474981CB5D");

                entity.ToTable("NhanVien");

                entity.Property(e => e.MaNhanVien)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ChucVu).HasMaxLength(50);

                entity.Property(e => e.DiaChi).HasMaxLength(255);

                entity.Property(e => e.DienThoai)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.NgaySinh).HasColumnType("date");

                entity.Property(e => e.TenNhanVien).HasMaxLength(100);

                entity.HasOne(d => d.MaTaiKhoanNavigation)
                    .WithMany(p => p.NhanViens)
                    .HasForeignKey(d => d.MaTaiKhoan)
                    .HasConstraintName("FK_NhanVien_TaiKhoan");
            });

            modelBuilder.Entity<Phong>(entity =>
            {
                entity.HasKey(e => e.MaPhong)
                    .HasName("PK__Phong__20BD5E5B29621A33");

                entity.ToTable("Phong");

                entity.Property(e => e.MaPhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MaLoaiPhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.SoPhong)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TrangThai)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.MaLoaiPhongNavigation)
                    .WithMany(p => p.Phongs)
                    .HasForeignKey(d => d.MaLoaiPhong)
                    .HasConstraintName("FK__Phong__MaLoaiPho__3D5E1FD2");
            });

            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.HasKey(e => e.MaTaiKhoan)
                    .HasName("PK__TaiKhoan__AD7C652953631F21");

                entity.ToTable("TaiKhoan");

                entity.Property(e => e.MaTaiKhoan).ValueGeneratedNever();

                entity.Property(e => e.MatKhau)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TenDangNhap)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VaiTro)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ThuePhong>(entity =>
            {
                entity.HasKey(e => e.MaThuePhong)
                    .HasName("PK__ThuePhon__4859A8781FEF241D");

                entity.ToTable("ThuePhong");

                entity.Property(e => e.MaThuePhong)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MaKhachHang)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.NgayNhan).HasColumnType("date");

                entity.Property(e => e.NgayTra).HasColumnType("date");

                entity.HasOne(d => d.MaKhachHangNavigation)
                    .WithMany(p => p.ThuePhongs)
                    .HasForeignKey(d => d.MaKhachHang)
                    .HasConstraintName("FK_ThuePhong_KhachHang");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
