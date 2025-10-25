// Sticky Header với hiệu ứng
let lastScroll = 0;
const header = document.querySelector('.site-header');

window.addEventListener('scroll', () => {
    const currentScroll = window.pageYOffset;

    if (currentScroll > 100) {
        header.classList.add('scrolled');

        // Hide header on scroll down, show on scroll up
        if (currentScroll > lastScroll && currentScroll > 300) {
            header.style.transform = 'translateY(-100%)';
        } else {
            header.style.transform = 'translateY(0)';
        }
    } else {
        header.classList.remove('scrolled');
    }

    lastScroll = currentScroll;
});

// Search functionality
const searchInput = document.querySelector('.search-input');
const searchBtn = document.querySelector('.search-btn');

searchBtn.addEventListener('click', (e) => {
    if (!searchInput.value.trim()) {
        e.preventDefault();
        searchInput.focus();
    }
});

// Language selector
const langBtn = document.querySelector('.language-selector');
if (langBtn) {
    langBtn.addEventListener('click', () => {
        // Toggle language dropdown logic here
        console.log('Language selector clicked');
    });
}

// Mobile menu toggle (nếu cần)
const createMobileMenu = () => {
    const navWrapper = document.querySelector('.nav-wrapper');
    const menuBtn = document.createElement('button');
    menuBtn.className = 'mobile-menu-btn';
    menuBtn.innerHTML = '<i class="icon-menu"></i>';

    menuBtn.addEventListener('click', () => {
        navWrapper.classList.toggle('mobile-active');
    });

    if (window.innerWidth <= 768) {
        document.querySelector('.header-nav .container-fluid').prepend(menuBtn);
    }
};

window.addEventListener('resize', () => {
    if (window.innerWidth <= 768) {
        createMobileMenu();
    }
});

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