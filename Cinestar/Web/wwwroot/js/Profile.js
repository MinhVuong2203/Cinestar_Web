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
            else if (page === 'purchase-history') {
                window.location.href = '/Account/PurchaseHistory';
            }

            // Remove selected class from all items
            menuItems.forEach(mi => mi.classList.remove('selected'));

            // Add selected class to clicked item (except active)
            if (!this.classList.contains('active')) {
                this.classList.add('selected');
            }
        });
    });

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

    // Handle save personal information
    const saveInfoBtn = document.querySelector('.section:first-of-type .save-btn');
    if (saveInfoBtn) {
        saveInfoBtn.addEventListener('click', function () {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: 'Thông tin đã được lưu thành công!',
                    confirmButtonColor: '#667eea'
                });
            } else {
                alert('Thông tin đã được lưu thành công!');
            }
        });
    }

    // Handle change password
    const changePasswordBtn = document.getElementById('changePasswordBtn');
    if (changePasswordBtn) {
        changePasswordBtn.addEventListener('click', function () {
            const oldPassword = document.getElementById('oldPassword')?.value || '';
            const newPassword = document.getElementById('newPassword')?.value || '';
            const confirmPassword = document.getElementById('confirmPassword')?.value || '';

            if (!oldPassword || !newPassword || !confirmPassword) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Vui lòng điền đầy đủ thông tin!',
                        confirmButtonColor: '#667eea'
                    });
                } else {
                    alert('Vui lòng điền đầy đủ thông tin!');
                }
                return;
            }

            if (newPassword !== confirmPassword) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Mật khẩu mới và xác thực mật khẩu không khớp!',
                        confirmButtonColor: '#667eea'
                    });
                } else {
                    alert('Mật khẩu mới và xác thực mật khẩu không khớp!');
                }
                return;
            }

            if (newPassword.length < 6) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Mật khẩu mới phải có ít nhất 6 ký tự!',
                        confirmButtonColor: '#667eea'
                    });
                } else {
                    alert('Mật khẩu mới phải có ít nhất 6 ký tự!');
                }
                return;
            }

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: 'Đổi mật khẩu thành công!',
                    confirmButtonColor: '#667eea'
                });
            } else {
                alert('Đổi mật khẩu thành công!');
            }

            // Clear password fields
            document.getElementById('oldPassword').value = '';
            document.getElementById('newPassword').value = '';
            document.getElementById('confirmPassword').value = '';
        });
    }

    // Form validation
    const inputs = document.querySelectorAll('input');
    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            if (this.hasAttribute('required') && !this.value) {
                this.style.borderColor = '#ff4444';
            } else {
                this.style.borderColor = '#ccc';
            }
        });
    });
});