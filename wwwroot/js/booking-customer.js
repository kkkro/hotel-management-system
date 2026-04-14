/**
 * Customer creation API for public booking
 */

const BookingCustomer = {
    /**
     * Create a new customer for public booking
     * @param {string} tenKhachHang - Customer name
     * @param {string} dienThoai - Phone number
     * @param {string} diaChi - Address
     * @returns {Promise<{success: boolean, maKhachHang?: string, message?: string}>}
     */
    async createCustomer(tenKhachHang, dienThoai, diaChi = '') {
        try {
            const response = await fetch('/KhachHang/CreateCustomer', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    tenKhachHang: tenKhachHang,
                    dienThoai: dienThoai,
                    diaChi: diaChi
                })
            });

            if (!response.ok) {
                return {
                    success: false,
                    message: 'Lỗi kết nối với máy chủ'
                };
            }

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error creating customer:', error);
            return {
                success: false,
                message: 'Lỗi: ' + error.message
            };
        }
    },

    /**
     * Get all customers for dropdown/autocomplete
     * @returns {Promise<Array>}
     */
    async getCustomers() {
        try {
            const response = await fetch('/KhachHang/GetCustomers', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                return [];
            }

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching customers:', error);
            return [];
        }
    },

    /**
     * Validate customer input
     * @param {string} tenKhachHang - Customer name
     * @param {string} dienThoai - Phone number
     * @returns {{valid: boolean, message?: string}}
     */
    validateInput(tenKhachHang, dienThoai) {
        if (!tenKhachHang || tenKhachHang.trim() === '') {
            return {
                valid: false,
                message: 'Vui lòng nhập tên khách hàng'
            };
        }

        if (!dienThoai || dienThoai.trim() === '') {
            return {
                valid: false,
                message: 'Vui lòng nhập số điện thoại'
            };
        }

        // Validate phone number (basic validation - 10-11 digits)
        const phoneRegex = /^(\+84|0)[0-9]{9,10}$/;
        if (!phoneRegex.test(dienThoai.replace(/\s/g, ''))) {
            return {
                valid: false,
                message: 'Số điện thoại không hợp lệ (vui lòng nhập 10-11 chữ số)'
            };
        }

        return { valid: true };
    }
};

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = BookingCustomer;
}
