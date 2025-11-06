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
    document.querySelectorAll('.box-time').forEach(function(boxTime) {
        boxTime.addEventListener('click', function() {
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
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('item-time')) {
            const container = e.target.closest('.showtime-container');
            container.querySelectorAll('.item-time').forEach(item => item.classList.remove('active'));
            e.target.classList.add('active');
  
            const showTimeId = e.target.dataset.showtimeId;
            const timeDisplay = e.target.textContent.trim();
            
            // Cập nhật sticky bar
            document.getElementById('selectedSeatsDisplay').textContent = `Suất: ${timeDisplay}`;
   
            // Tải thông tin vé
            loadTicketPrices(showTimeId);
        }
    });

    // === XỬ LÝ TĂNG/GIẢM SỐ LƯỢNG VÉ ===
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('decrease')) {
            const quantity = e.target.nextElementSibling;
            let count = parseInt(quantity.textContent);
            if (count > 0) {
                quantity.textContent = count - 1;
                updateTotalPrice();
            }
        }
        
        if (e.target.classList.contains('increase')) {
            const quantity = e.target.previousElementSibling;
            let count = parseInt(quantity.textContent);
            quantity.textContent = count + 1;
            updateTotalPrice();
        }
    });

    // === HÀM TỰI DANH SÁCH RẠP ===
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

    // === HÀM TỰI LỊCH CHIẾU ===
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
    function loadTicketPrices(showTimeId) {
        const container = document.getElementById('ticketContainer');
        container.innerHTML = '<p class="text-center">Đang tải...</p>';

        fetch(`/Movie/GetTicketPrices?showTimeId=${showTimeId}`)
            .then(response => response.json())
            .then(prices => {
                if (prices.length === 0) {
                    container.innerHTML = '<p class="no-data">Không có thông tin vé</p>';
                    return;
                }
                let html = '';
                prices.forEach(price => {
                    html += `
                    <div class="content">
                        <div class="content-top">
                            <p class="name">${price.ticketType}</p>
                            <div class="desc">
                                <p>${price.description}</p>
                            </div>
                            <div class="price">
                                <p>${price.price.toLocaleString('vi-VN')} VNĐ</p>
                            </div>
                        </div>
                        <div class="content-bottom">
                            <div class="count">
                                <div class="count-btn">
                                    <button class="decrease">-</button>
                                    <span class="quantity">0</span>
                                    <button class="increase">+</button>
                                </div>
                            </div>
                        </div>
                    </div>`;

  //      fetch(`/Movie/GetTicketPrices?showTimeId=${showTimeId}`)
  //      .then(response => response.json())
  //    .then(prices => {
  //if (prices.length === 0) {
  // container.innerHTML = '<p class="no-data">Không có thông tin vé</p>';
  //       return;
  //      }

  //    let html = '';
  //   prices.forEach(price => {
  //      html += `
  //         <div class="content">
  //<div class="content-top">
  //           <p class="name">${price.ticketType}</p>
  //<div class="desc">
  //      <p>${price.description}</p>
  //        </div>
  //     <div class="price">
  //               <p>${price.price.toLocaleString('vi-VN')} VNĐ</p>
  //          </div>
  //    </div>
  // <div class="content-bottom">
  // <div class="count">
  //            <div class="count-btn">
  //            <button class="decrease">-</button>
  //<span class="quantity">0</span>
  //          <button class="increase" ${price.availableCount === 0 ? 'disabled' : ''}>+</button>
  //          </div>
  //        </div>
  //       </div>
  //           </div>`;
         });
                container.innerHTML = html;
         })
       .catch(error => {
                console.error('Error:', error);
         container.innerHTML = '<p class="error">Không thể tải thông tin vé</p>';
            });
    }

    // === CẬP NHẬT TỔNG TIỀN ===
    function updateTotalPrice() {
        let total = 0;
        document.querySelectorAll('.ticket-container .content').forEach(content => {
 const qty = parseInt(content.querySelector('.quantity').textContent);
     const priceText = content.querySelector('.price p').textContent;
            const price = parseInt(priceText.replace(/[^\d]/g, ''));
            total += qty * price;
        });

        document.getElementById('totalPrice').textContent = total.toLocaleString('vi-VN') + ' VNĐ';
    document.getElementById('bookBtn').disabled = total === 0;
    }
});