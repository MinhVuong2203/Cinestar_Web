const loginBtn = document.getElementById('login-btn');
const registerBtn = document.getElementById('register-btn');
const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');
const switchLogin = document.getElementById('switch-login');

// Add debugging
console.log('Login.js loaded');
console.log('Login form:', loginForm);
console.log('Register form:', registerForm);

function resetForm(form) {
    form.reset();
    // Clear any error messages
    const errorMessages = form.querySelectorAll('.error-message');
    errorMessages.forEach(error => error.remove());
}

// Form switching functionality
loginBtn.addEventListener('click', function () {
    console.log('Login button clicked');
    loginForm.classList.add('active');
    registerForm.classList.remove('active');
    loginBtn.classList.add('active');
    registerBtn.classList.remove('active');
    resetForm(registerForm);
});

registerBtn.addEventListener('click', function () {
    console.log('Register button clicked');
    registerForm.classList.add('active');
    loginForm.classList.remove('active');
    registerBtn.classList.add('active');
    loginBtn.classList.remove('active');
    resetForm(loginForm);
});

switchLogin.addEventListener('click', function (e) {
    e.preventDefault();
    console.log('Switch to login clicked');
    loginForm.classList.add('active');
    registerForm.classList.remove('active');
    loginBtn.classList.add('active');
    registerBtn.classList.remove('active');
    resetForm(registerForm);
});

// Password visibility toggle
document.querySelectorAll('.toggle-password').forEach(function (icon) {
    icon.addEventListener('click', function () {
        const input = document.getElementById(icon.dataset.target);
        if (input.type === 'password') {
            input.type = 'text';
            icon.style.opacity = 0.5;
            icon.innerHTML = '&#128064;'; // Eye with slash
        } else {
            input.type = 'password';
            icon.style.opacity = 1;
            icon.innerHTML = '&#128065;'; // Normal eye
        }
    });
});

// Login function with debugging
function loginAccount(userInput) {
    console.log('loginAccount called with:', userInput);

    // Check if SweetAlert is available
    if (typeof Swal === 'undefined') {
        alert('SweetAlert2 library is not loaded!');
        console.error('SweetAlert2 is not loaded');
        return;
    }

    // Check if jQuery is available
    if (typeof $ === 'undefined') {
        alert('jQuery library is not loaded!');
        console.error('jQuery is not loaded');
        return;
    }

    // Show loading state
    const submitBtn = document.querySelector('#login-form .btn-submit');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Đang đăng nhập...';

    // Clear previous error messages
    const existingErrors = document.querySelectorAll('#login-form .error-message');
    existingErrors.forEach(error => error.remove());

    console.log('Sending AJAX request to /Account/Login');

    $.ajax({
        type: "POST",
        url: "/Account/Login",
        data: userInput,
        dataType: 'json',
        beforeSend: function () {
            console.log('AJAX request started');
        },
        success: function (res) {
            console.log('AJAX success response:', res);

            if (res.status === "success") {
                // Create personalized success message with customer name
                let welcomeMessage = 'Chào mừng bạn quay trở lại!';
                if (res.customerName) {
                    welcomeMessage = `Chào mừng ${res.customerName} quay trở lại!`;
                }

                // Show success message with customer name
                Swal.fire({
                    icon: 'success',
                    title: 'Đăng nhập thành công',
                    html: `
                        <div style="text-align: center;">
                            <p style="font-size: 16px; margin: 10px 0;">${welcomeMessage}</p>
                            ${res.userType === 'Customer' ?
                            '<p style="font-size: 14px; color: #666;">Bạn sẽ được chuyển hướng đến trang chủ...</p>' :
                            '<p style="font-size: 14px; color: #666;">Bạn sẽ được chuyển hướng đến trang quản lý...</p>'
                        }
                        </div>
                    `,
                    timer: 2500,
                    showConfirmButton: false,
                    allowOutsideClick: false
                }).then(() => {
                    // Redirect based on response
                    if (res.redirectUrl) {
                        window.location.href = res.redirectUrl;
                    } else {
                        window.location.href = "/";
                    }
                });

            } else {
                // Show error message with improved styling
                showErrorMessage(res.message || 'Đăng nhập thất bại');
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX error:', { xhr, status, error });
            console.error('Response Text:', xhr.responseText);

            let errorMessage = 'Có lỗi xảy ra khi đăng nhập. Vui lòng thử lại.';

            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            } else if (xhr.status === 0) {
                errorMessage = 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra kết nối mạng.';
            } else if (xhr.status >= 500) {
                errorMessage = 'Lỗi máy chủ. Vui lòng thử lại sau ít phút.';
            } else if (xhr.status === 404) {
                errorMessage = 'Không tìm thấy đường dẫn đăng nhập. Vui lòng liên hệ quản trị viên.';
            }

            showErrorMessage(errorMessage);
        },
        complete: function () {
            console.log('AJAX request completed');
            // Reset button state
            submitBtn.disabled = false;
            submitBtn.textContent = originalText;
        }
    });
}

