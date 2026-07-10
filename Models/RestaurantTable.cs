using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public enum TableStatus
    {
        Available,
        Reserved,
        Occupied,
        Cleaning
    }

    public class RestaurantTable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Table Number / Name")]
        public string TableNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        public int Capacity { get; set; }

        [Required]
        public TableStatus Status { get; set; } = TableStatus.Available;

        // Navigation properties
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
