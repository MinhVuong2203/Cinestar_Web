document.addEventListener('DOMContentLoaded', function () {
    // Handle menu item clicks
    const menuItems = document.querySelectorAll('.menu-item');
    menuItems.forEach(item => {
        item.addEventListener('click', function (e) {
            e.preventDefault();

            // Get the text content of the clicked item
            const itemText = this.textContent.trim();
            console.log('Clicked:', itemText); // Debug log

            // Check which menu item was clicked
            if (itemText.includes('Thông tin khách hàng')) {
                console.log('Navigating to Customer Info page'); // Debug log
                window.location.href = 'Profile';
                return;
            }

            // Remove selected class from all items first
            menuItems.forEach(mi => mi.classList.remove('selected'));

            // Add selected class to clicked item (except logout and active)
            if (!this.classList.contains('logout') && !this.classList.contains('active')) {
                this.classList.add('selected');
            }
        });
    });

    // Handle register button
    const registerBtn = document.querySelector('.register-btn');
    if (registerBtn) {
        registerBtn.addEventListener('click', function () {
            alert('Đăng ký thành viên CFriend thành công!');
        });
    }

    // Handle logout
    const logoutBtn = document.querySelector('.logout');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function () {
            if (confirm('Bạn có chắc chắn muốn đăng xuất?')) {
                alert('Đã đăng xuất thành công!');
                // window.location.href = 'login.html';
            }
        });
    }

    // Add hover effects to table rows
    const tableRows = document.querySelectorAll('.table-row');
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.backgroundColor = '#f8f9fa';
        });

        row.addEventListener('mouseleave', function () {
            this.style.backgroundColor = 'white';
        });
    });
});