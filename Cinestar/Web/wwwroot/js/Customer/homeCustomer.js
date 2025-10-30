// ========== BOOKING SECTION ==========
const cinema = document.getElementById('cinema');
const movie = document.getElementById('movie');
const date = document.getElementById('date');
const time = document.getElementById('time');
const bookBtn = document.getElementById('bookBtn');

cinema.disabled = false;
cinema.classList.remove('combo-disabled');

function checkBookButton() {
    const hasValue = cinema.value || movie.value || date.value || time.value;
    bookBtn.disabled = !hasValue;
}

cinema.addEventListener('change', function() {
    if (this.value) {
        movie.disabled = false;
        movie.classList.remove('combo-disabled');
        date.disabled = true;
        date.classList.add('combo-disabled');
        time.disabled = true;
        time.classList.add('combo-disabled');
        movie.value = '';
        date.value = '';
        time.value = '';
    }
    checkBookButton();
});

movie.addEventListener('change', function() {
    if (this.value) {
        date.disabled = false;
        date.classList.remove('combo-disabled');
        time.disabled = true;
        time.classList.add('combo-disabled');
        date.value = '';
        time.value = '';
    }
    checkBookButton();
});

date.addEventListener('change', function() {
    if (this.value) {
        time.disabled = false;
        time.classList.remove('combo-disabled');
        time.value = '';
    }
    checkBookButton();
});

time.addEventListener('change', function() {
    checkBookButton();
});

bookBtn.addEventListener('click', function() {
    let message = 'Đặt vé thành công!';
    if (cinema.value) message += '\nRạp: ' + cinema.options[cinema.selectedIndex].text;
    if (movie.value) message += '\nPhim: ' + movie.options[movie.selectedIndex].text;
    if (date.value) message += '\nNgày: ' + date.options[date.selectedIndex].text;
    if (time.value) message += '\nGiờ: ' + time.options[time.selectedIndex].text;
    alert(message);
});

checkBookButton();

// ========== MOVIE CAROUSEL SECTION 2 ==========
const carouselTrack = document.getElementById('movieCarousel');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');
const dotsContainer = document.getElementById('carouselDots');
const slides = carouselTrack.querySelectorAll('.carousel-slide');

let currentSlide = 0;
const totalSlides = slides.length;

// Tạo dots
function createDots() {
    dotsContainer.innerHTML = '';
    for (let i = 0; i < totalSlides; i++) {
        const dot = document.createElement('div');
        dot.classList.add('dot');
        if (i === 0) dot.classList.add('active');
        dot.addEventListener('click', () => goToSlide(i));
        dotsContainer.appendChild(dot);
    }
}

// Cập nhật dots
function updateDots() {
    const dots = document.querySelectorAll('.dot');
    dots.forEach((dot, index) => {
        dot.classList.toggle('active', index === currentSlide);
    });
}

function updateButtons() {
    prevBtn.disabled = currentSlide === 0;
    nextBtn.disabled = currentSlide >= totalSlides - 1;
}

function goToSlide(slideIndex) {
    currentSlide = Math.max(0, Math.min(slideIndex, totalSlides - 1));
    
    const translateX = -(currentSlide * 100);
    carouselTrack.style.transform = `translateX(${translateX}%)`;
    carouselTrack.style.transition = 'transform 0.5s ease';
    carouselTrack.style.height = 'auto';
    
    updateDots();
    updateButtons();
}

// Nút Next
nextBtn.addEventListener('click', () => {
    if (currentSlide < totalSlides - 1) {
        goToSlide(currentSlide + 1);
    }
});

// Nút Previous
prevBtn.addEventListener('click', () => {
    if (currentSlide > 0) {
        goToSlide(currentSlide - 1);
    }
});

// Khởi tạo carousel
if (totalSlides > 1) {
    createDots();
}
updateButtons();

// Xử lý khi resize window
window.addEventListener('resize', () => {
    goToSlide(currentSlide);
});

// ========== MOVIE CAROUSEL SECTION 3 ==========
const carouselTrack3 = document.getElementById('movieCarousel3');
const prevBtn3 = document.getElementById('prevBtn3');
const nextBtn3 = document.getElementById('nextBtn3');
const dotsContainer3 = document.getElementById('carouselDots3');
const slides3 = document.querySelectorAll('.section-3 .carousel-slide');

let currentSlide3 = 0;
const totalSlides3 = slides3.length;

// Tạo dots
function createDots3() {
    dotsContainer3.innerHTML = '';
    for (let i = 0; i < totalSlides3; i++) {
        const dot = document.createElement('div');
        dot.classList.add('dot');
        if (i === 0) dot.classList.add('active');
        dot.addEventListener('click', () => goToSlide3(i));
        dotsContainer3.appendChild(dot);
    }
}

// Cập nhật dots
function updateDots3() {
    const dots = document.querySelectorAll('.section-3 .dot');
    dots.forEach((dot, index) => {
        dot.classList.toggle('active', index === currentSlide3);
    });
}

function updateButtons3() {
    prevBtn3.disabled = currentSlide3 === 0;
    nextBtn3.disabled = currentSlide3 >= totalSlides3 - 1;
}

function goToSlide3(slideIndex) {
    currentSlide3 = Math.max(0, Math.min(slideIndex, totalSlides3 - 1));
    
    const translateX = -(currentSlide3 * 100);
    carouselTrack3.style.transform = `translateX(${translateX}%)`;
    carouselTrack3.style.transition = 'transform 0.5s ease';
    
    updateDots3();
    updateButtons3();
}

// Nút Next
nextBtn3.addEventListener('click', () => {
    if (currentSlide3 < totalSlides3 - 1) {
        goToSlide3(currentSlide3 + 1);
    }
});

// Nút Previous
prevBtn3.addEventListener('click', () => {
    if (currentSlide3 > 0) {
        goToSlide3(currentSlide3 - 1);
    }
});

// Khởi tạo carousel
if (totalSlides3 > 1) {
    createDots3();
}
updateButtons3();

// Xử lý khi resize window
window.addEventListener('resize', () => {
    goToSlide3(currentSlide3);
});

/* JS của HMINH*/

document.addEventListener('DOMContentLoaded', function () {
    const wrapper = document.querySelector('.cards-wrapper');
    const nextBtn = document.getElementById('nextSlide');
    const prevBtn = document.getElementById('prevSlide');

    // Lấy tất cả các thẻ
    const cards = document.querySelectorAll('.cards-wrapper > div');
    const cardWidth = cards[0].offsetWidth; // Lấy chiều rộng của 1 thẻ

    let currentIndex = 0;
    // Tính toán số lần trượt tối đa (hiển thị 3 thẻ 1 lúc)
    const maxIndex = cards.length - Math.floor(wrapper.parentElement.offsetWidth / cardWidth);

    function updateCarousel() {
        // Dịch chuyển wrapper sang trái
        wrapper.style.transform = `translateX(-${currentIndex * cardWidth}px)`;
    }

    nextBtn.addEventListener('click', () => {
        // Tăng index, nếu vượt quá max thì quay về 0 (vòng lặp)
        currentIndex = (currentIndex + 1) > maxIndex ? 0 : currentIndex + 1;
        updateCarousel();
    });

    prevBtn.addEventListener('click', () => {
        // Giảm index, nếu nhỏ hơn 0 thì quay về max (vòng lặp)
        currentIndex = (currentIndex - 1) < 0 ? maxIndex : currentIndex - 1;
        updateCarousel();
    });
});
