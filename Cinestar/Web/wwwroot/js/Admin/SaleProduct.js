const bookingData = {
    combos: {},
    products: {}, // Lưu thông tin sản phẩm
    // ✅ Customer info
    customerPhone: null,
    customerId: null,
    customerName: null,
    isGuest: true
};

let customerData = null;
let productList = []; // Danh sách sản phẩm từ server

// ✅ Setup customer input handlers
function setupCustomerInput() {
    const customerPhone = document.getElementById('customerPhone');
    const checkCustomerBtn = document.getElementById('checkCustomerBtn');
    const skipCustomerBtn = document.getElementById('skipCustomerBtn');

    console.log('Setting up customer input...');

    // Kiểm tra khách hàng
    if (checkCustomerBtn) {
        checkCustomerBtn.addEventListener('click', function () {
            const phone = customerPhone.value.trim();

            if (!phone) {
                alert('Vui lòng nhập số điện thoại!');
                return;
            }

            // Validate phone number
            const phoneRegex = /^(0[3|5|7|8|9])+([0-9]{8})$/;
            if (!phoneRegex.test(phone)) {
                alert('Số điện thoại không hợp lệ! (VD: 0912345678)');
                return;
            }

            checkCustomerBtn.disabled = true;
            checkCustomerBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang kiểm tra...';

            console.log('Checking customer with phone:', phone);

            //check customer
            fetch(`/Admin/EmployeeSale/CheckCustomerByPhone?phone=${phone}`)
                .then(response => {
                    console.log('Response status:', response.status);
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('API Response:', data);

                    if (data.success) {
                        // Customer found
                        customerData = data.customer;
                        bookingData.customerId = data.customer.customerId;
                        bookingData.customerPhone = phone;
                        bookingData.customerName = data.customer.fullName;
                        bookingData.isGuest = false;

                        console.log('Customer found:', customerData);
                        displayCustomerInfo(customerData);
                    } else {
                        // Customer not found
                        console.log('Customer not found');
                        if (confirm('Không tìm thấy khách hàng. Tiếp tục với khách vãng lai?')) {
                            proceedAsGuest();
                        }
                    }
                })
                .catch(error => {
                    console.error('Error checking customer:', error);
                    alert('Có lỗi xảy ra khi kiểm tra khách hàng. Vui lòng thử lại.');
                })
                .finally(() => {
                    checkCustomerBtn.disabled = false;
                    checkCustomerBtn.innerHTML = '<i class="fas fa-search"></i> Kiểm tra';
                });
        });
    } else {
        console.error('❌ checkCustomerBtn not found!');
    }

    // Skip customer - proceed as guest
    if (skipCustomerBtn) {
        skipCustomerBtn.addEventListener('click', function () {
            console.log('Skip customer - proceed as guest');
            proceedAsGuest();
        });
    } else {
        console.error('❌ skipCustomerBtn not found!');
    }

    // Enter key to check
    if (customerPhone) {
        customerPhone.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                checkCustomerBtn.click();
            }
        });
    } else {
        console.error('❌ customerPhone input not found!');
    }

    console.log('✅ Customer input setup completed');
}

// ✅ Display customer info
function displayCustomerInfo(customer) {
    console.log('=== displayCustomerInfo called ===');
    console.log('Customer data received:', customer);

    const nameEl = document.getElementById('displayCustomerName');
    const emailEl = document.getElementById('displayCustomerEmail');
    const pointEl = document.getElementById('displayCustomerPoint');
    const vipEl = document.getElementById('displayCustomerVip');
    const infoDisplay = document.getElementById('customerInfoDisplay');
    const guestDisplay = document.getElementById('guestCustomerDisplay');

    if (!nameEl || !emailEl || !pointEl || !vipEl || !infoDisplay) {
        console.error('❌ Không tìm thấy các element customer info!');
        alert('Có lỗi xảy ra khi hiển thị thông tin khách hàng. Vui lòng tải lại trang.');
        return;
    }

    // Set values
    nameEl.textContent = customer.fullName || customer.FullName || '-';
    emailEl.textContent = customer.email || customer.Email || '-';
    pointEl.textContent = customer.point || customer.Point || 0;

    const vipLevel = customer.vipLevel || customer.VipLevel || 0;
    vipEl.textContent = `VIP ${vipLevel}`;
    vipEl.className = `customer-vip-badge ${vipLevel > 0 ? 'vip-active' : 'vip-inactive'}`;

    // Hide guest display first
    if (guestDisplay) {
        guestDisplay.style.display = 'none';
    }

    // Show customer info
    infoDisplay.style.display = 'block';
    console.log('✅ Customer info displayed');

    // Update order summary
    updateOrderSummary();

    console.log('✅ displayCustomerInfo completed successfully');
}

