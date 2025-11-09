document.addEventListener('DOMContentLoaded', function () {
    const selected = document.querySelector('.location-selected');
    const dropdown = document.querySelector('.location-dropdown');
    const options = document.querySelectorAll('.location-option');
    const locationName = document.querySelector('.location-name');
    const dropdownArrow = document.querySelector('.dropdown-arrow');

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

            // Cập nhật sticky bar
            document.getElementById('selectedSeatsDisplay').textContent = `Suất: ${timeDisplay}`;

            // Tải thông tin vé
            //loadTicketPrices(showTimeId);
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
      <li class="cinestar-item ${index === 0 ? 'open' : ''}" data-branch-id="${branch.branchId}">
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

        // Lặp qua từng loại vé (mỗi .content là một loại vé riêng biệt)
        document.querySelectorAll('#ticketContainer .content').forEach(content => {
            const qty = parseInt(content.querySelector('.quantity').textContent) || 0;
            const priceText = content.querySelector('.price p').textContent;
            const price = parseInt(priceText.replace(/[^\d]/g, '')) || 0;
            total += qty * price;
        });

        document.getElementById('totalPrice').textContent = total.toLocaleString('vi-VN') + ' VNĐ';
        document.getElementById('bookBtn').disabled = total === 0;
    }


    // Khởi tạo SignalR connection


    // Kết nối SignalR
    let connection = null;
    let currentShowTimeId = null;
    let currentCustomerId = null;
    function initSignalR() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/seatHub")
            .withAutomaticReconnect()
            .build();

        // Lắng nghe sự kiện: Người khác CHỌN ghế
        connection.on("SeatSelected", function (data) {
            console.log("Ghế được chọn:", data);

            // Tìm ghế trong DOM
            const seatElement = document.querySelector(`td.seat[data-seat-id="${data.seatId}"]`);
            if (seatElement) {
                // Kiểm tra: Ghế do mình chọn hay người khác?
                if (data.customerId === currentCustomerId) {
                    // Ghế của mình → màu xanh dương
                    seatElement.className = 'seat selected';
                } else {
                    // Ghế người khác → màu cam, không cho click
                    seatElement.className = 'seat choosing';
                }
            }
        });

        // Lắng nghe sự kiện: Người khác BỎ CHỌN ghế
        connection.on("SeatDeselected", function (data) {
            console.log("Ghế bị bỏ chọn:", data);

            const seatElement = document.querySelector(`td.seat[data-seat-id="${data.seatId}"]`);
            if (seatElement) {
                const seatType = seatElement.dataset.seatType;

                // Trả về màu gốc
                if (seatType === 'Ghế đôi') {
                    seatElement.className = 'seat couple';
                } else if (seatType === 'VIP') {
                    seatElement.className = 'seat vip';
                } else {
                    seatElement.className = 'seat regular';
                }
            }
        });

        // Kết nối
        connection.start()
            .then(() => console.log("SignalR connected"))
            .catch(err => console.error("SignalR error:", err));
    }
        


    // Click vào li để load ghế
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('item-time')) {
            // Bỏ active cũ
            document.querySelectorAll('.item-time').forEach(item => {
                item.classList.remove('active');
            });

            // Thêm active vào item được click
            e.target.classList.add('active');

            const showTimeId = e.target.dataset.showtimeId;
            currentCustomerId = document.getElementById("CustomerId").innerHTML;

            console.log("ShowTimeID: " + showTimeId);
            console.log("CustomerID: " + currentCustomerId);

            // Rời nhóm cũ (nếu có)
            if (currentShowTimeId && connection) {
                connection.invoke("LeaveShowTime", currentShowTimeId);
            }

            // Join vào nhóm suất chiếu mới
            currentShowTimeId = showTimeId;
            if (connection) {
                connection.invoke("JoinShowTime", showTimeId);
            }

            loadSeatingLayout(showTimeId, currentCustomerId);
        }
    });

    // Khởi tạo SignalR khi trang load
    document.addEventListener('DOMContentLoaded', function () {
        initSignalR();
    });

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
                td.dataset.seatId = seat.seatID; // ← SỬA: Dùng seat.seatID thay vì seatId
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
                const seatId = this.dataset.seatId; // ← Lấy seatId từ element được click
                const seatType = this.dataset.seatType;

                if (this.classList.contains('selected')) {
                    // Bỏ chọn
                    try {
                        const response = await fetch(`/Movie/DeselectSeat?showTimeId=${showTimeId}&seatId=${seatId}`, {
                            method: 'POST'
                        });

                        const result = await response.json();

                        if (result.success) {
                            this.classList.remove('selected');

                            // Trả về class gốc
                            if (seatType === 'Ghế đôi') {
                                this.classList.add('couple');
                            } else if (seatType === 'VIP') {
                                this.classList.add('vip');
                            } else {
                                this.classList.add('regular');
                            }
                        } else {
                            alert('Không thể bỏ chọn ghế này!');
                        }
                    } catch (error) {
                        console.error('Lỗi bỏ chọn:', error);
                    }

                } else {
                    // Chọn ghế
                    try {
                        const response = await fetch(`/Movie/SelectSeats?showTimeId=${showTimeId}&seatId=${seatId}`, {
                            method: 'POST'
                        });

                        const result = await response.json();

                        if (result.success) { // ← Chú ý: Controller trả về {result}, không phải {success}
                            this.classList.add('selected');
                            this.classList.remove('regular', 'couple', 'vip');
                        } else {
                            alert('Ghế này đã được người khác chọn!');
                        }
                    } catch (error) {
                        console.error('Lỗi chọn ghế:', error);
                    }
                }
            });
        });
    }
});