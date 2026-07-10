using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Order")]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [Required]
        [Display(Name = "Menu Item")]
        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem? MenuItem { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.00, 10000.00, ErrorMessage = "Price must be non-negative.")]
        public decimal Price { get; set; } // Price active at the time order was placed

        [StringLength(250)]
        public string? Notes { get; set; }
    }
}
