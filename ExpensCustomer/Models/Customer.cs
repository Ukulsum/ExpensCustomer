using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpensCustomer.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

    }

    public class Expense
    {
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        [ValidateNever]
        public Customer Customer { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;
        public string Note { get; set; }

        public List<ExpenseItem> ExpenseItems { get; set; }
    }

    public class ExpenseItem
    {
        public int Id { get; set; }

        [ForeignKey("Expense")]
        public int ExpenseId { get; set; }
        [ValidateNever]
        public Expense Expense { get; set; }
        public string ExpenseType { get; set; } // Transport, Food, Hotel
        public decimal Amount { get; set; }
    }
}
