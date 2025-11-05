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
            icon.innerHTML = '&#128064;';
        } else {
            input.type = 'password';
            icon.style.opacity = 1;
            icon.innerHTML = '&#128065;';
        }
    });
});

// Login function
function loginAccount(userInput) {
    console.log('loginAccount called');

    if (typeof Swal === 'undefined' || typeof $ === 'undefined') {
        alert('Required libraries not loaded!');
        return;
    }

    const submitBtn = document.querySelector('#login-form .btn-submit');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Đang đăng nhập...';

    const existingErrors = document.querySelectorAll('#login-form .error-message');
    existingErrors.forEach(error => error.remove());

    $.ajax({
        type: "POST",
        url: "/Account/Login",
        data: userInput,
        dataType: 'json',
        success: function (res) {
            console.log('Login response:', res);

            if (res.status === "success") {
                let welcomeMessage = 'Chào mừng bạn quay trở lại!';
                if (res.customerName) {
                    welcomeMessage = `Chào mừng ${res.customerName} quay trở lại!`;
                }

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
                    window.location.href = res.redirectUrl || "/";
                });
            } else {
                showErrorMessage(res.message || 'Đăng nhập thất bại');
            }
        },
        error: function (xhr) {
            console.error('Login error:', xhr);
            let errorMessage = 'Có lỗi xảy ra khi đăng nhập.';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            showErrorMessage(errorMessage);
        },
        complete: function () {
            submitBtn.disabled = false;
            submitBtn.textContent = originalText;
        }
    });
}

// Register function
function registerAccount(userInput) {
    console.log('registerAccount called');
    console.log('Data:', userInput);

    if (typeof Swal === 'undefined' || typeof $ === 'undefined') {
        alert('Required libraries not loaded!');
        return;
    }

    const submitBtn = document.querySelector('#register-form .btn-submit');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Đang đăng ký...';

    const existingErrors = document.querySelectorAll('#register-form .error-message');
    existingErrors.forEach(error => error.remove());

    $.ajax({
        type: "POST",
        url: "/Account/Register",
        data: userInput,
        dataType: 'json',
        success: function (res) {
            console.log('Register response:', res);

            if (res.status === "success") {
                Swal.fire({
                    icon: 'success',
                    title: 'Đăng ký thành công',
                    html: `
                        <div style="text-align: center;">
                            <p style="font-size: 16px; margin: 10px 0;">Chào mừng ${res.customerName || 'bạn'} đến với Cinestar!</p>
                            <p style="font-size: 14px; color: #666;">Bạn sẽ được chuyển hướng đến trang đăng nhập...</p>
                        </div>
                    `,
                    timer: 2500,
                    showConfirmButton: false,
                    allowOutsideClick: false
                }).then(() => {
                    // Switch to login form
                    loginForm.classList.add('active');
                    registerForm.classList.remove('active');
                    loginBtn.classList.add('active');
                    registerBtn.classList.remove('active');
                    resetForm(registerForm);
                });
            } else {
                console.error('Registration failed:', res.message);
                showRegisterErrorMessage(res.message || 'Đăng ký thất bại');
            }
        },
        error: function (xhr) {
            console.error('Register error:', xhr);
            console.error('Status:', xhr.status);
            console.error('Response:', xhr.responseText);

            let errorMessage = 'Có lỗi xảy ra khi đăng ký.';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            } else if (xhr.status === 500) {
                errorMessage = 'Lỗi máy chủ. Vui lòng thử lại sau.';
            } else if (xhr.status === 400) {
                errorMessage = 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.';
            }
            showRegisterErrorMessage(errorMessage);
        },
        complete: function () {
            submitBtn.disabled = false;
            submitBtn.textContent = originalText;
        }
    });
}

// Error message functions
function showErrorMessage(message) {
    console.log('Login error:', message);
    const existingErrors = document.querySelectorAll('#login-form .error-message');
    existingErrors.forEach(error => error.remove());

    const errorDiv = document.createElement('div');
    errorDiv.className = 'form-it error-message';
    errorDiv.innerHTML = `<span class="error" style="color: #dc3545; font-weight: 500;">${message}</span>`;

    const submitDiv = document.querySelector('#login-form .form-it:has(.btn-submit)');
    if (submitDiv) {
        submitDiv.parentNode.insertBefore(errorDiv, submitDiv);
    }

    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'error',
            title: 'Đăng nhập thất bại',
            html: `<p style="color: #dc3545;">${message}</p>`,
            confirmButtonText: 'Thử lại',
            confirmButtonColor: '#dc3545'
        });
    }
}

