namespace kursah_5semestr.Abstractions
{
    public interface ITransactionsService
    {
        public Task<Transaction> CreateTransaction(Transaction Transaction);

        public Task<bool> UpdateTransactionStatus(Guid id, string status);

        public Task<Transaction?> GetTransactionById(Guid id);

        public Task<IList<Transaction>> GetTransactionsByUser(Guid user);

        public Task<IList<Transaction>> GetAllTransactions();

        public Task<bool> DeleteTransaction(Guid id);
    }
}
