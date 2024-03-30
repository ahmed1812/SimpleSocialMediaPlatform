using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SimpleSocialMediaPlatform.Data;
using SimpleSocialMediaPlatform.Models;

namespace SimpleSocialMediaPlatform.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            //var postVM = new UserPostCommentViewModel();
            //var userInfos = await _context.userInfos.ToListAsync();
            //var rss = await _context.Posts.ToListAsync();
            //List<UserPostCommentViewModel> all = new List<UserPostCommentViewModel>();
            //foreach (var info in userInfos)
            //{
            //    postVM.UserInfoDetails = info;

            //    var allUserPost = await _context.Posts.Where(post => post.UserId == info.UserId).ToListAsync();
            //    postVM.UserPosts = allUserPost;
            //    foreach (var post in allUserPost)
            //    {
            //        //postVM.UserPosts = post;
            //        var allPostComment = await _context.Comments.Where(com => com.PostId == post.Id).ToListAsync();

            //        postVM.UserComments = allPostComment;

            //    }
            //    all.Add(postVM);
            //}

            //return View(all);


            //IEnumerable<UserPostCommentViewModel> userPerPost = (from userInfo in _context.userInfos
            //                                                     from post in _context.Posts
            //                                                     where userInfo.UserId == post.UserId
            //                                                     orderby post.Id
            //                                                     select new UserPostCommentViewModel
            //                                                     {
            //                                                         UserInfoDetails = userInfo,
            //                                                         UserPosts = (_context.Posts.Where(x => x.UserId == userInfo.UserId && x.Id == post.Id)).ToList(),
            //                                                         UserComments = (_context.Comments.Where(x => x.Id == post.Id)).ToList()
            //                                                     }).ToList();
            //return View(userPerPost);

          
                var userPerPost = await (from userInfo in _context.userInfos
                                         join post in _context.Posts on userInfo.UserId equals post.UserId
                                         orderby post.Id
                                         select new UserPostCommentViewModel
                                         {
                                             UserInfoDetails = userInfo,
                                             UserPosts = new List<Post> { post },
                                             UserComments = _context.Comments.Where(comment => comment.PostId == post.Id).ToList()
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
        public async Task<IActionResult> Create([Bind("Id,Titel,Body,CreateAt,UserId")] Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
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
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
