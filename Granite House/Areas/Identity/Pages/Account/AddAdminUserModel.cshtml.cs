using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasarsoft.GraniteHouse.Models;
using Jasarsoft.GraniteHouse.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Jasarsoft.GraniteHouse.Areas.Identity.Pages.Account
{
    public class AddAdminUserModelModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AddAdminUserModelModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            //create roles and super admin
            if (!await _roleManager.RoleExistsAsync(SD.AdminEndUser))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.AdminEndUser));
            }
            if (!await _roleManager.RoleExistsAsync(SD.SuperAdminEndUser))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.SuperAdminEndUser));

                var userAdmin = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    PhoneNumber = "12345678",
                    Name = "Admin Ben Spark"
                };

                var resultUser = await _userManager.CreateAsync(userAdmin, "Admin123!");
                await _userManager.AddToRoleAsync(userAdmin, SD.AdminEndUser);
            }

            return Page();
        }
    }
}
