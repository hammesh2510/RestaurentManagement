using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Loyalty Points")]
        public int LoyaltyPoints { get; set; } = 0;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
