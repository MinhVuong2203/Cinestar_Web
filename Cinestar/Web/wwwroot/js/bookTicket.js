document.addEventListener('DOMContentLoaded', function () {
    const selected = document.querySelector('.location-selected');
    const dropdown = document.querySelector('.location-dropdown');
    const options = document.querySelectorAll('.location-option');
    const locationName = document.querySelector('.location-name');
    const dropdownArrow = document.querySelector('.dropdown-arrow');

    // ✅ Thêm biến lưu thông tin đặt vé
    let selectedSeatsData = [];
    let selectedShowTimeId = null

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
    document.querySelectorAll('.cinestar-heading').forEach(function (heading) {
        heading.addEventListener('click', function () {
            const item = heading.closest('.cinestar-item');
            const wasOpen = item.classList.contains('open');

            // Đóng tất cả các item khác
            document.querySelectorAll('.cinestar-item').forEach(i => i.classList.remove('open'));

            // Mở/đóng item hiện tại
            if (!wasOpen) {
                item.classList.add('open');

                // Cập nhật tên rạp trong sticky bar
                const cinemaName = item.querySelector('.title').textContent;
                document.getElementById('cinemaName').textContent = cinemaName;

                // Load lịch chiếu cho rạp này
                const branchId = item.dataset.branchId;
                const selectedDate = document.querySelector('.box-time.active')?.dataset.date;
                const movieId = document.querySelector('.box-time.active')?.dataset.movieId;

                if (branchId && selectedDate && movieId) {
                    loadShowTimes(branchId, movieId, selectedDate);
                }
            }
        });
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
        container.innerHTML = '<p class="text-center">Đang tải...</p>';

        fetch(`/Movie/GetBranchesByCity?city=${encodeURIComponent(city)}&movieId=${movieId}`)
            .then(response => response.json())
            .then(branches => {
                if (branches.length === 0) {
                    container.innerHTML = '<p class="no-data">Không có rạp chiếu phim này tại khu vực đã chọn</p>';
                    return;
                }

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
                                    <p class="loading">Đang tải lịch chiếu...</p>
                                </div>
                            </div>
                        </li>`;
                });
                html += '</ul>';
                container.innerHTML = html;

                // Tải lịch chiếu cho rạp đầu tiên
                if (branches.length > 0) {
                    const selectedDate = document.querySelector('.box-time.active')?.dataset.date ||
                        new Date().toISOString().split('T')[0];
                    loadShowTimes(branches[0].branchId, movieId, selectedDate);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                container.innerHTML = '<p class="error">Không thể tải danh sách rạp</p>';
            });
    }

    // === HÀM TẠO LỊCH CHIẾU ===
    function loadShowTimes(branchId, movieId, date) {
        const container = document.querySelector(`.showtime-container[data-branch-id="${branchId}"]`);
        if (!container) return;

        container.innerHTML = '<p class="loading">Đang tải lịch chiếu...</p>';

        fetch(`/Movie/GetShowTimes?branchId=${branchId}&movieId=${movieId}&date=${date}`)
            .then(response => response.json())
            .then(showTimeGroups => {
                if (showTimeGroups.length === 0) {
                    container.innerHTML = '<p class="no-data">Không có suất chiếu trong ngày này</p>';
                    return;
                }

                let html = '<ul class="list-infor">';
                showTimeGroups.forEach(group => {
                    html += `
                        <li class="item-infor">
                            <div class="tt">${group.roomType}</div>
                            <ul class="list-time">`;

                            group.showTimes.forEach(st => {
                            html += `<li class="item-time" data-showtime-id="${st.showTimeID}" data-price="${st.basePrice}">
                            ${st.timeDisplay}
                        </li>`;
                    });

                    html += `</ul>
                        </li>`;
                });
                html += '</ul>';
                container.innerHTML = html;
            })
            .catch(error => {
                console.error('Error:', error);
                container.innerHTML = '<p class="error">Không thể tải lịch chiếu</p>';
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

        // ✅ Tính tổng tiền từ dữ liệu vé thực tế
        selectedSeatsData.forEach(seat => {
            total += seat.price || 0;
        });

        document.getElementById('totalPrice').textContent = total.toLocaleString('vi-VN') + ' VNĐ';

        // ✅ Enable nút thanh toán khi đã chọn ghế
        const bookBtn = document.getElementById('bookBtn');
        if (bookBtn) {
            bookBtn.disabled = selectedSeatsData.length === 0;
        }
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

                // Xác định class theo trạng thái
                let seatClass = 'seat';

                if (seat.status === 'Đã đặt') {
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

            // ✅ Lấy thông tin phim từ DOM
            const movieTitle = document.querySelector('.movie-title1')?.textContent || 'N/A';

            // ✅ Lấy tên rạp từ sticky bar (đã được cập nhật khi chọn suất chiếu)
            const cinemaName = document.getElementById('cinemaName')?.textContent || 'N/A';

            // ✅ Lấy địa chỉ rạp từ cinestar-item đang open
            const activeBranch = document.querySelector('.cinestar-item.open');
            const cinemaAddress = activeBranch?.querySelector('.address')?.textContent || '';

            // ✅ Debug log
            console.log('=== CINEMA INFO ===');
            console.log('Cinema Name from sticky bar:', cinemaName);
            console.log('Active Branch Element:', activeBranch);
            console.log('Cinema Address:', cinemaAddress);

            // ✅ Lấy thông tin suất chiếu
            const selectedTimeSlot = document.querySelector('.item-time.active');
            const showTimeDisplay = selectedTimeSlot?.textContent.trim() || '';
            const selectedDate = document.querySelector('.box-time.active')?.dataset.date || '';

            // ✅ Format ngày giờ
            const dateObj = new Date(selectedDate);
            const dateStr = dateObj.toLocaleDateString('vi-VN', {
                weekday: 'long',
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            });

            // ✅ Lấy thông tin phòng chiếu
            const roomType = selectedTimeSlot?.closest('.item-infor')?.querySelector('.tt')?.textContent || '';

            // ✅ Tính tổng tiền
            let totalAmount = 0;
            selectedSeatsData.forEach(seat => {
                totalAmount += seat.price || 0;
            });

            // ✅ Chuẩn bị dữ liệu booking
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

                // Thông tin giá
                totalAmount: totalAmount,

                // Thời gian đặt vé
                bookingTime: new Date().toISOString()
            };

            console.log('✅ Full Booking Data:', JSON.stringify(bookingData, null, 2));

            // ✅ Lưu vào sessionStorage
            sessionStorage.setItem('bookingData', JSON.stringify(bookingData));

            // ✅ Verify data was saved
            const savedData = sessionStorage.getItem('bookingData');
            console.log('✅ Verified saved data:', savedData);

            // ✅ Chuyển sang trang thanh toán
            window.location.href = '/Payment/Index';
        });
    }
});