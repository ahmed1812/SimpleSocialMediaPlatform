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
    //[Authorize]
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

            var userPerPost = await (from userInfo in _context.userInfos
                                     join post in _context.Posts on userInfo.UserId equals post.UserId
                                     orderby post.Id
                                     select new UserPostCommentViewModel
                                     {
                                         UserInfoDetails = userInfo,
                                         UserPosts = new List<Post> { post },
                                         UserComments = _context.Comments.Where(comment => comment.PostId == post.Id).ToList(),
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
                try
                {
                    if (post.ImageFile != null && post.ImageFile.Length > 0)
                    {
                        // Constructing a server-relative path
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

                        // Ensure a unique file name
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(post.ImageFile.FileName);

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Ensuring the directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Saving the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await post.ImageFile.CopyToAsync(fileStream);
                        }

                        // Update your model as necessary
                        post.ImageName = uniqueFileName;
                        post.ImageUrl = "/Images/" + uniqueFileName; // Relative path

                        _context.Add(post);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // Handle the case when no image is uploaded
                        ModelState.AddModelError("ImageFile", "Please upload an image file.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it appropriately
                    ModelState.AddModelError("", "An error occurred while saving the image.");
                }
            }
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Body,CreateAt,UserId")] Post post)
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

                    if (post.ImageFile != null && post.ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(post.ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await post.ImageFile.CopyToAsync(fileStream);
                        }

                        // Delete the old file if it exists and is different
                        if (!string.IsNullOrEmpty(existingUser.ImageName) && existingUser.ImageName != uniqueFileName)
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, existingUser.ImageName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Update the database entry
                        post.ImageName = uniqueFileName;
                        post.ImageUrl = "/Images/" + uniqueFileName; // Update to store relative path
                    }
                    else
                    {
                        // Keep the old image if no new image was uploaded
                        post.ImageName = existingUser.ImageName;
                        post.ImageUrl = existingUser.ImageUrl;
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
                return RedirectToAction(nameof(Index));
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
                return Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", post.ImageName);
            //string uniqueFileName = imageClass.ImageFile.FileName;
            //string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
