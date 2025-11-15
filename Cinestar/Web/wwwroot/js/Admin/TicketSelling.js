// Verify movieId is defined from inline script
let movieIdFromScript = typeof movieId !== 'undefined' ? movieId : null;

console.log('=== Initialization ===');
console.log('movieId from inline script:', movieIdFromScript);

const bookingData = {
    movieId: movieIdFromScript,
    branchId: null,
    showTime: null,
    tickets: {},
    seats: [],
    date: null,
    time: null,
    roomName: null,
    roomType: null,
    combos: {},
    // ✅ Customer info
    customerPhone: null,
    customerId: null,
    customerName: null,
    isGuest: true
};

let ticketPrices = {};
let selectedSeats = [];
let customerData = null;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing...');

    // ✅ Đợi DOM render xong rồi mới setup
    setTimeout(() => {
        setupCustomerInput();
        initializeTicketTypes();
        setupTimeSlotSelection();
        console.log('✅ All handlers initialized');
    }, 100);
});

// ✅ Setup customer input handlers
function setupCustomerInput() {
    const customerPhone = document.getElementById('customerPhone');
    const checkCustomerBtn = document.getElementById('checkCustomerBtn');
    const skipCustomerBtn = document.getElementById('skipCustomerBtn');

    console.log('Setting up customer input...');
    console.log('customerPhone element:', customerPhone);
    console.log('checkCustomerBtn element:', checkCustomerBtn);
    console.log('skipCustomerBtn element:', skipCustomerBtn);

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

                        console.log('Customer found:', data.customer);
                        displayCustomerInfo(data.customer);
                        showStep2();
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
    console.log('Customer data:', customer);

    // ✅ Kiểm tra từng element
    const nameEl = document.getElementById('displayCustomerName');
    const emailEl = document.getElementById('displayCustomerEmail');
    const pointEl = document.getElementById('displayCustomerPoint');
    const vipEl = document.getElementById('displayCustomerVip');
    const infoDisplay = document.getElementById('customerInfoDisplay');
    const guestDisplay = document.getElementById('guestCustomerDisplay');

    console.log('Elements check:');
    console.log('- nameEl:', nameEl);
    console.log('- emailEl:', emailEl);
    console.log('- pointEl:', pointEl);
    console.log('- vipEl:', vipEl);
    console.log('- infoDisplay:', infoDisplay);
    console.log('- guestDisplay:', guestDisplay);

    if (!nameEl || !emailEl || !pointEl || !vipEl) {
        console.error('❌ Không tìm thấy các element customer info!');
        console.error('Missing elements:', {
            name: !nameEl,
            email: !emailEl,
            point: !pointEl,
            vip: !vipEl
        });

        alert('Có lỗi xảy ra khi hiển thị thông tin khách hàng. Vui lòng tải lại trang.');
        return;
    }

    // Set values
    nameEl.textContent = customer.fullName || '-';
    emailEl.textContent = customer.email || '-';
    pointEl.textContent = customer.point || 0;

    const vipLevel = customer.vipLevel || 0;
    vipEl.textContent = `VIP ${vipLevel}`;
    vipEl.className = `badge ${vipLevel > 0 ? 'bg-warning' : 'bg-secondary'}`;

    if (infoDisplay) {
        infoDisplay.style.display = 'block';
        console.log('✅ Customer info displayed');
    }
    if (guestDisplay) {
        guestDisplay.style.display = 'none';
    }

    // Update summary
    const summaryCustomer = document.getElementById('summary-customer');
    if (summaryCustomer) {
        summaryCustomer.textContent = customer.fullName;
        console.log('✅ Summary updated with customer name');
    }

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
    const summaryCustomer = document.getElementById('summary-customer');

    if (infoDisplay) infoDisplay.style.display = 'none';
    if (guestDisplay) {
        guestDisplay.style.display = 'block';
        console.log('✅ Guest display shown');
    }
    if (summaryCustomer) {
        summaryCustomer.textContent = 'Khách vãng lai';
        console.log('✅ Summary updated for guest');
    }

    showStep2();
    console.log('✅ proceedAsGuest completed');
}

