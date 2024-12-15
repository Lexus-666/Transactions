using System;
using System.Collections.Generic;

namespace kursah_5semestr;

public partial class Transaction
{
    public Guid Id { get; set; }

    public string TransactionDetails { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; }

    public Guid OrderId { get; set; }

    public Order Order { get; set; }

    public double Amount { get; set; }

    public string Status { get; set; }

    public DateTime CreatedAt {  get; set; }

    public DateTime? PaidAt { get; set; }
}
