using kursah_5semestr.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace kursah_5semestr.Services
{
    public class TransactionsService : ITransactionsService
    {
        private AppDbContext _context;
        private IOrdersService _ordersService;
        private ILogger _logger;

        public TransactionsService(AppDbContext context, IOrdersService ordersService, ILogger<TransactionsService> logger)
        {
            _context = context;
            _ordersService = ordersService;
            _logger = logger;
        }

        public async Task<Transaction> CreateTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created transaction {transaction.Id}");
            return transaction;
        }


        public async Task<bool> UpdateTransactionStatus(Guid id, TransactionStatus newStatus)
        {
            using (var tx = _context.Database.BeginTransaction())
            {
                try
                {
                    var transaction = await _context.Transactions.FindAsync(id);
                    if (transaction == null)
                    {
                        _logger.LogInformation($"Transaction {id} not found");
                        return false;
                    }

                    transaction.Status = newStatus;
                    transaction.PaidAt = DateTime.UtcNow;
                    _context.Transactions.Update(transaction);
                    await _context.SaveChangesAsync();
                    if (newStatus == TransactionStatus.Paid)
                    {
                        await _ordersService.UpdateOrderStatus(transaction.OrderId, OrderStatus.Paid);
                    }
                    tx.Commit();
                    _logger.LogInformation($"Updated transaction {id} status");
                    return true;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    _logger.LogError(ex, $"Error updating transaction {id} status");
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
                _logger.LogInformation($"Transaction {id} not found");
                return false;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Deleted transaction {id}");

            return true;
        }
    }
}
