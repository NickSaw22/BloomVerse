using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager)
        {
            if(await userManager.Users.AnyAsync()) return;
            var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

            if(members == null)
            {
                Console.WriteLine("No member data found to seed.");
                return;
            }
            foreach (var member in members)
            {
                var user = new AppUser
                {
                    Id = member.Id,
                    DisplayName = member.DisplayName,
                    Email = $"{member.DisplayName.ToLower()}@bloomverse.com",
                    ImageUrl = member.ImageUrl,
                    UserName = $"{member.DisplayName.ToLower()}@bloomverse.com",
                    Member = new Member
                    {

                        Id = member.Id,
                        DateOfBirth = member.DateOfBirth,
                        Description = member.Description,
                        ImageUrl = member.ImageUrl,
                        DisplayName = member.DisplayName,
                        Gender = member.Gender,
                        Created = member.Created,
                        LastActive = member.LastActive,
                        City = member.City,
                        Country = member.Country
                    }
                };
                user.Member.Photos.Add(new Photo
                {
                    Url = member.ImageUrl ?? string.Empty,
                    MemberId = member.Id,
                });
                var result = await userManager.CreateAsync(user, "Pa$$w0rd");
                if (!result.Succeeded)
                {
                    Console.WriteLine($"Failed to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                else
                {
                    Console.WriteLine($"User {user.UserName} created successfully.");
                }
                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser
            {
                UserName = "admin@bloomverse.com",
                Email = "admin@bloomverse.com",
                DisplayName = "Admin"
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator"});
        }
    }
    
}