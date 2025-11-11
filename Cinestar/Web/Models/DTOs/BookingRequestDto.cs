namespace Web.Models.DTOs
{
    public class BookingRequestDto
    {
        // Customer Info
        public string? CustomerId { get; set; }
        public string? CustomerPhone { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public bool IsGuest { get; set; }

        // Movie & Showtime Info
        public string MovieId { get; set; } = string.Empty;
        public string ShowTimeId { get; set; } = string.Empty;
        public string? BranchId { get; set; }
        public string? RoomName { get; set; }
        public string? RoomType { get; set; }
        public string? ShowDate { get; set; }
        public string? ShowTime { get; set; }

        // Tickets Info
        public List<TicketItemDto> Tickets { get; set; } = new();

        // Seats Info
        public List<SeatItemDto> Seats { get; set; } = new();

        // Products/Combos Info
        public List<ProductItemDto> Products { get; set; } = new();

        // Total Amount
        public decimal TotalAmount { get; set; }
        public int PointsToEarn { get; set; }

    }

    public class TicketItemDto
    {
        public string TicketType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class SeatItemDto
    {
        public string SeatId { get; set; } = string.Empty;
        public string SeatName { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
    }

    public class ProductItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
