// Verify movieId is defined from inline script
let movieIdFromScript = typeof movieId !== 'undefined' ? movieId : null;

console.log('=== Initialization ===');
console.log('movieId from inline script:', movieIdFromScript);

const bookingData = {
    movieId: movieIdFromScript,
    showTime: null,
    tickets: {},
    seats: [],  // ✅ THÊM: Lưu danh sách ghế đã chọn
    date: null,
    time: null,
    roomName: null,
    combos: {}
};

let ticketPrices = {};
let selectedSeats = []; // ✅ THÊM: Track ghế đã chọn

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing...');

    // Initialize ticket types from server-rendered data
    initializeTicketTypes();

    // Setup time slot selection
    setupTimeSlotSelection();

    // ✅ THÊM: Setup seat selection
    setupSeatSelection();
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

    // Setup ticket quantity controls
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
            // Remove selected class from all slots
            timeSlots.forEach(s => s.classList.remove('selected'));

            // Add selected class to clicked slot
            this.classList.add('selected');

            // Get showtime data
            const showTimeId = this.dataset.showtimeId;
            const time = this.dataset.time;
            const room = this.dataset.room;
            const date = this.dataset.date || new Date().toISOString().split('T')[0];

            // Update booking data
            bookingData.showTime = showTimeId;
            bookingData.time = time;
            bookingData.roomName = room;
            bookingData.date = date;

            console.log('Selected showtime:', { showTimeId, time, room, date });

            // Show step 3 (ticket selection)
            showStep3();

            // Update summary
            updateSummary();
        });
    });
}

