namespace SimpleSocialMediaPlatform.Models
{
    public class CombinedPostsCreateViewModel
    {
        public List<UserPostCommentViewModel> Posts { get; set; } = new List<UserPostCommentViewModel>();
        public Post NewPost { get; set; } = new Post();
    }
}
