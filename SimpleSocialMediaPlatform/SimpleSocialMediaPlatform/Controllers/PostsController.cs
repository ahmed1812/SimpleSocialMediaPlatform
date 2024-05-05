using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // For authorization attributes
using Microsoft.AspNetCore.Hosting; // For hosting environment
using Microsoft.AspNetCore.Identity; // For managing users
using Microsoft.AspNetCore.Mvc; // For MVC framework
using Microsoft.EntityFrameworkCore; // For Entity Framework Core
using Microsoft.Extensions.Hosting; // For hosting environment
using SimpleSocialMediaPlatform.Data; // For accessing data context
using SimpleSocialMediaPlatform.Models; // For accessing models


namespace SimpleSocialMediaPlatform.Controllers
{
    [Authorize] // Requires authentication for accessing controller actions
    public class PostsController : Controller, DesignPatterns // Inherits from Controller class and implements DesignPatterns interface
    {
        private readonly ApplicationDbContext _context; // Database context
        private readonly IWebHostEnvironment _webHostEnvironment; // Hosting environment
        private readonly UserManager<ApplicationUser> _userManager; // User manager for managing user-related operations



        public PostsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context; // Dependency injection for database context
            _webHostEnvironment = webHostEnvironment; // Dependency injection for hosting environment
            _userManager = userManager; // Dependency injection for user manager
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            // Retrieve current user's ID
            var UserId = _userManager.GetUserId(User);
            // Store current user's ID in ViewData
            ViewData["UserID"] = UserId;
            // Another way to store current user's ID in ViewData
            ViewData["UserID"] = _userManager.GetUserId(this.User);
            var userId = _userManager.GetUserId(User); // Retrieve current user's ID
            var userP = await _userManager.FindByIdAsync(userId); // Find the user entity asynchronously


            // Assuming the profile picture is stored as a byte array in the user entity
            var profilePictureBase64 = userP.ProfilePicture != null ? Convert.ToBase64String(userP.ProfilePicture) : string.Empty;

            // Pass the profile picture to the view using ViewData (you could also add it to your model)
            ViewData["ProfilePictureBase64"] = profilePictureBase64;

            ApplicationUser userA = await _userManager.GetUserAsync(User);

            if (userA != null)
            {
                // If current user exists, set ViewData with username and full name (if available), otherwise default to "Guest"
                ViewData["UserName"] = userA.UserName;
                ViewData["UserFullName"] = string.IsNullOrWhiteSpace(userA.FullName) ? userA.UserName : userA.FullName;
            }
            else
            {
                // If current user doesn't exist, set ViewData with default values
                ViewData["UserName"] = "Guest";
                ViewData["UserFullName"] = "Guest";
            }



            var userPerPost = await (from user in _context.Users // LINQ query to join Users and Posts tables
                                     join post in _context.Posts on user.Id equals post.UserId // Joining Users and Posts tables on UserId
                                     orderby post.CreateAt descending // Ordering posts by creation date in descending order
                                     select new UserPostCommentViewModel // Projecting the result into a ViewModel
                                     {
                                         UserInfoDetails = new UserInfo // Creating UserInfo object
                                         {
                                             UserId = user.Id, // Setting user's ID
                                             FullName = user.UserName, // Setting user's full name
                                             UserPostImage = user.ProfilePicture != null ? Convert.ToBase64String(user.ProfilePicture) : string.Empty // Converting user's profile picture to Base64 string (if exists)
                                         },
                                         UserPosts = new List<Post> { post }, // Creating a list of posts for the user
                                         UserComments = _context.Comments // Querying comments associated with the post
                                             .Include(c => c.User) // Including user details for each comment
                                             .Where(comment => comment.PostId == post.Id) // Filtering comments by post ID
                                             .Select(c => new Comments // Projecting comments into Comments ViewModel
                                             {
                                                 Body = c.Body, // Setting comment body
                                                 UserName = c.User.FullName, // Setting commenter's full name
                                                 ImageName = c.ImageName, // Setting comment image name
                                                 UserProfilePicture = c.User.ProfilePicture != null ? Convert.ToBase64String(c.User.ProfilePicture) : string.Empty // Converting commenter's profile picture to Base64 string (if exists)
                                             }).ToList(), // Converting comments to list
                                         AppUsers = user // Setting user entity
                                     }).ToListAsync(); // Executing the query asynchronously and converting result to a list

