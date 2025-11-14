
// Search functionality
const searchInput = document.querySelector('.search-input');
const searchBtn = document.querySelector('.search-btn');
if (searchBtn && searchInput) {
    searchBtn.addEventListener('click', (e) => {
        if (!searchInput.value.trim()) {
            e.preventDefault();
            searchInput.focus();
        }
    });
}

// Language selector
const langBtn = document.querySelector('.language-selector');
if (langBtn) {
    langBtn.addEventListener('click', () => {
        // Toggle language dropdown logic here
        console.log('Language selector clicked');
    });
}

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
    document.getElementById('loadingOverlay').style.display = 'flex';
}
function hideLoading() {
    document.getElementById('loadingOverlay').style.display = 'none';
}

document.addEventListener('DOMContentLoaded', function () {
    const forms = document.querySelectorAll('form');
    forms.forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (form.checkValidity()) {
                showLoading();
            }
        });
    });

    // Ẩn loading khi trang load xong
    hideLoading();

    // Ẩn loading nếu có validation error từ server
    const validationErrors = document.querySelector('.validation-summary-errors, .field-validation-error, [class*="error"]');
    if (validationErrors) {
        hideLoading();
    }
});
// Show loading khi click link có class 'loading-link'
const loadingLinks = document.querySelectorAll('.loading-link');
loadingLinks.forEach(function (link) {
    link.addEventListener('click', function () {
        showLoading();
    });
});
// ẩn đi khi loading xong
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