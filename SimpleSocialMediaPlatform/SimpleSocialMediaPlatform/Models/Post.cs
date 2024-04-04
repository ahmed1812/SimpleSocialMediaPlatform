using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SimpleSocialMediaPlatform.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string? Titel { get; set; }
        public string? Body { get; set; }
        public DateTime CreateAt { get; set; }
        [ForeignKey("UserInfo")]
        public string? UserId { get; set; }
        [NotMapped]
        public UserInfo? PostUserInfo { get; set; } // This is used for RunTime ONLY

        public string? ImageName { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile? ImageFile { get; set; }
       
    }
}