            return View(userPerPost); // Returning the view with the userPerPost data

        }


        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            if (ModelState.IsValid)
            {
                // Setting the date here
                post.CreateAt = DateTime.Now;
                if (post.PostImageFile != null && post.PostImageFile.Length > 0)
                {
                    try
                    {
                        // Constructing a server-relative path
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                        // Ensure a unique file name
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(post.PostImageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        // Ensuring the directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        // Saving the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await post.PostImageFile.CopyToAsync(fileStream);
                        }
                        // Update your model as necessary
                        post.PostImageName = uniqueFileName;
                        post.PostImageUrl = "/Images/" + uniqueFileName; // Relative path
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it appropriately
                        ModelState.AddModelError("", "An error occurred while saving the image.");
                        return View(post); // Return the view with an error message if image save fails
                    }
                }
                // No else clause needed here. If no image is provided, just continue without setting the image properties.

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we get here, it means something was invalid in the ModelState or image saving failed
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Body,PostImageFile")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingPost = await _context.Posts.FindAsync(id);
                if (existingPost == null)
                {
                    return NotFound();
                }

                try
                {
                    if (post.PostImageFile != null && post.PostImageFile.Length > 0)
                    {
                        // Handling the new image upload
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(post.PostImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Delete the old image file if it exists
                        if (!string.IsNullOrEmpty(existingPost.PostImageName))
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, existingPost.PostImageName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await post.PostImageFile.CopyToAsync(fileStream);
                        }

                        // Update the database entry with new file name
                        existingPost.PostImageName = uniqueFileName;
                        existingPost.PostImageUrl = "/Images/" + uniqueFileName;
                    }

                    // Update only the fields that should be updated
                    existingPost.Titel = post.Titel;
                    existingPost.Body = post.Body;
                    existingPost.CreateAt = DateTime.Now; // Optionally update the modified date

                    _context.Update(existingPost);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", new { id = existingPost.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(existingPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;  // Log this exception or handle it as needed
                    }
                }
            }

            // If we get here, something was wrong with the ModelState
            return View(post);
        }

   


        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts' is null.");
            }
            var post = await _context.Posts
                                     .Include(p => p.Comments) // Ensure to include comments in the query
                                     .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            // Deleting associated images from the filesystem
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
            if (!string.IsNullOrEmpty(post.PostImageName))
            {
                string postImagePath = Path.Combine(uploadsFolder, post.PostImageName);
                if (System.IO.File.Exists(postImagePath))
                {
                    System.IO.File.Delete(postImagePath);
                }
            }

            // Delete images associated with each comment (if applicable)
            foreach (var comment in post.Comments)
            {
                if (!string.IsNullOrEmpty(comment.ImageName))
                {
                    string commentImagePath = Path.Combine(uploadsFolder, comment.ImageName);
                    if (System.IO.File.Exists(commentImagePath))
                    {
                        System.IO.File.Delete(commentImagePath);
                    }
                }
            }

            // Remove all comments associated with the post
            _context.Comments.RemoveRange(post.Comments);

            // Finally, remove the post
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        public async Task<IActionResult> Search(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return View("Index", new List<UserPostCommentViewModel>()); // Return empty or all posts optionally
            }

            var posts = await _context.Users
                .Where(u => u.FullName.Contains(userName))
                .SelectMany(u => _context.Posts.Where(p => p.UserId == u.Id))
                .Select(p => new UserPostCommentViewModel
                {
                    UserInfoDetails = new UserInfo
                    {
                        UserId = p.UserId,
                        FullName = p.AppUserInfo.UserName,
                        UserPostImage = p.AppUserInfo.ProfilePicture != null ? Convert.ToBase64String(p.AppUserInfo.ProfilePicture) : string.Empty
                    },
                    UserPosts = new List<Post> { p },
                    UserComments = _context.Comments
                        .Include(c => c.User)
                        .Where(comment => comment.PostId == p.Id)
                        .Select(c => new Comments
                        {
                            Body = c.Body,
                            UserName = c.User.FullName,
                            ImageName = c.ImageName,
                            UserProfilePicture = c.User.ProfilePicture != null ? Convert.ToBase64String(c.User.ProfilePicture) : string.Empty
                        }).ToList(),
                    AppUsers = p.AppUserInfo
                })
                .ToListAsync();


            return View("Index", posts); // Reuse the Index view with filtered posts
        }

    }

}
