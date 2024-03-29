﻿using System.ComponentModel.DataAnnotations;

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
    }
}
