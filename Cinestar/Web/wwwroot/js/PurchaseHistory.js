document.addEventListener('DOMContentLoaded', function () {
    // Handle menu item clicks (KHÔNG preventDefault cho logout)
    const menuItems = document.querySelectorAll('.menu-item:not(.logout)');
    menuItems.forEach(item => {
        item.addEventListener('click', function (e) {
            // Get the data-page attribute
            const page = this.getAttribute('data-page');

            // Navigate to member page if clicked
            if (page === 'member') {
                window.location.href = '/Account/CinestartMember';
                return;
            }
            else if (page === 'profile') {
                window.location.href = '/Account/Profile';
            }

            // Remove selected class from all items
            menuItems.forEach(mi => mi.classList.remove('selected'));

            // Add selected class to clicked item (except active)
            if (!this.classList.contains('active')) {
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