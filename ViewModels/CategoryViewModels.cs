using System;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Category name must be between 3 and 50 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Status (Active)")]
        public bool IsActive { get; set; } = true;
    }

    public class CategoryDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Total Menu Items")]
        public int MenuItemCount { get; set; }
    }
}
