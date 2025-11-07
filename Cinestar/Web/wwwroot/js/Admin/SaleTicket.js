const bookingData = {
    movie: null,
    tickets: { standard: 0, vip: 0, couple: 0 },
  date: null,
    time: null,
    combos: { combo1: 0, combo2: 0, popcorn: 0, drink: 0 }
};

let ticketPrices = { standard: 80000, vip: 120000, couple: 200000 };
const comboPrices = { combo1: 70000, combo2: 120000, popcorn: 45000, drink: 30000 };

//// Đợi DOM load xong
//document.addEventListener('DOMContentLoaded', function() {
//    console.log('SaleTicket.js loaded');
    
//  // Gắn sự kiện click cho tất cả các nút đặt vé
//    const bookButtons = document.querySelectorAll('.btn-book');
//    console.log('Found', bookButtons.length, 'book buttons');
    
//    bookButtons.forEach(function(button) {
//        button.addEventListener('click', function(e) {
//   e.preventDefault();
//   e.stopPropagation();
            
//   const movieId = this.getAttribute('data-movie');
//   console.log('Button clicked, movieId:', movieId);
  
//         selectMovie(movieId);
//   });
//    });
//});

////hàm cho button chọn phim
//function selectMovie(movieId) {
//    console.log('selectMovie called with movieId:', movieId);
    
//    if (!movieId || movieId === '') {
//  alert('Không tìm thấy mã phim!');
//   return;
//    }
 
//    // Chuyển sang trang TicketSelling với movieId (với Area Admin)
//    const url = `/Admin/EmployeeSale/TicketSelling?movieId=${encodeURIComponent(movieId)}`;
//    console.log('Navigating to:', url);
  
//    window.location.href = url;
//}

// Hàm format giá tiền
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + ' ₫';
}