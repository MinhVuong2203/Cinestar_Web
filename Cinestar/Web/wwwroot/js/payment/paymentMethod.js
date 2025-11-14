document.addEventListener('DOMContentLoaded', function () {
    console.log('=== PAYMENT METHOD PAGE LOADED ===');

    let selectedMethod = null;
    let countdownInterval = null;
    let timeLeft = 0;

    // ✅ Load booking info từ sessionStorage (KHÔNG PHẢI localStorage)
    loadBookingInfo();

    // ✅ Start countdown
    startCountdown();

    function loadBookingInfo() {
        const bookingInfoStr = sessionStorage.getItem('bookingInfo'); // ✅ ĐỔI TỪ localStorage

        if (!bookingInfoStr) {
            console.error('❌ No booking info found!');
            alert('Không tìm thấy thông tin đặt vé. Vui lòng đặt vé lại!');
            window.location.href = '/Movie';
            return;
        }

        const bookingInfo = JSON.parse(bookingInfoStr);
        console.log('✅ Booking Info loaded:', bookingInfo);

        // ✅ Lấy timeLeft
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

        updateElement('showtime', `${bookingInfo.showtime} ${bookingInfo.showDate || ''}`);
        updateElement('room', bookingInfo.room);
        updateElement('quantity', bookingInfo.quantity);
        updateElement('ticketType', bookingInfo.ticketType);
        updateElement('seat', bookingInfo.seat);

        // ✅ Hiển thị products
        displayProducts(bookingInfo.products || []);

        // ✅ Hiển thị tổng tiền
        const totalPriceElements = document.querySelectorAll('#totalAmount, .total-price');
        totalPriceElements.forEach(el => {
            el.textContent = formatCurrency(bookingInfo.amount);
        });

        console.log('✅ UI updated successfully');
        console.log('💰 Total amount:', bookingInfo.amount);
    }

    function displayProducts(products) {
        const foodSection = document.querySelector('.food-section');

        if (!foodSection) {
            console.warn('⚠️ Food section not found');
            return;
        }

        foodSection.innerHTML = '';

        if (!products || products.length === 0) {
            foodSection.innerHTML = '<div class="food-label">Bắp nước</div><div class="no-products">Không có</div>';
            return;
        }

        let html = '<div class="food-label">Bắp nước</div>';
        html += '<div class="product-list">';

        products.forEach(product => {
            html += `
                <div class="product-item">
                    <span class="product-name">${product.productName} x${product.quantity}</span>
                    <span class="product-price">${formatCurrency(product.totalPrice)}</span>
                </div>
            `;
        });

        html += '</div>';
        foodSection.innerHTML = html;

        console.log('✅ Products displayed:', products);
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

                if (timeLeft <= 60) {
                    countdownElement.style.color = '#ff3333';
                }
            }

            if (timeLeft > 0) {
                timeLeft--;

                // ✅ ĐỔI TỪ localStorage SANG sessionStorage
                const bookingInfo = JSON.parse(sessionStorage.getItem('bookingInfo') || '{}');
                bookingInfo.timeLeft = timeLeft;
                sessionStorage.setItem('bookingInfo', JSON.stringify(bookingInfo));
            } else {
                clearInterval(countdownInterval);
                alert('Thời gian giữ vé đã hết hạn!');

                sessionStorage.removeItem('bookingData');
                sessionStorage.removeItem('bookingInfo');
                window.location.href = '/Movie';
            }
        }

        countdownInterval = setInterval(updateCountdown, 1000);
        updateCountdown();
    }

    function formatCurrency(amount) {
        if (!amount || isNaN(amount)) return '0 VNĐ';
        return new Intl.NumberFormat('vi-VN').format(Math.floor(amount)) + ' VNĐ';
    }

    // ✅ Sửa phần proceedPayment
    window.proceedPayment = async function () {
        if (!selectedMethod) {
            alert('Vui lòng chọn phương thức thanh toán!');
            return;
        }

        const payBtn = document.getElementById('payBtn');
        if (payBtn) {
            payBtn.textContent = 'ĐANG XỬ LÝ...';
            payBtn.disabled = true;
        }

        if (countdownInterval) {
            clearInterval(countdownInterval);
        }

        // ✅ ĐỔI TỪ localStorage SANG sessionStorage
        const bookingInfo = JSON.parse(sessionStorage.getItem('bookingInfo') || '{}');
        const bookingData = JSON.parse(sessionStorage.getItem('bookingData') || '{}');

        console.log('=== PROCEED PAYMENT ===');
        console.log('Booking Info:', bookingInfo);
        console.log('Booking Data:', bookingData);

        try {
            if (selectedMethod === 'payOS') {
                // ✅ Tạo items list
                const items = [];

                // Thêm vé
                if (bookingData.seats && bookingData.seats.length > 0) {
                    items.push({
                        name: `Ve ${bookingInfo.movieTitle?.substring(0, 15) || 'phim'}`,
                        quantity: bookingData.seats.length,
                        price: Math.floor(bookingData.seatsTotal || 0)
                    });
                }

                // Thêm products
                if (bookingData.products && bookingData.products.length > 0) {
                    bookingData.products.forEach(product => {
                        items.push({
                            name: product.productName?.substring(0, 20) || 'Product',
                            quantity: product.quantity,
                            price: Math.floor(product.price || 0)
                        });
                    });
                }

                // ✅ Tính tổng tiền
                const totalAmount = Math.floor(bookingInfo.amount || 0);

                // ✅ Rút ngắn description (max 25 ký tự)
                const shortTitle = bookingInfo.movieTitle?.substring(0, 15) || 'Phim';
                const description = `Ve ${shortTitle}`;

                // ✅ Chuẩn bị BookingData
                const bookingDataPayload = {
                    showTimeId: bookingData.showTimeId || '',
                    customerId: bookingData.customerId || '00000000-0000-0000-0000-000000000000',
                    customerName: bookingData.customerName || '',
                    customerPhone: bookingData.customerPhone || '',
                    customerEmail: bookingData.customerEmail || '',
                    seats: (bookingData.seats || []).map(seat => ({
                        seatId: seat.seatId || '',
                        ticketId: seat.ticketId || '',
                        seatName: seat.seatName || '',
                        price: seat.price || 0
                    })),
                    products: (bookingData.products || []).map(product => ({
                        productId: product.productId || '',
                        productName: product.productName || '',
                        quantity: product.quantity || 0,
                        price: product.price || 0
                    }))
                };

                // ✅ Tạo payment request
                const paymentRequest = {
                    orderCode: Date.now(),
                    amount: totalAmount,
                    description: description, // ✅ Tối đa 25 ký tự
                    buyerName: 'Customer',
                    buyerEmail: 'customer@email.com',
                    buyerPhone: '0000000000',
                    buyerAddress: 'Viet Nam',
                    items: items,
                    cancelUrl: `${window.location.origin}/Payment/PaymentCancel`,
                    returnUrl: `${window.location.origin}/Payment/PaymentSuccess`,
                    expiredAt: Math.floor(Date.now() / 1000) + (15 * 60),
                    bookingData: bookingDataPayload
                };

                console.log('✅ Payment Request:', paymentRequest);
                console.log('💰 Total Amount:', totalAmount);
                console.log('📝 Description length:', description.length);

                // Gọi API
                const response = await fetch('/Payment/CreatePayOsPayment', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(paymentRequest)
                });

                //const result = await response.json();
                let result;

                // ✅ THÊM ĐOẠN NÀY ĐỂ DEBUG
                console.log('Response status:', response.status);
                console.log('Response ok:', response.ok);

                try {
                    result = await response.json(); // ✅ Chỉ gọi .json() một lần
                    console.log('✅ API Result:', result);
                } catch (error) {
                    console.error('❌ Failed to parse JSON:', error);
                    const responseText = await response.text();
                    console.error('Response text:', responseText);
                    alert('Lỗi server: Không thể parse JSON response');

                    if (payBtn) {
                        payBtn.textContent = 'THANH TOÁN';
                        payBtn.disabled = false;
                    }
                    startCountdown();
                    return;
                }

                if (result.success && result.checkoutUrl) {
                    localStorage.setItem('currentOrderCode', paymentRequest.orderCode);
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

    // ✅ Global functions
    window.selectPaymentMethod = function (method) {
        selectedMethod = method;

        document.querySelectorAll('.method-option').forEach(option => {
            option.classList.remove('selected');
        });

        event.currentTarget.classList.add('selected');

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