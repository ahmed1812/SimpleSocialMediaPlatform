using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SimpleSocialMediaPlatform.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Titel { get; set; }
        public string Body { get; set; }
        public string CreateAt { get; set; }
        [ForeignKey("UserInfo")]
        public string UserId { get; set; }
        [NotMapped]
        public UserInfo? PostUserInfo { get; set; } // This is used for RunTime ONLY
    }
}
