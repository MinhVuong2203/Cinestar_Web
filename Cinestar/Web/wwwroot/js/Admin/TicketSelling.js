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
    combos: {}
};

let ticketPrices = {};
let selectedSeats = [];

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing...');
    initializeTicketTypes();
    setupTimeSlotSelection();
    // Không gọi setupSeatSelection ở đây vì ghế sẽ được load động
});

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

            // Cập nhật TitleBrand
            updateRoomTitle(room, roomType);

            // ✅ Load ticket types theo showTimeId
            loadTicketTypes(showTimeId, movieId, branchId);

            // ✅ Load ghế ngồi từ server
            loadSeatingLayout(showTimeId);

            // Show step 3 (ticket selection)
            showStep3();

            // Update summary
            updateSummary();
        });
    });
}

// ✅ THÊM: Load ticket types theo showTimeId
function loadTicketTypes(showTimeId, movieId, branchId) {
    const ticketOptions = document.getElementById('ticketOptions');

    console.log('=== loadTicketTypes called ===');
    console.log('Parameters:', { showTimeId, movieId, branchId });

    // Show loading
    ticketOptions.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-3 text-muted">Đang tải thông tin vé...</p>
        </div>
    `;

    const url = `/Admin/EmployeeSale/GetTicketTypesByShowTime?showTimeId=${showTimeId}&movieId=${movieId}&branchId=${branchId}`;
    console.log('Fetching URL:', url);

    fetch(url)
        .then(response => {
            console.log('Response status:', response.status);
            console.log('Response ok:', response.ok);
            return response.json();
        })
        .then(data => {
            console.log('=== Response data ===');
            console.log('Full response:', data);
            console.log('data.success:', data.success);
            console.log('data.ticketTypes:', data.ticketTypes);

            if (data.success) {
                console.log('SUCCESS: Rendering ticket types');
                console.log('Ticket types:', JSON.stringify(data.ticketTypes, null, 2));

                // ✅ SỬA: Từ data.ticketType thành data.ticketTypes (có 's')
                renderTicketTypes(data.ticketTypes);

                // Re-initialize after rendering
                initializeTicketTypes();
            } else {
                console.error('FAILED: Error loading ticket types:', data.message);
                ticketOptions.innerHTML = `
                    <div class="alert alert-warning text-center">
                        <i class="fas fa-exclamation-triangle"></i>
                        ${data.message || 'Không thể tải thông tin vé'}
                    </div>
                `;
            }
        })
        .catch(error => {
            console.error('EXCEPTION: Error fetching ticket types:', error);
            ticketOptions.innerHTML = `
                <div class="alert alert-danger text-center">
                    <i class="fas fa-exclamation-circle"></i>
                    Có lỗi xảy ra khi tải thông tin vé: ${error.message}
                </div>
            `;
        });
}

// ✅ THÊM: Render ticket types HTML
function renderTicketTypes(ticketTypes) {
    const ticketOptions = document.getElementById('ticketOptions');

    console.log('=== renderTicketTypes called ===');
    console.log('Received ticketTypes:', ticketTypes);

    // ✅ SỬA: Kiểm tra với key viết thường
    if (!ticketTypes || (!ticketTypes.standard && !ticketTypes.vip && !ticketTypes.couple)) {
        console.log('No ticket types available');
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

    // ✅ SỬA: Truy cập với key viết thường
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
            </div>
        </div>
        `;
    }

    //html += `
    //`;

    ticketOptions.innerHTML = html;
    console.log('Ticket types rendered successfully');
}

// Hàm cập nhật TitleBrand
function updateRoomTitle(roomName, roomType) {
    const roomTitle = document.getElementById('roomTitle');
    if (roomTitle) {
        roomTitle.textContent = `Chọn ghế - ${roomName}`;
        if (roomType) {
            roomTitle.textContent += ` (${roomType})`;
        }
        console.log('Updated room title:', roomTitle.textContent);
    }
}