// ✅ Proceed as guest
function proceedAsGuest() {
    console.log('=== proceedAsGuest called ===');

    bookingData.isGuest = true;
    bookingData.customerId = "VL-" + Date.now();
    bookingData.customerPhone = null;
    bookingData.customerName = 'Khách vãng lai';

    const infoDisplay = document.getElementById('customerInfoDisplay');
    const guestDisplay = document.getElementById('guestCustomerDisplay');

    if (infoDisplay) {
        infoDisplay.style.display = 'none';
    }
    if (guestDisplay) {
        guestDisplay.style.display = 'block';
        console.log('✅ Guest display shown');
    }

    // Update order summary
    updateOrderSummary();

    console.log('✅ proceedAsGuest completed');
}

// ✅ Update order summary with customer info and products
function updateOrderSummary() {
    const orderDetail = document.getElementById('order-detail');
    const orderTotal = document.getElementById('order-total');
    const orderProducts = document.getElementById('order-products');

    if (!orderDetail) {
        console.error('❌ order-detail not found!');
        return;
    }

    const customerName = bookingData.isGuest ? 'Khách vãng lai' : (bookingData.customerName || 'Khách vãng lai');

    // Update customer info
    let customerRow = orderDetail.querySelector('.customer-row');
    if (!customerRow) {
        customerRow = document.createElement('div');
        customerRow.className = 'row customer-row mb-2';
        customerRow.innerHTML = `
            <div class="col-6 text-secondary">Khách hàng:</div>
            <div class="col-6 text-end" id="order-customer">${customerName}</div>
        `;
        orderDetail.insertBefore(customerRow, orderDetail.firstChild);
    } else {
        const orderCustomer = document.getElementById('order-customer');
        if (orderCustomer) {
            orderCustomer.textContent = customerName;
        }
    }

    // Calculate products and total
    let productsList = [];
    let total = 0;

    for (const [productId, quantity] of Object.entries(bookingData.combos)) {
        if (quantity > 0) {
            const product = bookingData.products[productId];
            if (product) {
                const subtotal = product.price * quantity;
                total += subtotal;
                productsList.push(`${product.name} x${quantity}`);
            }
        }
    }

    // Update products display
    if (orderProducts) {
        if (productsList.length > 0) {
            orderProducts.innerHTML = productsList.join('<br>');
        } else {
            orderProducts.textContent = 'Không có';
        }
    }

    // ✅ Update total - CHỈ HIỂN THỊ SỐ NGUYÊN
    if (orderTotal) {
        orderTotal.textContent = `TỔNG CỘNG: ${formatPrice(total)}`;
    }

    console.log('✅ Order summary updated:', { customerName, products: productsList, total });
}

// ✅ Change combo quantity
function changeComboQuantity(productId, delta) {
    console.log(`changeComboQuantity called: productId=${productId}, delta=${delta}`);

    // Initialize quantity if not exists
    if (!bookingData.combos[productId]) {
        bookingData.combos[productId] = 0;
    }

    // Update quantity (không cho phép giá trị âm)
    bookingData.combos[productId] = Math.max(0, bookingData.combos[productId] + delta);

    console.log(`New quantity for ${productId}: ${bookingData.combos[productId]}`);

    // Update UI - quantity display
    const qtyElement = document.querySelector(`[data-product-id="${productId}"] .qty-value`);
    if (qtyElement) {
        qtyElement.textContent = bookingData.combos[productId];
        console.log('✅ Quantity display updated');
    } else {
        console.error('❌ Quantity element not found for productId:', productId);
    }

    // Update order summary
    updateOrderSummary();
}