function showRegisterErrorMessage(message) {
    console.log('Register error:', message);
    const existingErrors = document.querySelectorAll('#register-form .error-message');
    existingErrors.forEach(error => error.remove());

    const errorDiv = document.createElement('div');
    errorDiv.className = 'form-it error-message';
    errorDiv.innerHTML = `<span class="error" style="color: #dc3545; font-weight: 500;">${message}</span>`;

    const submitDiv = document.querySelector('#register-form .form-it:has(.btn-submit)');
    if (submitDiv) {
        submitDiv.parentNode.insertBefore(errorDiv, submitDiv);
    }

    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'error',
            title: 'Đăng ký thất bại',
            html: `<p style="color: #dc3545;">${message}</p>`,
            confirmButtonText: 'Thử lại',
            confirmButtonColor: '#dc3545'
        });
    }
}

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

// Validation functions
function validateLoginForm(username, password) {
    const errors = [];
    if (!username || username.trim().length === 0) {
        errors.push('Vui lòng nhập tên đăng nhập');
    }
    if (!password || password.trim().length === 0) {
        errors.push('Vui lòng nhập mật khẩu');
    }
    if (password && password.length < 6) {
        errors.push('Mật khẩu phải có ít nhất 6 ký tự');
    }
    return errors;
}

function validateRegistrationForm(data) {
    const errors = [];

    // Required fields
    if (!data.fullname) errors.push('Vui lòng nhập họ và tên');
    if (!data.birthday) errors.push('Vui lòng chọn ngày sinh');
    if (!data.phone) errors.push('Vui lòng nhập số điện thoại');
    if (!data.username) errors.push('Vui lòng nhập tên đăng nhập');
    if (!data.email) errors.push('Vui lòng nhập email');
    if (!data.password) errors.push('Vui lòng nhập mật khẩu');
    if (data.password !== data.confirmPassword) errors.push('Mật khẩu xác nhận không khớp');
    if (!data.agreePolicy) errors.push('Vui lòng đồng ý với điều khoản');

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (data.email && !emailRegex.test(data.email)) {
        errors.push('Email không hợp lệ');
    }

    // Phone validation
    const phoneRegex = /^(0[3|5|7|8|9])+([0-9]{8})$/;
    if (data.phone && !phoneRegex.test(data.phone)) {
        errors.push('Số điện thoại không hợp lệ (VD: 0912345678)');
    }

    // Username validation
    if (data.username && data.username.length < 3) {
        errors.push('Tên đăng nhập phải có ít nhất 3 ký tự');
    }

    // Password validation
    if (data.password && data.password.length < 6) {
        errors.push('Mật khẩu phải có ít nhất 6 ký tự');
    }

    if (errors.length > 0) {
        console.log('Validation errors:', errors);
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'Thông tin không hợp lệ',
                html: `
                    <div style="text-align: left;">
                        <ul style="margin: 10px 0; padding-left: 20px;">
                            ${errors.map(error => `<li>${error}</li>`).join('')}
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

// Event listeners
document.addEventListener("DOMContentLoaded", function () {
    console.log('DOM loaded');
    console.log('jQuery:', typeof $ !== 'undefined');
    console.log('SweetAlert:', typeof Swal !== 'undefined');

    // Login form
    $(document).off('submit', '#login-form');
    $(document).on('submit', '#login-form', function (e) {
        console.log('Login form submitted');
        e.preventDefault();

        const username = $('#login-account').val().trim();
        const password = $('#login-password').val().trim();
        const remember = $('#remember').is(':checked');

        const validationErrors = validateLoginForm(username, password);
        if (validationErrors.length > 0) {
            showErrorMessage(validationErrors[0]);
            return;
        }

        if (username && password) {
            showSuccessMessage('Đang xác thực...');
        }

        loginAccount({
            username: username,
            password: password,
            remember: remember
        });
    });

    // Register form
    $(document).off('submit', '#register-form');
    $(document).on('submit', '#register-form', function (e) {
      console.log('Register form submitted');
        e.preventDefault();

        const formData = {
      fullname: $('#register-fullname').val().trim(),
       birthday: $('#register-birthday').val(),
 phone: $('#register-phone').val().trim(),
 username: $('#register-username').val().trim(),
     // ✅ BỎ CCCD VÌ ĐÃ COMMENT OUT TRONG HTML
      // cccd: $('#register-cccd').val().trim(),
       email: $('#register-email').val().trim(),
   password: $('#register-password').val(),
      confirmPassword: $('#register-confirm-password').val(),
            agreePolicy: $('#register-policy').is(':checked')
 };

   console.log('Form data:', formData);

        if (!validateRegistrationForm(formData)) {
       return;
        }

        console.log('Validation passed');

        // ✅ GỬI DATA KHÔNG CÓ CCCD
        registerAccount({
            fullname: formData.fullname,
            birthday: formData.birthday,
          phone: formData.phone,
    username: formData.username,
    // cccd: formData.cccd, // ✅ BỎ DÒNG NÀY
    email: formData.email,
       password: formData.password
        });
    });
});

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
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.href = "/";
                    });
                }
            });
        }
    });
}

window.logoutAccount = logoutAccount;