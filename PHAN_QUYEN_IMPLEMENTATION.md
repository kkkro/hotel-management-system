# Hướng Dẫn Phân Quyền - Hệ Thống Quản Lý Khách Sạn

## 📋 Tóm Tắt Thay Đổi

Hệ thống đã được cập nhật để áp dụng phân quyền rõ ràng dựa trên vai trò (Role-Based Authorization). Nhân viên (nhanvien) chỉ có thể truy cập 4 chức năng quản lý được phép.

---

## 🔐 Vai Trò Và Quyền Hạn

### 1️⃣ Vai Trò: **Admin** (Quản Trị Viên)
**Quyền hạn:**
- ✅ Quản Lý Nhân Viên (NhanVienController)
- ✅ Quản Lý Loại Phòng (LoaiPhongController)
- ✅ Quản Lý Phòng (PhongController)
- ✅ Quản Lý Giá Phòng (GiaPhongController)
- ✅ Quản Lý Khách Hàng (KhachHangController)
- ✅ Đặt Phòng (DatPhongController)
- ✅ Thuê Phòng (ThuePhongController)
- ✅ Quản Lý Hóa Đơn (HoaDonController)
- ✅ Dashboard

### 2️⃣ Vai Trò: **Nhanvien** (Nhân Viên)
**Quyền hạn:** Chỉ 4 chức năng
- ✅ Quản Lý Khách Hàng (KhachHangController)
- ✅ Đặt Phòng (DatPhongController)
- ✅ Thuê Phòng (ThuePhongController)
- ✅ Quản Lý Hóa Đơn (HoaDonController)

**Không được phép truy cập:**
- ❌ Dashboard (Home/Index) - chỉ Admin
- ❌ Quản Lý Nhân Viên
- ❌ Quản Lý Loại Phòng
- ❌ Quản Lý Phòng
- ❌ Quản Lý Giá Phòng

---

## 🛠️ Chi Tiết Kỹ Thuật

### 📝 File Đã Sửa Đổi

#### 1. **Program.cs** - Cấu Hình Authorization Policies
```csharp
// Tạo 3 policies:
- "AdminOnly": Chỉ Admin có quyền
- "StaffAndAbove": Admin và Nhanvien đều có quyền
- "AllRoles": Tất cả vai trò có quyền
```

#### 2. **AccountController.cs** - Chuẩn Hóa Role
- Đổi role thành **lowercase** khi đăng nhập
- `VaiTro` được normalize: `taiKhoan.VaiTro.ToLower().Trim()`
- Đảm bảo consistency trong toàn hệ thống

#### 3. **Tất Cả Controllers** - Cập Nhật Authorization
```
HomeController              → [Authorize(Policy = "AdminOnly")]
KhachHangController        → [Authorize(Policy = "StaffAndAbove")]
DatPhongController         → [Authorize(Policy = "StaffAndAbove")]
ThuePhongController        → [Authorize(Policy = "StaffAndAbove")]
HoaDonController           → [Authorize(Policy = "StaffAndAbove")]
LoaiPhongController        → [Authorize(Policy = "AdminOnly")]
PhongController            → [Authorize(Policy = "AdminOnly")]
GiaPhongController         → [Authorize(Policy = "AdminOnly")]
NhanVienController         → [Authorize(Policy = "AdminOnly")]
```

#### 4. **Views/Shared/_Layout.cshtml** - Menu Dựa Trên Role
Thêm navigation menu động:
- **Admin Menu:** Hiển thị dropdown với tất cả chức năng quản lý
- **Common Menu:** Hiển thị 4 chức năng cho Admin + Nhanvien
  - Khách Hàng
  - Đặt Phòng
  - Thuê Phòng
  - Hóa Đơn

#### 5. **Models/RoleConstants.cs** - File Hỗ Trợ (Tùy Chọn)
Tạo lớp hằng số để dễ quản lý các role trong tương lai:
- `Admin = "admin"`
- `NhanVien = "nhanvien"`
- Các hàm utility để kiểm tra role

---

## 📊 Quy Trình Đăng Nhập

1. Người dùng nhập tên đăng nhập và mật khẩu
2. Hệ thống kiểm tra database `TaiKhoan`
3. Role được **normalize thành lowercase** (admin hoặc nhanvien)
4. Claim được tạo với role chuẩn hóa
5. User được redirect dựa trên vai trò:
   - Admin → Có thể truy cập tất cả
   - Nhanvien → Chỉ truy cập 4 chức năng

---

## 🧪 Cách Kiểm Tra

### Test 1: Đăng Nhập Với Admin
```
- Tên Đăng Nhập: [admin account]
- Vai Trò: admin
- Kết Quả: Nhìn thấy tất cả menu (Admin + 4 chức năng)
```

### Test 2: Đăng Nhập Với Nhân Viên
```
- Tên Đăng Nhập: [nhanvien account]
- Vai Trò: nhanvien
- Kết Quả: Chỉ nhìn thấy 4 menu
- Nếu cố truy cập /Home/Index → 403 Forbidden
```

### Test 3: Truy Cập Không Phép
```
URL: http://localhost:5007/NhanVien/Index (khi login as nhanvien)
Kết Quả: Trang "Access Denied"
```

---

## 🔄 Chuẩn Hóa Role Trong Database

**Quan Trọng:** Đảm bảo database có các role chính xác:
- Tất cả admin users → VaiTro = **"admin"** (lowercase)
- Tất cả nhân viên users → VaiTro = **"nhanvien"** (lowercase)

### Query SQL để kiểm tra/cập nhật:
```sql
-- Xem các vai trò hiện tại
SELECT DISTINCT VaiTro FROM TaiKhoans;

-- Chuẩn hóa tất cả role thành lowercase (nếu cần)
UPDATE TaiKhoans 
SET VaiTro = LOWER(TRIM(VaiTro));
```

---

## 📌 Ghi Chú Quan Trọng

1. **Case-Insensitive:** Hệ thống xử lý role case-insensitive nhưng lưu trữ dưới dạng lowercase
2. **Authorization Check:** Được thực hiện ở cấp Controller (Attribute)
3. **Menu Visibility:** Được kiểm soát trong Layout.cshtml
4. **Access Denied:** Nhân viên cố truy cập chức năng Admin sẽ thấy trang AccessDenied

---

## 🚀 Các Bước Tiếp Theo (Tùy Chọn)

Để nâng cao hơn, có thể:
1. Thêm role-based authorization cho action level (ngoài controller)
2. Tạo permission matrix chi tiết hơn
3. Thêm audit logging cho access attempts
4. Implement role management UI (chỉnh sửa role của user)
5. Thêm more granular policies (view vs edit vs delete permissions)

---

## 📞 Troubleshooting

| Vấn Đề | Nguyên Nhân | Giải Pháp |
|--------|-----------|---------|
| Nhanvien vẫn thấy menu Admin | Role chưa chuẩn hóa | Check `User.IsInRole()` logic trong Layout |
| 403 Error khi bấm menu | Policy chưa áp dụng | Verify `[Authorize(Policy = "...")]` ở Controller |
| Tất cả user bị 403 | Authorization service chưa add | Check Program.cs AddAuthorization() |
| Role không match | Case không khớp | Đảm bảo database role là lowercase |

---

**Build Status:** ✅ Build Succeeded (0 Errors)
**Last Updated:** 2026-04-27
