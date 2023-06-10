﻿using BookShop.DataAccess.Data;
using BookShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ManageUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ManageUserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

        }
        public IActionResult Index()
        {
            IEnumerable<ApplicationUser> appUserList = _context.Users.ToList();
            return View(appUserList);
        }


        [HttpPost]
        public IActionResult Delete(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user != null)
            {
                var result = _userManager.DeleteAsync(user).Result;
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "User deletion failed.");
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found.");
            }
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(string Id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(Id);
            return View(user);
        }


    }
}