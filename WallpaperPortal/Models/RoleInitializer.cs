using Microsoft.AspNetCore.Identity;

namespace WallpaperPortal.Models
{
    public static class RoleInitializer
    {
        public static async Task CreateRoles(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<User>>();
            string[] roleNames = { "Admin", "Moderator", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var _user = await UserManager.FindByEmailAsync(configuration["AppSettings:UserEmail"]);

            if (_user == null)
            {
                var poweruser = new User
                {
                    UserName = configuration["AppSettings:UserName"],
                    Email = configuration["AppSettings:UserEmail"],
                };

                string userPWD = configuration["AppSettings:UserPassword"];

                var createPowerUser = await UserManager.CreateAsync(poweruser, userPWD);
                if (createPowerUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(poweruser, "Admin");

                }
            }
        }

    }
}
