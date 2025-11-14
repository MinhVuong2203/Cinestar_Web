// ========== ĐỢI DOM LOAD XONG ==========
document.addEventListener('DOMContentLoaded', function () {
    console.log('🎬 Home Customer JS loaded');

    // Khởi tạo các carousel
    initMovieCarousel();
    initComingSoonCarousel();
    initPromotionCarousel();
});

// ========== CAROUSEL PHIM ĐANG CHIẾU ==========
function initMovieCarousel() {
    const carouselTrack = document.getElementById('movieCarousel');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const dotsContainer = document.getElementById('carouselDots');

    if (!carouselTrack || !prevBtn || !nextBtn || !dotsContainer) {
        console.log('⚠️ Carousel "Phim Đang Chiếu" không đầy đủ elements');
        return;
    }

    const slides = carouselTrack.querySelectorAll('.carousel-slide');
    if (slides.length === 0) {
        console.log('⚠️ Không có slide nào trong "Phim Đang Chiếu"');
        return;
    }

    console.log(`✅ Carousel "Phim Đang Chiếu" - ${slides.length} slides`);

    let currentSlide = 0;

    function createDots() {
        dotsContainer.innerHTML = '';
        for (let i = 0; i < slides.length; i++) {
            const dot = document.createElement('button');
            dot.classList.add('dot');
            dot.setAttribute('type', 'button');
            if (i === 0) dot.classList.add('active');
            dot.addEventListener('click', () => goToSlide(i));
            dotsContainer.appendChild(dot);
        }
    }

    function updateDots() {
        const dots = dotsContainer.querySelectorAll('.dot');
        dots.forEach((dot, index) => {
            if (index === currentSlide) {
                dot.classList.add('active');
            } else {
                dot.classList.remove('active');
            }
        });
    }

    function updateButtons() {
        prevBtn.disabled = (currentSlide === 0);
        nextBtn.disabled = (currentSlide >= slides.length - 1);
    }

    function goToSlide(index) {
        currentSlide = Math.max(0, Math.min(index, slides.length - 1));
        const offset = -(currentSlide * 100);
        carouselTrack.style.transform = `translateX(${offset}%)`;
        updateDots();
        updateButtons();
    }

    prevBtn.addEventListener('click', () => {
        if (currentSlide > 0) goToSlide(currentSlide - 1);
    });

    nextBtn.addEventListener('click', () => {
        if (currentSlide < slides.length - 1) goToSlide(currentSlide + 1);
    });

    if (slides.length > 1) createDots();
    updateButtons();
}

// ========== CAROUSEL PHIM SẮP CHIẾU ==========
function initComingSoonCarousel() {
    const carouselTrack = document.getElementById('movieCarousel3');
    const prevBtn = document.getElementById('prevBtn3');
    const nextBtn = document.getElementById('nextBtn3');
    const dotsContainer = document.getElementById('carouselDots3');

    if (!carouselTrack || !prevBtn || !nextBtn || !dotsContainer) {
        console.log('⚠️ Carousel "Phim Sắp Chiếu" không đầy đủ elements');
        return;
    }

    const slides = carouselTrack.querySelectorAll('.carousel-slide');
    if (slides.length === 0) {
        console.log('⚠️ Không có slide nào trong "Phim Sắp Chiếu"');
        return;
    }

    console.log(`✅ Carousel "Phim Sắp Chiếu" - ${slides.length} slides`);

    let currentSlide = 0;

    function createDots() {
        dotsContainer.innerHTML = '';
        for (let i = 0; i < slides.length; i++) {
            const dot = document.createElement('button');
            dot.classList.add('dot');
            dot.setAttribute('type', 'button');
            if (i === 0) dot.classList.add('active');
            dot.addEventListener('click', () => goToSlide(i));
            dotsContainer.appendChild(dot);
        }
    }

    function updateDots() {
        const dots = dotsContainer.querySelectorAll('.dot');
        dots.forEach((dot, index) => {
            if (index === currentSlide) {
                dot.classList.add('active');
            } else {
                dot.classList.remove('active');
            }
        });
    }

    function updateButtons() {
        prevBtn.disabled = (currentSlide === 0);
        nextBtn.disabled = (currentSlide >= slides.length - 1);
    }

    function goToSlide(index) {
        currentSlide = Math.max(0, Math.min(index, slides.length - 1));
        const offset = -(currentSlide * 100);
        carouselTrack.style.transform = `translateX(${offset}%)`;
        updateDots();
        updateButtons();
    }

    prevBtn.addEventListener('click', () => {
        if (currentSlide > 0) goToSlide(currentSlide - 1);
    });

    nextBtn.addEventListener('click', () => {
        if (currentSlide < slides.length - 1) goToSlide(currentSlide + 1);
    });

    if (slides.length > 1) createDots();
    updateButtons();
}

// ========== CAROUSEL KHUYẾN MÃI ==========
function initPromotionCarousel() {
    const wrapper = document.querySelector('.cards-wrapper');
    const nextBtn = document.getElementById('nextSlide');
    const prevBtn = document.getElementById('prevSlide');

    if (!wrapper || !nextBtn || !prevBtn) {
        console.log('⚠️ Carousel "Khuyến Mãi" không đầy đủ elements');
        return;
    }

    const cards = wrapper.querySelectorAll(':scope > div');
    if (cards.length === 0) {
        console.log('⚠️ Không có card nào trong "Khuyến Mãi"');
        return;
    }

    console.log(`✅ Carousel "Khuyến Mãi" - ${cards.length} cards`);

    let currentIndex = 0;

    function updateCarousel() {
        const cardWidth = cards[0].offsetWidth;
        const containerWidth = wrapper.parentElement.offsetWidth;
        const visibleCards = Math.floor(containerWidth / cardWidth);
        const maxIndex = Math.max(0, cards.length - visibleCards);

        currentIndex = Math.min(currentIndex, maxIndex);

        wrapper.style.transform = `translateX(-${currentIndex * cardWidth}px)`;
        wrapper.style.transition = 'transform 0.3s ease';

        // Update button states
        prevBtn.disabled = currentIndex === 0;
        nextBtn.disabled = currentIndex >= maxIndex;
    }

    nextBtn.addEventListener('click', () => {
        const cardWidth = cards[0].offsetWidth;
        const containerWidth = wrapper.parentElement.offsetWidth;
        const visibleCards = Math.floor(containerWidth / cardWidth);
        const maxIndex = Math.max(0, cards.length - visibleCards);

        if (currentIndex < maxIndex) {
            currentIndex++;
        } else {
            currentIndex = 0; // Loop back
        }
        updateCarousel();
    });

    prevBtn.addEventListener('click', () => {
        const cardWidth = cards[0].offsetWidth;
        const containerWidth = wrapper.parentElement.offsetWidth;
        const visibleCards = Math.floor(containerWidth / cardWidth);
        const maxIndex = Math.max(0, cards.length - visibleCards);

        if (currentIndex > 0) {
            currentIndex--;
        } else {
            currentIndex = maxIndex; // Loop to end
        }
        updateCarousel();
    });

    // Initial setup
    updateCarousel();

    // Handle window resize
    window.addEventListener('resize', updateCarousel);
}