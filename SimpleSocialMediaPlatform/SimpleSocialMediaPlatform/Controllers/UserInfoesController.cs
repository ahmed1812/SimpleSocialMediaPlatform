using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMediaPlatform.Data;
using SimpleSocialMediaPlatform.Models;

namespace SimpleSocialMediaPlatform.Controllers
{
    public class UserInfoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserInfoesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: UserInfoes
        public async Task<IActionResult> Index()
        {
              return _context.userInfos != null ? 
                          View(await _context.userInfos.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.userInfos'  is null.");
        }

        // GET: UserInfoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.userInfos == null)
            {
                return NotFound();
            }

            var userInfo = await _context.userInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userInfo == null)
            {
                return NotFound();
            }

            return View(userInfo);
        }

        // GET: UserInfoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (userInfo.ImageFile != null && userInfo.ImageFile.Length > 0)
                    {
                        // Constructing a server-relative path
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

                        // Ensure a unique file name
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(userInfo.ImageFile.FileName);

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Ensuring the directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Saving the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await userInfo.ImageFile.CopyToAsync(fileStream);
                        }

                        // Update your model as necessary
                        userInfo.ImageName = uniqueFileName;
                        userInfo.ImageUrl = "/Images/" + uniqueFileName; // Relative path

                        _context.Add(userInfo);
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
            return View(userInfo);
        }


        // GET: UserInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.userInfos == null)
            {
                return NotFound();
            }

            var userInfo = await _context.userInfos.FindAsync(id);
            if (userInfo == null)
            {
                return NotFound();
            }
            return View(userInfo);
        }

        // POST: UserInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,UserId,Phone,Address,Address2,City,State,ZipCode,ImageName,ImageUrl,DOB,CreateAt")] UserInfo userInfo)
        {
            if (id != userInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    _context.Update(userInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserInfoExists(userInfo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userInfo);
        }

        // GET: UserInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.userInfos == null)
            {
                return NotFound();
            }

            var userInfo = await _context.userInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userInfo == null)
            {
                return NotFound();
            }

            return View(userInfo);
        }

        // POST: UserInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.userInfos == null)
            {
                return Problem("Entity set 'ApplicationDbContext.userInfos'  is null.");
            }
            var userInfo = await _context.userInfos.FindAsync(id);
            if (userInfo != null)
            {
                _context.userInfos.Remove(userInfo);
            }

            string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", userInfo.ImageName);
            //string uniqueFileName = imageClass.ImageFile.FileName;
            //string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserInfoExists(int id)
        {
          return (_context.userInfos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