// ✅ Setup product cards
function setupProductCards() {
    console.log('=== Setting up product cards ===');

    const productCards = document.querySelectorAll('.product-card');
    console.log(`Found ${productCards.length} product cards`);

    productCards.forEach((card) => {
        const productTitle = card.querySelector('.product-title')?.textContent.trim();
        const productPriceText = card.querySelector('.product-price')?.textContent.trim();
        // ✅ Parse giá tiền, loại bỏ ký tự không phải số
        const productPrice = parseInt(productPriceText?.replace(/[^0-9]/g, '') || '0');
        const productId = card.dataset.productId || `product-${Math.random().toString(36).substr(2, 9)}`;

        // Set product ID
        card.dataset.productId = productId;

        // ✅ Store product info với giá đã parse thành integer
        bookingData.products[productId] = {
            name: productTitle,
            price: productPrice
        };

        console.log(`Product setup: ${productId}`, bookingData.products[productId]);

        // Get buttons
        const minusBtn = card.querySelector('.qty-btn:first-of-type');
        const plusBtn = card.querySelector('.qty-btn:last-of-type');
        const qtyValue = card.querySelector('.qty-value');

        if (minusBtn && plusBtn && qtyValue) {
            // Remove old listeners
            const newMinusBtn = minusBtn.cloneNode(true);
            const newPlusBtn = plusBtn.cloneNode(true);
            minusBtn.replaceWith(newMinusBtn);
            plusBtn.replaceWith(newPlusBtn);

            // Add new listeners
            newMinusBtn.addEventListener('click', function (e) {
                e.preventDefault();
                console.log('Minus button clicked for:', productId);
                changeComboQuantity(productId, -1);
            });

            newPlusBtn.addEventListener('click', function (e) {
                e.preventDefault();
                console.log('Plus button clicked for:', productId);
                changeComboQuantity(productId, 1);
            });

            console.log(`✅ Buttons setup for ${productId}`);
        } else {
            console.error(`❌ Missing buttons for ${productId}`, { minusBtn, plusBtn, qtyValue });
        }
    });

    console.log('✅ Product cards setup completed');
}

// ✅ Setup confirm button - SỬA LẠI CHO BÁN SẢN PHẨM
function setupConfirmButton() {
    const confirmBtn = document.getElementById('btn-confirm');

    if (!confirmBtn) {
        console.error('❌ Confirm button not found!');
        return;
    }

    confirmBtn.addEventListener('click', async function () {
        console.log('=== Confirm button clicked ===');

        // Validate: Phải có ít nhất 1 sản phẩm
        let totalProducts = 0;
        for (const quantity of Object.values(bookingData.combos)) {
            totalProducts += quantity;
        }

        if (totalProducts === 0) {
            alert('Vui lòng chọn ít nhất một sản phẩm!');
            return;
        }

        // ✅ Tính tổng tiền - CHUYỂN THÀNH INTEGER
        let totalAmount = 0;
        const products = [];

        for (const [productId, quantity] of Object.entries(bookingData.combos)) {
            if (quantity > 0) {
                const product = bookingData.products[productId];
                if (product) {
                    const subtotal = product.price * quantity;
                    totalAmount += subtotal;

                    products.push({
                        productId: productId,
                        quantity: quantity,
                        price: product.price // ✅ Đã là integer
                    });
                }
            }
        }

        // ✅ Đảm bảo totalAmount là integer
        totalAmount = Math.floor(totalAmount);

        // ✅ Tạo request data - CHỈ CÓ PRODUCTS, KHÔNG CÓ TICKETS
        const requestData = {
            customerId: bookingData.customerId,
            customerPhone: bookingData.customerPhone,
            customerName: bookingData.customerName,
            isGuest: bookingData.isGuest,
            products: products,
            totalAmount: totalAmount,
            // ✅ ĐẶC BIỆT: Không có movieId, showTimeId, seats, tickets
            movieId: null,
            showTimeId: null,
            seats: [],
            tickets: []
        };

        console.log('Request data:', requestData);

        // Disable button
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';

        try {
            // ✅ Gọi API tạo booking (sẽ cần sửa lại controller)
            const response = await fetch('/Admin/EmployeeSale/CreateProductBooking', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            });

            const result = await response.json();

            if (result.success) {
                //alert('Đặt hàng thành công!');
                // Redirect to payment method page
                window.location.href = `/Admin/EmployeeSale/ProductPaymentMethod?invoiceId=${result.invoiceId}`;
            } else {
                alert('Đặt hàng thất bại: ' + (result.message || 'Vui lòng thử lại'));
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.');
        } finally {
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = '<i class="bi bi-check-circle"></i> XÁC NHẬN ĐẶT HÀNG';
        }
    });

    console.log('✅ Confirm button setup completed');
}

// ✅ Format price helper - CHỈ HIỂN THỊ SỐ NGUYÊN
function formatPrice(price) {
    // ✅ Chuyển về integer trước khi format
    const intPrice = Math.floor(price);
    return new Intl.NumberFormat('vi-VN').format(intPrice) + ' ₫';
}

// ✅ Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('=== DOM Content Loaded ===');
    console.log('Initializing SaleProduct page...');

    setupCustomerInput();
    setupProductCards();
    setupConfirmButton();

    console.log('✅ SaleProduct page initialized');
});