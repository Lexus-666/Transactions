﻿using System;
using System.Collections.Generic;

namespace kursah_5semestr;

public partial class Order
{

    public Guid Id { get; set; }

    public DateTime Date { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }

    public string Status { get; set; }

    public double Amount { get; set; }
}