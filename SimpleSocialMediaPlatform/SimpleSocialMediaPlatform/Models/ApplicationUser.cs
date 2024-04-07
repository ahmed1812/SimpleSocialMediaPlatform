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
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public DateTime DOB { get; set; }
        public DateTime CreateAt { get; set; }
      
    }
}
