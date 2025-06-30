using BookShoppingCartMvcUI.Constants;
using Microsoft.AspNetCore.Identity;

namespace BookShoppingCartMvcUI.Data
{
    public class DbSeeder
    {
        public static async Task seedDefultData(IServiceProvider service)
        {
            var userMgr = service.GetRequiredService<UserManager<IdentityUser>>();
            var roleMgr = service.GetRequiredService<RoleManager<IdentityRole>>();
            // adding some role to DB
            await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));

            var user = new IdentityUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
            };

            var userInDb = await userMgr.FindByEmailAsync(user.Email);
            if (userInDb is null)
            {
                await userMgr.CreateAsync(user, "Admin@123");
                await userMgr.AddToRoleAsync(user, Roles.Admin.ToString());
            }
        }
    } 
}
