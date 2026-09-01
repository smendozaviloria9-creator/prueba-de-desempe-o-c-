using System;

namespace Financiera.Models
{
    public enum MovementType
    {
        Deposit,
        Withdrawal
    }

    public class Movement
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public MovementType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}