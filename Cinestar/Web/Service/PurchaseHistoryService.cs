using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models.DTOs;

namespace Web.Service
{
    public class PurchaseHistoryService : IPurchaseHistoryService
    {
        private readonly CineStarContext _context;

        public PurchaseHistoryService(CineStarContext context)
        {
            _context = context;
        }

        public List<PurchasedTicketDto> GetPurchasedTicketsByCustomerId(Guid customerId)
        {
            var tickets = _context.Invoices
                .Where(i => i.CustomerID == customerId && i.Status == "Đã thanh toán" && !i.IsDeleted)
                .Include(i => i.InvoiceTickets)
                    .ThenInclude(it => it.Ticket)
                        .ThenInclude(t => t.Seat)
                .Include(i => i.InvoiceTickets)
                    .ThenInclude(it => it.Ticket)
                        .ThenInclude(t => t.ShowTime)
                            .ThenInclude(st => st.Movie)
                .Include(i => i.InvoiceTickets)
                    .ThenInclude(it => it.Ticket)
                        .ThenInclude(t => t.ShowTime)
                            .ThenInclude(st => st.Room)
                                .ThenInclude(r => r.Branch)
                .SelectMany(i => i.InvoiceTickets.Select(it => new PurchasedTicketDto
                {
                    MovieTitle = it.Ticket.ShowTime.Movie.Title,
                    ShowTime = it.Ticket.ShowTime.StartTime,
                    SeatName = it.Ticket.Seat.SeatName,
                    Price = (decimal)it.Ticket.Price,
                    BranchName = it.Ticket.ShowTime.Room.Branch.BranchName,
                    RoomName = it.Ticket.ShowTime.Room.RoomName,
                    TicketType = it.Ticket.TicketType,
                    Status = it.Ticket.Status,
                    PurchaseDate = i.IssueDate
                }))
                .OrderByDescending(t => t.PurchaseDate)
                .ToList();

            return tickets;
        }
    }
}
