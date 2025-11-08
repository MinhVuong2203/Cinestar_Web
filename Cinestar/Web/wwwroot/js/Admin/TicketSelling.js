// Verify movieId is defined from inline script
let movieIdFromScript = typeof movieId !== 'undefined' ? movieId : null;

console.log('=== Initialization ===');
console.log('movieId from inline script:', movieIdFromScript);

const bookingData = {
    movieId: movieIdFromScript,
    showTime: null,
    tickets: {},
    date: null,
    time: null,
    roomName: null,
    combos: { combo1: 0, combo2: 0, popcorn: 0, drink: 0 }
};

let ticketPrices = {};
const comboPrices = { combo1: 70000, combo2: 120000, popcorn: 45000, drink: 30000 };

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded, initializing...');
    
    // Initialize ticket types from server-rendered data
    initializeTicketTypes();
    
    // Setup time slot selection
    setupTimeSlotSelection();
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
        slot.addEventListener('click', function() {
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
    if (totalTickets > 0) {
        const step4 = document.getElementById('step4');
        if (step4) {
            step4.classList.remove('hidden');
        }
    }
    updateSummary();
}

// Combo quantity
function changeComboQuantity(combo, delta) {
    bookingData.combos[combo] = Math.max(0, bookingData.combos[combo] + delta);
    const comboElement = document.getElementById(`combo-${combo}`);
    if (comboElement) {
        comboElement.textContent = bookingData.combos[combo];
    }
    updateSummary();
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

    // Tickets summary
    const ticketsSummary = [];
    for (let type in bookingData.tickets) {
        if (bookingData.tickets[type] > 0) {
            const typeName = type === 'standard' ? 'Thường' : type === 'vip' ? 'VIP' : 'Đôi';
            ticketsSummary.push(`${typeName} x${bookingData.tickets[type]}`);
        }
    }
    if (summaryTickets) summaryTickets.textContent = ticketsSummary.join(', ') || '-';

    // Combos summary - CẬP NHẬT
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

    // Calculate total - CẬP NHẬT
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

    console.log('Booking data:', bookingData);
    alert('Đặt vé thành công! Cảm ơn quý khách đã sử dụng dịch vụ.');

    // Reset or redirect
    window.location.href = '/Admin/EmployeeSale/SaleTicket';
}