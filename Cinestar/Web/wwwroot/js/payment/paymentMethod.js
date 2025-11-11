document.addEventListener('DOMContentLoaded', function () {
    console.log('=== PAYMENT METHOD PAGE LOADED ===');

    let selectedMethod = null;
    let countdownInterval = null;
    let timeLeft = 0;

    // ✅ Load booking info từ localStorage (đã được lưu bởi payment.js)
    loadBookingInfo();

    // ✅ Start countdown
    startCountdown();

    function loadBookingInfo() {
        // ✅ Lấy từ localStorage
        const bookingInfoStr = localStorage.getItem('bookingInfo');
        
        if (!bookingInfoStr) {
            console.error('❌ No booking info found!');
            alert('Không tìm thấy thông tin đặt vé. Vui lòng đặt vé lại!');
            window.location.href = '/Movie';
            return;
        }

        const bookingInfo = JSON.parse(bookingInfoStr);
        console.log('✅ Booking Info loaded:', bookingInfo);

        // ✅ Lấy timeLeft từ bookingInfo hoặc mặc định 5 phút
        timeLeft = bookingInfo.timeLeft || (5 * 60);

        // ✅ Cập nhật UI
        updateElement('movieTitle', bookingInfo.movieTitle);
        
        const cinemaNameEl = document.querySelector('.cinema-info h4');
        if (cinemaNameEl) {
            cinemaNameEl.textContent = bookingInfo.cinema;
        }

        const cinemaAddressEl = document.querySelector('.cinema-info p');
        if (cinemaAddressEl) {
            cinemaAddressEl.textContent = bookingInfo.cinemaAddress || '';
        }

        updateElement('showtime', bookingInfo.showtime);
        updateElement('room', bookingInfo.room);
        updateElement('quantity', bookingInfo.quantity);
        updateElement('ticketType', bookingInfo.ticketType);
        updateElement('seat', bookingInfo.seat);
        updateElement('totalAmount', formatCurrency(bookingInfo.amount));

        console.log('✅ UI updated successfully');
    }

    function updateElement(id, value) {
        const element = document.getElementById(id);
        if (element && value !== undefined && value !== null) {
            element.textContent = value;
        }
    }

    function startCountdown() {
        function updateCountdown() {
            const minutes = Math.floor(timeLeft / 60);
            const seconds = timeLeft % 60;
            const countdownElement = document.getElementById('countdown');

            if (countdownElement) {
                countdownElement.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
                
                // Thay đổi màu khi còn ít thời gian
                if (timeLeft <= 60) {
                    countdownElement.style.color = '#ff3333';
                }
            }

            if (timeLeft > 0) {
                timeLeft--;
                
                // ✅ Cập nhật thời gian trong localStorage
                const bookingInfo = JSON.parse(localStorage.getItem('bookingInfo') || '{}');
                bookingInfo.timeLeft = timeLeft;
                localStorage.setItem('bookingInfo', JSON.stringify(bookingInfo));
            } else {
                clearInterval(countdownInterval);
                alert('Thời gian giữ vé đã hết hạn!');
                
                // Clear stored data and redirect
                sessionStorage.removeItem('bookingData');
                localStorage.removeItem('bookingInfo');
                window.location.href = '/Movie';
            }
        }

        countdownInterval = setInterval(updateCountdown, 1000);
        updateCountdown(); // Update immediately
    }

    function formatCurrency(amount) {
        if (!amount) return '0 VNĐ';
        return new Intl.NumberFormat('vi-VN').format(amount) + ' VNĐ';
    }

    // ✅ Global functions
    window.selectPaymentMethod = function (method) {
        selectedMethod = method;

        // Remove previous selection
        document.querySelectorAll('.method-option').forEach(option => {
            option.classList.remove('selected');
        });

        // Add selection to clicked method
        event.currentTarget.classList.add('selected');

        // Enable payment button
        const payBtn = document.getElementById('payBtn');
        if (payBtn) {
            payBtn.disabled = false;
            payBtn.textContent = 'THANH TOÁN';
        }

        console.log('✅ Payment method selected:', method);
    };

    window.goBack = function () {
        if (confirm('Bạn có chắc muốn quay lại? Thông tin đặt vé vẫn được giữ.')) {
            if (countdownInterval) {
                clearInterval(countdownInterval);
            }
            window.history.back();
        }
    };

    window.proceedPayment = async function () {
        if (!selectedMethod) {
            alert('Vui lòng chọn phương thức thanh toán!');
            return;
        }

        // Show loading state
        const payBtn = document.getElementById('payBtn');
        if (payBtn) {
            payBtn.textContent = 'ĐANG XỬ LÝ...';
            payBtn.disabled = true;
        }

        // Clear countdown interval
        if (countdownInterval) {
            clearInterval(countdownInterval);
        }

        // ✅ Lấy booking info
        const bookingInfo = JSON.parse(localStorage.getItem('bookingInfo') || '{}');
        const bookingData = JSON.parse(sessionStorage.getItem('bookingData') || '{}');

        try {
            if (selectedMethod === 'payOS') {
                // ✅ Tạo payment request cho PayOS
                const paymentRequest = {
                    orderCode: Date.now(),
                    amount: parseInt(bookingInfo.amount) || 0,
                    description: 'Ve xem phim',
                    buyerName: 'Customer',
                    buyerEmail: 'customer@email.com',
                    buyerPhone: '0000000000',
                    buyerAddress: 'Viet Nam',
                    items: [
                        {
                            name: bookingInfo.movieTitle || 'Ve xem phim',
                            quantity: parseInt(bookingInfo.quantity) || 1,
                            price: parseInt(bookingInfo.amount) || 0
                        }
                    ],
                    cancelUrl: `${window.location.origin}/Payment/PaymentCancel`,
                    returnUrl: `${window.location.origin}/Payment/PaymentSuccess`,
                    expiredAt: Math.floor(Date.now() / 1000) + (15 * 60),
                    // ✅ Thêm thông tin vé để xử lý sau khi thanh toán
                    bookingData: bookingData
                };

                console.log('✅ Payment Request:', paymentRequest);

                // Gọi API tạo payment link
                const response = await fetch('/Payment/CreatePayOsPayment', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(paymentRequest)
                });

                const result = await response.json();

                if (result.success && result.checkoutUrl) {
                    // Lưu order code để tracking
                    localStorage.setItem('currentOrderCode', paymentRequest.orderCode);
                    
                    // Redirect đến trang thanh toán PayOS
                    window.location.href = result.checkoutUrl;
                } else {
                    alert('Không thể tạo link thanh toán: ' + (result.message || 'Lỗi không xác định'));
                    if (payBtn) {
                        payBtn.textContent = 'THANH TOÁN';
                        payBtn.disabled = false;
                    }
                    startCountdown();
                }
            } else if (selectedMethod === 'momo') {
                // Redirect to MoMo payment
                window.location.href = '/Payment/MomoPayment';
            } else {
                alert('Phương thức thanh toán chưa được hỗ trợ!');
                if (payBtn) {
                    payBtn.textContent = 'THANH TOÁN';
                    payBtn.disabled = false;
                }
                startCountdown();
            }
        } catch (error) {
            console.error('❌ Payment error:', error);
            alert('Có lỗi xảy ra khi xử lý thanh toán. Vui lòng thử lại!');
            if (payBtn) {
                payBtn.textContent = 'THANH TOÁN';
                payBtn.disabled = false;
            }
            startCountdown();   
        }
    };

    // ✅ Cleanup
    window.addEventListener('beforeunload', function () {
        if (countdownInterval) {
            clearInterval(countdownInterval);
        }
    });

    window.addEventListener('popstate', function () {
        if (countdownInterval) {
            clearInterval(countdownInterval);
        }
    });
});