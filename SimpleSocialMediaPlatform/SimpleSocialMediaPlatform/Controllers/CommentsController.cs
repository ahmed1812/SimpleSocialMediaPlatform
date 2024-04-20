using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SimpleSocialMediaPlatform.Data;
using SimpleSocialMediaPlatform.Models;

namespace SimpleSocialMediaPlatform.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public CommentsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _userManager = userManager;
            _logger = logger;
        }


        // GET: Comments
        public async Task<IActionResult> Index()
        {

            return _context.Comments != null ?
                          View(await _context.Comments.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Comments'  is null.");
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");  // Redirect to login if not authenticated
            }

            var userId = _userManager.GetUserId(User);
            ViewData["UserID"] = userId;

            

            return View();
        }



        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comments comments)
        {
            comments.CreateAt = DateTime.Now;
            
                try
                {
                    if (comments.ImageFile != null && comments.ImageFile.Length > 0)
                    {
                        // Constructing a server-relative path
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

                        // Ensure a unique file name
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(comments.ImageFile.FileName);

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Ensuring the directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Saving the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await comments.ImageFile.CopyToAsync(fileStream);
                        }

                        // Update your model as necessary
                        comments.ImageName = uniqueFileName;
                        comments.ImageUrl = "/Images/" + uniqueFileName; // Relative path

                        _context.Add(comments);
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
            
            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCommentForPost(Comments comments)
        {
            comments.CreateAt = DateTime.Now; // Set the creation time to now

            try
            {
                if (comments.ImageFile != null && comments.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(comments.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await comments.ImageFile.CopyToAsync(fileStream);
                    }
                    comments.ImageName = uniqueFileName;
                    comments.ImageUrl = "/Images/" + uniqueFileName;
                }

                // Add the comment to the database context and save changes
                _context.Add(comments);
                await _context.SaveChangesAsync();

                // Retrieve user's full name using the UserId
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == comments.UserId);
                var userFullName = user?.FullName ?? "Anonymous";

                return Json(new { success = true, message = "Comment added successfully!", commentBody = comments.Body, userName = userFullName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment.");
                return Json(new { success = false, message = "An error occurred while saving the comment." });
                //return Json(new
                //{
                //    success = true,
                //    message = "Comment added successfully!",
                //    commentBody = comments.Body,
                //    userName = comments.User.FullName,
                //    imageUrl = comments.ImageUrl // Ensure this is included
                //});

            }
        }





        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments.FindAsync(id);
            if (comments == null)
            {
                return NotFound();
            }
            return View(comments);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Comments comments)
        {
            if (id != comments.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingComment = await _context.Comments.FindAsync(id);
                    if (existingComment == null)
                    {
                        return NotFound();
                    }

                    // Update existing comment properties
                    existingComment.Body = comments.Body;
                    existingComment.UserId = comments.UserId;
                    existingComment.PostId = comments.PostId;
                    existingComment.CreateAt = comments.CreateAt;

                    if (comments.ImageFile != null && comments.ImageFile.Length > 0)
                    {
                        // Handle image upload
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(comments.ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save the new image file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await comments.ImageFile.CopyToAsync(fileStream);
                        }

                        // Delete the old image file
                        if (!string.IsNullOrEmpty(existingComment.ImageName))
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, existingComment.ImageName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Update comment's image properties
                        existingComment.ImageName = uniqueFileName;
                        existingComment.ImageUrl = "/Images/" + uniqueFileName;
                    }

                    _context.Update(existingComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentsExists(comments.Id))
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
            return View(comments);
        }


        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Comments' is null.");
            }

            // Find the comment by id
            var comments = await _context.Comments.FindAsync(id);
            if (comments == null)
            {
                return NotFound();
            }

            // Delete the comment
            _context.Comments.Remove(comments);

            // Delete associated image file
            if (!string.IsNullOrEmpty(comments.ImageName))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", comments.ImageName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool CommentsExists(int id)
        {
          return (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
