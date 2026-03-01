using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService) : BaseApiController
    {
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var users = await userManager.Users.ToListAsync();
            var userList = new List<object>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userList.Add(new 
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }
            return Ok(userList);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{userId}")]
        public async Task<IActionResult> EditRoles(string userId, [FromQuery] string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var selectedRoles = roles.Split(',').Select(r => r.Trim()).ToArray();
            var userRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Failed to add roles");

            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove roles");

            return Ok(await userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratorPhotoRole")]
        [HttpGet("photo-moderation")]
        public async Task<IActionResult> GetPhotoModeration()
        {
            return Ok(await unitOfWork.PhotoRepository.GetUnapprovedPhotos());
        }

        [Authorize(Policy = "ModeratorPhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if (photo == null) return BadRequest("Photo not found");

            var member = await unitOfWork.MemberRepository.GetMemberForUpdateAsync(photo.MemberId);
            photo.IsApproved = true;

            if(member.ImageUrl == null)
            {
                member.ImageUrl = photo.Url;
                member.User.ImageUrl = photo.Url;
            }

            await unitOfWork.Complete();

            return Ok();
        }

        [Authorize(Policy = "ModeratorPhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if (photo == null) return BadRequest("Photo not found");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Result == "ok")
                {
                    unitOfWork.PhotoRepository.RemovePhoto(photo);
                }
            }
            else
            {
                unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
            await unitOfWork.Complete();
            return Ok();
        }
    }
}