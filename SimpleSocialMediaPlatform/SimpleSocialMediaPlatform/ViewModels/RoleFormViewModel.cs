using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SimpleSocialMediaPlatform.ViewModels
{
    public class RoleFormViewModel
    {
        [Required, StringLength(256)]
        public string Name { get; set; }
        // List of roles for the dropdown
        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
    }
}
