namespace WebKhachSan.Models
{
    /// <summary>
    /// Hằng số định nghĩa các vai trò trong hệ thống
    /// </summary>
    public static class RoleConstants
    {
        /// <summary>
        /// Vai trò Admin - có quyền truy cập toàn bộ hệ thống
        /// </summary>
        public const string Admin = "admin";

        /// <summary>
        /// Vai trò Nhân viên - chỉ có quyền quản lý khách hàng, đặt phòng, thuê phòng, hóa đơn
        /// </summary>
        public const string NhanVien = "nhanvien";

        /// <summary>
        /// Danh sách tất cả các vai trò trong hệ thống
        /// </summary>
        public static class RoleNames
        {
            public const string AdminRoles = "admin";
            public const string StaffRoles = "nhanvien";
            public const string AllRoles = "admin,nhanvien";
        }

        /// <summary>
        /// Kiểm tra xem người dùng có vai trò nào
        /// </summary>
        public static bool HasRole(string userRole, string requiredRole)
        {
            if (string.IsNullOrEmpty(userRole))
                return false;

            return userRole.Equals(requiredRole, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra xem người dùng có một trong các vai trò được chỉ định
        /// </summary>
        public static bool HasAnyRole(string userRole, params string[] requiredRoles)
        {
            if (string.IsNullOrEmpty(userRole))
                return false;

            return requiredRoles.Any(role => userRole.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Chuẩn hóa tên vai trò (chuyển thành lowercase)
        /// </summary>
        public static string NormalizeRole(string role)
        {
            return string.IsNullOrWhiteSpace(role) ? string.Empty : role.ToLower().Trim();
        }
    }
}
