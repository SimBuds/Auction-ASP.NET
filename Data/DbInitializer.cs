using Microsoft.AspNetCore.Identity;
using web_project.Models;

namespace web_project.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Create roles if they don't exist
            string[] roleNames = { "Admin", "Buyer", "Seller" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create an admin user and assign the "Admin" role
            string adminEmail = "admin@test.com";
            string adminPassword = "123Password1$";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
            };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (createResult.Succeeded)
                {

                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            string buyerEmail = "buyer@test.com";
            string buyerPassword = "123Password1$";
            var buyerUser = await userManager.FindByEmailAsync(buyerEmail);

            if (buyerUser == null)
            {
                buyerUser = new User
                {
                    UserName = buyerEmail,
                    Email = buyerEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(buyerUser, buyerPassword);

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(buyerUser, "Buyer");
                }
            }

            string sellerEmail = "seller@test.com";
            string sellerPassword = "123Password1$";
            var sellerUser = await userManager.FindByEmailAsync(sellerEmail);

            if (sellerUser == null)
            {
                sellerUser = new User
                {
                    UserName = sellerEmail,
                    Email = sellerEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(sellerUser, sellerPassword);

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(sellerUser, "Seller");
                }
            }
        }

    }
}