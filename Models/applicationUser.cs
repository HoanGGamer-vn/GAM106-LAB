using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GAM106_LAB.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [ForeignKey("Region")]
        public int? RegionId { get; set; }

        // Navigation to Region
        public Region? Region { get; set; }

        // URL to profile picture
        public string? ProfilePictureUrl { get; set; }

        public bool IsDeleted { get; set; } = false;

        [JsonIgnore]
        public string OTP { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "none";

        // Optional Role FK
        [ForeignKey("Role")]
        public int? RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
