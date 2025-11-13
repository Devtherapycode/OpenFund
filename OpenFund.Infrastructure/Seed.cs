using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenFund.Infrastructure.Context;

namespace OpenFund.Infrastructure;

public class Seed
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var serviceScope = serviceProvider.CreateScope();
        var serviceProvier = serviceScope.ServiceProvider;

        var userManager = serviceProvier.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvier.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
        
        var developmentInitAdmin = new IdentityUser()
        {
            UserName = "Admin",
            Email = "admin@openfund.io",
            EmailConfirmed = true,
        };

        var developmentAdminRole = new IdentityRole()
        {
            Name = "Admin",
            Id = Guid.NewGuid().ToString()
        };

        
        var user = await userManager.FindByNameAsync(developmentInitAdmin.UserName);
        if (user == null)
            await userManager.CreateAsync(developmentInitAdmin);

        var role = await roleManager.FindByNameAsync(developmentAdminRole.Name);
        if (role == null)
            await roleManager.CreateAsync(developmentAdminRole);

        var createdUser = await userManager.FindByNameAsync(developmentInitAdmin.UserName);
        var userHasRole = await userManager.IsInRoleAsync(createdUser!, developmentAdminRole.Name);

        if (!userHasRole)
            await userManager.AddToRoleAsync(createdUser!, developmentAdminRole.Name);
    }
}