document.addEventListener('DOMContentLoaded', function () {
    console.log('=== PAYMENT PAGE LOADED ===');
    
    // ✅ Lấy dữ liệu từ sessionStorage
    const bookingDataStr = sessionStorage.getItem('bookingData');
    
    if (!bookingDataStr) {
        console.error('❌ No booking data found!');
        alert('Không tìm thấy thông tin đặt vé. Vui lòng đặt vé lại!');
        window.location.href = '/Movie';
        return;
    }

    const bookingData = JSON.parse(bookingDataStr);
    console.log('✅ Booking Data:', bookingData);

    // ✅ Hiển thị thông tin vé lên trang
    displayTicketInfo(bookingData);
    
    // ✅ Lưu vào localStorage để paymentMethod.js có thể sử dụng
    localStorage.setItem('bookingInfo', JSON.stringify({
        movieTitle: bookingData.movieTitle,
        cinema: bookingData.cinemaName,
        cinemaAddress: bookingData.cinemaAddress,
        showtime: `${bookingData.showTime} ${bookingData.showDate}`,
        room: bookingData.roomNumber,
        seat: bookingData.seats.map(s => s.seatName).join(', '),
        ticketType: getTicketTypesDisplay(bookingData.seats),
        quantity: bookingData.totalSeats,
        amount: bookingData.totalAmount
    }));
    
    // ✅ Khởi động đồng hồ đếm ngược
    startCountdown();

    // ✅ Real-time validation
    setupValidation();
});

// ✅ Hàm hiển thị thông tin vé
function displayTicketInfo(data) {
    console.log('=== Displaying ticket info ===');

    // 1. Tiêu đề phim
    updateElement('movieTitle', data.movieTitle);

    // 2. Tên rạp
    const cinemaNameEl = document.querySelector('.cinema-info h4');
    if (cinemaNameEl) {
        cinemaNameEl.textContent = data.cinemaName || 'N/A';
    }

    // 3. Địa chỉ rạp
    const cinemaAddressEl = document.querySelector('.cinema-info p');
    if (cinemaAddressEl) {
        cinemaAddressEl.textContent = data.cinemaAddress || '';
    }

    // 4. Thời gian chiếu
    updateElement('showtime', `${data.showTime} ${data.showDate}`);

    // 5. Phòng chiếu
    updateElement('room', data.roomNumber || 'N/A');

    // 6. Số lượng vé
    updateElement('quantity', data.totalSeats || 0);

    // 7. Loại vé
    const ticketTypeText = getTicketTypesDisplay(data.seats);
    updateElement('ticketType', ticketTypeText);

    // 8. Số ghế
    if (data.seats && data.seats.length > 0) {
        const seatNames = data.seats.map(s => s.seatName).join(', ');
        updateElement('seat', seatNames);
    }

    // 9. Tổng tiền
    updateElement('totalAmount', formatCurrency(data.totalAmount));

    console.log('✅ Ticket info displayed successfully');
}

// ✅ Hàm lấy loại vé hiển thị
function getTicketTypesDisplay(seats) {
    if (!seats || seats.length === 0) return 'N/A';

    const types = [...new Set(seats.map(s => {
        if (s.ticketType === 'couple' || s.seatType === 'couple') return 'Ghế Đôi';
        if (s.ticketType === 'vip' || s.seatType === 'vip') return 'VIP';
        return 'Người Lớn';
    }))];

    return types.join(', ');
}

// ✅ Hàm cập nhật element
function updateElement(id, value) {
    const element = document.getElementById(id);
    if (element && value !== undefined && value !== null) {
        element.textContent = value;
    }
}

// ✅ Format tiền tệ
function formatCurrency(amount) {
    if (!amount) return '0 VNĐ';
    return new Intl.NumberFormat('vi-VN').format(amount) + ' VNĐ';
}

// ✅ Đồng hồ đếm ngược 5 phút
function startCountdown() {
    const countdownElement = document.querySelector('.countdown');
    if (!countdownElement) return;

    let timeLeft = 5 * 60; // 5 phút

    const timer = setInterval(() => {
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        
        countdownElement.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        
        // ✅ Thay đổi màu khi còn 1 phút
        if (timeLeft <= 60) {
            countdownElement.style.color = '#ff3333';
            countdownElement.style.fontWeight = 'bold';
        }
        
        // ✅ Cảnh báo khi còn 1 phút
        if (timeLeft === 60) {
            alert('⏰ Còn 1 phút để hoàn tất thanh toán!');
        }
        
        // ✅ Hết thời gian
        if (timeLeft <= 0) {
            clearInterval(timer);
            alert('⏰ Hết thời gian giữ vé! Vui lòng đặt lại.');
            sessionStorage.removeItem('bookingData');
            localStorage.removeItem('bookingInfo');
            window.location.href = '/Movie';
        }
        
        timeLeft--;

        // ✅ Cập nhật timeLeft vào localStorage
        const bookingInfo = JSON.parse(localStorage.getItem('bookingInfo') || '{}');
        bookingInfo.timeLeft = timeLeft;
        localStorage.setItem('bookingInfo', JSON.stringify(bookingInfo));
    }, 1000);

    // ✅ Cleanup khi rời trang
    window.addEventListener('beforeunload', function () {
        clearInterval(timer);
    });
}

// ✅ Setup validation cho form input
function setupValidation() {
    const inputs = document.querySelectorAll('input[required]');
    
    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            validateField(this);
        });

        input.addEventListener('input', function () {
            // Remove error styling when user starts typing
            this.style.borderColor = '';
            this.style.backgroundColor = '';
            hideFieldError(this);
        });
    });
}

function validateField(field) {
    const value = field.value.trim();
    let isValid = true;
    let message = '';

    if (field.hasAttribute('required') && !value) {
        isValid = false;
        message = 'Trường này là bắt buộc';
    } else if (field.type === 'tel' && value) {
        const phoneRegex = /^(0|\+84)[3-9][0-9]{8,9}$/;
        if (!phoneRegex.test(value)) {
            isValid = false;
            message = 'Số điện thoại không hợp lệ';
        }
    } else if (field.type === 'email' && value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) {
            isValid = false;
            message = 'Email không hợp lệ';
        }
    }

    if (!isValid) {
        field.style.borderColor = '#ff4444';
        field.style.backgroundColor = '#fff5f5';
        showFieldError(field, message);
    } else {
        field.style.borderColor = '#28a745';
        field.style.backgroundColor = '#f8fff8';
        hideFieldError(field);
    }

    return isValid;
}

function showFieldError(field, message) {
    hideFieldError(field);

    const errorDiv = document.createElement('div');
    errorDiv.className = 'field-error validation-message';
    errorDiv.style.color = '#ff4444';
    errorDiv.style.fontSize = '0.8rem';
    errorDiv.style.marginTop = '5px';
    errorDiv.textContent = message;

    field.parentNode.appendChild(errorDiv);
}

function hideFieldError(field) {
    const existingError = field.parentNode.querySelector('.field-error');
    if (existingError) {
        existingError.remove();
    }
}