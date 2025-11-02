document.addEventListener('DOMContentLoaded', function () {
    let selectedMethod = null;
    let countdownInterval = null;
    let timeLeft = 0;

    // Load customer and booking info
    loadBookingInfo();

    // Start countdown
    startCountdown();

    function loadBookingInfo() {
        // Ưu tiên lấy dữ liệu từ server, sau đó fallback về localStorage
        let customerInfo = null;
        
        if (window.serverCustomerInfo) {
            customerInfo = window.serverCustomerInfo;
            timeLeft = customerInfo.timeLeft || 230;
            
            // Lưu vào localStorage để đồng bộ
            localStorage.setItem('customerInfo', JSON.stringify(customerInfo));
        } else {
            // Fallback về localStorage
            const storedInfo = localStorage.getItem('customerInfo');
            if (storedInfo) {
                customerInfo = JSON.parse(storedInfo);
                timeLeft = customerInfo.timeLeft || 230;
            }
        }

        if (!customerInfo) {
            alert('Không tìm thấy thông tin khách hàng. Vui lòng nhập lại.');
            window.location.href = '/Payment/Index';
            return;
        }

        console.log('Customer info loaded:', customerInfo);

        // Load booking info từ localStorage
        const bookingInfo = localStorage.getItem('bookingInfo');
        if (bookingInfo) {
            const booking = JSON.parse(bookingInfo);

            // Cập nhật UI với thông tin booking
            updateElement('movieTitle', booking.movieTitle);
            updateElement('cinemaName', booking.cinema);
            updateElement('showtime', booking.showtime);
            updateElement('room', booking.room);
            updateElement('quantity', booking.quantity);
            updateElement('ticketType', booking.ticketType);
            updateElement('seat', booking.seat);
            updateElement('totalAmount', formatCurrency(booking.amount));
        }
    }

    function updateElement(id, value) {
        const element = document.getElementById(id);
        if (element && value) {
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
            }

            if (timeLeft > 0) {
                timeLeft--;
                
                // Cập nhật thời gian trong localStorage
                const customerInfo = JSON.parse(localStorage.getItem('customerInfo') || '{}');
                customerInfo.timeLeft = timeLeft;
                localStorage.setItem('customerInfo', JSON.stringify(customerInfo));
            } else {
                clearInterval(countdownInterval);
                alert('Thời gian giữ vé đã hết hạn!');
                
                // Clear stored data and redirect
                localStorage.removeItem('customerInfo');
                localStorage.removeItem('bookingInfo');
                window.location.href = '/Home/Index';
            }
        }

        countdownInterval = setInterval(updateCountdown, 1000);
        updateCountdown(); // Update immediately
    }

    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN').format(amount) + ' VND';
    }

    // Global functions cho onclick events
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
    };

    window.goBack = function () {
        if (countdownInterval) {
            clearInterval(countdownInterval);
        }
        window.location.href = '/Payment/Index';
    };

    window.proceedPayment = function () {
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

        // Redirect based on selected method
        setTimeout(() => {
            switch (selectedMethod) {
                case 'momo':
                    window.location.href = '/Payment/MomoPayment';
                    break;
                default:
                    alert('Phương thức thanh toán chưa được hỗ trợ!');
                    if (payBtn) {
                        payBtn.textContent = 'THANH TOÁN';
                        payBtn.disabled = false;
                    }
                    startCountdown(); // Restart countdown
                    break;
            }
        }, 1000);
    };

    // Prevent page refresh from losing data
    window.addEventListener('beforeunload', function () {
        if (countdownInterval) {
            clearInterval(countdownInterval);
        }
    });

    // Handle browser back/forward buttons
    window.addEventListener('popstate', function () {
        if (countdownInterval) {
            clearInterval(countdownInterval);
        }
    });
});