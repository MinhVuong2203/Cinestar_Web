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
});