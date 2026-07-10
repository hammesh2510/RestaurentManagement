using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public enum PaymentMethod
    {
        Cash,
        Card,
        UPI
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }

    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Order")]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 100000.00, ErrorMessage = "Payment amount must be between 0.01 and 100000.00.")]
        public decimal AmountPaid { get; set; }

        [StringLength(100)]
        [Display(Name = "Transaction ID")]
        public string? TransactionId { get; set; }

        [Required]
        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}
