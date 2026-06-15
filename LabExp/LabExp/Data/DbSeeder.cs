using LabExp.Data;
using LabExp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Scientist>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        string[] roles = { "Admin", "Scientist" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        if (!context.Clearances.Any())
        {
            context.Clearances.AddRange(
                new Clearance { LevelName = "Junior Scientist", LevelPriority = 1 },
                new Clearance { LevelName = "Scientist", LevelPriority = 2 },
                new Clearance { LevelName = "Lead Research Scientist", LevelPriority = 10 }
            );

            await context.SaveChangesAsync();
        }

        if (!context.Genders.Any())
        {
            context.Genders.AddRange(
                new Gender { Name = "Male"},
                new Gender { Name = "Female"}
                );

            await context.SaveChangesAsync();
        }

        if (!context.Severities.Any())
        {
            context.Severities.AddRange(
                new Severity { SeverityLevel = 1, SeverityName = "Minimal Risk" },
                new Severity { SeverityLevel = 2, SeverityName = "Low Risk" },
                new Severity { SeverityLevel = 3, SeverityName = "Moderate Risk" },
                new Severity { SeverityLevel = 4, SeverityName = "High Risk" },
                new Severity { SeverityLevel = 5, SeverityName = "Extreme Risk" }
            );

            await context.SaveChangesAsync();
        }

        if (!context.Statuses.Any())
        {
            context.Statuses.AddRange(
                new Status { Name = "Pending" },
                new Status { Name = "Alive" },
                new Status { Name = "Stable" },
                new Status { Name = "Critical" },
                new Status { Name = "Deceased" }
            );

            await context.SaveChangesAsync();
        }

        if (!context.Subjects.Any())
        {
            var male = context.Genders.First(g => g.Name == "Male").GenderId;
            var female = context.Genders.First(g => g.Name == "Female").GenderId;

            var pending = context.Statuses.First(s => s.Name == "Pending").StatusId;
            var alive = context.Statuses.First(s => s.Name == "Alive").StatusId;
            var stable = context.Statuses.First(s => s.Name == "Stable").StatusId;

            context.Subjects.AddRange(
                new Subject
                {
                    Name = "John Carter",
                    Age = 28,
                    GenderId = male,
                    StatusId = alive
                },
                new Subject
                {
                    Name = "Sarah Mitchell",
                    Age = 34,
                    GenderId = female,
                    StatusId = alive
                },
                new Subject
                {
                    Name = "Michael Reed",
                    Age = 22,
                    GenderId = male,
                    StatusId = alive
                },
                new Subject
                {
                    Name = "Simeon Dobrev",
                    Age=21,
                    GenderId=male,
                    StatusId=alive
                },
                new Subject
                {
                    Name = "Aleksandar Giganta",
                    Age=25,
                    GenderId=male,
                    StatusId=alive
                }
            );

            await context.SaveChangesAsync();
        }

        if (!context.Substances.Any())
        {
            var minimal = context.Severities.First(s => s.SeverityName == "Minimal Risk").SeverityId;
            var low = context.Severities.First(s => s.SeverityName == "Low Risk").SeverityId;
            var moderate = context.Severities.First(s => s.SeverityName == "Moderate Risk").SeverityId;
            var high = context.Severities.First(s => s.SeverityName == "High Risk").SeverityId;
            var extreme = context.Severities.First(s => s.SeverityName == "Extreme Risk").SeverityId;

            context.Substances.AddRange(
                new Substance
                {
                    Name = "S-001 Lucid Serum",
                    Description = "Clear liquid causing mild alertness and short-term insomnia.",
                    SeverityId = minimal
                },
                new Substance
                {
                    Name = "S-014 Cinder Mist",
                    Description = "Reactive airborne compound causing visual distortions and elevated heart rate.",
                    SeverityId = low
                },
                new Substance
                {
                    Name = "S-033 Blackveil Extract",
                    Description = "Viscous substance causing memory disruption and behavioral instability.",
                    SeverityId = moderate
                },
                new Substance
                {
                    Name = "S-052 Crimson Haze",
                    Description = "Gaseous compound inducing severe hallucinations and physiological stress responses.",
                    SeverityId = high
                },
                new Substance
                {
                    Name = "S-099 Iridium Bloom",
                    Description = "Highly unstable anomalous substance linked to rapid mutation and extreme mortality risk.",
                    SeverityId = extreme
                }
            );

            await context.SaveChangesAsync();
        }



        var highestClearance = await context.Clearances
            .OrderByDescending(c => c.LevelPriority)
            .FirstOrDefaultAsync();

        var adminEmail = "K@secretcorp.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new Scientist
            {
                UserName = "K",
                Email = adminEmail,
                EmailConfirmed = true,
                ClearanceId = highestClearance!.ClearanceId
            };

            var result = await userManager.CreateAsync(newAdmin, "Admin123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
}