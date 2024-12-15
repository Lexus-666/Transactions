using kursah_5semestr.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace kursah_5semestr.Services
{
    public class TransactionsService : ITransactionsService
    {
        private AppDbContext _context;
        private IOrdersService _ordersService;

        public TransactionsService(AppDbContext context, IOrdersService ordersService)
        {
            _context = context;
            _ordersService = ordersService;
        }

        public async Task<Transaction> CreateTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }


        public async Task<bool> UpdateTransactionStatus(Guid id, string newStatus)
        {
            using (var tx = _context.Database.BeginTransaction())
            {
                try
                {
                    var transaction = await _context.Transactions.FindAsync(id);
                    if (transaction == null)
                    {
                        return false;
                    }

                    transaction.Status = newStatus;
                    transaction.PaidAt = DateTime.UtcNow;
                    _context.Transactions.Update(transaction);
                    await _context.SaveChangesAsync();
                    if (newStatus == "paid")
                    {
                        await _ordersService.UpdateOrderStatus(transaction.OrderId, "paid");
                    }
                    tx.Commit();
                    return true;
                }
                catch (Exception)
                {
                    tx.Rollback();
                    return false;
                }
            }
        }

        public async Task<Transaction?> GetTransactionById(Guid id)
        {
            return await _context.Transactions
                .Include(o => o.User)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IList<Transaction>> GetTransactionsByUser(Guid userId)
        {
            return await _context.Transactions
                .Where(o => o.UserId == userId)
                .Include(o => o.Order)
                .ToListAsync();
        }

        public async Task<IList<Transaction>> GetAllTransactions()
        {
            return await _context.Transactions
                .Include(o => o.User)
                .Include(o => o.Order)
                .ToListAsync();
        }

        public async Task<bool> DeleteTransaction(Guid id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return false;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
