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

    // Chọn rạp và cập nhật sticky bar
    options.forEach(option => {
        option.addEventListener('click', function () {
            locationName.textContent = this.textContent;
            options.forEach(opt => opt.classList.remove('active'));
            this.classList.add('active');
            dropdown.classList.remove('show');
            dropdownArrow.style.transform = 'rotate(0deg)';

            // Cập nhật sticky bar khi thay đổi location
            updateCinemaInStickyBar(this.textContent);
        });
    });

    // Function để cập nhật cinema name trong sticky bar
    function updateCinemaInStickyBar(location) {
        const cinemaName = document.getElementById('cinemaName');
        if (cinemaName) {
            const cinemaMapping = {
                'Hồ Chí Minh': 'Cinestar Quốc Thanh (TPHCM)',
                'Đà Lạt': 'Cinestar Đà Lạt (Lâm Đồng)',
                'Bình Dương': 'Cinestar Bình Dương',
                'Huế': 'Cinestar Huế',
                'Tiền Giang': 'Cinestar Mỹ Tho (Tiền Giang)',
                'Kiên Giang': 'Cinestar Kiên Giang',
                'Lâm Đồng': 'Cinestar Lâm Đồng'
            };

            cinemaName.textContent = cinemaMapping[location] || 'Cinestar Mỹ Tho (Tiền Giang)';
        }
    }

    // Đóng dropdown khi click ra ngoài
    document.addEventListener('click', function (e) {
        if (!selected.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.remove('show');
            dropdownArrow.style.transform = 'rotate(0deg)';
        }
    });

    // Xử lý cinestar dropdown
    document.querySelectorAll('.cinestar-heading').forEach(function (heading) {
        heading.addEventListener('click', function () {
            const item = heading.closest('.cinestar-item');
            item.classList.toggle('open');
        });
    });

    // Xử lý click cho time slots
    document.querySelectorAll('.item-time').forEach(function (timeItem) {
        timeItem.addEventListener('click', function () {
            const sameList = this.closest('.list-time');
            sameList.querySelectorAll('.item-time').forEach(item => {
                item.classList.remove('active');
            });
            this.classList.add('active');
        });
    });

    document.querySelectorAll('.decrease').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const quantity = this.nextElementSibling;
            let count = parseInt(quantity.textContent);
            if (count > 0) {
                quantity.textContent = count - 1;
            }
        });
    });

    document.querySelectorAll('.increase').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const quantity = this.previousElementSibling;
            let count = parseInt(quantity.textContent);
            quantity.textContent = count + 1;
        });
    });
    // === TẠO 3 NGÀY TỰ ĐỘNG ===
        function generateDateBoxes() {
            const container = document.getElementById('selectedTimeContainer');
            if (!container) return;

            const daysOfWeek = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
            
            // Tạo 3 ngày: hôm nay, ngày mai, ngày kia
            for (let i = 0; i < 3; i++) {
                const date = new Date();
                date.setDate(date.getDate() + i);
                
                const day = date.getDate();
                const month = date.getMonth() + 1;
                const dayOfWeek = daysOfWeek[date.getDay()];
                
                // Format ngày: dd/mm
                const dateString = `${day.toString().padStart(2, '0')}/${month.toString().padStart(2, '0')}`;
                
                // Tạo box-time
                const boxTime = document.createElement('div');
                boxTime.className = 'box-time';
                if (i === 0) boxTime.classList.add('active'); // Mặc định chọn hôm nay
                
                boxTime.innerHTML = `
                    <p class="date">${dateString}</p>
                    <p class="day">${dayOfWeek}</p>
                `;
                
                // Thêm sự kiện click để chọn ngày
                boxTime.addEventListener('click', function() {
                    // Xóa active khỏi tất cả box-time
                    document.querySelectorAll('.box-time').forEach(box => {
                        box.classList.remove('active');
                    });
                    // Thêm active vào box được click
                    this.classList.add('active');
                    
                    // Có thể thêm logic để load lịch chiếu theo ngày ở đây
                    console.log('Ngày được chọn:', dateString, dayOfWeek);
                });
                
                container.appendChild(boxTime);
            }
        }

        // Gọi hàm tạo ngày khi trang load
        generateDateBoxes();

});