using BookShop.DataAccess.Data;
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


        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(Id);
            IEnumerable<IdentityRole> roles = _roleManager.Roles; // Retrieve all available roles

            UserRoleVM userRoleVM = new UserRoleVM
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };

            ViewBag.Roles = new SelectList(roles, "Name", "Name", userRoleVM.Role);

            return View(userRoleVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserRoleVM userRoleVM)
        {
            ApplicationUser user = await _userManager.FindByIdAsync (userRoleVM.Id);
            user.FirstName = userRoleVM.FirstName;
            user.LastName = userRoleVM.LastName;
            user.Email = userRoleVM.Email;

            await _userManager.UpdateAsync(user);

            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            // Assign the new role to the user
            await _userManager.AddToRoleAsync(user, userRoleVM.Role);


            return RedirectToAction("Index");
        }

    }
}