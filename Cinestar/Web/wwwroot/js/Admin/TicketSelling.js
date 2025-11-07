// Verify movieId is defined from inline script OR data-attribute
let movieIdFromScript = typeof movieId !== 'undefined' ? movieId : null;
let movieIdFromElement = null;

console.log('=== Initialization ===');
console.log('movieId from inline script:', movieIdFromScript);

const bookingData = {
    movieId: null, // Will be set below
    showTime: null,
    tickets: { standard: 0, vip: 0, couple: 0 },
    date: null,
    time: null,
    roomName: null,
    combos: { combo1: 0, combo2: 0, popcorn: 0, drink: 0 }
};

let ticketPrices = { standard: 80000, vip: 120000, couple: 200000 };
const comboPrices = { combo1: 70000, combo2: 120000, popcorn: 45000, drink: 30000 };


// Helper function to show error messages
function showError(containerId, message) {
    const container = document.getElementById(containerId);
    if (container) {
        container.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle"></i>
                ${message}
            </div>
        `;
    }
}



// Function to load ticket types dynamically
async function loadTicketTypes(movieId) {
    const ticketOptionsContainer = document.getElementById('ticketOptions');

    if (!ticketOptionsContainer) {
        console.error('ticketOptions container not found');
        return;
    }

    if (!movieId) {
        console.error('Cannot load ticket types: movieId is missing');
        showError('ticketOptions', 'Không tìm thấy thông tin phim!');
        return;
    }

    // Show loading
    ticketOptionsContainer.innerHTML = `
        <div class="loading-message text-center py-4">
            <i class="fas fa-spinner fa-spin fa-2x"></i>
            <p class="mt-2">Đang tải thông tin vé...</p>
        </div>
    `;

    try {
        console.log(`Loading ticket types for movieId: ${movieId}`);
        const response = await fetch(`/Admin/EmployeeSale/GetTicketTypes?movieId=${encodeURIComponent(movieId)}`);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();
        console.log('Ticket types result:', result);

        if (result.success) {
            const ticketTypes = result.data;

            // Update ticket prices
            ticketPrices = {
                standard: ticketTypes.Standard.Price,
                vip: ticketTypes.VIP.Price,
                couple: ticketTypes.Couple.Price
            };

            console.log('Updated ticket prices:', ticketPrices);

            // Generate ticket options HTML
            generateTicketOptionsHTML(ticketTypes);
        } else {
            console.error('API Error:', result.message);
            ticketOptionsContainer.innerHTML = `
                <div class="alert alert-warning">
                    <i class="fas fa-exclamation-triangle"></i>
                    ${result.message}
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading ticket types:', error);
        ticketOptionsContainer.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle"></i>
                Có lỗi xảy ra khi tải thông tin vé
            </div>
        `;
    }
}

// Function to generate ticket options HTML
function generateTicketOptionsHTML(ticketTypes) {
    const ticketOptionsContainer = document.getElementById('ticketOptions');

    ticketOptionsContainer.innerHTML = `
        <div class="ticket-option" data-type="standard" data-price="${ticketTypes.Standard.Price}">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h5><i class="${ticketTypes.Standard.Icon}"></i> ${ticketTypes.Standard.Name}</h5>
                    <p class="text-muted mb-0">${ticketTypes.Standard.Description}</p>
                </div>
                <div class="text-end">
                    <h4 class="text-primary mb-0">${formatPrice(ticketTypes.Standard.Price)}</h4>
                    <div class="quantity-control mt-2">
                        <button class="quantity-btn" onclick="changeQuantity('standard', -1)">-</button>
                        <span class="quantity-value" id="qty-standard">0</span>
                        <button class="quantity-btn" onclick="changeQuantity('standard', 1)">+</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="ticket-option" data-type="vip" data-price="${ticketTypes.VIP.Price}">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h5><i class="${ticketTypes.VIP.Icon}"></i> ${ticketTypes.VIP.Name}</h5>
                    <p class="text-muted mb-0">${ticketTypes.VIP.Description}</p>
                </div>
                <div class="text-end">
                    <h4 class="text-primary mb-0">${formatPrice(ticketTypes.VIP.Price)}</h4>
                    <div class="quantity-control mt-2">
                        <button class="quantity-btn" onclick="changeQuantity('vip', -1)">-</button>
                        <span class="quantity-value" id="qty-vip">0</span>
                        <button class="quantity-btn" onclick="changeQuantity('vip', 1)">+</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="ticket-option" data-type="couple" data-price="${ticketTypes.Couple.Price}">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h5><i class="${ticketTypes.Couple.Icon}"></i> ${ticketTypes.Couple.Name}</h5>
                    <p class="text-muted mb-0">${ticketTypes.Couple.Description}</p>
                </div>
                <div class="text-end">
                    <h4 class="text-primary mb-0">${formatPrice(ticketTypes.Couple.Price)}</h4>
                    <div class="quantity-control mt-2">
                        <button class="quantity-btn" onclick="changeQuantity('couple', -1)">-</button>
                        <span class="quantity-value" id="qty-couple">0</span>
                        <button class="quantity-btn" onclick="changeQuantity('couple', 1)">+</button>
                    </div>
                </div>
            </div>
        </div>
    `;
}

// Format price function
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + ' ₫';
}

// Ticket quantity
function changeQuantity(type, delta) {
    bookingData.tickets[type] = Math.max(0, bookingData.tickets[type] + delta);
    const qtyElement = document.getElementById(`qty-${type}`);
    if (qtyElement) {
        qtyElement.textContent = bookingData.tickets[type];
    }

    const totalTickets = Object.values(bookingData.tickets).reduce((a, b) => a + b, 0);
    if (totalTickets > 0) {
        document.getElementById('step4').classList.remove('hidden');
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

// Update summary
function updateSummary() {
    const summaryBox = document.getElementById('summaryBox');
    const summaryMovie = document.getElementById('summary-movie');
    const summaryDateTime = document.getElementById('summary-datetime');
    const summaryTickets = document.getElementById('summary-tickets');
    const summaryCombos = document.getElementById('summary-combos');
    const summaryTotal = document.getElementById('summary-total');

    if (summaryBox) summaryBox.classList.remove('hidden');
    if (summaryMovie) summaryMovie.textContent = bookingData.movieId;

    if (bookingData.date && bookingData.time && summaryDateTime) {
        const dateStr = new Date(bookingData.date).toLocaleDateString('vi-VN');
        summaryDateTime.textContent = `${dateStr} - ${bookingData.time} (${bookingData.roomName || ''})`;
    }

    // Tickets summary
    const ticketsSummary = [];
    if (bookingData.tickets.standard > 0) ticketsSummary.push(`Thường x${bookingData.tickets.standard}`);
    if (bookingData.tickets.vip > 0) ticketsSummary.push(`VIP x${bookingData.tickets.vip}`);
    if (bookingData.tickets.couple > 0) ticketsSummary.push(`Đôi x${bookingData.tickets.couple}`);
    if (summaryTickets) summaryTickets.textContent = ticketsSummary.join(', ') || '-';

    // Combos summary
    const combosSummary = [];
    if (bookingData.combos.combo1 > 0) combosSummary.push(`Combo 1 x${bookingData.combos.combo1}`);
    if (bookingData.combos.combo2 > 0) combosSummary.push(`Combo 2 x${bookingData.combos.combo2}`);
    if (bookingData.combos.popcorn > 0) combosSummary.push(`Bắp x${bookingData.combos.popcorn}`);
    if (bookingData.combos.drink > 0) combosSummary.push(`Nước x${bookingData.combos.drink}`);
    if (summaryCombos) summaryCombos.textContent = combosSummary.join(', ') || 'Không có';

    // Calculate total
    let total = 0;
    for (let type in bookingData.tickets) {
        total += bookingData.tickets[type] * (ticketPrices[type] || 0);
    }
    for (let combo in bookingData.combos) {
        total += bookingData.combos[combo] * (comboPrices[combo] || 0);
    }
    if (summaryTotal) summaryTotal.textContent = formatPrice(total);
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