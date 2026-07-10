using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete

        [Required]
        [StringLength(50)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string KeyValues { get; set; } = string.Empty; // Primary key details in JSON

        public string? OldValues { get; set; } // JSON of older fields

        public string? NewValues { get; set; } // JSON of newer fields
    }
}
