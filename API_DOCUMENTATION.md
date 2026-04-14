# Customer Management API Documentation

## Overview
API endpoint cho phép tạo khách hàng mới từ trang đặt phòng công khai (public booking).

## Endpoints

### 1. Create Customer (Public Endpoint)
**Endpoint:** `POST /KhachHang/Create`

**Authentication:** None required (`[AllowAnonymous]`)

**Request Body:**
```json
{
  "tenKhachHang": "Nguyễn Văn A",
  "dienThoai": "0901234567",
  "diaChi": "123 Đường ABC, Quận 1, TP.HCM"
}
```

**request Parameter Details:**
- `tenKhachHang` (string, required): Tên khách hàng - tối thiểu 1 ký tự
- `dienThoai` (string, required): Số điện thoại - định dạng VN
- `diaChi` (string, optional): Địa chỉ khách hàng

**Success Response (201):**
```json
{
  "success": true,
  "maKhachHang": "KH000001"
}
```

**Error Response (400):**
```json
{
  "success": false,
  "message": "Thông tin không hợp lệ"
}
```

**Behavior:**
- Tự động sinh mã khách hàng dạng "KHxxxxxx"
- Nếu số điện thoại đã tồn tại, trả về khách hàng hiện có thay vì tạo mới
- Hỗ trợ tạo khách hàng từ trang booking công khai

---

### 2. Get Customers List
**Endpoint:** `GET /KhachHang/GetCustomers`

**Authentication:** None required

**Response:**
```json
[
  {
    "maKhachHang": "KH000001",
    "tenKhachHang": "Nguyễn Văn A",
    "dienthoai": "0901234567",
    "diachi": "123 Đường ABC"
  },
  {
    "maKhachHang": "KH000002",
    "tenKhachHang": "Trần Thị B",
    "dienthoai": "0912345678",
    "diachi": "456 Đường XYZ"
  }
]
```

---

## Usage Examples

### JavaScript Example
```javascript
// Create a new customer
const result = await BookingCustomer.createCustomer(
  'Nguyễn Văn A',
  '0901234567',
  '123 Đường ABC'
);

if (result.success) {
  console.log('Customer created:', result.maKhachHang);
} else {
  console.error('Error:', result.message);
}

// Get all customers
const customers = await BookingCustomer.getCustomers();
```

### HTML Form Integration
```html
<form id="bookingForm">
  <input type="text" id="tenKhachHang" placeholder="Tên khách hàng" required>
  <input type="tel" id="dienThoai" placeholder="Số điện thoại" required>
  <input type="text" id="diaChi" placeholder="Địa chỉ (tùy chọn)">
  
  <button type="submit">Booking</button>
</form>

<script src="/js/booking-customer.js"></script>
<script>
  document.getElementById('bookingForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const tenKhachHang = document.getElementById('tenKhachHang').value;
    const dienThoai = document.getElementById('dienThoai').value;
    const diaChi = document.getElementById('diaChi').value;
    
    // Validate input
    const validation = BookingCustomer.validateInput(tenKhachHang, dienThoai);
    if (!validation.valid) {
      alert(validation.message);
      return;
    }
    
    // Create customer
    const result = await BookingCustomer.createCustomer(tenKhachHang, dienThoai, diaChi);
    
    if (result.success) {
      // Proceed with booking using result.maKhachHang
      console.log('Customer ID:', result.maKhachHang);
      // Submit booking form here
    } else {
      alert('Lỗi: ' + result.message);
    }
  });
</script>
```

---

## Error Handling

### Possible Errors:
1. **Empty Input** - `"Thông tin không hợp lệ"`
2. **Invalid Phone** - Phone number không đúng định dạng
3. **Server Error** - `"Lỗi máy chủ"`
4. **Network Error** - Connection timeout

### Entry Log
All customer creations are logged:
- Success: `"Tạo khách hàng mới từ booking công khai: KH000001 - Nguyễn Văn A"`
- Error: `"Lỗi khi tạo khách hàng"`

---

## Frontend Integration Notes

1. **Include the helper script** in your booking page:
   ```html
   <script src="/js/booking-customer.js"></script>
   ```

2. **Use validation** before API call:
   ```javascript
   const validation = BookingCustomer.validateInput(name, phone);
   if (!validation.valid) {
     showError(validation.message);
     return;
   }
   ```

3. **Handle phone number duplicates** - API returns existing customer if phone exists

4. **CORS** - No CORS issues since API is on same domain

---

## Security Considerations

- Endpoint is public (`[AllowAnonymous]`) - ONLY for booking page usage
- No authentication required for customer creation
- Minimal input validation at API level
- Phone number is NOT required to be unique (duplicate handling built in)
- All changes are logged for audit trail

---

## API Response Codes

| Code | Status | Description |
|------|--------|-------------|
| 200 | OK | Success |
| 400 | Bad Request | Invalid input data |
| 500 | Server Error | Database or server error |

---

## Future Enhancements

- [ ] Email verification for customer
- [ ] OTP verification for phone number
- [ ] Customer consent checkbox
- [ ] Rate limiting to prevent abuse
- [ ] Email notification on new customer creation
