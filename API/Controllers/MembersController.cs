using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.Entities;
using System.Security.Claims;
using API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using API.Extensions;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(IMemberRepository memberRepository, IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
        {
            var users = await memberRepository.GetMembersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMemberById(string id)
        {
            var user = await memberRepository.GetMemberByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            var photos = await memberRepository.GetPhotosByMemberIdAsync(id);
            return Ok(photos);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDTO memberUpdateDTO)
        {
            var memberId = User.GetMemberId();
            if (memberId == null)
            {
                return BadRequest("Member ID is missing");
            }

            var member = await memberRepository.GetMemberForUpdateAsync(memberId);
            if (member == null)
            {
                return NotFound("Could not find member");
            }

            member.DisplayName = memberUpdateDTO.DisplayName ?? member.DisplayName;
            member.Description = memberUpdateDTO.Description ?? member.Description;
            member.City = memberUpdateDTO.City ?? member.City;
            member.Country = memberUpdateDTO.Country ?? member.Country;

            member.User.DisplayName = member.DisplayName;
            memberRepository.Update(member);

            if(await memberRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update member");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<Photo>> AddPhoto([FromForm] PhotoUploadDto uploadDto)
        {
            var memberId = User.GetMemberId();
            if (memberId == null)
            {
                return BadRequest("Member ID is missing");
            }

            if (uploadDto.File == null) return BadRequest("File is missing");

            var member = await memberRepository.GetMemberForUpdateAsync(memberId);
            if (member == null)
            {
                return NotFound("Could not find member");
            }

            var result = await photoService.UploadPhotoAsync(uploadDto.File);
            if (result.Error != null) return BadRequest(result.Error.Message);
            
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                MemberId = memberId
            };

            if(member.ImageUrl == null)
            {
                member.ImageUrl = photo.Url;
                member.User.ImageUrl = photo.Url;
            }
            member.Photos.Add(photo);
            if(await memberRepository.SaveAllAsync()) return photo;
            return BadRequest("Failed to save photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var memberId = User.GetMemberId();
            if (memberId == null)
            {
                return BadRequest("Member ID is missing");
            }

            var member = await memberRepository.GetMemberForUpdateAsync(memberId);
            if (member == null)
            {
                return NotFound("Could not find member");
            }

            var photo = member.Photos.SingleOrDefault(p => p.Id == photoId);
            if (photo == null) return NotFound("Could not find photo");
            if(member.ImageUrl == photo.Url) return BadRequest("This is already your main photo");
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;

            if(await memberRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var memberId = User.GetMemberId();
            if (memberId == null)
            {
                return BadRequest("Member ID is missing");
            }

            var member = await memberRepository.GetMemberForUpdateAsync(memberId);
            if (member == null)
            {
                return NotFound("Could not find member");
            }

            var photo = member.Photos.SingleOrDefault(p => p.Id == photoId);
            if (photo == null) return NotFound("Could not find photo");
            if(member.ImageUrl == photo.Url) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId != null) {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            member.Photos.Remove(photo);
            if(await memberRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to delete photo");
        }
    }
}
