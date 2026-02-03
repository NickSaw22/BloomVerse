using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API.Controllers
{
    public class AccountController(AppDBContext context) : BaseApiController
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
            return Ok(user);
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
                PasswordSalt = hash.Key
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
            return Ok("Login endpoint");

        }
    }
}