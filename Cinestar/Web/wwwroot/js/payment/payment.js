document.addEventListener('DOMContentLoaded', function () {
    console.log('=== PAYMENT PAGE LOADED ===');

    // ✅ Load booking data từ sessionStorage
    const bookingDataStr = sessionStorage.getItem('bookingData');
    const bookingInfoStr = sessionStorage.getItem('bookingInfo');

    if (!bookingDataStr || !bookingInfoStr) {
        console.error('❌ No booking data found!');
        alert('Không tìm thấy thông tin đặt vé. Vui lòng đặt vé lại!');
        window.location.href = '/Movie';
        return;
    }

    const bookingData = JSON.parse(bookingDataStr);
    const bookingInfo = JSON.parse(bookingInfoStr);
    
    console.log('✅ Booking Data loaded:', bookingData);
    console.log('✅ Booking Info loaded:', bookingInfo);

    // ✅ Hiển thị thông tin đặt vé
    displayBookingInfo(bookingData);

    // ✅ Setup countdown timer
    let timeLeft = bookingInfo.timeLeft || (5 * 60);
    startCountdown(timeLeft);

    function displayBookingInfo(data) {
        console.log('=== Displaying Booking Info ===');

        // Hiển thị thông tin phim
        updateElement('movieTitle', data.movieTitle);

        // Hiển thị thông tin rạp
        const cinemaNameEl = document.querySelector('.cinema-info h4');
        if (cinemaNameEl) {
            cinemaNameEl.textContent = data.cinemaName || 'N/A';
        }

        const cinemaAddressEl = document.querySelector('.cinema-info p');
        if (cinemaAddressEl) {
            cinemaAddressEl.textContent = data.cinemaAddress || '';
        }

        // Hiển thị thông tin suất chiếu
        updateElement('showtime', `${data.showTime || ''} ${data.showDate || ''}`);
        updateElement('room', data.roomType || 'N/A');

        // ✅ Hiển thị thông tin ghế và vé
        if (data.seats && data.seats.length > 0) {
            const seatNames = data.seats.map(s => s.seatName).join(', ');
            updateElement('seat', seatNames);
            updateElement('quantity', data.seats.length);

            const ticketType = data.seats[0].ticketType || 'Người lớn';
            updateElement('ticketType', ticketType);
        }

        // ✅ Hiển thị sản phẩm
        const productsContainer = document.getElementById('productsContainer');
        if (productsContainer) {
            if (data.products && data.products.length > 0) {
                const productList = data.products.map(p =>
                    `${p.productName} x${p.quantity}`
                ).join(', ');
                productsContainer.textContent = productList;
            } else {
                productsContainer.textContent = 'Không có';
            }
        }

        // ✅ Tính và hiển thị tổng tiền
        const seatsTotal = data.seatsTotal || 0;
        const productsTotal = data.productsTotal || 0;
        const totalAmount = seatsTotal + productsTotal;

        console.log('💰 Price breakdown:', {
            seatsTotal,
            productsTotal,
            totalAmount
        });

        // ✅ Cập nhật giá tiền
        updateElement('seatsTotalAmount', formatCurrency(seatsTotal));
        updateElement('productsTotalAmount', formatCurrency(productsTotal));

        const totalAmountElements = document.querySelectorAll('#totalAmount, .total-price');
        totalAmountElements.forEach(el => {
            el.textContent = formatCurrency(totalAmount);
        });

        console.log('✅ Display completed');
    }

    function updateElement(id, value) {
        const element = document.getElementById(id);
        if (element && value !== undefined && value !== null) {
            element.textContent = value;
            console.log(`✅ Updated ${id}:`, value);
        } else {
            console.warn(`⚠️ Element not found or value is null: ${id}`, value);
        }
    }

    function formatCurrency(amount) {
        if (!amount || isNaN(amount)) return '0 VNĐ';
        return new Intl.NumberFormat('vi-VN').format(Math.floor(amount)) + ' VNĐ';
    }

    function startCountdown(seconds) {
        const countdownEl = document.getElementById('countdown');

        if (!countdownEl) {
            console.warn('⚠️ Countdown element not found');
            return;
        }

        const interval = setInterval(() => {
            const minutes = Math.floor(seconds / 60);
            const secs = seconds % 60;

            countdownEl.textContent = `${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;

            if (seconds <= 60) {
                countdownEl.style.color = '#ff3333';
            }

            if (seconds <= 0) {
                clearInterval(interval);
                alert('Hết thời gian giữ vé!');
                sessionStorage.removeItem('bookingData');
                sessionStorage.removeItem('bookingInfo');
                window.location.href = '/Movie';
                return;
            }

            seconds--;
            timeLeft = seconds;

            // ✅ Cập nhật timeLeft vào sessionStorage
            const currentInfo = JSON.parse(sessionStorage.getItem('bookingInfo') || '{}');
            currentInfo.timeLeft = timeLeft;
            sessionStorage.setItem('bookingInfo', JSON.stringify(currentInfo));

        }, 1000);
    }

    // ✅ Form validation và submit
    const form = document.querySelector('.payment-form form');
    if (form) {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Validate form
            const fullname = document.getElementById('fullname').value.trim();
            const phone = document.getElementById('phone').value.trim();
            const email = document.getElementById('email').value.trim();
            const ageConfirm = document.getElementById('age-confirm').checked;
            const terms = document.getElementById('terms').checked;

            if (!fullname || !phone || !email || !ageConfirm || !terms) {
                alert('Vui lòng điền đầy đủ thông tin và đồng ý điều khoản!');
                return;
            }

            // ✅ Lưu thông tin customer vào bookingData
            const currentBookingData = JSON.parse(sessionStorage.getItem('bookingData') || '{}');
            currentBookingData.customerName = fullname;
            currentBookingData.customerPhone = phone;
            currentBookingData.customerEmail = email;
            sessionStorage.setItem('bookingData', JSON.stringify(currentBookingData));

            // Chuyển trang
            window.location.href = '/Payment/PaymentMethod';
        });
    }
});