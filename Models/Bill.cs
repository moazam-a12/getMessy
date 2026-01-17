using System;

public class Bill
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime Date { get; set; }

    public decimal Amount { get; set; }

    public bool Paid { get; set; } = false;
}