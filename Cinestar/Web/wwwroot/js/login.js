const loginBtn = document.getElementById('login-btn');
const registerBtn = document.getElementById('register-btn');
const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');
const switchLogin = document.getElementById('switch-login');

function resetForm(form) {
    form.reset();
}

loginBtn.addEventListener('click', function () {
    loginForm.classList.add('active');
    registerForm.classList.remove('active');
    loginBtn.classList.add('active');
    registerBtn.classList.remove('active');
    resetForm(registerForm);
});

registerBtn.addEventListener('click', function () {
    registerForm.classList.add('active');
    loginForm.classList.remove('active');
    registerBtn.classList.add('active');
    loginBtn.classList.remove('active');
    resetForm(loginForm);
});

switchLogin.addEventListener('click', function () {
    loginForm.classList.add('active');
    registerForm.classList.remove('active');
    loginBtn.classList.add('active');
    registerBtn.classList.remove('active');
    resetForm(registerForm);
});

document.querySelectorAll('.toggle-password').forEach(function (icon) {
    icon.addEventListener('click', function () {
        const input = document.getElementById(icon.dataset.target);
        if (input.type === 'password') {
            input.type = 'text';
            icon.style.opacity = 0.5;
        } else {
            input.type = 'password';
            icon.style.opacity = 1;
        }
    });
});