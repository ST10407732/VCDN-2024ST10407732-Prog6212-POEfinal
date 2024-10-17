using ProgPoePart2_6212.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure the roles exist
        string[] roles = new string[] { "ProgrammeCoordinator", "AcademicManager", "Lecturer" };// "HR",
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create the Programme Coordinator
        var programmeCoordinator = new User
        {
            UserName = "programmecoordinator@cmcs.com",
            Email = "programmecoordinator@cmcs.com",
            FullName = "Programme Coordinator",
            EmailConfirmed = true
        };

        if (await userManager.FindByEmailAsync(programmeCoordinator.Email) == null)
        {
            await userManager.CreateAsync(programmeCoordinator, "Cmcs@123");
            await userManager.AddToRoleAsync(programmeCoordinator, "ProgrammeCoordinator");
        }

        // Create the Academic Manager
        var academicManager = new User
        {
            UserName = "academicmanager@cmcs.com",
            Email = "academicmanager@cmcs.com",
            FullName = "Academic Manager",
            EmailConfirmed = true
        };

        if (await userManager.FindByEmailAsync(academicManager.Email) == null)
        {
            await userManager.CreateAsync(academicManager, "Cmcs@123");
            await userManager.AddToRoleAsync(academicManager, "AcademicManager");
        }

        //// Create the HR
        //var HR = new User
        //{
        //    UserName = "hr@admin.com",
        //    Email = "hr@admin.com",
        //    FullName = "HR",
        //    EmailConfirmed = true
        //};

        //if (await userManager.FindByEmailAsync(HR.Email) == null)
        //{
        //    await userManager.CreateAsync(HR, "Admin@22");
        //    await userManager.AddToRoleAsync(HR, "HR");
        //}
    }
}