const bookingData = {
    movie: null,
    tickets: { standard: 0, vip: 0, couple: 0 },
    date: null,
    time: null,
    combos: { combo1: 0, combo2: 0, popcorn: 0, drink: 0 }
};

const prices = {
    tickets: { standard: 80000, vip: 120000, couple: 200000 },
    combos: { combo1: 70000, combo2: 120000, popcorn: 45000, drink: 30000 }
};

// Set min date to today
const today = new Date().toISOString().split('T')[0];
document.getElementById('dateSelect').min = today;
document.getElementById('dateSelect').value = today;
bookingData.date = today;

// Movie selection
document.querySelectorAll('.movie-card').forEach(card => {
    card.addEventListener('click', function () {
        document.querySelectorAll('.movie-card').forEach(c => c.classList.remove('selected'));
        this.classList.add('selected');
        bookingData.movie = {
            id: this.dataset.movie,
            name: this.dataset.name
        };
        document.getElementById('step2').classList.remove('hidden');
        updateSummary();
    });
});

// Ticket quantity
function changeQuantity(type, delta) {
    bookingData.tickets[type] = Math.max(0, bookingData.tickets[type] + delta);
    document.getElementById(`qty-${type}`).textContent = bookingData.tickets[type];

    const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);
    if (totalTickets > 0) {
        document.getElementById('step3').classList.remove('hidden');
    }
    updateSummary();
}

// Time selection
document.getElementById('dateSelect').addEventListener('change', function () {
    bookingData.date = this.value;
    updateSummary();
});

document.querySelectorAll('.time-slot').forEach(slot => {
    slot.addEventListener('click', function () {
        document.querySelectorAll('.time-slot').forEach(s => s.classList.remove('selected'));
        this.classList.add('selected');
        bookingData.time = this.dataset.time;
        document.getElementById('step4').classList.remove('hidden');
        updateSummary();
    });
});

// Combo quantity
function changeComboQuantity(combo, delta) {
    bookingData.combos[combo] = Math.max(0, bookingData.combos[combo] + delta);
    document.getElementById(`combo-${combo}`).textContent = bookingData.combos[combo];
    updateSummary();
}

// Update summary
function updateSummary() {
    if (!bookingData.movie) return;

    document.getElementById('summaryBox').classList.remove('hidden');
    document.getElementById('summary-movie').textContent = bookingData.movie.name;

    if (bookingData.date && bookingData.time) {
        const dateStr = new Date(bookingData.date).toLocaleDateString('vi-VN');
        document.getElementById('summary-datetime').textContent = `${dateStr} - ${bookingData.time}`;
    }

    // Tickets summary
    const ticketsSummary = [];
    if (bookingData.tickets.standard > 0) ticketsSummary.push(`Thường x${bookingData.tickets.standard}`);
    if (bookingData.tickets.vip > 0) ticketsSummary.push(`VIP x${bookingData.tickets.vip}`);
    if (bookingData.tickets.couple > 0) ticketsSummary.push(`Đôi x${bookingData.tickets.couple}`);
    document.getElementById('summary-tickets').textContent = ticketsSummary.join(', ') || '-';

    // Combos summary
    const combosSummary = [];
    if (bookingData.combos.combo1 > 0) combosSummary.push(`Combo 1 x${bookingData.combos.combo1}`);
    if (bookingData.combos.combo2 > 0) combosSummary.push(`Combo 2 x${bookingData.combos.combo2}`);
    if (bookingData.combos.popcorn > 0) combosSummary.push(`Bắp x${bookingData.combos.popcorn}`);
    if (bookingData.combos.drink > 0) combosSummary.push(`Nước x${bookingData.combos.drink}`);
    document.getElementById('summary-combos').textContent = combosSummary.join(', ') || 'Không có';

    // Calculate total
    let total = 0;
    for (let type in bookingData.tickets) {
        total += bookingData.tickets[type] * prices.tickets[type];
    }
    for (let combo in bookingData.combos) {
        total += bookingData.combos[combo] * prices.combos[combo];
    }
    document.getElementById('summary-total').textContent = total.toLocaleString('vi-VN') + ' ₫';
}

function confirmBooking() {
    const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);

    if (!bookingData.movie) {
        alert('Vui lòng chọn phim!');
        return;
    }
    if (totalTickets === 0) {
        alert('Vui lòng chọn ít nhất 1 vé!');
        return;
    }
    if (!bookingData.time) {
        alert('Vui lòng chọn giờ xem!');
        return;
    }

    alert('Đặt vé thành công! Cảm ơn quý khách đã sử dụng dịch vụ.');

    // Reset form
    location.reload();
}

// Biến lưu trữ thông tin đặt vé
let selectedMovie = null;
let selectedTickets = {};
let selectedCombos = {};
let selectedDateTime = null;

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function () {
    initializeDatePicker();
    initializeMovieSelection();
    initializeStepNavigation();
});

// Khởi tạo date picker
function initializeDatePicker() {
    const dateSelect = document.getElementById('dateSelect');
    if (dateSelect) {
        // Set ngày tối thiểu là hôm nay
        const today = new Date();
        dateSelect.min = today.toISOString().split('T')[0];
        dateSelect.value = today.toISOString().split('T')[0];
    }
}

