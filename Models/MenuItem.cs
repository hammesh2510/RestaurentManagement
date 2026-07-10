using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Menu item name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00.")]
        public decimal Price { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Vegetarian")]
        public bool IsVeg { get; set; } = true;

        [Required]
        [Display(Name = "Recommended")]
        public bool IsRecommended { get; set; } = false;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [Display(Name = "Tax Configuration")]
        public int? TaxId { get; set; }

        [ForeignKey("TaxId")]
        public virtual Tax? Tax { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string RestaurantId { get; set; } = string.Empty;

        // Navigation property for order items
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
