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
using API.Helpers;
namespace API.Controllers
{
    [Authorize]
    public class LikesController(IUnitOfWork _unitOfWork) : BaseApiController
    {
        
        [HttpPost("{targetMemberId}")]
        public async Task<ActionResult> ToggleLike(string targetMemberId)
        {
            var sourceMemberId = User.GetMemberId();
            if (sourceMemberId == targetMemberId) return BadRequest("You cannot like yourself.");

            var like = await _unitOfWork.LikesRepository.GetMemberLike(sourceMemberId, targetMemberId);
            if (like != null) 
            {
                _unitOfWork.LikesRepository.DeleteLike(like);
            }
            else 
            {
                like = new MemberLike
                {
                    SourceMemberId = sourceMemberId,
                    TargetMemberId = targetMemberId
                };

                _unitOfWork.LikesRepository.AddLike(like);
            }
            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to like member.");
        }

        [HttpGet("list")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetCurrentMemberLikeIds()
        {
            return Ok(await _unitOfWork.LikesRepository.GetCurrentMemberLikeIds(User.GetMemberId(), 1, int.MaxValue));
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Member>>> GetMemberLikes([FromQuery] LikeParams likeParams)
        {
            likeParams.MemberId = User.GetMemberId();
            var members = await _unitOfWork.LikesRepository.GetMemberLikes(likeParams);
            return Ok(members);
        }


    }
}