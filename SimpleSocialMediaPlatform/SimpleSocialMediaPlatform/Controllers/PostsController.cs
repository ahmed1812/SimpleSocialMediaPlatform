using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SimpleSocialMediaPlatform.Data;
using SimpleSocialMediaPlatform.Models;

namespace SimpleSocialMediaPlatform.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager; // Use ApplicationUser if you have a custom user class


        public PostsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var UserId = _userManager.GetUserId(User);
            ViewData["UserID"] = UserId;
            ViewData["UserID"] = _userManager.GetUserId(this.User);
            var userId = _userManager.GetUserId(User); // Get the current user's ID
            var userP = await _userManager.FindByIdAsync(userId); // Find the user

            // Assuming the profile picture is stored as a byte array in the user entity
            var profilePictureBase64 = userP.ProfilePicture != null ? Convert.ToBase64String(userP.ProfilePicture) : string.Empty;

            // Pass the profile picture to the view using ViewData (you could also add it to your model)
            ViewData["ProfilePictureBase64"] = profilePictureBase64;

            ApplicationUser userA = await _userManager.GetUserAsync(User);

            if (userA != null)
            {
                // Assuming you want the username
                ViewData["UserName"] = userA.UserName;

                // If you have a full name property and prefer to use it if available
                ViewData["UserFullName"] = string.IsNullOrWhiteSpace(userA.FullName) ? userA.UserName : userA.FullName;
            }
            else
            {
                ViewData["UserName"] = "Guest";
                ViewData["UserFullName"] = "Guest";
            }


            var userPerPost = await (from user in _context.Users
                                     join post in _context.Posts on user.Id equals post.UserId
                                     orderby post.CreateAt descending
                                     select new UserPostCommentViewModel
                                     {
                                         UserInfoDetails = new UserInfo
                                         {
                                             UserId = user.Id,
                                             FullName = user.UserName,
                                             UserPostImage = user.ProfilePicture != null ? Convert.ToBase64String(user.ProfilePicture) : string.Empty
                                         },
                                         UserPosts = new List<Post> { post },
                                         UserComments = _context.Comments
                                             .Include(c => c.User)
                                             .Where(comment => comment.PostId == post.Id)
                                             .Select(c => new Comments
                                             {
                                                 Body = c.Body,
                                                 UserName = c.User.FullName,
                                                 ImageName = c.ImageName,
                                                 UserProfilePicture = c.User.ProfilePicture != null ? Convert.ToBase64String(c.User.ProfilePicture) : string.Empty
                                             }).ToList(),
                                         AppUsers = user
                                     }).ToListAsync();





            return View(userPerPost);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.userInfos.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    if (post.PostImageFile != null && post.PostImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(post.PostImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await post.PostImageFile.CopyToAsync(fileStream);
                        }

                        // Delete the old file if it exists and is different
                        if (!string.IsNullOrEmpty(existingUser.UserPostImage) && existingUser.UserPostImage != uniqueFileName)
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, existingUser.UserPostImage);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Update the database entry
                        post.PostImageName = uniqueFileName;
                        post.PostImageUrl = "/Images/" + uniqueFileName; // Update to store relative path
                    }
                    else
                    {
                        // Keep the old image if no new image was uploaded
                        post.PostImageName = existingUser.UserPostImage;
                        post.PostImageUrl = existingUser.ImageUrl;
                    }

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        // Log the exception
                        throw;
                    }
                }
                return RedirectToAction("Index", "Posts");
            }
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts' is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // If you intend to delete the post only if it does NOT have an associated image
            // Check if the ImageName is null or empty and then decide to delete or not
            if (string.IsNullOrEmpty(post.PostImageName))
            {
                // If no image is associated, then delete the post
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                // Optionally, add logic here if you want to redirect to a different action 
                // when a post without an image is deleted
            }
            else
            {
                // If an image is associated, delete both the post and its image from the filesystem
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", post.PostImageName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