// ✅ Show step 2
function showStep2() {
    console.log('=== showStep2 called ===');
    const step2 = document.getElementById('step2');
    if (step2) {
        step2.classList.remove('hidden');
        console.log('✅ Step 2 shown');
        setTimeout(() => {
            step2.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    } else {
        console.error('❌ step2 element not found!');
    }

    const timeSlots = document.querySelectorAll('.time-slot');

    timeSlots.forEach(slot => {
        slot.addEventListener('click', function (e) {
            const isExpired = this.getAttribute('data-is-expired') === 'true';

            // Ngăn chặn click vào slot đã hết hạn
            if (isExpired) {
                e.preventDefault();
                e.stopPropagation();
                alert('Suất chiếu này đã hết hạn đặt vé.\nVui lòng chọn suất chiếu khác (ít nhất 15 phút trước giờ chiếu).');
                return false;
            }

            // Xóa class active khỏi tất cả các slot khác
            timeSlots.forEach(s => s.classList.remove('active'));

            // Thêm class active vào slot được chọn
            this.classList.add('active');

            // Lấy thông tin từ data attributes
            const showTimeId = this.getAttribute('data-showtime-id');
            const time = this.getAttribute('data-time');
            const room = this.getAttribute('data-room');
            const roomType = this.getAttribute('data-room-type');

            console.log('Đã chọn suất chiếu:', {
                showTimeId,
                time,
                room,
                roomType
            });

        });
    });
}

// Initialize ticket types and prices from server data
function initializeTicketTypes() {
    const ticketContents = document.querySelectorAll('#ticketOptions .content');

    ticketContents.forEach(content => {
        const ticketType = content.dataset.ticketType;
        const price = parseFloat(content.dataset.price);
        const available = parseInt(content.dataset.available || '0');

        if (ticketType && !isNaN(price)) {
            ticketPrices[ticketType] = price;
            bookingData.tickets[ticketType] = 0;
            console.log(`Initialized ${ticketType}: price=${price}, available=${available}`);
        }
    });

    console.log('Ticket prices:', ticketPrices);
    setupTicketQuantityControls();
}

// Setup ticket quantity controls
function setupTicketQuantityControls() {
    const ticketContents = document.querySelectorAll('#ticketOptions .content');

    ticketContents.forEach(content => {
        const decreaseBtn = content.querySelector('.decrease');
        const increaseBtn = content.querySelector('.increase');
        const quantitySpan = content.querySelector('.quantity');
        const ticketType = content.dataset.ticketType;
        const maxAvailable = parseInt(content.dataset.available || '999');

        if (decreaseBtn && increaseBtn && quantitySpan && ticketType) {
            decreaseBtn.addEventListener('click', () => {
                changeQuantity(ticketType, -1, quantitySpan, decreaseBtn, increaseBtn, maxAvailable);
            });

            increaseBtn.addEventListener('click', () => {
                changeQuantity(ticketType, 1, quantitySpan, decreaseBtn, increaseBtn, maxAvailable);
            });
        }
    });
}

// Setup time slot selection
function setupTimeSlotSelection() {
    const timeSlots = document.querySelectorAll('.time-slot');

    timeSlots.forEach(slot => {
        slot.addEventListener('click', function () {
            timeSlots.forEach(s => s.classList.remove('selected'));
            this.classList.add('selected');

            const showTimeId = this.dataset.showtimeId;
            const movieId = this.dataset.movieId;
            const branchId = this.dataset.branchId;
            const time = this.dataset.time;
            const room = this.dataset.room;
            const roomType = this.dataset.roomType;
            const date = this.dataset.date || new Date().toISOString().split('T')[0];

            bookingData.showTime = showTimeId;
            bookingData.time = time;
            bookingData.roomName = room;
            bookingData.roomType = roomType;
            bookingData.date = date;

            console.log('Selected showtime:', { showTimeId, movieId, time, room, roomType, date });

            updateRoomTitle(room, roomType);
            loadTicketTypes(showTimeId, movieId, branchId);
            loadSeatingLayout(showTimeId);
            showStep3();
            showStep5();
            updateSummary();
        });
    });
}

// Load ticket types
function loadTicketTypes(showTimeId, movieId, branchId) {
    const ticketOptions = document.getElementById('ticketOptions');

    ticketOptions.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-3 text-muted">Đang tải thông tin vé...</p>
        </div>
    `;

    const url = `/Admin/EmployeeSale/GetTicketTypesByShowTime?showTimeId=${showTimeId}&movieId=${movieId}&branchId=${branchId}`;

    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success && data.ticketTypes) {
                renderTicketTypes(data.ticketTypes);
                initializeTicketTypes();
            } else {
                ticketOptions.innerHTML = `
                    <div class="alert alert-warning text-center">
                        <i class="fas fa-exclamation-triangle"></i>
                        ${data.message || 'Không thể tải thông tin vé'}
                    </div>
                `;
            }
        })
        .catch(error => {
            console.error('Error fetching ticket types:', error);
            ticketOptions.innerHTML = `
                <div class="alert alert-danger text-center">
                    <i class="fas fa-exclamation-circle"></i>
                    Có lỗi xảy ra khi tải thông tin vé: ${error.message}
                </div>
            `;
        });
}

// Render ticket types HTML
function renderTicketTypes(ticketTypes) {
    const ticketOptions = document.getElementById('ticketOptions');

    if (!ticketTypes || (!ticketTypes.standard && !ticketTypes.vip && !ticketTypes.couple)) {
        ticketOptions.innerHTML = `
            <div class="alert alert-warning text-center">
                <i class="fas fa-exclamation-triangle"></i>
                Không có vé nào còn trống cho suất chiếu này
            </div>
        `;
        return;
    }

    let html = `
        <div class="ticket-wr">
            <h2 class="heading">CHỌN LOẠI VÉ</h2>
            <div class="ticket-container">
    `;

    // Standard ticket
    if (ticketTypes.standard) {
        const standard = ticketTypes.standard;
        html += `
            <div class="content" data-ticket-type="standard" 
                 data-price="${standard.price}" 
                 data-available="${standard.availableCount}">
                <div class="content-top">
                    <p class="name">
                        <i class="${standard.icon}"></i>
                        ${standard.name}
                    </p>
                    <div class="desc">
                        <p>${standard.description}</p>
                        <p class="text-muted small">Còn lại: ${standard.availableCount} vé</p>
                    </div>
                    <div class="price">
                        <p>${standard.price.toLocaleString('vi-VN')} VNĐ</p>
                    </div>
                </div>
                <div class="content-bottom">
                    <div class="count">
                        <div class="count-btn">
                            <button class="decrease" disabled>-</button>
                            <span class="quantity">0</span>
                            <button class="increase">+</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // VIP ticket
    if (ticketTypes.vip) {
        const vip = ticketTypes.vip;
        html += `
            <div class="content" data-ticket-type="vip" 
                 data-price="${vip.price}" 
                 data-available="${vip.availableCount}">
                <div class="content-top">
                    <p class="name">
                        <i class="${vip.icon}"></i>
                        ${vip.name}
                    </p>
                    <div class="desc">
                        <p>${vip.description}</p>
                        <p class="text-muted small">Còn lại: ${vip.availableCount} vé</p>
                    </div>
                    <div class="price">
                        <p>${vip.price.toLocaleString('vi-VN')} VNĐ</p>
                    </div>
                </div>
                <div class="content-bottom">
                    <div class="count">
                        <div class="count-btn">
                            <button class="decrease" disabled>-</button>
                            <span class="quantity">0</span>
                            <button class="increase">+</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Couple ticket
    if (ticketTypes.couple) {
        const couple = ticketTypes.couple;
        html += `
            <div class="content" data-ticket-type="couple" 
                 data-price="${couple.price}" 
                 data-available="${couple.availableCount}">
                <div class="content-top">
                    <p class="name">
                        <i class="${couple.icon}"></i>
                        ${couple.name}
                    </p>
                    <div class="desc">
                        <p>${couple.description}</p>
                        <p class="text-muted small">Còn lại: ${couple.availableCount} vé</p>
                    </div>
                    <div class="price">
                        <p>${couple.price.toLocaleString('vi-VN')} VNĐ</p>
                    </div>
                </div>
                <div class="content-bottom">
                    <div class="count">
                        <div class="count-btn">
                            <button class="decrease" disabled>-</button>
                            <span class="quantity">0</span>
                            <button class="increase">+</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    html += `
            </div>
        </div>
    `;

    ticketOptions.innerHTML = html;
}

function updateRoomTitle(roomName, roomType) {
    const roomTitle = document.getElementById('roomTitle');
    if (roomTitle) {
        roomTitle.textContent = `Chọn ghế - ${roomName}`;
        if (roomType) {
            roomTitle.textContent += ` (${roomType})`;
        }
    }
}

function showStep3() {
    const step3 = document.getElementById('step3');
    if (step3) {
        step3.classList.remove('hidden');
        setTimeout(() => {
            step3.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}

function changeQuantity(type, delta, quantityElement, decreaseBtn, increaseBtn, maxAvailable) {
    if (!bookingData.tickets.hasOwnProperty(type)) {
        bookingData.tickets[type] = 0;
    }

    const newQuantity = Math.max(0, Math.min(bookingData.tickets[type] + delta, maxAvailable));
    bookingData.tickets[type] = newQuantity;

    if (quantityElement) {
        quantityElement.textContent = newQuantity;
    }

    if (decreaseBtn) {
        decreaseBtn.disabled = newQuantity <= 0;
    }
    if (increaseBtn) {
        increaseBtn.disabled = newQuantity >= maxAvailable;
    }

    const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);

    //if (totalTickets > 0) {
    //    showStep5();
    //} else {
    //    const step5 = document.getElementById('step5');
    //    if (step5) {
    //        step5.classList.add('hidden');
    //    }
    //}

    updateSummary();
}

function showStep5() {
    const step5 = document.getElementById('step5');
    if (step5) {
        step5.classList.remove('hidden');
        setTimeout(() => {
            step5.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}

function loadSeatingLayout(showTimeId) {
    fetch(`/Admin/EmployeeSale/GetSeatingLayout?showTimeId=${showTimeId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                renderSeats(data.seats);
            } else {
                alert('Không thể tải danh sách ghế. Vui lòng thử lại.');
            }
        })
        .catch(error => {
            console.error('Error fetching seats:', error);
            alert('Có lỗi xảy ra khi tải danh sách ghế.');
        });
}

function renderSeats(seats) {
    const table = document.getElementById('seatingTable');
    const rows = {};

    seats.forEach(seat => {
        const row = seat.seatName[0];
        if (!rows[row]) rows[row] = [];
        rows[row].push(seat);
    });

    table.innerHTML = '';

    for (let rowName in rows) {
        const tr = document.createElement('tr');
        const tdLabel = document.createElement('td');
        tdLabel.className = 'row-label';
        tdLabel.textContent = rowName;
        tr.appendChild(tdLabel);

        const totalCols = 21;
        const seatCount = rows[rowName].length;
        const emptyBefore = Math.floor((totalCols - seatCount) / 2);

        for (let i = 0; i < emptyBefore; i++) {
            const tdEmpty = document.createElement('td');
            tdEmpty.className = 'empty';
            tr.appendChild(tdEmpty);
        }

        rows[rowName].forEach(seat => {
            const td = document.createElement('td');
            td.textContent = seat.seatName;
            td.dataset.seatId = seat.seatId;

            let seatClass = 'seat';
            if (seat.status === 'Đã đặt') {
                seatClass += ' booked';
            } else {
                if (seat.seatType === 'Ghế Couple' || seat.seatType === 'Ghế đôi') {
                    seatClass += ' couple';
                } else if (seat.seatType === 'Ghế VIP') {
                    seatClass += ' vip';
                } else {
                    seatClass += ' regular';
                }
            }

            td.className = seatClass;
            tr.appendChild(td);
        });

        const emptyAfter = totalCols - seatCount - emptyBefore;
        for (let i = 0; i < emptyAfter; i++) {
            const tdEmpty = document.createElement('td');
            tdEmpty.className = 'empty';
            tr.appendChild(tdEmpty);
        }

        table.appendChild(tr);
    }

    selectedSeats = [];
    bookingData.seats = [];
    setupSeatClickEvents();
}

function setupSeatClickEvents() {
    const seats = document.querySelectorAll('td.seat:not(.booked)');

    seats.forEach(seat => {
        seat.addEventListener('click', function () {
            if (this.classList.contains('booked')) return;

            const seatName = this.textContent.trim();
            const seatId = this.dataset.seatId;

            // Xác định loại ghế và map sang loại vé
            let seatType, ticketType;
            if (this.classList.contains('couple')) {
                seatType = 'couple';
                ticketType = 'couple';
            } else if (this.classList.contains('vip')) {
                seatType = 'vip';
                ticketType = 'vip';
            } else {
                seatType = 'regular';
                ticketType = 'standard';
            }

            // BỎ CHỌN ghế
            if (this.classList.contains('selected')) {
                this.classList.remove('selected');
                removeSeat(seatName);

                // ✅ Tự động GIẢM số lượng vé tương ứng
                decreaseTicketType(ticketType);
            }
            // CHỌN ghế mới
            else {
                // Kiểm tra ghế đôi
                //if (seatType === 'couple') {
                //    if (!isValidCoupleSelection(seatName)) {
                //        alert('Ghế đôi phải được chọn theo cặp');
                //        return;
                //    }
                //}

                this.classList.add('selected');
                addSeat(seatName, seatType, seatId);

                // ✅ Tự động TĂNG số lượng vé tương ứng
                increaseTicketType(ticketType);
            }

            // Hiển thị step 4 khi chọn đủ ghế
            const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);
            if (selectedSeats.length === totalTickets && totalTickets > 0) {
                showStep4();
            }

            updateSummary();
        });
    });
}

// ✅ Hàm tự động TĂNG loại vé
function increaseTicketType(ticketType) {
    const content = document.querySelector(`#ticketOptions .content[data-ticket-type="${ticketType}"]`);

    if (!content) {
        console.warn(`Không tìm thấy loại vé: ${ticketType}`);
        return;
    }

    const quantitySpan = content.querySelector('.quantity');
    const decreaseBtn = content.querySelector('.decrease');
    const increaseBtn = content.querySelector('.increase');
    const maxAvailable = parseInt(content.dataset.available || '999');

    // Tăng số lượng vé
    if (!bookingData.tickets.hasOwnProperty(ticketType)) {
        bookingData.tickets[ticketType] = 0;
    }

    const newQuantity = Math.min(bookingData.tickets[ticketType] + 1, maxAvailable);
    bookingData.tickets[ticketType] = newQuantity;

    // Cập nhật UI
    if (quantitySpan) {
        quantitySpan.textContent = newQuantity;
    }

    if (decreaseBtn) {
        decreaseBtn.disabled = newQuantity <= 0;
    }
    if (increaseBtn) {
        increaseBtn.disabled = newQuantity >= maxAvailable;
    }

    console.log(`✅ Tự động tăng ${ticketType}: ${newQuantity}`);
}

// ✅ Hàm tự động GIẢM loại vé
function decreaseTicketType(ticketType) {
    const content = document.querySelector(`#ticketOptions .content[data-ticket-type="${ticketType}"]`);

    if (!content) {
        console.warn(`Không tìm thấy loại vé: ${ticketType}`);
        return;
    }

    const quantitySpan = content.querySelector('.quantity');
    const decreaseBtn = content.querySelector('.decrease');
    const increaseBtn = content.querySelector('.increase');
    const maxAvailable = parseInt(content.dataset.available || '999');

    // Giảm số lượng vé
    if (!bookingData.tickets.hasOwnProperty(ticketType)) {
        bookingData.tickets[ticketType] = 0;
    }

    const newQuantity = Math.max(bookingData.tickets[ticketType] - 1, 0);
    bookingData.tickets[ticketType] = newQuantity;

    // Cập nhật UI
    if (quantitySpan) {
        quantitySpan.textContent = newQuantity;
    }

    if (decreaseBtn) {
        decreaseBtn.disabled = newQuantity <= 0;
    }
    if (increaseBtn) {
        increaseBtn.disabled = newQuantity >= maxAvailable;
    }

    console.log(`✅ Tự động giảm ${ticketType}: ${newQuantity}`);
}

function isValidCoupleSelection(seatName) {
    const row = seatName.charAt(0);
    const num = parseInt(seatName.substring(1));
    const pairSeatNum = num % 2 === 0 ? num - 1 : num + 1;
    const pairSeatName = row + String(pairSeatNum).padStart(2, '0');
    const isPairSelected = selectedSeats.some(s => s.seatName === pairSeatName);

    if (!isPairSelected) {
        const pairElement = Array.from(document.querySelectorAll('.seat.couple'))
            .find(el => el.textContent.trim() === pairSeatName);

        if (pairElement && !pairElement.classList.contains('booked') && !pairElement.classList.contains('selected')) {
            pairElement.classList.add('selected');
            const pairSeatId = pairElement.dataset.seatId;
            addSeat(pairSeatName, 'couple', pairSeatId);
            return true;
        }
    }

    return isPairSelected;
}

function addSeat(seatName, seatType, seatId) {
    if (!selectedSeats.find(s => s.seatName === seatName)) {
        selectedSeats.push({
            seatId: seatId,
            seatName: seatName,
            seatType: seatType
        });
        bookingData.seats = selectedSeats.map(s => s.seatName);
    }
}

function removeSeat(seatName) {
    const index = selectedSeats.findIndex(s => s.seatName === seatName);
    if (index > -1) {
        const removedSeat = selectedSeats[index];
        selectedSeats.splice(index, 1);
        bookingData.seats = selectedSeats.map(s => s.seatName);

        // Nếu là ghế đôi, tự động bỏ chọn ghế còn lại
        if (removedSeat.seatType === 'couple') {
            const row = seatName.charAt(0);
            const num = parseInt(seatName.substring(1));
            const pairSeatNum = num % 2 === 0 ? num - 1 : num + 1;
            const pairSeatName = row + String(pairSeatNum).padStart(2, '0');

            const pairIndex = selectedSeats.findIndex(s => s.seatName === pairSeatName);
            if (pairIndex > -1) {
                selectedSeats.splice(pairIndex, 1);
                bookingData.seats = selectedSeats.map(s => s.seatName);

                const pairElement = Array.from(document.querySelectorAll('.seat'))
                    .find(el => el.textContent.trim() === pairSeatName);
                if (pairElement) {
                    pairElement.classList.remove('selected');
                }
            }
        }
    }
}

function showStep4() {
    const step4 = document.getElementById('step4');
    if (step4) {
        step4.classList.remove('hidden');
    }
}

function changeComboQuantity(productId, delta) {
    if (!bookingData.combos[productId]) {
        bookingData.combos[productId] = 0;
    }

    bookingData.combos[productId] = Math.max(0, bookingData.combos[productId] + delta);

    const comboElement = document.getElementById(`combo-${productId}`);
    if (comboElement) {
        comboElement.textContent = bookingData.combos[productId];
    }

    updateSummary();
}

// ✅ Update summary với customer info và điểm tích lũy
function updateSummary() {
    const summaryBox = document.getElementById('summaryBox');
    const summaryMovie = document.getElementById('summary-movie');
    const summaryDateTime = document.getElementById('summary-datetime');
    const summaryTickets = document.getElementById('summary-tickets');
    const summaryCombos = document.getElementById('summary-combos');
    const summaryTotal = document.getElementById('summary-total');

    if (summaryBox) summaryBox.classList.remove('hidden');
    if (summaryMovie) summaryMovie.textContent = bookingData.movieId || '-';

    if (bookingData.date && bookingData.time && summaryDateTime) {
        const dateStr = new Date(bookingData.date).toLocaleDateString('vi-VN');
        summaryDateTime.textContent = `${dateStr} - ${bookingData.time} (${bookingData.roomName || ''})`;
    }

    const ticketsSummary = [];
    for (let type in bookingData.tickets) {
        if (bookingData.tickets[type] > 0) {
            const typeName = type === 'standard' ? 'Thường' : type === 'vip' ? 'VIP' : 'Đôi';
            ticketsSummary.push(`${typeName} x${bookingData.tickets[type]}`);
        }
    }

    if (selectedSeats.length > 0) {
        const seatNames = selectedSeats.map(s => s.seatName).join(', ');
        ticketsSummary.push(`<br><small>Ghế: ${seatNames}</small>`);
    }

    if (summaryTickets) summaryTickets.innerHTML = ticketsSummary.join(', ') || '-';

    const combosSummary = [];
    for (let productId in bookingData.combos) {
        if (bookingData.combos[productId] > 0) {
            const comboItem = document.querySelector(`[data-combo-id="${productId}"]`);
            if (comboItem) {
                const comboName = comboItem.dataset.comboName;
                const quantity = bookingData.combos[productId];
                combosSummary.push(`${comboName} x${quantity}`);
            }
        }
    }
    if (summaryCombos) summaryCombos.textContent = combosSummary.join(', ') || 'Không có';

    let total = 0;
    for (let type in bookingData.tickets) {
        total += bookingData.tickets[type] * (ticketPrices[type] || 0);
    }

    for (let productId in bookingData.combos) {
        const comboItem = document.querySelector(`[data-combo-id="${productId}"]`);
        if (comboItem && bookingData.combos[productId] > 0) {
            const price = parseFloat(comboItem.dataset.price || '0');
            total += bookingData.combos[productId] * price;
        }
    }

    if (summaryTotal) summaryTotal.textContent = formatPrice(total);

    // ✅ Hiển thị điểm tích lũy nếu không phải khách vãng lai
    const pointEarnedRow = document.getElementById('pointEarnedRow');
    const summaryPointEarned = document.getElementById('summary-point-earned');

    if (pointEarnedRow && summaryPointEarned && !bookingData.isGuest && total > 0) {
        const pointsEarned = Math.floor(total / 10000);
        summaryPointEarned.textContent = `+${pointsEarned} điểm`;
        pointEarnedRow.style.display = 'flex';
    } else if (pointEarnedRow) {
        pointEarnedRow.style.display = 'none';
    }
}

function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + ' ₫';
}

// ✅ Confirm booking với customer info
function confirmBooking() {
    const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);

    // Validation
    if (!bookingData.movieId) {
        alert('Vui lòng chọn phim!');
        return;
    }
    if (!bookingData.showTime) {
        alert('Vui lòng chọn suất chiếu!');
        return;
    }
    if (totalTickets === 0) {
        alert('Vui lòng chọn ít nhất 1 vé!');
        return;
    }
    if (selectedSeats.length !== totalTickets) {
        alert(`Vui lòng chọn đủ ${totalTickets} ghế (đã chọn: ${selectedSeats.length})`);
        return;
    }

    // ✅ Tính tổng tiền
    let totalAmount = 0;

    for (let type in bookingData.tickets) {
        totalAmount += bookingData.tickets[type] * (ticketPrices[type] || 0);
    }

    for (let productId in bookingData.combos) {
        const comboItem = document.querySelector(`[data-combo-id="${productId}"]`);
        if (comboItem && bookingData.combos[productId] > 0) {
            const price = parseFloat(comboItem.dataset.price || '0');
            totalAmount += bookingData.combos[productId] * price;
        }
    }

    // ✅ Chuẩn bị request data
    const bookingRequest = {
        customerId: bookingData.customerId,
        customerPhone: bookingData.customerPhone,
        customerName: bookingData.customerName || 'Khách vãng lai',
        isGuest: bookingData.isGuest,
        movieId: bookingData.movieId,
        showTimeId: bookingData.showTime,
        branchId: bookingData.branchId,
        roomName: bookingData.roomName,
        roomType: bookingData.roomType,
        showDate: bookingData.date,
        showTime: bookingData.time,
        tickets: Object.entries(bookingData.tickets)
            .filter(([type, quantity]) => quantity > 0)
            .map(([type, quantity]) => ({
                ticketType: type,
                quantity: quantity,
                price: ticketPrices[type]
            })),
        seats: selectedSeats.map(s => ({
            seatId: s.seatId,
            seatName: s.seatName,
            seatType: s.seatType
        })),
        products: Object.entries(bookingData.combos)
            .filter(([productId, quantity]) => quantity > 0)
            .map(([productId, quantity]) => {
                const comboItem = document.querySelector(`[data-combo-id="${productId}"]`);
                return {
                    productId: productId,
                    productName: comboItem?.dataset.comboName || '',
                    quantity: quantity,
                    price: parseFloat(comboItem?.dataset.price || '0')
                };
            }),
        totalAmount: totalAmount,
        pointsToEarn: bookingData.isGuest ? 0 : Math.floor(totalAmount / 10000)
    };

    console.log('=== BOOKING REQUEST ===');
    console.log(JSON.stringify(bookingRequest, null, 2));

    // Show loading
    const confirmBtn = document.querySelector('.btn-book');
    if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
    }

    // ✅ Gửi request tạo invoice
    fetch('/Admin/EmployeeSale/CreateBooking', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(bookingRequest)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('✅ Booking created:', data);

                // ✅ Chuyển sang trang payment
                window.location.href = `/Admin/EmployeeSale/PaymentMethod?invoiceId=${data.invoiceId}`;
            } else {
                console.error('❌ Booking failed:', data.message);
                alert('Đặt vé thất bại: ' + data.message);

                if (confirmBtn) {
                    confirmBtn.disabled = false;
                    confirmBtn.innerHTML = '<i class="fas fa-check-circle"></i> XÁC NHẬN ĐẶT VÉ';
                }
            }
        })
        .catch(error => {
            console.error('❌ Error:', error);
            alert('Có lỗi xảy ra. Vui lòng thử lại!');

            if (confirmBtn) {
                confirmBtn.disabled = false;
                confirmBtn.innerHTML = '<i class="fas fa-check-circle"></i> XÁC NHẬN ĐẶT VÉ';
            }
        });
}
//// === XỬ LÝ CHỌN GHẾ VỚI SIGNALR ===

//// Kết nối SignalR
//let connection = null;
//let currentShowTimeId = null;
//let currentCustomerId = null;

//async function initSignalR() {
//    connection = new signalR.HubConnectionBuilder()
//        .withUrl("/seatHub")
//        .configureLogging(signalR.LogLevel.Information)
//        .withAutomaticReconnect()
//        .build();

//    connection.on("SeatSelected", function (data) {
//        const seatElement = document.querySelector(`td.seat[data-seat-id="${data.seatId}"]`);
//        if (seatElement) {
//            console.log("Tìm thấy ghế:", data.seatId);
//            if (data.customerId.toString() === currentCustomerId.toString()) {
//                seatElement.className = 'seat selected';
//            } else {
//                seatElement.className = 'seat choosing';
//            }
//        } else {
//            console.log("Không tìm thấy ghế:", data.seatId);
//        }
//    });

//    // Lắng nghe sự kiện: Người khác BỎ CHỌN ghế
//    connection.on("SeatDeselected", function (data) {
//        const seatElement = document.querySelector(`td.seat[data-seat-id="${data.seatId}"]`);
//        if (seatElement) {
//            const seatType = seatElement.dataset.seatType;

//            if (seatType === 'Ghế đôi') {
//                seatElement.className = 'seat couple';
//            } else if (seatType === 'Ghế VIP') {
//                seatElement.className = 'seat vip';
//            } else {
//                seatElement.className = 'seat regular';
//            }
//        }
//    });

//    // Kết nối
//    try {
//        await connection.start();
//        console.log("SignalR connected successfully!");
//    } catch (err) {
//        console.error("SignalR connection error:", err);
//    }
//}

//// Click vào li để load ghế
//document.addEventListener('click', function (e) {
//    if (e.target.classList.contains('item-time')) {
//        document.querySelectorAll('.item-time').forEach(item => {
//            item.classList.remove('active');
//        });

//        e.target.classList.add('active');
//        const showTimeId = e.target.dataset.showtimeId;
//        currentCustomerId = document.getElementById("CustomerId").innerHTML;
//        console.log("ShowTimeID: " + showTimeId);
//        console.log("CustomerID: " + currentCustomerId);

//        // Rời nhóm cũ (nếu có)
//        if (currentShowTimeId && connection) {
//            connection.invoke("LeaveShowTime", currentShowTimeId);
//        }

//        currentShowTimeId = showTimeId;
//        if (connection) {
//            connection.invoke("JoinShowTime", showTimeId);
//        }

//        loadSeatingLayout(showTimeId, currentCustomerId);
//    }
//});

//console.log("Khởi động SignalR...");
//initSignalR();

//// Load danh sách ghế từ server
//function loadSeatingLayout(showTimeId, currentCustomerId) {
//    fetch(`/Admin/EmployeeSale/GetSeatingLayout?showTimeId=${showTimeId}&currentCustomerId=${currentCustomerId}`)
//        .then(response => response.json())
//        .then(seats => {
//            renderSeats(seats, showTimeId);
//        })
//        .catch(error => {
//            console.error('Lỗi:', error);
//        });
//}

//// Render ghế ra bảng
//function renderSeats(seats, showTimeId) {
//    const table = document.getElementById('seatingTable');

//    // Nhóm ghế theo hàng (A, B, C...)
//    const rows = {};
//    seats.forEach(seat => {
//        const row = seat.seatName[0];
//        if (!rows[row]) rows[row] = [];
//        rows[row].push(seat);
//    });

//    table.innerHTML = '';

//    // Số cột tối đa của hàng
//    const totalCols = 17;

//    for (let rowName in rows) {
//        const tr = document.createElement('tr');

//        // Label hàng (A, B, C...)
//        const tdLabel = document.createElement('td');
//        tdLabel.className = 'row-label';
//        tdLabel.textContent = rowName;
//        tr.appendChild(tdLabel);

//        // Tính số cột thực tế
//        let realCols = 0;
//        rows[rowName].forEach(seat => {
//            realCols += (seat.seatType === 'Ghế đôi') ? 2 : 1;
//        });

//        const emptyBefore = Math.floor((totalCols - realCols) / 2);
//        const emptyAfter = totalCols - realCols - emptyBefore;

//        // Thêm ô trống bên trái
//        for (let i = 0; i < emptyBefore; i++) {
//            const tdEmpty = document.createElement('td');
//            tdEmpty.className = 'empty';
//            tr.appendChild(tdEmpty);
//        }

//        // Vẽ từng ghế
//        rows[rowName].forEach(seat => {
//            const td = document.createElement('td');
//            td.textContent = seat.seatName;
//            td.dataset.seatId = seat.seatID;
//            td.dataset.seatType = seat.seatType;

//            // Xác định class theo trạng thái
//            let seatClass = 'seat';

//            if (seat.status === 'Đã đặt') {
//                seatClass += ' booked';
//            }
//            else if (seat.status === 'Đang được chọn') {
//                // Kiểm tra: Ghế do mình chọn hay người khác chọn?
//                if (seat.isMyChoice) {
//                    seatClass += ' selected'; // Ghế của mình
//                } else {
//                    seatClass += ' choosing'; // Ghế người khác
//                }
//            }
//            else {
//                // Ghế trống
//                if (seat.seatType === 'Ghế đôi') {
//                    seatClass += ' couple';
//                } else if (seat.seatType === 'Ghế VIP') {
//                    seatClass += ' vip';
//                } else {
//                    seatClass += ' regular';
//                }
//            }

//            // Ghế đôi chiếm 2 cột
//            if (seat.seatType === 'Ghế đôi') {
//                td.colSpan = 2;
//            }

//            td.className = seatClass;
//            tr.appendChild(td);
//        });

//        // Thêm ô trống bên phải
//        for (let i = 0; i < emptyAfter; i++) {
//            const tdEmpty = document.createElement('td');
//            tdEmpty.className = 'empty';
//            tr.appendChild(tdEmpty);
//        }

//        table.appendChild(tr);
//    }

//    // Gắn sự kiện click
//    addSeatClickEvents(showTimeId);
//}