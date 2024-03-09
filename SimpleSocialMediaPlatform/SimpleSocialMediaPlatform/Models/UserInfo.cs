using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SimpleSocialMediaPlatform.Models
{
    public class UserInfo
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserId { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string? ImageName { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile? ImageFile { get; set; }
        public DateTime DOB { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