// Show step 3
function showStep3() {
    const step3 = document.getElementById('step3');
    if (step3) {
        step3.classList.remove('hidden');
        setTimeout(() => {
            step3.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}

// Ticket quantity change
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

    if (totalTickets > 0) {
        showStep5();
    } else {
        const step5 = document.getElementById('step5');
        if (step5) {
            step5.classList.add('hidden');
        }
    }

    updateSummary();
}

// Show step 5
function showStep5() {
    const step5 = document.getElementById('step5');
    if (step5) {
        step5.classList.remove('hidden');
        setTimeout(() => {
            step5.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}

// ================== LOAD SEATING LAYOUT ==================
function loadSeatingLayout(showTimeId) {
    console.log('Loading seating layout for showTimeId:', showTimeId);

    fetch(`/Admin/EmployeeSale/GetSeatingLayout?showTimeId=${showTimeId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Seats loaded:', data.seats);
                renderSeats(data.seats);
            } else {
                console.error('Error loading seats:', data.message);
                alert('Không thể tải danh sách ghế. Vui lòng thử lại.');
            }
        })
        .catch(error => {
            console.error('Error fetching seats:', error);
            alert('Có lỗi xảy ra khi tải danh sách ghế.');
        });
}

// Render ghế ra bảng
function renderSeats(seats) {
    const table = document.getElementById('seatingTable');

    // Nhóm ghế theo hàng (A, B, C...)
    const rows = {};
    seats.forEach(seat => {
        const row = seat.seatName[0];
        if (!rows[row]) rows[row] = [];
        rows[row].push(seat);
    });

    // Xóa table cũ
    table.innerHTML = '';

    // Tạo từng hàng
    for (let rowName in rows) {
        const tr = document.createElement('tr');

        // Label hàng (A, B, C...)
        const tdLabel = document.createElement('td');
        tdLabel.className = 'row-label';
        tdLabel.textContent = rowName;
        tr.appendChild(tdLabel);

        // Tính toán số cột và căn giữa
        const totalCols = 21; // Tổng số cột (có thể điều chỉnh)
        const seatCount = rows[rowName].length;
        const emptyBefore = Math.floor((totalCols - seatCount) / 2);

        // Thêm ô trống bên trái
        for (let i = 0; i < emptyBefore; i++) {
            const tdEmpty = document.createElement('td');
            tdEmpty.className = 'empty';
            tr.appendChild(tdEmpty);
        }

        // Vẽ ghế
        rows[rowName].forEach(seat => {
            const td = document.createElement('td');
            td.textContent = seat.seatName;
            td.dataset.seatId = seat.seatId;

            // Xác định class dựa trên Status và SeatType
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

        // Thêm ô trống bên phải
        const emptyAfter = totalCols - seatCount - emptyBefore;
        for (let i = 0; i < emptyAfter; i++) {
            const tdEmpty = document.createElement('td');
            tdEmpty.className = 'empty';
            tr.appendChild(tdEmpty);
        }

        table.appendChild(tr);
    }

    // Reset selected seats khi load ghế mới
    selectedSeats = [];
    bookingData.seats = [];

    // Thêm sự kiện click cho ghế sau khi render
    setupSeatClickEvents();
}

// Setup seat click events
function setupSeatClickEvents() {
    const seats = document.querySelectorAll('td.seat:not(.booked)');

    seats.forEach(seat => {
        seat.addEventListener('click', function () {
            if (this.classList.contains('booked')) {
                return;
            }

            const seatName = this.textContent.trim();
            const seatId = this.dataset.seatId;
            const seatType = this.classList.contains('couple') ? 'couple' :
                this.classList.contains('vip') ? 'vip' : 'regular';

            if (this.classList.contains('selected')) {
                // Bỏ chọn ghế
                this.classList.remove('selected');
                removeSeat(seatName);
            } else {
                // Chọn ghế
                const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);
                const totalSeatsSelected = selectedSeats.length;

                if (totalSeatsSelected >= totalTickets) {
                    alert(`Bạn chỉ có thể chọn tối đa ${totalTickets} ghế (theo số vé đã chọn)`);
                    return;
                }

                // Validate ghế đôi
                if (seatType === 'couple') {
                    if (!isValidCoupleSelection(seatName)) {
                        alert('Ghế đôi phải được chọn theo cặp');
                        return;
                    }
                }

                this.classList.add('selected');
                addSeat(seatName, seatType, seatId);
            }

            // Show step 4 khi đã chọn đủ ghế
            const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);
            if (selectedSeats.length === totalTickets && totalTickets > 0) {
                showStep4();
            }

            updateSummary();
        });
    });
}

// Validate ghế đôi
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

// Add seat to selection
function addSeat(seatName, seatType, seatId) {
    if (!selectedSeats.find(s => s.seatName === seatName)) {
        selectedSeats.push({
            seatId: seatId,
            seatName: seatName,
            seatType: seatType
        });
        bookingData.seats = selectedSeats.map(s => s.seatName);
        console.log('Seat added:', seatName, '| Total seats:', selectedSeats.length);
    }
}

// Remove seat from selection
function removeSeat(seatName) {
    const index = selectedSeats.findIndex(s => s.seatName === seatName);
    if (index > -1) {
        selectedSeats.splice(index, 1);
        bookingData.seats = selectedSeats.map(s => s.seatName);
        console.log('Seat removed:', seatName, '| Total seats:', selectedSeats.length);

        // Nếu là ghế đôi, tự động bỏ chọn ghế cặp
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

// Show step 4
function showStep4() {
    const step4 = document.getElementById('step4');
    if (step4) {
        step4.classList.remove('hidden');
        setTimeout(() => {
            step4.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}

// Combo quantity
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

// Update summary
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
}

// Format price
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + ' ₫';
}

// Confirm booking
function confirmBooking() {
    const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);

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

    console.log('=== BOOKING DATA ===');
    console.log('Movie:', bookingData.movieId);
    console.log('ShowTime:', bookingData.showTime);
    console.log('Tickets:', bookingData.tickets);
    console.log('Seats:', bookingData.seats);
    console.log('Combos:', bookingData.combos);

    alert(`Đặt vé thành công!\nGhế: ${bookingData.seats.join(', ')}\nTổng tiền: ${document.getElementById('summary-total').textContent}`);
}