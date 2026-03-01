using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using API.DTOs;
using API.Services;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using API.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using API.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace API.Controllers
{
    public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {            
            var user = new AppUser
            {
                DisplayName = registerDTO.DisplayName,
                Email = registerDTO.Email,
                UserName = registerDTO.Email,
                Member = new Member
                {
                    DisplayName = registerDTO.DisplayName,
                    Gender = registerDTO.Gender,
                    City = registerDTO.City,
                    Country = registerDTO.Country,
                    DateOfBirth = registerDTO.DateOfBirth
                }
            };

            var result = await userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Identity", error.Description);
                }
                return ValidationProblem();
            }
            await userManager.AddToRoleAsync(user, "Member");
            await SetRefreshToken(user);
            return Ok(await user.AsUserDTO(tokenService));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            // Placeholder for login logic
            var userdetails = await userManager.FindByEmailAsync(loginDTO.Email);
            if (userdetails == null)
            {
                return Unauthorized("Invalid email");
            }
            var result = await userManager.CheckPasswordAsync(userdetails, loginDTO.Password);
            if (!result) return Unauthorized("Invalid password");
            await SetRefreshToken(userdetails);
            return Ok(await userdetails.AsUserDTO(tokenService));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<UserDTO>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return NoContent();

            var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken
                && u.RefreshTokenExpiry > DateTime.Now
            );
            if (user == null) return Unauthorized("Invalid or expired refresh token");
            
            await SetRefreshToken(user);
            return Ok(await user.AsUserDTO(tokenService));
        }
        private async Task SetRefreshToken(AppUser user)
        {
            var refreshToken = tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
            await userManager.UpdateAsync(user);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = user.RefreshTokenExpiry,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await userManager.Users
                .Where(u => u.Id == User.GetMemberId())
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.RefreshToken, _ => null)
                    .SetProperty(p => p.RefreshTokenExpiry, _ => null)
                    );
            Response.Cookies.Delete("refreshToken");
            return Ok();
        }
    }
}