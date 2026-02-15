using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using API.DTOs;
using API.Services;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using API.Extensions;

namespace API.Controllers
{
    public class AccountController(AppDBContext context, ITokenService tokenService) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var userExists = await isExistingUser(registerDTO.Email);
            if(userExists)
            {
                return BadRequest("User already exists");
            }
            var user = await createUser(registerDTO);

            return Ok(user.AsUserDTO(tokenService));
        }
        private async Task<bool> isExistingUser(string email)
        {
            // Check if user exists in the database
            return await context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        private async Task<AppUser> createUser(RegisterDTO registerDTO)
        {
            
            using var hash = new HMACSHA512();

            var user = new AppUser
            {
                DisplayName = registerDTO.DisplayName,
                Email = registerDTO.Email,
                PasswordHash = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hash.Key,
                Member = new Member
                {
                    DisplayName = registerDTO.DisplayName,
                    Gender = registerDTO.Gender,
                    City = registerDTO.City,
                    Country = registerDTO.Country,
                    DateOfBirth = registerDTO.DateOfBirth
                }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            // Placeholder for login logic
            var userdetails = await context.Users
                .SingleOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower());
            

            if(userdetails == null)
            {
                return Unauthorized("Invalid email");
            }

            var passworddetails = userdetails.PasswordHash;
            var passwordsalt = userdetails.PasswordSalt;
            using var hash = new HMACSHA512(passwordsalt);
            var computedHash = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDTO.Password));
            for(int i=0; i < computedHash.Length; i++)
            {
                if(computedHash[i] != passworddetails[i])
                {
                    return Unauthorized("Invalid password");
                }
            }
            return Ok(userdetails.AsUserDTO(tokenService));
        }
    }
}