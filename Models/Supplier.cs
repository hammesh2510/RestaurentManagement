using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Supplier Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [Required]
        [Phone]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        // Navigation property for purchase history
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
