﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMediaPlatform.Models;
using SimpleSocialMediaPlatform.ViewModels;

namespace SimpleSocialMediaPlatform.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Add this line


        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager) // Add this parameter
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager; // Assign it here
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            public string FullName { get; set; }
            public string Address { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string Category { get; set; }
            [Display(Name = "Profile Picture")]
            public byte[] ProfilePicture { get; set; }
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            
            public DateTime DOB { get; set; }
            public DateTime CreateAt { get; set; }
        }
        public List<IdentityRole> Roles { get; set; } // Add this property

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            // Load roles from the database
            Roles = await _roleManager.Roles
                              .Where(r => r.Name != "Admin" && r.Name != "User") // Exclude "Admin" role
                              .ToListAsync();

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FullName = user.FullName,
                Address = user.Address,
                Address2 = user.Address2,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                ProfilePicture = user.ProfilePicture,
                DOB = user.DOB == DateTime.MinValue ? DateTime.Today : user.DOB,
                CreateAt = user.CreateAt,
                Category = user.Category
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var roles = await _roleManager.Roles.ToListAsync(); // Assuming _roleManager is injected

            var viewModel = new RoleFormViewModel
            {
                Roles = roles
            };

            await LoadAsync(user);
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            // Update PhoneNumber if changed
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            // Updating other properties
            user.FullName = Input.FullName ?? user.FullName;
            user.Address = Input.Address ?? user.Address;
            user.Address2 = Input.Address2 ?? user.Address2;
            user.City = Input.City ?? user.City;
            user.State = Input.State ?? user.State;
            user.ZipCode = Input.ZipCode ?? user.ZipCode;
            user.DOB = Input.DOB != default ? Input.DOB : user.DOB;
            user.Category = Input.Category ?? user.Category;
            // Assuming CreateAt is not meant to be updated by user form submission
            // If it is, handle similarly to DOB
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files.FirstOrDefault();

                //check file size and extension

                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    user.ProfilePicture = dataStream.ToArray();
                }

                await _userManager.UpdateAsync(user);
            }
            // Save the updates to the user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "An unexpected error occurred while updating your profile.";
                return RedirectToPage();
            }
            // Check the age to be at least 2 years
            if (DateTime.Today < Input.DOB.AddYears(2))
            {
                ModelState.AddModelError("Input.DOB", "You must be at least 15 years old.");
                await LoadAsync(user);
                return Page();
            }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

    }
}
