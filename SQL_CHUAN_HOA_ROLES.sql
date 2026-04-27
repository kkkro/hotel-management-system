-- Chuẩn hóa tất cả VaiTro trong bảng TaiKhoans
-- Chỉ giữ lại: "admin" hoặc "nhanvien" (chữ thường)

-- 1. Kiểm tra VaiTro hiện tại
SELECT MaTaiKhoan, TenDangNhap, VaiTro, 'BEFORE' AS Status 
FROM TaiKhoans 
WHERE VaiTro IS NOT NULL
ORDER BY MaTaiKhoan;

-- 2. Chuẩn hóa: Convert thành lowercase và trim khoảng trắng
UPDATE TaiKhoans 
SET VaiTro = LOWER(TRIM(VaiTro))
WHERE VaiTro IS NOT NULL;

-- 3. Sửa các VaiTro không hợp lệ thành "nhanvien" (nếu cần)
-- Lưu ý: Chỉ giữ "admin" và "nhanvien"
UPDATE TaiKhoans 
SET VaiTro = 'nhanvien'
WHERE VaiTro NOT IN ('admin', 'nhanvien')
  AND VaiTro IS NOT NULL;

-- 4. Kiểm tra lại sau khi chuẩn hóa
SELECT MaTaiKhoan, TenDangNhap, VaiTro, 'AFTER' AS Status 
FROM TaiKhoans 
WHERE VaiTro IS NOT NULL
ORDER BY MaTaiKhoan;

-- 5. Thống kê VaiTro
SELECT VaiTro, COUNT(*) AS SoTaiKhoan
FROM TaiKhoans
WHERE VaiTro IS NOT NULL
GROUP BY VaiTro;
