document.addEventListener('DOMContentLoaded', function () {
    const selected = document.querySelector('.location-selected');
    const dropdown = document.querySelector('.location-dropdown');
    const options = document.querySelectorAll('.location-option');
    const locationName = document.querySelector('.location-name');
    const dropdownArrow = document.querySelector('.dropdown-arrow');

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
    //let selectedSeats = [];
    let customerData = null;
    // ✅ Thêm biến lưu thông tin đặt vé
    let selectedSeatsData = [];
    let selectedShowTimeId = null
    let selectedProducts = []; // ✅ THÊM MỚI: Lưu sản phẩm đã chọn

    // Toggle dropdown
    selected.addEventListener('click', function () {
        dropdown.classList.toggle('show');
        // Xoay mũi tên giống cinestar
        if (dropdown.classList.contains('show')) {
            dropdownArrow.style.transform = 'rotate(180deg)';
        } else {
            dropdownArrow.style.transform = 'rotate(0deg)';
        }
    });

    // Chọn thành phố và tải lại danh sách rạp
    options.forEach(option => {
        option.addEventListener('click', function () {
            const city = this.dataset.city;
            const movieId = this.dataset.movieId;

            locationName.textContent = this.textContent;
            options.forEach(opt => opt.classList.remove('active'));
            this.classList.add('active');
            dropdown.classList.remove('show');
            dropdownArrow.style.transform = 'rotate(0deg)';

            // Tải lại danh sách rạp
            loadBranches(city, movieId);
        });
    });

    // Đóng dropdown khi click ra ngoài
    document.addEventListener('click', function (e) {
        if (!selected.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.remove('show');
            dropdownArrow.style.transform = 'rotate(0deg)';
        }
    });

    // === XỬ LÝ CHỌN NGÀY (7 ngày đã được render từ server) ===
    document.querySelectorAll('.box-time').forEach(function (boxTime) {
        boxTime.addEventListener('click', function () {
            // Xóa active khỏi tất cả các ngày
            document.querySelectorAll('.box-time').forEach(box => box.classList.remove('active'));

            // Thêm active vào ngày được chọn
            this.classList.add('active');

            const selectedDate = this.dataset.date;
            const movieId = this.dataset.movieId;
            const activeBranch = document.querySelector('.cinestar-item.open');

            // Nếu đã chọn rạp, load lại lịch chiếu theo ngày mới
            if (activeBranch) {
                const branchId = activeBranch.dataset.branchId;
                loadShowTimes(branchId, movieId, selectedDate);
            }
        });
    });

    // === XỬ LÝ CINESTAR DROPDOWN ===
    document.addEventListener('click', function (e) {
        // Kiểm tra xem element được click có phải là cinestar-heading hay element con của nó
        const heading = e.target.closest('.cinestar-heading');

        if (heading) {
            console.log('🎯 Cinestar heading clicked');
            console.log('Đã nhấn vào cinestar-heading');

            const item = heading.closest('.cinestar-item');
            const wasOpen = item.classList.contains('open');

            // Đóng tất cả các item khác
            document.querySelectorAll('.cinestar-item').forEach(i => i.classList.remove('open'));

            // Mở/đóng item hiện tại
            if (!wasOpen) {
                item.classList.add('open');
                console.log('✅ Opening branch:', item.dataset.branchId);

                // Cập nhật tên rạp trong sticky bar
                const cinemaName = item.querySelector('.title').textContent;
                const cinemaNameEl = document.getElementById('cinemaName');
                if (cinemaNameEl) {
                    cinemaNameEl.textContent = cinemaName;
                    console.log('✅ Cinema name updated:', cinemaName);
                }

                // Load lịch chiếu cho rạp này (nếu chưa có)
                const branchId = item.dataset.branchId;
                const showtimeContainer = item.querySelector('.showtime-container');

                // Kiểm tra xem đã load showtimes chưa
                const hasShowtimes = showtimeContainer.querySelector('.list-infor');

                if (!hasShowtimes) {
                    const selectedDate = document.querySelector('.box-time.active')?.dataset.date;
                    const movieId = document.querySelector('.box-time.active')?.dataset.movieId;

                    if (branchId && selectedDate && movieId) {
                        console.log('📅 Loading showtimes for:', { branchId, movieId, selectedDate });
                        loadShowTimes(branchId, movieId, selectedDate);
                    }
                } else {
                    console.log('ℹ️ Showtimes already loaded');
                }
            } else {
                console.log('🔽 Closing branch');
            }
        }
    });

    // === XỬ LÝ CLICK CHO TIME SLOTS ===
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('item-time')) {
            const container = e.target.closest('.showtime-container');
            container.querySelectorAll('.item-time').forEach(item => item.classList.remove('active'));
            e.target.classList.add('active');

            const showTimeId = e.target.dataset.showtimeId;
            const timeDisplay = e.target.textContent.trim();

            selectedShowTimeId = showTimeId;

            // ✅ Lấy tên rạp từ cinestar-item cha
            const cinemaItem = e.target.closest('.cinestar-item');
            const cinemaName = cinemaItem?.querySelector('.title')?.textContent || 'Chưa chọn rạp';

            console.log('=== SHOWTIME CLICKED ===');
            console.log('Cinema Item:', cinemaItem);
            console.log('Cinema Name:', cinemaName);
            console.log('ShowTime ID:', showTimeId);

            // ✅ Cập nhật tên rạp vào sticky bar
            const cinemaNameEl = document.getElementById('cinemaName');
            if (cinemaNameEl) {
                cinemaNameEl.textContent = cinemaName;
                console.log('✅ Cinema name updated in sticky bar:', cinemaName);
            }

            // Cập nhật sticky bar
            document.getElementById('selectedSeatsDisplay').textContent = `Suất: ${timeDisplay}`;
        }
    });

    //// === XỬ LÝ TĂNG/GIẢM SỐ LƯỢNG VÉ ===
    //document.addEventListener('click', function(e) {
    //    if (e.target.classList.contains('decrease')) {
    //        const quantity = e.target.nextElementSibling;
    //        let count = parseInt(quantity.textContent);
    //        if (count > 0) {
    //            quantity.textContent = count - 1;
    //            updateTotalPrice();
    //        }
    //    }

    //    if (e.target.classList.contains('increase')) {
    //        const quantity = e.target.previousElementSibling;
    //        let count = parseInt(quantity.textContent);
    //        quantity.textContent = count + 1;
    //        updateTotalPrice();
    //    }
    //});

    // === HÀM TẠO DANH SÁCH RẠP ===
    function loadBranches(city, movieId) {
        const container = document.getElementById('branchesContainer');
        container.innerHTML = '<p class="text-center" style="color: white;">Đang tải...</p>';

        console.log('🌆 Loading branches for city:', city, 'movie:', movieId);

        fetch(`/Movie/GetBranchesByCity?city=${encodeURIComponent(city)}&movieId=${movieId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(branches => {
                console.log('📍 Branches loaded:', branches.length);

                if (branches.length === 0) {
                    container.innerHTML = '<p class="no-data" style="color: white;">Không có rạp chiếu phim này tại khu vực đã chọn</p>';
                    return;
                }

                // ✅ Render HTML cho các rạp
                let html = '<ul class="cinestar-list">';
                branches.forEach((branch, index) => {
                    html += `
                    <li class="cinestar-item ${index === 0 ? 'open' : ''}"
                        data-branch-id="${branch.branchId}">
                        <div class="cinestar-heading">
                            <h4 class="title">${branch.branchName}</h4>
                            <span class="chevron">
                                <i class="fa-solid fa-chevron-down" style="color: #ffffff;"></i>
                            </span>
                        </div>
                        <div class="cinestar-body">
                            <p class="address">${branch.address}, ${branch.district}</p>
                            <div class="showtime-container" data-branch-id="${branch.branchId}">
                                <p class="loading" style="color: white;">Đang tải lịch chiếu...</p>
                            </div>
                        </div>
                    </li>`;
                });
                html += '</ul>';
                container.innerHTML = html;

                console.log('✅ Branches rendered to DOM');

                // ✅ Auto load showtimes cho rạp đầu tiên
                const selectedDate = document.querySelector('.box-time.active')?.dataset.date ||
                    new Date().toISOString().split('T')[0];

                const firstBranch = branches[0];
                if (firstBranch) {
                    console.log('🎬 Auto-loading showtimes for first branch:', firstBranch.branchId);

                    // Cập nhật tên rạp trong sticky bar
                    document.getElementById('cinemaName').textContent = firstBranch.branchName;

                    // Load showtimes
                    loadShowTimes(firstBranch.branchId, movieId, selectedDate);
                }

                // ✅ KHÔNG CẦN gắn event listener ở đây nữa vì đã dùng event delegation
            })
            .catch(error => {
                console.error('❌ Error loading branches:', error);
                container.innerHTML = '<p class="error" style="color: red;">Không thể tải danh sách rạp. Vui lòng thử lại.</p>';
            });
    }

    // === HÀM TẠO LỊCH CHIẾU ===
    function loadShowTimes(branchId, movieId, date) {
        console.log('📺 loadShowTimes called:', { branchId, movieId, date });

        const container = document.querySelector(`.showtime-container[data-branch-id="${branchId}"]`);
        if (!container) {
            console.error('❌ Container not found for branchId:', branchId);
            return;
        }

        console.log('✅ Container found, loading...');
        container.innerHTML = '<p class="loading" style="color: white;">Đang tải lịch chiếu...</p>';

        fetch(`/Movie/GetShowTimes?branchId=${branchId}&movieId=${movieId}&date=${date}`)
            .then(response => {
                console.log('📡 Response status:', response.status);
                return response.json();
            })
            .then(showTimeGroups => {
                console.log('📅 Showtimes loaded:', showTimeGroups);

                if (!Array.isArray(showTimeGroups) || showTimeGroups.length === 0) {
                    container.innerHTML = '<p class="no-data" style="color: white;">Không có suất chiếu trong ngày này</p>';
                    return;
                }

                let html = '<ul class="list-infor">';
                showTimeGroups.forEach(group => {
                    html += `
                    <li class="item-infor">
                        <div class="tt">${group.roomType}</div>
                        <ul class="list-time">`;

                    group.showTimes.forEach(st => {
                        html += `<li class="item-time" data-showtime-id="${st.showTimeID}" data-price="${st.basePrice}" data-room-name="${st.nameRoom}">
                        ${st.timeDisplay}
                    </li>`;
                    });

                    html += `</ul>
                    </li>`;
                });
                html += '</ul>';
                container.innerHTML = html;

                console.log('✅ Showtimes rendered');
            })
            .catch(error => {
                console.error('❌ Error loading showtimes:', error);
                container.innerHTML = '<p class="error" style="color: red;">Không thể tải lịch chiếu</p>';
            });
    }

    // === HÀM TẠO THÔNG TIN GIÁ VÉ ===
    //function loadTicketPrices(showTimeId) {
    //    const container = document.getElementById('ticketContainer');
    //    container.innerHTML = '<p class="text-center">Đang tải...</p>';

    //    fetch(`/Movie/GetTicketPrices?showTimeId=${showTimeId}`)
    //        .then(response => response.json())
    //        .then(prices => {
    //            if (prices.length === 0) {
    //                container.innerHTML = '<p class="no-data">Không có thông tin vé</p>';
    //                return;
    //            }

    //            let html = '';
    //            // Tạo một .content riêng biệt cho mỗi loại vé
    //            prices.forEach(price => {
    //                html += `
    //                <div class="content">
    //                    <div class="content-top">
    //                        <p class="name">${price.ticketType}</p>
    //                        <div class="desc">
    //                            <p>${price.description}</p>
    //                        </div>
    //                        <div class="price">
    //                            <p>${price.price.toLocaleString('vi-VN')} VNĐ</p>
    //                        </div>
    //                    </div>
    //                    <div class="content-bottom">
    //                        <div class="count">
    //                            <div class="count-btn">
    //                                <button class="decrease">-</button>
    //                                <span class="quantity">0</span>
    //                                <button class="increase">+</button>
    //                            </div>
    //                        </div>
    //                    </div>
    //                </div>`;
    //            });
    //            container.innerHTML = html;
    //        })
    //   .catch(error => {
    //            console.error('Error:', error);
    //     container.innerHTML = '<p class="error">Không thể tải thông tin vé</p>';
    //        });
    //}

    // === CẬP NHẬT TỔNG TIỀN ===
    function updateTotalPrice() {
        let total = 0;

        // Tính tổng tiền từ ghế
        selectedSeatsData.forEach(seat => {
            total += seat.price || 0;
        });

        //  tổng tiền từ sản phẩm
        selectedProducts.forEach(product => {
            total += product.totalPrice || 0;
        });

        document.getElementById('totalPrice').textContent = total.toLocaleString('vi-VN') + ' VNĐ';

        // Enable nút thanh toán khi đã chọn ghế
        const bookBtn = document.getElementById('bookBtn');
        if (bookBtn) {
            bookBtn.disabled = selectedSeatsData.length === 0;
        }
    }

    // Xử lý tăng/giảm số lượng sản phẩm
    document.addEventListener('click', function (e) {
        // Xử lý nút tăng
        if (e.target.classList.contains('plus')) {
            const card = e.target.closest('.product-card');
            const productId = card.dataset.productId;
            const qtyValue = card.querySelector('.qty-value');
            const productName = card.querySelector('.product-name').textContent;
            const priceElement = card.querySelector('.product-price');
            const price = parseInt(priceElement.dataset.price);

            let currentQty = parseInt(qtyValue.textContent);
            currentQty += 1;
            qtyValue.textContent = currentQty;

            // Cập nhật hoặc thêm sản phẩm vào danh sách
            updateProductSelection(productId, productName, price, currentQty);
            updateTotalPrice();
        }

        // Xử lý nút giảm
        if (e.target.classList.contains('minus')) {
            const card = e.target.closest('.product-card');
            const productId = card.dataset.productId;
            const qtyValue = card.querySelector('.qty-value');
            const productName = card.querySelector('.product-name').textContent;
            const priceElement = card.querySelector('.product-price');
            const price = parseInt(priceElement.dataset.price);

            let currentQty = parseInt(qtyValue.textContent);
            if (currentQty > 0) {
                currentQty--;
                qtyValue.textContent = currentQty;

                // Cập nhật hoặc xóa sản phẩm khỏi danh sách
                updateProductSelection(productId, productName, price, currentQty);
                updateTotalPrice();
            }
        }
    });

    // Hàm cập nhật sản phẩm đã chọn
    function updateProductSelection(productId, productName, price, quantity) {
        const existingProductIndex = selectedProducts.findIndex(p => p.productId === productId);

        if (quantity === 0) {
            // Xóa sản phẩm nếu số lượng = 0
            if (existingProductIndex !== -1) {
                selectedProducts.splice(existingProductIndex, 1);
            }
        } else {
            // Cập nhật hoặc thêm sản phẩm
            if (existingProductIndex !== -1) {
                selectedProducts[existingProductIndex].quantity = quantity;
                selectedProducts[existingProductIndex].totalPrice = price * quantity;
            } else {
                selectedProducts.push({
                    productId: productId,
                    productName: productName,
                    price: price,
                    quantity: quantity,
                    totalPrice: price * quantity
                });
            }
        }

        console.log('✅ Selected Products:', selectedProducts);
    }

    
    // === XỬ LÝ CHỌN GHẾ VỚI SIGNALR ===

    // Kết nối SignalR
    let connection = null;
    let currentShowTimeId = null;
    let currentCustomerId = null;

    async function initSignalR() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/seatHub")
            .configureLogging(signalR.LogLevel.Information) 
            .withAutomaticReconnect()
            .build();

        connection.on("SeatSelected", function (data) {
            const seatElement = document.querySelector(`td.seat[data-seat-id="${data.seatId}"]`);
            if (seatElement) {
                console.log("Tìm thấy ghế:", data.seatId);
                if (data.customerId.toString() === currentCustomerId.toString()) {
                    seatElement.className = 'seat selected'; 
                } else {
                    seatElement.className = 'seat choosing';                  
                }
            } else {
                console.log("Không tìm thấy ghế:", data.seatId);
            }
        });

        // Lắng nghe sự kiện: Người khác BỎ CHỌN ghế
        connection.on("SeatDeselected", function (data) {
            const seatElement = document.querySelector(`td.seat[data-seat-id="${data.seatId}"]`);
            if (seatElement) {
                const seatType = seatElement.dataset.seatType;

                if (seatType === 'Ghế đôi') {
                    seatElement.className = 'seat couple';
                } else if (seatType === 'Ghế VIP') {
                    seatElement.className = 'seat vip';
                } else {
                    seatElement.className = 'seat regular';
                }
            }
        });

        // Kết nối
        try {
            await connection.start();
            console.log("SignalR connected successfully!");
        } catch (err) {
            console.error("SignalR connection error:", err);
        }
    }
        
    // Click vào li để load ghế
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('item-time')) {
            document.querySelectorAll('.item-time').forEach(item => {
                item.classList.remove('active');
            });
        
            e.target.classList.add('active');
            const showTimeId = e.target.dataset.showtimeId;
            const roomName = e.target.dataset.roomName;

            const titleElement = document.querySelector('.TitleBrand');
            if (titleElement) {
                titleElement.textContent = 'Chọn ghế - ' + roomName;
            }

            currentCustomerId = document.getElementById("CustomerId").innerHTML;
            console.log("ShowTimeID: " + showTimeId);
            console.log("CustomerID: " + currentCustomerId);

            // Rời nhóm cũ (nếu có)
            if (currentShowTimeId && connection) {
                connection.invoke("LeaveShowTime", currentShowTimeId);
            }
      
            currentShowTimeId = showTimeId;
            if (connection) {
                connection.invoke("JoinShowTime", showTimeId);
            }

            loadSeatingLayout(showTimeId, currentCustomerId);
        }
    });

    console.log("Khởi động SignalR...");
    initSignalR();

    // Load danh sách ghế từ server
    function loadSeatingLayout(showTimeId, currentCustomerId) {
        fetch(`/Movie/GetSeatingLayout?showTimeId=${showTimeId}&currentCustomerId=${currentCustomerId}`)
            .then(response => response.json())
            .then(seats => {
                renderSeats(seats, showTimeId);
            })
            .catch(error => {
                console.error('Lỗi:', error);
            });
    }

    // Render ghế ra bảng
    function renderSeats(seats, showTimeId) {
        const table = document.getElementById('seatingTable');

        // Nhóm ghế theo hàng (A, B, C...)
        const rows = {};
        seats.forEach(seat => {
            const row = seat.seatName[0];
            if (!rows[row]) rows[row] = [];
            rows[row].push(seat);
        });

        table.innerHTML = '';

        // Số cột tối đa của hàng
        const totalCols = 17;

        for (let rowName in rows) {
            const tr = document.createElement('tr');

            // Label hàng (A, B, C...)
            const tdLabel = document.createElement('td');
            tdLabel.className = 'row-label';
            tdLabel.textContent = rowName;
            tr.appendChild(tdLabel);

            // Tính số cột thực tế
            let realCols = 0;
            rows[rowName].forEach(seat => {
                realCols += (seat.seatType === 'Ghế đôi') ? 2 : 1;
            });

            const emptyBefore = Math.floor((totalCols - realCols) / 2);
            const emptyAfter = totalCols - realCols - emptyBefore;

            // Thêm ô trống bên trái
            for (let i = 0; i < emptyBefore; i++) {
                const tdEmpty = document.createElement('td');
                tdEmpty.className = 'empty';
                tr.appendChild(tdEmpty);
            }

            // Vẽ từng ghế
            rows[rowName].forEach(seat => {
                const td = document.createElement('td');
                td.textContent = seat.seatName;
                td.dataset.seatId = seat.seatID;
                td.dataset.seatType = seat.seatType;

                // Xác định class theo trạng tháis
                let seatClass = 'seat';

                if (seat.status === 'Đã đặt' || seat.status === 'Đã thanh toán') {
                    seatClass += ' booked';
                }
                else if (seat.status === 'Đang được chọn') {
                    // Kiểm tra: Ghế do mình chọn hay người khác chọn?
                    if (seat.isMyChoice) {
                        seatClass += ' selected'; // Ghế của mình
                    } else {
                        seatClass += ' choosing'; // Ghế người khác
                    }
                }
                else {
                    // Ghế trống
                    if (seat.seatType === 'Ghế đôi') {
                        seatClass += ' couple';
                    } else if (seat.seatType === 'Ghế VIP') {
                        seatClass += ' vip';
                    } else {
                        seatClass += ' regular';
                    }
                }

                // Ghế đôi chiếm 2 cột
                if (seat.seatType === 'Ghế đôi') {
                    td.colSpan = 2;
                }

                td.className = seatClass;
                tr.appendChild(td);
            });

            // Thêm ô trống bên phải
            for (let i = 0; i < emptyAfter; i++) {
                const tdEmpty = document.createElement('td');
                tdEmpty.className = 'empty';
                tr.appendChild(tdEmpty);
            }

            table.appendChild(tr);
        }

        // Gắn sự kiện click
        addSeatClickEvents(showTimeId);
    }

    // Hàm xử lý click chọn ghế
    function addSeatClickEvents(showTimeId) {
        const seats = document.querySelectorAll('td.seat:not(.booked):not(.choosing)');

        seats.forEach(seat => {
            seat.addEventListener('click', async function () {
                const seatId = this.dataset.seatId;
                const seatType = this.dataset.seatType;
                const seatName = this.textContent.trim();

                if (this.classList.contains('selected')) {
                    // ✅ BỎ CHỌN GHẾ
                    try {
                        const response = await fetch(`/Movie/DeselectSeat?showTimeId=${showTimeId}&seatId=${seatId}`, {
                            method: 'POST'
                        });

                        const result = await response.json();

                        if (result.success) {
                            this.classList.remove('selected');

                            // ✅ Xóa khỏi danh sách đã chọn
                            selectedSeatsData = selectedSeatsData.filter(s => s.seatId !== seatId);

                            // Trả về class gốc
                            if (seatType === 'Ghế đôi') {
                                this.classList.add('couple');
                            } else if (seatType === 'Ghế VIP') {
                                this.classList.add('vip');
                            } else {
                                this.classList.add('regular');
                            }

                            // ✅ Cập nhật tổng tiền
                            updateTotalPrice();
                            updateSeatDisplay();
                        } else {
                            alert('Không thể bỏ chọn ghế này!');
                        }
                    } catch (error) {
                        console.error('Lỗi bỏ chọn:', error);
                    }

                } else {
                    // ✅ CHỌN GHẾ MỚI
                    const selectedSeats = document.querySelectorAll('td.seat.selected');
                    if (selectedSeats.length >= 8) {
                        alert('Bạn chỉ được chọn tối đa 8 ghế!');
                        return;
                    }

                    try {
                        // Chọn ghế trên server
                        const selectResponse = await fetch(`/Movie/SelectSeats?showTimeId=${showTimeId}&seatId=${seatId}`, {
                            method: 'POST'
                        });

                        const selectResult = await selectResponse.json();

                        if (selectResult.success) {
                            // ✅ LẤY THÔNG TIN VÉ
                            const ticketResponse = await fetch(`/Movie/GetTicketBySeatId?showTimeId=${showTimeId}&seatId=${seatId}`);
                            const ticketData = await ticketResponse.json();

                            console.log('✅ Ticket Info:', ticketData);

                            if (!ticketData) {
                                alert('Không thể lấy thông tin vé!');
                                return;
                            }

                            // ✅ Thêm vào UI
                            this.classList.add('selected');
                            this.classList.remove('regular', 'couple', 'vip');

                            // ✅ Lưu thông tin vé vào danh sách
                            selectedSeatsData.push({
                                seatId: seatId,
                                seatName: seatName,
                                seatType: seatType,
                                ticketId: ticketData.ticketID,
                                ticketType: ticketData.ticketType,
                                price: ticketData.price || 0,
                                status: ticketData.status
                            });

                            console.log('✅ Selected Seats Data:', selectedSeatsData);

                            // ✅ Cập nhật tổng tiền
                            updateTotalPrice();
                            updateSeatDisplay();

                        } else {
                            alert('Ghế này đã được người khác chọn!');
                        }
                    } catch (error) {
                        console.error('Lỗi chọn ghế:', error);
                        alert('Có lỗi xảy ra khi chọn ghế!');
                    }
                }
            });
        });
    }

    // ✅ Hàm cập nhật hiển thị ghế đã chọn
    function updateSeatDisplay() {
        const displayElement = document.getElementById('selectedSeatsDisplay');
        if (selectedSeatsData.length > 0) {
            const seatNames = selectedSeatsData.map(s => s.seatName).join(', ');
            displayElement.textContent = `Ghế: ${seatNames}`;
        } else {
            displayElement.textContent = 'Chưa chọn ghế';
        }
    }

    // ✅ XỬ LÝ NÚT ĐẶT VÉ
    const bookBtn = document.getElementById('bookBtn');
    if (bookBtn) {
        bookBtn.addEventListener('click', function () {
            console.log('=== BOOK BUTTON CLICKED ===');

            // Validation
            if (selectedSeatsData.length === 0) {
                alert('Vui lòng chọn ghế trước khi đặt vé!');
                return;
            }

            if (!selectedShowTimeId) {
                alert('Vui lòng chọn suất chiếu!');
                return;
            }

            // Lấy thông tin phim
            const movieTitle = document.querySelector('.movie-title1')?.textContent || 'N/A';
            const cinemaName = document.getElementById('cinemaName')?.textContent || 'N/A';
            const activeBranch = document.querySelector('.cinestar-item.open');
            const cinemaAddress = activeBranch?.querySelector('.address')?.textContent || '';

            // Lấy thông tin suất chiếu
            const selectedTimeSlot = document.querySelector('.item-time.active');
            const showTimeDisplay = selectedTimeSlot?.textContent.trim() || '';
            const selectedDate = document.querySelector('.box-time.active')?.dataset.date || '';

            // Format ngày giờ
            const dateObj = new Date(selectedDate);
            const dateStr = dateObj.toLocaleDateString('vi-VN', {
                weekday: 'long',
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            });

            // Lấy thông tin phòng chiếu
            const roomType = selectedTimeSlot?.closest('.item-infor')?.querySelector('.tt')?.textContent || '';

            // Tính tổng tiền ghế
            let seatsTotal = 0;
            selectedSeatsData.forEach(seat => {
                seatsTotal += seat.price || 0;
            });

            // ✅ Tính tổng tiền sản phẩm
            let productsTotal = 0;
            selectedProducts.forEach(product => {
                productsTotal += product.totalPrice || 0;
            });

            // ✅ Tổng tiền cuối cùng
            const totalAmount = seatsTotal + productsTotal;

            // ✅ Lấy CustomerId (nếu user đã login)
            const customerIdElement = document.getElementById('CustomerId');
            let customerId = '00000000-0000-0000-0000-000000000000'; // Default GUID

            if (customerIdElement && customerIdElement.innerHTML) {
                const customerIdStr = customerIdElement.innerHTML.trim();
                if (customerIdStr && customerIdStr !== '') {
                    customerId = customerIdStr;
                }
            }

            console.log('✅ Customer ID:', customerId);

            // ✅ Chuẩn bị dữ liệu booking (có thêm customerId)
            const bookingData = {
                // Thông tin phim
                movieTitle: movieTitle,
                movieId: document.querySelector('.box-time')?.dataset.movieId || '',

                // Thông tin rạp
                cinemaName: cinemaName,
                cinemaAddress: cinemaAddress,

                // Thông tin suất chiếu
                showTimeId: selectedShowTimeId,
                showTime: showTimeDisplay,
                showDate: dateStr,
                roomType: roomType,
                roomNumber: roomType.match(/\d+/)?.[0] || 'N/A',

                // Thông tin vé và ghế
                seats: selectedSeatsData,
                totalSeats: selectedSeatsData.length,
                seatsTotal: seatsTotal,

                // ✅ THÊM: Thông tin sản phẩm
                products: selectedProducts,
                productsTotal: productsTotal,

                // Thông tin giá
                totalAmount: totalAmount,

                // ✅ THÊM: Customer ID
                customerId: customerId,

                // Thời gian đặt vé
                bookingTime: new Date().toISOString()
            };

            // ✅ Tạo bookingInfo cho payment.js
            const bookingInfo = {
                movieTitle: movieTitle,
                cinema: cinemaName,
                cinemaAddress: cinemaAddress,
                showtime: showTimeDisplay,
                showDate: dateStr,
                room: roomType.match(/\d+/)?.[0] || 'N/A',
                quantity: selectedSeatsData.length,
                ticketType: selectedSeatsData.map(s => s.ticketType).join(', '),
                seat: selectedSeatsData.map(s => s.seatName).join(', '),
                products: selectedProducts,
                seatsTotal: seatsTotal,
                productsTotal: productsTotal,
                amount: totalAmount,
                timeLeft: 5 * 60 // 5 phút
            };

            console.log('✅ Full Booking Data:', JSON.stringify(bookingData, null, 2));
            console.log('✅ Booking Info:', JSON.stringify(bookingInfo, null, 2));

            // ✅ Lưu vào sessionStorage
            sessionStorage.setItem('bookingData', JSON.stringify(bookingData));
            sessionStorage.setItem('bookingInfo', JSON.stringify(bookingInfo));

            // Verify
            const savedData = sessionStorage.getItem('bookingData');
            const savedInfo = sessionStorage.getItem('bookingInfo');
            console.log('✅ Verified saved data:', savedData);
            console.log('✅ Verified saved info:', savedInfo);

            console.log('=== BOOKING SUMMARY ===');
            console.log('Seats Total:', seatsTotal);
            console.log('Products Total:', productsTotal);
            console.log('Total Amount:', totalAmount);
            console.log('Selected Products:', selectedProducts);
            console.log('Customer ID:', customerId);

            // Chuyển sang trang thanh toán
            window.location.href = '/Payment/Index';
        });
    }
});
// ✅ AUTO-LOAD SHOWTIMES KHI TRANG LOAD
document.addEventListener('DOMContentLoaded', function () {
    // Lấy ngày hiện tại (ngày đầu tiên đã active)
    const activeDate = document.querySelector('.box-time.active');
    const movieId = activeDate?.dataset.movieId;
    const selectedDate = activeDate?.dataset.date;

    console.log('🚀 Page loaded - Auto loading showtimes');
    console.log('Movie ID:', movieId);
    console.log('Selected Date:', selectedDate);

    // Lấy tất cả các rạp đang hiển thị
    const allBranches = document.querySelectorAll('.cinestar-item');

    if (allBranches.length > 0 && movieId && selectedDate) {
        console.log(`📍 Found ${allBranches.length} branches, loading showtimes...`);

        allBranches.forEach((branch, index) => {
            const branchId = branch.dataset.branchId;

            // Mở rạp đầu tiên
            if (index === 0) {
                branch.classList.add('open');

                // Cập nhật tên rạp vào sticky bar
                const cinemaName = branch.querySelector('.title')?.textContent;
                if (cinemaName) {
                    document.getElementById('cinemaName').textContent = cinemaName;
                }
            }

            // Load showtimes cho mỗi rạp
            if (branchId) {
                console.log(`Loading showtimes for branch ${branchId}`);
                loadShowTimes(branchId, movieId, selectedDate);
            }
        });
    } else {
        console.log('⚠️ No branches found or missing date/movie info');
    }
});