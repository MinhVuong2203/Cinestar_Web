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

// Set min date to today
const today = new Date().toISOString().split('T')[0];
const dateSelectElement = document.getElementById('dateSelect');

if (dateSelectElement) {
    dateSelectElement.min = today;
    dateSelectElement.value = today;
    bookingData.date = today;

    // Get movieId from data-attribute
    movieIdFromElement = dateSelectElement.dataset.movieId;
    console.log('movieId from data-attribute:', movieIdFromElement);

    // Use movieId from inline script first, fallback to data-attribute
    bookingData.movieId = movieIdFromScript || movieIdFromElement;

    console.log('Final movieId:', bookingData.movieId);
    console.log('Date:', bookingData.date);

    if (bookingData.movieId) {
        // Load showtimes for today immediately
        loadShowTimes(bookingData.movieId, today);
        // Load ticket types
        loadTicketTypes(bookingData.movieId);
    } else {
        console.error('CRITICAL: movieId not found in inline script or data-attribute!');
        showError('timeGrid', 'Không tìm thấy thông tin phim!');
    }
} else {
    console.error('dateSelect element not found!');
}

// Date selection - load showtimes
if (dateSelectElement) {
    dateSelectElement.addEventListener('change', function () {
        console.log('Date changed to:', this.value);
        bookingData.date = this.value;

        if (bookingData.movieId) {
            console.log(`Calling loadShowTimes with movieId='${bookingData.movieId}', date='${bookingData.date}'`);
            loadShowTimes(bookingData.movieId, bookingData.date);
        } else {
            console.error('Cannot load showtimes: movieId is missing');
        }
    });
}

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

// Load showtimes for selected date
async function loadShowTimes(movieId, date) {
    const timeGridContainer = document.getElementById('timeGrid');

    if (!timeGridContainer) {
        console.error('timeGrid container not found');
        return;
    }

    // Validate inputs
    if (!movieId) {
        console.error('ERROR: movieId is null or undefined');
        showError('timeGrid', 'Không tìm thấy thông tin phim!');
        return;
    }

    if (!date) {
        console.error('ERROR: date is null or undefined');
        showError('timeGrid', 'Vui lòng chọn ngày!');
        return;
    }

    // Show loading
    timeGridContainer.innerHTML = `
        <div class="loading-message text-center py-4">
            <i class="fas fa-spinner fa-spin fa-2x"></i>
            <p class="mt-2">Đang tải suất chiếu...</p>
        </div>
    `;

    try {
        console.log('=== Loading Showtimes ===');
        console.log(`MovieId: '${movieId}'`);
        console.log(`Date: '${date}'`);

        // CRITICAL: Use the parameters passed to function, not bookingData
        const url = `/Admin/EmployeeSale/GetShowTimes?movieId=${encodeURIComponent(movieId)}&date=${encodeURIComponent(date)}`;
        console.log(`Fetching: ${url}`);

        //const response = await fetch(url);
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            console.log('JS - Response status:', response.status);
            if (!response.ok) {
                const errorText = await response.text();
                console.error('JS - Error body:', errorText);
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const data = await response.json();
            console.log('JS - Success data:', data);
        } catch (error) {
            console.error('JS - Fetch error:', error);
        }
        console.log(`Response status: ${response.status} ${response.statusText}`);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();
        console.log('API Response:', result);

        if (result.success && result.data && result.data.length > 0) {
            console.log(`Found ${result.data.length} showtimes`);
            generateShowTimesHTML(result.data);
        } else {
            console.log('No showtimes found');
            timeGridContainer.innerHTML = `
                <div class="no-movies text-center py-4">
                    <i class="fas fa-exclamation-circle fa-3x text-muted"></i>
                    <p class="text-muted mt-3">Không có suất chiếu nào cho ngày này</p>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading showtimes:', error);
        timeGridContainer.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle"></i>
                Có lỗi xảy ra khi tải suất chiếu: ${error.message}
            </div>
        `;
    }
}

// Generate showtimes HTML with proper date formatting
function generateShowTimesHTML(showTimes) {
    const timeGridContainer = document.getElementById('timeGrid');

    let html = '';
    showTimes.forEach(showTime => {
        // Format StartTime - handle both string and Date object
        let timeDisplay = showTime.startTime || showTime.StartTime;

        // If it's a full datetime string, extract time only
        if (typeof timeDisplay === 'string') {
            // Try to parse as Date
            const dateObj = new Date(timeDisplay);
            if (!isNaN(dateObj.getTime())) {
                // Format as HH:mm
                timeDisplay = dateObj.toLocaleTimeString('vi-VN', {
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false
                });
            }
        }

        const showTimeId = showTime.showTimeID || showTime.ShowTimeID;
        const roomName = showTime.roomName || showTime.RoomName;
        const roomType = showTime.roomType || showTime.RoomType;
        const availableSeats = showTime.availableSeats || showTime.AvailableSeats || 0;
        const totalSeats = showTime.totalSeats || showTime.TotalSeats || 0;

        console.log(`Rendering showtime: ${showTimeId} at ${timeDisplay} in ${roomName}`);

        html += `
            <div class="time-slot" 
                 data-showtime-id="${showTimeId}" 
                 data-time="${timeDisplay}" 
                 data-room="${roomName}">
                <h5>${timeDisplay}</h5>
                <small class="text-muted">${roomName} (${roomType})</small>
                <small class="d-block text-success">${availableSeats}/${totalSeats} ghế trống</small>
            </div>
        `;
    });

    timeGridContainer.innerHTML = html;

    // Add click event listeners to time slots
    document.querySelectorAll('.time-slot').forEach(slot => {
        slot.addEventListener('click', function () {
            document.querySelectorAll('.time-slot').forEach(s => s.classList.remove('selected'));
            this.classList.add('selected');

            bookingData.showTime = this.dataset.showtimeId;
            bookingData.time = this.dataset.time;
            bookingData.roomName = this.dataset.room;

            console.log('Selected showtime:', bookingData);

            document.getElementById('step3').classList.remove('hidden');
            updateSummary();
        });
    });
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