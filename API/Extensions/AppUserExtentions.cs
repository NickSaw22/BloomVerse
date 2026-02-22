using API.DTOs;
using System;
using API.Entities;
using API.Interfaces;
namespace API.Extensions
{
    public static class AppUserExtensions
    {
        public static async Task<UserDTO> AsUserDTO(this AppUser user, ITokenService tokenService)
        {
            var token = await tokenService.CreateToken(user);
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                DisplayName = user.DisplayName,
                Token = token
            };
        }
    }
}