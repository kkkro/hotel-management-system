# Implementation Guide: Public Booking API

## Overview
Hướng dẫn chi tiết thêm API tạo khách hàng vào trang đặt phòng công khai của hệ thống quản lý khách sạn.

---

## Architecture

```
┌─────────────────────────────────────────┐
│     Public Booking Page                 │
│  (HTML + JavaScript)                    │
│  ├── Customer Selection Form            │
│  └── Booking Details Form               │
└────────────────┬────────────────────────┘
                 │
                 │ POST /KhachHang/Create
                 │ (JSON: name, phone, address)
                 │
                 ▼
┌─────────────────────────────────────────┐
│  KhachHangController.Create()           │
│  [AllowAnonymous] [HttpPost]            │
│                                         │
│  1. Validate input                      │
│  2. Generate MaKhachHang                │
│  3. Check phone duplicate               │
│  4. Save to database                    │
│  5. Return { success, maKhachHang }     │
└────────────────┬────────────────────────┘
                 │
                 ▼
        ┌────────────────┐
        │   Database     │
        │  KhachHang     │
        └────────────────┘
```

---

## Step-by-Step Implementation

### Step 1: Include JavaScript Helper
Thêm script sau vào trang booking công khai:

```html
<script src="/js/booking-customer.js"></script>
```

### Step 2: Create Customer Creation Form
```html
<div class="customer-form">
  <h4>Thông Tin Khách Hàng</h4>
  
  <input type="text" id="tenKhachHang" placeholder="Tên khách hàng" required>
  <input type="tel" id="dienThoai" placeholder="Số điện thoại" required>
  <input type="text" id="diaChi" placeholder="Địa chỉ (tùy chọn)">
  
  <button id="createCustomerBtn">Tạo Khách Hàng</button>
  <div id="createCustomerLoading" class="spinner d-none"></div>
</div>

<div id="selectedCustomerInfo" class="alert" style="display: none;">
  <p>Khách hàng: <span id="selectedCustomerId"></span></p>
  <button id="changeCustomerBtn">Chọn Khách Hàng Khác</button>
</div>
```

### Step 3: Add JavaScript Event Handlers
```javascript
// Create new customer
document.getElementById('createCustomerBtn').addEventListener('click', async () => {
  const tenKhachHang = document.getElementById('tenKhachHang').value;
  const dienThoai = document.getElementById('dienThoai').value;
  const diaChi = document.getElementById('diaChi').value;
  
  // Validate
  const validation = BookingCustomer.validateInput(tenKhachHang, dienThoai);
  if (!validation.valid) {
    alert(validation.message);
    return;
  }
  
  // Create
  const result = await BookingCustomer.createCustomer(tenKhachHang, dienThoai, diaChi);
  
  if (result.success) {
    console.log('Customer created:', result.maKhachHang);
    // Show selected customer info
    showSelectedCustomer(result.maKhachHang, tenKhachHang);
    // Continue with booking...
  } else {
    alert('Error: ' + result.message);
  }
});

function showSelectedCustomer(maKhachHang, tenKhachHang) {
  document.getElementById('customerForm').style.display = 'none';
  document.getElementById('selectedCustomerId').textContent = maKhachHang;
  document.getElementById('selectedCustomerInfo').style.display = 'block';
}
```

### Step 4: Get Existing Customers (Optional)
Nếu muốn hiển thị danh sách khách hàng hiện có:

```javascript
// Load customers on page load
document.addEventListener('DOMContentLoaded', async () => {
  const customers = await BookingCustomer.getCustomers();
  
  // Populate dropdown/list
  const select = document.getElementById('customerSelect');
  customers.forEach(customer => {
    const option = document.createElement('option');
    option.value = customer.maKhachHang;
    option.text = `${customer.tenKhachHang} (${customer.dienthoai})`;
    select.appendChild(option);
  });
});

// Select existing customer
document.getElementById('customerSelect').addEventListener('change', (e) => {
  if (e.target.value) {
    showSelectedCustomer(e.target.value, e.target.selectedOptions[0].text);
  }
});
```

---

## API Details

### Request
```javascript
POST /KhachHang/Create
Content-Type: application/json

{
  "tenKhachHang": "string",
  "dienThoai": "string", 
  "diaChi": "string (optional)"
}
```

### Response Success
```json
{
  "success": true,
  "maKhachHang": "KH000001"
}
```

### Response Error
```json
{
  "success": false,
  "message": "Error description"
}
```

---

## Error Handling

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| "Thông tin không hợp lệ" | Missing name or phone | Check input validation |
| "Lỗi kết nối với máy chủ" | Server connection error | Check network/server |
| "Lỗi máy chủ" | Database/server error | Check server logs |

### Validation Flow

```
User Input
    ↓
[Client Validation]
  - Name not empty
  - Phone not empty
  - Phone format valid (10-11 digits)
    ↓
[Server Validation]
  - Name not empty
  - Phone not empty
    ↓
[Business Logic]
  - Generate MaKhachHang (KHxxxxxx)
  - Check for duplicate phone
  - If exists: return existing customer
  - If new: create and save
    ↓
Response
```

---

## Frontend Integration Examples

