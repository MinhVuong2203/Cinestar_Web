document.addEventListener('DOMContentLoaded', function () {
    // Countdown timer
    let timeLeft = 230; // 3 minutes 50 seconds

    function updateCountdown() {
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        const countdownElement = document.querySelector('.countdown');

        if (countdownElement) {
            countdownElement.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        }

        if (timeLeft > 0) {
            timeLeft--;
        } else {
            alert('Thời gian giữ vé đã hết hạn!');
            // Redirect về trang chủ khi hết thời gian
            window.location.href = '/Home/Index';
        }
    }

    // Update countdown every second
    const countdownInterval = setInterval(updateCountdown, 1000);
    updateCountdown(); // Update immediately

    // Real-time validation feedback
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

    // Store movie and booking info
    const bookingInfo = {
        movieTitle: 'CỤC VÀNG CỦA NGOẠI (T13)',
        cinema: 'Cinestar Satra Quận 6 (TP.HCM)',
        showtime: '14:10 Thứ Hai 03/11/2025',
        room: '03',
        seat: 'A04',
        ticketType: 'Người Lớn',
        quantity: 1,
        amount: 45000
    };

    localStorage.setItem('bookingInfo', JSON.stringify(bookingInfo));

    // Clean up interval when page unloads
    window.addEventListener('beforeunload', function () {
        clearInterval(countdownInterval);
    });
});