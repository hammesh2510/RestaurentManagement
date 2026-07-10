using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Category name must be between 3 and 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Category description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string RestaurantId { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