// Show step 3
function showStep3() {
    const step3 = document.getElementById('step3');
    if (step3) {
        step3.classList.remove('hidden');

        // Scroll to step 3 smoothly
        setTimeout(() => {
            step3.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}

// Ticket quantity
function changeQuantity(type, delta, quantityElement, decreaseBtn, increaseBtn, maxAvailable) {
    if (!bookingData.tickets.hasOwnProperty(type)) {
        bookingData.tickets[type] = 0;
    }

    const newQuantity = Math.max(0, Math.min(bookingData.tickets[type] + delta, maxAvailable));
    bookingData.tickets[type] = newQuantity;

    if (quantityElement) {
        quantityElement.textContent = newQuantity;
    }

    // Update button states
    if (decreaseBtn) {
        decreaseBtn.disabled = newQuantity <= 0;
    }
    if (increaseBtn) {
        increaseBtn.disabled = newQuantity >= maxAvailable;
    }

    const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);

    // ✅ SỬA: Show step5 (chọn ghế) khi đã chọn vé
    if (totalTickets > 0) {
        showStep5();
    } else {
        // Hide step5 nếu không có vé
        const step5 = document.getElementById('step5');
        if (step5) {
            step5.classList.add('hidden');
        }
    }

    updateSummary();
}

// Setup time slot selection
function setupTimeSlotSelection() {
    const timeSlots = document.querySelectorAll('.time-slot');

    timeSlots.forEach(slot => {
        slot.addEventListener('click', function () {
            // Remove selected class from all slots
            timeSlots.forEach(s => s.classList.remove('selected'));

            // Add selected class to clicked slot
            this.classList.add('selected');

            // Get showtime data
            const showTimeId = this.dataset.showtimeId;
            const movieId = this.dataset.movieId;
            const time = this.dataset.time;
            const room = this.dataset.room;
            const roomType = this.dataset.roomType;
            const date = this.dataset.date || new Date().toISOString().split('T')[0];

            // Update booking data
            bookingData.showTime = showTimeId;
            bookingData.time = time;
            bookingData.roomName = room;
            bookingData.roomType = roomType;
            bookingData.date = date;

            console.log('Selected showtime:', { showTimeId, movieId, time, room, roomType, date });

            // ✅ Cập nhật TitleBrand ngay lập tức với dữ liệu có sẵn
            updateRoomTitle(room, roomType);

            // Gọi API để xác nhận thông tin phòng (optional)
            fetchRoomInfo(movieId, showTimeId);

            // Show step 3 (ticket selection)
            showStep3();

            // Update summary
            updateSummary();
        });
    });
}

// ✅ THÊM: Hàm cập nhật TitleBrand
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

// ✅ Hàm gọi API lấy thông tin phòng chiếu (optional - để xác nhận)
function fetchRoomInfo(movieId, showTimeId) {
    const formData = new FormData();
    formData.append('movieId', movieId);
    formData.append('showTimeId', showTimeId);

    fetch('/Admin/EmployeeSale/GetRoomNameByMovieShowTimeDate', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Room info from server:', data);

                // Cập nhật lại nếu server trả về thông tin khác
                bookingData.roomName = data.roomName;
                bookingData.roomType = data.roomType;

                // Cập nhật TitleBrand với dữ liệu từ server
                updateRoomTitle(data.roomName, data.roomType);

                // Update summary
                updateSummary();
            } else {
                console.error('Error fetching room info:', data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

// ✅ SỬA: Show step 5 và cuộn đến phần chọn ghế
function showStep5() {
    const step5 = document.getElementById('step5');
    if (step5) {
        step5.classList.remove('hidden');

        // Scroll to step 5 smoothly
        setTimeout(() => {
            step5.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}

// ✅ THÊM: Setup seat selection
function setupSeatSelection() {
    const seats = document.querySelectorAll('.seat:not(.booked)');

    seats.forEach(seat => {
        seat.addEventListener('click', function () {
            // Kiểm tra nếu ghế đã được đặt
            if (this.classList.contains('booked')) {
                return;
            }

            const seatName = this.textContent.trim();
            const seatType = this.classList.contains('couple') ? 'couple' :
                this.classList.contains('regular') ? 'regular' : 'regular';

            // Toggle selected state
            if (this.classList.contains('selected')) {
                // Bỏ chọn ghế
                this.classList.remove('selected');
                removeSeat(seatName);
            } else {
                // Chọn ghế - kiểm tra số lượng vé đã chọn
                const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);
                const totalSeatsSelected = selectedSeats.length;

                if (totalSeatsSelected >= totalTickets) {
                    alert(`Bạn chỉ có thể chọn tối đa ${totalTickets} ghế (theo số vé đã chọn)`);
                    return;
                }

                // ✅ Validate ghế đôi phải chọn cặp
                if (seatType === 'couple') {
                    if (!isValidCoupleSelection(seatName)) {
                        alert('Ghế đôi phải được chọn theo cặp (ví dụ: P01-P02, Q03-Q04)');
                        return;
                    }
                }

                this.classList.add('selected');
                addSeat(seatName, seatType);
            }

            // Show step 4 (chọn bắp nước) khi đã chọn đủ ghế
            const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);
            if (selectedSeats.length === totalTickets && totalTickets > 0) {
                showStep4();
            }

            updateSummary();
        });
    });
}

// ✅ THÊM: Validate ghế đôi
function isValidCoupleSelection(seatName) {
    // Extract row and seat number (e.g., "P01" -> row="P", num=1)
    const row = seatName.charAt(0);
    const num = parseInt(seatName.substring(1));

    // Ghế đôi phải có số chẵn hoặc lẻ liên tiếp
    // Kiểm tra xem ghế bên cạnh có được chọn không
    const pairSeatNum = num % 2 === 0 ? num - 1 : num + 1;
    const pairSeatName = row + String(pairSeatNum).padStart(2, '0');

    const pairSeat = document.querySelector(`.seat.couple:not(.booked)`).textContent === pairSeatName;
    const isPairSelected = selectedSeats.some(s => s.seatName === pairSeatName);

    // Tự động chọn ghế cặp nếu có thể
    if (!isPairSelected) {
        const pairElement = Array.from(document.querySelectorAll('.seat.couple'))
            .find(el => el.textContent.trim() === pairSeatName);

        if (pairElement && !pairElement.classList.contains('booked') && !pairElement.classList.contains('selected')) {
            pairElement.classList.add('selected');
            addSeat(pairSeatName, 'couple');
            return true;
        }
    }

    return isPairSelected;
}

// ✅ THÊM: Add seat to selection
function addSeat(seatName, seatType) {
    if (!selectedSeats.find(s => s.seatName === seatName)) {
        selectedSeats.push({
            seatName: seatName,
            seatType: seatType
        });
        bookingData.seats = selectedSeats.map(s => s.seatName);
        console.log('Seat added:', seatName, '| Total seats:', selectedSeats.length);
    }
}

// ✅ THÊM: Remove seat from selection
function removeSeat(seatName) {
    const index = selectedSeats.findIndex(s => s.seatName === seatName);
    if (index > -1) {
        selectedSeats.splice(index, 1);
        bookingData.seats = selectedSeats.map(s => s.seatName);
        console.log('Seat removed:', seatName, '| Total seats:', selectedSeats.length);

        // ✅ Nếu là ghế đôi, tự động bỏ chọn ghế cặp
        const row = seatName.charAt(0);
        const num = parseInt(seatName.substring(1));
        const pairSeatNum = num % 2 === 0 ? num - 1 : num + 1;
        const pairSeatName = row + String(pairSeatNum).padStart(2, '0');

        const pairIndex = selectedSeats.findIndex(s => s.seatName === pairSeatName);
        if (pairIndex > -1) {
            selectedSeats.splice(pairIndex, 1);
            bookingData.seats = selectedSeats.map(s => s.seatName);

            // Remove visual selection
            const pairElement = Array.from(document.querySelectorAll('.seat'))
                .find(el => el.textContent.trim() === pairSeatName);
            if (pairElement) {
                pairElement.classList.remove('selected');
            }
        }
    }
}

// ✅ THÊM: Show step 4 (bắp nước)
function showStep4() {
    const step4 = document.getElementById('step4');
    if (step4) {
        step4.classList.remove('hidden');

        setTimeout(() => {
            step4.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}

// Combo quantity (cập nhật để hỗ trợ dynamic product IDs)
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

    // ✅ THÊM: Hiển thị ghế đã chọn
    const ticketsSummary = [];
    for (let type in bookingData.tickets) {
        if (bookingData.tickets[type] > 0) {
            const typeName = type === 'standard' ? 'Thường' : type === 'vip' ? 'VIP' : 'Đôi';
            ticketsSummary.push(`${typeName} x${bookingData.tickets[type]}`);
        }
    }

    // Thêm thông tin ghế đã chọn
    if (selectedSeats.length > 0) {
        const seatNames = selectedSeats.map(s => s.seatName).join(', ');
        ticketsSummary.push(`<br><small>Ghế: ${seatNames}</small>`);
    }

    if (summaryTickets) summaryTickets.innerHTML = ticketsSummary.join(', ') || '-';

    // Combos summary
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

    // Calculate total
    let total = 0;
    for (let type in bookingData.tickets) {
        total += bookingData.tickets[type] * (ticketPrices[type] || 0);
    }

    // Tính tổng combo từ data attributes
    for (let productId in bookingData.combos) {
        const comboItem = document.querySelector(`[data-combo-id="${productId}"]`);
        if (comboItem && bookingData.combos[productId] > 0) {
            const price = parseFloat(comboItem.dataset.price || '0');
            total += bookingData.combos[productId] * price;
        }
    }

    if (summaryTotal) summaryTotal.textContent = formatPrice(total);
}

// Format price function
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + ' ₫';
}

// ✅ CẬP NHẬT: Confirm booking với validation ghế
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

    console.log('=== BOOKING DATA ===');
    console.log('Movie:', bookingData.movieId);
    console.log('ShowTime:', bookingData.showTime);
    console.log('Tickets:', bookingData.tickets);
    console.log('Seats:', bookingData.seats);
    console.log('Combos:', bookingData.combos);

    // TODO: Gọi API để lưu booking
    alert(`Đặt vé thành công!\nGhế: ${bookingData.seats.join(', ')}\nTổng tiền: ${document.getElementById('summary-total').textContent}`);

    // Reset or redirect
    // window.location.href = '/Admin/EmployeeSale/SaleTicket';
}