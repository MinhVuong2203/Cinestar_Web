namespace Web.Models.DTOs
{
    public class PurchasedTicketDto
    {
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ShowTime { get; set; }
        public string SeatName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string TicketType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; }

    }
}
