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