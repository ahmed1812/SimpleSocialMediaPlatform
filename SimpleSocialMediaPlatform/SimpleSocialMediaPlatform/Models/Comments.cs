using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSocialMediaPlatform.Models
{
    public class Comments
    {
        [Key]
        public int Id { get; set; }
        public string? Body { get; set; }
        public string UserId { get; set; } // change it to string
        //[ForeignKey("PostId")]
        public int PostId { get; set; }
        public DateTime CreateAt { get; set; }
        public string? ImageName { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile? ImageFile { get; set; }
        [ForeignKey("UserId")]
        public  ApplicationUser User { get; set; } // Navigation property
    }
}
