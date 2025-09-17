using ChefSmart_Api.DAL.Data;
using ChefSmart_Api.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefSmart_Api.DAL.Utils
{
    public class SeedData:ISeedData
    {

        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public SeedData(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task DataSeeding()
        {
            if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                await _context.Database.MigrateAsync();
            }
          
            await _context.SaveChangesAsync();
        }




        public async Task IdentityDataSeeding()
        {
            if (!await _roleManager.Roles.AnyAsync())
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
                await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
            }
            if (!await _userManager.Users.AnyAsync())
            {
                var user1 = new ApplicationUser()
                {
                    Email = "boshrasami@gmail.com",
                    UserName = "Boshrasami",
                    EmailConfirmed = true
                };
                var user2 = new ApplicationUser()
                {
                    Email = "Ahmad@gmail.com",
                    UserName = "ANofal1",
                    EmailConfirmed = true

                };
                var user3 = new ApplicationUser()
                {
                    Email = "Ali@gmail.com",
                    UserName = "ANofal",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(user1, "Pass@12345");
                await _userManager.CreateAsync(user2, "Pass@12345");
                await _userManager.CreateAsync(user3, "Pass@12345");

                await _userManager.AddToRoleAsync(user1, "Admin");
                await _userManager.AddToRoleAsync(user2, "SuperAdmin");
                await _userManager.AddToRoleAsync(user3, "Customer");


            }
            await _context.SaveChangesAsync();
        }
    }
}
