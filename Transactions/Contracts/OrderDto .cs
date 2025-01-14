﻿using System.Security.Cryptography.X509Certificates;

namespace kursah_5semestr.Contracts
{
    public record OrderDto(
        Guid Id,
        Guid UserId,
        DateTime Date,
        OrderStatus Status,
        double Amount
        );
}
