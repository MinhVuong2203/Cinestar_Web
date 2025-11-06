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
            else if (itemText.includes('Lịch sử mua hàng')) {
                console.log('Navigating to Purchase History page'); // Debug log
                window.location.href = 'PurchaseHistory';
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

    // ✅ Handle logout with confirmation (ĐỔI TỪ FORM SANG LINK)
    const logoutLink = document.getElementById('logoutLink');
    if (logoutLink) {
        logoutLink.addEventListener('click', function (e) {
            e.preventDefault(); // Ngăn chặn click mặc định

            // Using SweetAlert2 if available
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Xác nhận đăng xuất',
                    text: 'Bạn có chắc chắn muốn đăng xuất?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#667eea',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Đăng xuất',
                    cancelButtonText: 'Hủy'
                }).then((result) => {
                    if (result.isConfirmed) {
                        // Show loading
                        Swal.fire({
                            title: 'Đang đăng xuất...',
                            allowOutsideClick: false,
                            didOpen: () => {
                                Swal.showLoading();
                            }
                        });
                        // ✅ Redirect đến link logout
                        window.location.href = '/Account/Logout';
                    }
                });
            } else {
                // Fallback to confirm dialog
                if (confirm('Bạn có chắc chắn muốn đăng xuất?')) {
                    window.location.href = '/Account/Logout';
                }
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