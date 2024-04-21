// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleSocialMediaPlatform.Data;
using SimpleSocialMediaPlatform.Models;

namespace SimpleSocialMediaPlatform.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        //public async Task<IActionResult> OnPostAsync()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //    }

        //    RequirePassword = await _userManager.HasPasswordAsync(user);
        //    if (RequirePassword)
        //    {
        //        if (!await _userManager.CheckPasswordAsync(user, Input.Password))
        //        {
        //            ModelState.AddModelError(string.Empty, "Incorrect password.");
        //            return Page();
        //        }
        //    }

        //    var result = await _userManager.DeleteAsync(user);
        //    var userId = await _userManager.GetUserIdAsync(user);
        //    if (!result.Succeeded)
        //    {
        //        throw new InvalidOperationException($"Unexpected error occurred deleting user.");
        //    }

        //    await _signInManager.SignOutAsync();

        //    _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

        //    return Redirect("~/");
        //}

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            // Begin transaction to ensure all deletions (user, posts, comments, images) are successful
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Delete all posts and related data owned by the user
                    var posts = await _context.Posts
                                              .Include(p => p.Comments)
                                              .Where(p => p.UserId == user.Id)
                                              .ToListAsync();

                    foreach (var post in posts)
                    {
                        // Delete images associated with the post
                        DeleteImage(post.PostImageName);

                        foreach (var comment in post.Comments)
                        {
                            // Delete images associated with comments
                            DeleteImage(comment.ImageName);
                        }

                        // Delete all comments of the post
                        _context.Comments.RemoveRange(post.Comments);
                    }

                    // Delete all posts
                    _context.Posts.RemoveRange(posts);

                    // Delete the user
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{user.Id}'.");
                    }

                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Rollback transaction if any error occurs
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while deleting the user and their related data.");
                    throw;  // Rethrow the exception to handle it further (e.g., return error page)
                }
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User with ID '{UserId}' deleted themselves along with all related data.", user.Id);

            return Redirect("~/");
        }

        private void DeleteImage(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", imageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

    }
}

