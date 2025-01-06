namespace kursah_5semestr;

public partial class Transaction
{
    public Transaction() { }

    private Transaction(Guid id, string transactionDetails, Guid userId, Guid orderId, double amount)
    {
        Id = id;
        TransactionDetails = transactionDetails;
        UserId = userId;
        OrderId = orderId;
        Amount = amount;
        Status = TransactionStatus.New;
        CreatedAt = DateTime.UtcNow;
    }

    public static string BasicCheck(string transactionDetails, double amount)
    {
        var error = string.Empty;

        if (string.IsNullOrWhiteSpace(transactionDetails))
        {
            error = "Transaction details cannot be empty.";
        }
        else if (amount <= 0)
        {
            error = $"Amount must be greater than zero";
        }

        return error;
    }

    public static (Transaction Transaction, string Error) Create(string transactionDetails, Order order)
    {
        var error = BasicCheck(transactionDetails, order.Amount);

        if (!string.IsNullOrEmpty(error))
        {
            return (null!, error);
        }

        var transaction = new Transaction(Guid.NewGuid(), transactionDetails, order.UserId, order.Id, order.Amount);

        return (transaction, error);
    }
}