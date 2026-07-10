using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Tax
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tax Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.00, 100.00, ErrorMessage = "Tax percentage must be between 0 and 100.")]
        [Display(Name = "Tax Percentage (%)")]
        public decimal Percentage { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for menu items using this tax scale
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
