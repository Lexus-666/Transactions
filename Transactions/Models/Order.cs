﻿using kursah_5semestr;

namespace kursah_5semestr;

public partial class Order
{
    public Order() { }

    private Order(Guid id, DateTime date, Guid userId, string status, double amount)
    {
        Id = id;
        UserId = userId;
        Date = date;
        Status = status;
        Amount = amount;
    }

    private static string BasicCheck(double amount, string status)
    {
        var error = string.Empty;

        if (amount <= 0)
        {
            error = $"Amount must be greater than zero";
        }
        else if (string.IsNullOrWhiteSpace(status))
        {
            error = "Status cannot be empty or null.";
        }

        return error;
    }

    public static (Order? Order, string Error) Create(Guid id, DateTime date, Guid userId, string status, double amount)
    {
        var error = BasicCheck(amount, status);

        if (!string.IsNullOrEmpty(error))
        {
            return (null, error);
        }

        var order = new Order(id, date, userId, status, amount);

        return (order, error);
    }
}