### Example 1: Modal Form
```html
<div class="modal fade" id="customerModal" tabindex="-1">
  <div class="modal-dialog">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Tạo Khách Hàng Mới</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <!-- Customer form here -->
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
        <button type="button" class="btn btn-primary" id="submitCustomerForm">Tạo</button>
      </div>
    </div>
  </div>
</div>

<script>
  document.getElementById('submitCustomerForm').addEventListener('click', async () => {
    // Create customer
    const result = await BookingCustomer.createCustomer(...);
    
    if (result.success) {
      // Close modal
      bootstrap.Modal.getInstance(document.getElementById('customerModal')).hide();
      // Continue with booking
    }
  });
</script>
```

### Example 2: Inline Form
```html
<form id="bookingForm">
  <!-- Customer section -->
  <fieldset>
    <legend>Khách Hàng</legend>
    <input type="text" id="tenKhachHang" name="tenKhachHang" required>
    <input type="tel" id="dienThoai" name="dienThoai" required>
    <input type="text" id="diaChi" name="diaChi">
  </fieldset>
  
  <!-- Room section -->
  <fieldset>
    <legend>Chọn Phòng</legend>
    <!-- Room selection here -->
  </fieldset>
  
  <button type="submit">Booking</button>
</form>

<script>
  document.getElementById('bookingForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    // Create customer
    const result = await BookingCustomer.createCustomer(...);
    
    if (result.success) {
      // Store maKhachHang for later use
      document.getElementById('bookingForm').maKhachHang = result.maKhachHang;
      // Submit booking
    }
  });
</script>
```

---

## Testing

### Manual Testing
1. Open booking page
2. Enter customer details:
   - Name: "Test Customer"  
   - Phone: "0901234567"
   - Address: "Test Address"
3. Click "Create Customer"
4. Check results:
   - Success message appears
   - Customer ID returned
   - Form updates to show selected customer

### Testing Phone Duplicate
1. Create customer with phone "0901234567"
2. Try create another with same phone
3. Should return existing customer (different behavior)

### API Testing (cURL)
```bash
curl -X POST http://localhost:5000/KhachHang/Create \
  -H "Content-Type: application/json" \
  -d '{
    "tenKhachHang": "Test Customer",
    "dienThoai": "0901234567",
    "diaChi": "123 Test Street"
  }'
```

### API Testing (Postman)
1. Create new POST request
2. URL: `http://localhost:5000/KhachHang/Create`
3. Header: `Content-Type: application/json`
4. Body (raw JSON):
   ```json
   {
     "tenKhachHang": "Test Customer",
     "dienThoai": "0901234567",
     "diaChi": "123 Test Street"
   }
   ```
5. Send and check response

---

## Performance Optimization

### Caching Customer List
```javascript
let customersCache = null;

async function getCustomers(maxAge = 5 * 60 * 1000) { // 5 minutes
  const now = Date.now();
  
  if (customersCache && (now - customersCache.timestamp) < maxAge) {
    return customersCache.data;
  }
  
  const data = await BookingCustomer.getCustomers();
  customersCache = { data, timestamp: now };
  return data;
}
```

### Debounce Customer Search
```javascript
const debounce = (fn, delay) => {
  let timeout;
  return (...args) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => fn(...args), delay);
  };
};

const searchCustomers = debounce(async (query) => {
  // Search implementation
}, 300);
```

---

## Security Best Practices

1. **HTTPS Only** - Always use HTTPS in production
2. **Rate Limiting** - Implement rate limit to prevent abuse
3. **Input Validation** - Validate all inputs on server side
4. **CORS** - Configure CORS if API accessed from different domain
5. **Logging** - Log all customer creations for audit

---

## Troubleshooting

### Issue: "Lỗi máy chủ" (Server Error)
- Check server logs: `dotnet run`
- Verify database connection
- Check database is running

### Issue: Customer not created but returns success
- Check database saves data
- Check if there's network/timeout issue
- Check server logs for exceptions

### Issue: Phone validation fails
- Phone must be 10-11 digits 
- Format: +84 or 0 prefix
- Example valid: 0901234567, +84901234567

### Issue: Customer creation very slow
- Check database performance
- Check server load
- Consider implementing caching

---

## Files Modified/Created

1. **Modified:** [Controllers/KhachHangController.cs](Controllers/KhachHangController.cs)
   - Added `Create([FromBody] CreateCustomerRequest)` method
   - Added `GetCustomers()` method  
   - Added `CreateCustomerRequest` class

2. **Created:** [wwwroot/js/booking-customer.js](wwwroot/js/booking-customer.js)
   - Customer creation helper functions
   - Validation helpers
   - API wrapper methods

3. **Reference:** [BOOKING_EXAMPLE.html](BOOKING_EXAMPLE.html)
   - Complete HTML example
   - JS event handlers
   - Styling

---

## Next Steps

1. Integrate API into actual booking page
2. Test with real data
3. Add additional features:
   - Email verification
   - OTP validation
   - Customer consent
   - Rate limiting
4. Monitor usage and performance
5. Gather user feedback

---

## Support & Questions

For issues or questions:
1. Check API_DOCUMENTATION.md
2. Review this guide
3. Check build_output.txt for errors
4. Check server logs: `dotnet run`
