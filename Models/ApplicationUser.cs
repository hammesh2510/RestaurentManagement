using Microsoft.AspNetCore.Identity;

namespace RestaurantManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for linked Employee database record
        public virtual Employee? Employee { get; set; }
    }
}
