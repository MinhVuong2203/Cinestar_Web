
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