using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(AppDBContext context)
        {
            if(await context.Users.AnyAsync()) return;
            var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

            if(members == null)
            {
                Console.WriteLine("No member data found to seed.");
                return;
            }
            using var hmac = new HMACSHA512();
            foreach (var member in members)
            {
                var user = new AppUser
                {
                    Id = member.Id,
                    DisplayName = member.DisplayName,
                    Email = $"{member.DisplayName.ToLower()}@bloomverse.com",
                    ImageUrl = member.ImageUrl,
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")),
                    PasswordSalt = hmac.Key,
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
                context.Users.Add(user);
            }         
            await context.SaveChangesAsync();
        }
    }
    
}