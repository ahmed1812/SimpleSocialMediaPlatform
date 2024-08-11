using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SimpleSocialMediaPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        //[Key]
        //public int Id { get; set; }
        public string? FullName { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty; // Ensure Address is not null
        public string? Address2 { get; set; } = string.Empty;
        public string? City { get; set; } = string.Empty;
        public string? State { get; set; } = string.Empty;
        public string? ZipCode { get; set; } = string.Empty;
        public byte[]? ProfilePicture { get; set; }
        public DateTime DOB { get; set; }
        public DateTime CreateAt { get; set; }
        // New Category Property
        public string Category { get; set; } = string.Empty;
        public  ICollection<Post> Posts { get; set; } = new List<Post>();
        public  ICollection<Comments> Comments { get; set; } = new List<Comments>();

    }
}
