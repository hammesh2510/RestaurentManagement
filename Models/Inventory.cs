using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Ingredient/Item Name")]
        public string IngredientName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Current Stock")]
        public decimal CurrentStock { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Minimum Stock Level")]
        public decimal MinStock { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = "units"; // kg, liters, pieces, etc.

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for purchases
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
