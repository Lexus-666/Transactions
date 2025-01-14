﻿using System.Security.Cryptography.X509Certificates;

namespace kursah_5semestr.Contracts
{
    public record TransactionOutDto(
        Guid Id,
        TransactionStatus Status,
        OrderOutDto Order,
        string TransactionDetails,
        DateTime CreatedAt,
        DateTime? PaidAt,
        double Amount
        );
}
