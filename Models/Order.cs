using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public enum OrderType
    {
        DineIn,
        TakeAway,
        Delivery
    }

    public enum OrderStatus
    {
        Pending,
        Preparing,
        Ready,
        Served,
        Completed,
        Cancelled
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Display(Name = "Table")]
        public int? TableId { get; set; }

        [ForeignKey("TableId")]
        public virtual RestaurantTable? Table { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [Required]
        [Display(Name = "Order Type")]
        public OrderType OrderType { get; set; } = OrderType.DineIn;

        [Required]
        [Display(Name = "Order Status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Sub Total")]
        public decimal SubTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount { get; set; } = 0.00m;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Grand Total")]
        public decimal GrandTotal { get; set; }

        [StringLength(500)]
        [Display(Name = "Order Notes")]
        public string? OrderNotes { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser? CreatedByUser { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