// Function to show error messages with improved styling
function showErrorMessage(message) {
    console.log('Showing error message:', message);

    // Remove existing error messages
    const existingErrors = document.querySelectorAll('#login-form .error-message');
    existingErrors.forEach(error => error.remove());

    // Create and insert new error message
    const errorDiv = document.createElement('div');
    errorDiv.className = 'form-it error-message';
    errorDiv.innerHTML = `<span class="error" style="color: #dc3545; font-weight: 500;">${message}</span>`;

    // Insert before the submit button
    const submitDiv = document.querySelector('#login-form .form-it:has(.btn-submit)');
    if (submitDiv) {
        submitDiv.parentNode.insertBefore(errorDiv, submitDiv);
    }

    // Fallback if SweetAlert is not available
    if (typeof Swal === 'undefined') {
        alert(message);
        return;
    }

    // Show SweetAlert with better error styling
    Swal.fire({
        icon: 'error',
        title: 'Đăng nhập thất bại',
        html: `
            <div style="text-align: center;">
                <p style="font-size: 16px; margin: 10px 0; color: #dc3545;">${message}</p>
                <p style="font-size: 14px; color: #666;">Vui lòng kiểm tra lại thông tin và thử lại.</p>
            </div>
        `,
        confirmButtonText: 'Thử lại',
        confirmButtonColor: '#dc3545',
        allowOutsideClick: true
    });
}

// Show success message for form validation
function showSuccessMessage(message) {
    if (typeof Swal === 'undefined') {
        console.log(message);
        return;
    }

    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });

    Toast.fire({
        icon: 'success',
        title: message
    });
}

// Form validation function with better error messages
function validateLoginForm(username, password) {
    const errors = [];

    if (!username || username.trim().length === 0) {
        errors.push('Vui lòng nhập tên đăng nhập, email hoặc số điện thoại');
    }

    if (!password || password.trim().length === 0) {
        errors.push('Vui lòng nhập mật khẩu');
    }

    if (password && password.length < 6) {
        errors.push('Mật khẩu phải có ít nhất 6 ký tự');
    }

    return errors;
}

