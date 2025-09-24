using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Organization { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties for user preferences
        public string? PreferredLanguage { get; set; } = "en-US";
        public string? TimeZone { get; set; } = "UTC";

        // Full name property for display
        public string FullName => $"{FirstName} {LastName}";

        // Profile picture URL (can be integrated with external providers)
        public string? ProfilePictureUrl { get; set; }
    }
}