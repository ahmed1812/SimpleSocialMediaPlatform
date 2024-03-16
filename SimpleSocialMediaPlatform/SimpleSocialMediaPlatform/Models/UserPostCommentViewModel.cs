namespace SimpleSocialMediaPlatform.Models
{
    public class UserPostCommentViewModel
    {
        public UserInfo UserInfoDetails { get; set; }
        public List<Post>  UserPost { get; set; }
        public List<Comments> UserComments { get; set; }
    }
}
