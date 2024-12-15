using kursah_5semestr.Abstractions;
using kursah_5semestr.Contracts;
using Microsoft.EntityFrameworkCore;

namespace kursah_5semestr.Services
{
    public class OrdersService : IOrdersService
    {
        private AppDbContext _context;
        private IBrokerService _brokerService;

        public OrdersService(AppDbContext context, IBrokerService brokerService)
        {
            _context = context;
            _brokerService = brokerService;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }


        public async Task<Order?> UpdateOrderStatus(Guid id, string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return null;
            }

            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            var dto = new OrderOutDto(Id: order.Id, Date: order.Date, Status: order.Status, Amount: order.Amount);
            var message = new InstanceChangedOut(Action: "update", Entity: "order", Data: dto);
            await _brokerService.SendMessage("changes", message);
            return order;
        }

        public async Task<Order?> GetOrderById(Guid id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IList<Order>> GetOrdersByUser(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<IList<Order>> GetAllOrders()
        {
            return await _context.Orders
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<bool> DeleteOrder(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
