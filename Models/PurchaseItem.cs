using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class PurchaseItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Purchase")]
        public int PurchaseId { get; set; }

        [ForeignKey("PurchaseId")]
        public virtual Purchase? Purchase { get; set; }

        [Required]
        [Display(Name = "Inventory Ingredient")]
        public int InventoryId { get; set; }

        [ForeignKey("InventoryId")]
        public virtual Inventory? Inventory { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 100000.00, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.00, 100000.00, ErrorMessage = "Price must be non-negative.")]
        [Display(Name = "Price Per Unit")]
        public decimal PricePerUnit { get; set; }
    }
}