// Enhanced form submission handler
document.addEventListener("DOMContentLoaded", function () {
    console.log('DOM Content Loaded');
    console.log('jQuery available:', typeof $ !== 'undefined');
    console.log('SweetAlert available:', typeof Swal !== 'undefined');

    // Remove any existing handlers
    $(document).off('submit', '#login-form');

    // Login form submission
    $(document).on('submit', '#login-form', function (e) {
        console.log('Login form submitted');
        e.preventDefault();

        const username = $('#login-account').val().trim();
        const password = $('#login-password').val().trim();
        const remember = $('#remember').is(':checked');

        console.log('Form data:', { username, password: '***', remember });

        // Client-side validation
        const validationErrors = validateLoginForm(username, password);


        // Show brief validation success
        if (username && password) {
            showSuccessMessage('Đang xác thực thông tin...');
        }

        // Submit login request
        loginAccount({
            username: username,
            password: password,
            remember: remember
        });
    });

    // Add click handler for submit button as backup
    $('#login-form .btn-submit').on('click', function (e) {
        console.log('Submit button clicked directly');
        // If form doesn't have proper submit handler, trigger it manually
        const form = $('#login-form');
        if (form.length > 0) {
            form.trigger('submit');
        }
    });

    // Register form submission (basic structure)
    $(document).off('submit', '#register-form');
    $(document).on('submit', '#register-form', function (e) {
        console.log('Register form submitted');
        e.preventDefault();

        // Get form data
        const formData = {
            fullname: $('#register-fullname').val().trim(),
            birthday: $('#register-birthday').val(),
            phone: $('#register-phone').val().trim(),
            username: $('#register-username').val().trim(),
            email: $('#register-email').val().trim(),
            password: $('#register-password').val(),
            confirmPassword: $('#register-confirm-password').val(),
            agreePolicy: $('#register-policy').is(':checked')
        };

        // Basic validation for registration
        if (validateRegistrationForm(formData)) {
            console.log('Registration data:', formData);

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'info',
                    title: 'Chức năng đăng ký',
                    text: 'Chức năng đăng ký đang được phát triển. Vui lòng liên hệ quản trị viên để được hỗ trợ.',
                    confirmButtonText: 'Đã hiểu'
                });
            } else {
                alert('Chức năng đăng ký đang được phát triển. Vui lòng liên hệ quản trị viên để được hỗ trợ.');
            }
        }
    });
});

// Registration validation function
function validateRegistrationForm(data) {
    const errors = [];

    if (!data.fullname) errors.push('Vui lòng nhập họ và tên');
    if (!data.birthday) errors.push('Vui lòng chọn ngày sinh');
    if (!data.phone) errors.push('Vui lòng nhập số điện thoại');
    if (!data.username) errors.push('Vui lòng nhập tên đăng nhập');
    if (!data.email) errors.push('Vui lòng nhập email');
    if (!data.password) errors.push('Vui lòng nhập mật khẩu');
    if (data.password !== data.confirmPassword) errors.push('Mật khẩu xác nhận không khớp');
    if (!data.agreePolicy) errors.push('Vui lòng đồng ý với điều khoản và điều kiện');

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (data.email && !emailRegex.test(data.email)) {
        errors.push('Định dạng email không hợp lệ');
    }

    // Phone validation (Vietnamese phone numbers)
    const phoneRegex = /^(\+84|84|0[3|5|7|8|9])+([0-9]{8})$/;
    if (data.phone && !phoneRegex.test(data.phone)) {
        errors.push('Số điện thoại không hợp lệ (VD: 0912345678)');
    }

    // Username validation
    if (data.username && data.username.length < 3) {
        errors.push('Tên đăng nhập phải có ít nhất 3 ký tự');
    }

    // Password strength validation
    if (data.password && data.password.length < 8) {
        errors.push('Mật khẩu phải có ít nhất 8 ký tự');
    }

    if (errors.length > 0) {
        console.log('Registration validation errors:', errors);

        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'Thông tin không hợp lệ',
                html: `
                    <div style="text-align: left;">
                        <ul style="margin: 10px 0; padding-left: 20px;">
                            ${errors.map(error => `<li style="margin: 5px 0;">${error}</li>`).join('')}
                        </ul>
                    </div>
                `,
                confirmButtonText: 'Đã hiểu',
                confirmButtonColor: '#dc3545'
            });
        } else {
            alert('Lỗi:\n' + errors.join('\n'));
        }
        return false;
    }

    return true;
}

// Logout function
function logoutAccount() {
    if (typeof Swal === 'undefined') {
        if (confirm('Bạn có chắc chắn muốn đăng xuất?')) {
            window.location.href = '/Account/Logout';
        }
        return;
    }

    Swal.fire({
        title: 'Đăng xuất',
        text: 'Bạn có chắc chắn muốn đăng xuất?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Đăng xuất',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: "/Account/Logout",
                success: function () {
                    Swal.fire({
                        icon: 'success',
                        title: 'Đăng xuất thành công',
                        text: 'Hẹn gặp lại bạn!',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.href = "/";
                    });
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Không thể đăng xuất. Vui lòng thử lại.'
                    });
                }
            });
        }
    });
}

// Export logout function for use in other files
window.logoutAccount = logoutAccount;