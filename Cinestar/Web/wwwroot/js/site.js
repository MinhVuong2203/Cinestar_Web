
// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Loading
function showLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) overlay.style.display = 'flex';
}

function hideLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) overlay.style.display = 'none';
}

document.addEventListener('DOMContentLoaded', function () {
    // Xử lý form submit
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            // Chỉ show loading nếu form hợp lệ
            if (form.checkValidity()) {
                showLoading();
            }
        });
    });

    // Xử lý loading links
    document.querySelectorAll('.loading-link').forEach(function (link) {
        link.addEventListener('click', function () {
            const href = link.getAttribute('href');
            // Không show loading cho link # hoặc _blank
            if (href && href !== '#' && link.target !== '_blank') {
                showLoading();
            }
        });
    });
});

// Ẩn loading khi trang load xong
window.addEventListener('load', function () {
    hideLoading();
});
// User dropdown functionality
//const userDropdown = document.querySelector('.user-dropdown');
//if (userDropdown) {
//    const userProfile = userDropdown.querySelector('.user-profile');

//    userProfile.addEventListener('click', (e) => {
//        e.stopPropagation();
//        userDropdown.classList.toggle('active');
//    });

//    // Close dropdown when clicking outside
//    document.addEventListener('click', (e) => {
//        if (!userDropdown.contains(e.target)) {
//            userDropdown.classList.remove('active');
//        }
//    });
//}