using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [Required]
        [Display(Name = "Table")]
        public int TableId { get; set; }

        [ForeignKey("TableId")]
        public virtual RestaurantTable? Table { get; set; }

        [Required]
        [Display(Name = "Reservation Date & Time")]
        public DateTime ReservationDate { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Number of guests must be at least 1.")]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [StringLength(500, ErrorMessage = "Special requests cannot exceed 500 characters.")]
        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