// Khởi tạo chọn phim
function initializeMovieSelection() {
    const movieCards = document.querySelectorAll('.movie-card');
    movieCards.forEach(card => {
        card.addEventListener('click', function () {
            selectMovie(this);
        });
    });
}

// Xử lý chọn phim
function selectMovie(movieCard) {
    // Bỏ chọn tất cả phim khác
    document.querySelectorAll('.movie-card').forEach(card => {
        card.classList.remove('selected');
    });

    // Chọn phim hiện tại
    movieCard.classList.add('selected');

    // Lưu thông tin phim
    selectedMovie = {
        id: movieCard.dataset.movie,
        name: movieCard.dataset.name
    };

    // Hiển thị bước tiếp theo
    showStep(2);
    updateSummary();
}

// Hiển thị bước
function showStep(stepNumber) {
    // Ẩn tất cả các bước
    document.querySelectorAll('.step-card').forEach((step, index) => {
        if (index + 1 <= stepNumber) {
            step.classList.remove('hidden');
        }
    });

    // Hiển thị summary box nếu đã chọn phim
    if (stepNumber >= 2) {
        document.getElementById('summaryBox').classList.remove('hidden');
    }
}

// Thay đổi số lượng vé
function changeQuantity(type, change) {
    const qtyElement = document.getElementById(`qty-${type}`);
    const currentQty = parseInt(qtyElement.textContent) || 0;
    const newQty = Math.max(0, currentQty + change);

    qtyElement.textContent = newQty;
    selectedTickets[type] = newQty;

    // Kiểm tra nếu có vé được chọn thì hiển thị bước tiếp theo
    const totalTickets = Object.values(selectedTickets).reduce((sum, qty) => sum + qty, 0);
    if (totalTickets > 0) {
        showStep(3);
    }

    updateSummary();
}

// Thay đổi số lượng combo
function changeComboQuantity(combo, change) {
    const qtyElement = document.getElementById(`combo-${combo}`);
    const currentQty = parseInt(qtyElement.textContent) || 0;
    const newQty = Math.max(0, currentQty + change);

    qtyElement.textContent = newQty;
    selectedCombos[combo] = newQty;

    updateSummary();
}

// Khởi tạo navigation cho các bước
function initializeStepNavigation() {
    // Xử lý chọn giờ
    document.querySelectorAll('.time-slot').forEach(slot => {
        slot.addEventListener('click', function () {
            document.querySelectorAll('.time-slot').forEach(s => s.classList.remove('selected'));
            this.classList.add('selected');

            const date = document.getElementById('dateSelect').value;
            const time = this.dataset.time;
            selectedDateTime = `${date} ${time}`;

            showStep(4);
            updateSummary();
        });
    });
}

// Cập nhật tổng kết
function updateSummary() {
    // Cập nhật tên phim
    document.getElementById('summary-movie').textContent = selectedMovie ? selectedMovie.name : '-';

    // Cập nhật ngày giờ
    document.getElementById('summary-datetime').textContent = selectedDateTime || '-';

    // Cập nhật vé
    const ticketSummary = [];
    const ticketPrices = { standard: 80000, vip: 120000, couple: 200000 };
    const ticketNames = { standard: 'Vé thường', vip: 'Vé VIP', couple: 'Vé đôi' };

    for (let [type, qty] of Object.entries(selectedTickets)) {
        if (qty > 0) {
            ticketSummary.push(`${ticketNames[type]}: ${qty}`);
        }
    }
    document.getElementById('summary-tickets').textContent = ticketSummary.length > 0 ? ticketSummary.join(', ') : '-';

    // Cập nhật combo
    const comboSummary = [];
    for (let [combo, qty] of Object.entries(selectedCombos)) {
        if (qty > 0) {
            comboSummary.push(`${combo}: ${qty}`);
        }
    }
    document.getElementById('summary-combos').textContent = comboSummary.length > 0 ? comboSummary.join(', ') : 'Không có';

    // Tính tổng tiền
    let total = 0;

    // Tiền vé
    for (let [type, qty] of Object.entries(selectedTickets)) {
        total += (ticketPrices[type] || 0) * qty;
    }

    // Tiền combo (cần định nghĩa giá combo)
    const comboPrices = { combo1: 70000, combo2: 120000, popcorn: 45000, drink: 30000 };
    for (let [combo, qty] of Object.entries(selectedCombos)) {
        total += (comboPrices[combo] || 0) * qty;
    }

    document.getElementById('summary-total').textContent = total.toLocaleString('vi-VN') + ' ₫';
}

// Xác nhận đặt vé
function confirmBooking() {
    if (!selectedMovie) {
        alert('Vui lòng chọn phim!');
        return;
    }

    const totalTickets = Object.values(selectedTickets).reduce((sum, qty) => sum + qty, 0);
    if (totalTickets === 0) {
        alert('Vui lòng chọn ít nhất một vé!');
        return;
    }

    if (!selectedDateTime) {
        alert('Vui lòng chọn ngày giờ chiếu!');
        return;
    }

    // Xử lý logic đặt vé ở đây
    alert('Đang xử lý đặt vé...');
}