using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using API.Helpers;
using System.Collections.Generic;
namespace API.Data
{
    public class LikesRepository(AppDBContext _context) : ILikesRepository
    {
        public void AddLike(MemberLike like)
        {
            _context.MemberLikes.Add(like);
        }

        public void DeleteLike(MemberLike like)
        {
            _context.MemberLikes.Remove(like);
        }

        public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
        {
            return await _context.MemberLikes.FindAsync(sourceMemberId, targetMemberId);
        }

        public async Task<PaginatedResult<Member>> GetMemberLikes(LikeParams likeParams)
        {
            var likes = _context.MemberLikes.AsQueryable();
            IQueryable<Member> result;
            switch (likeParams.Predicate)
            {
                case "liked":
                    result = likes.Where(l => l.SourceMemberId == likeParams.MemberId)
                        .Select(l => l.TargetMember);
                    break;
                case "likedBy":
                    result = likes.Where(l => l.TargetMemberId == likeParams.MemberId)
                        .Select(l => l.SourceMember);
                    break;
                default:
                    var likedIds = await GetCurrentMemberLikeIds(likeParams.MemberId, 1, int.MaxValue);
                    result = likes
                        .Where(x => x.TargetMemberId == likeParams.MemberId && likedIds.Contains(x.SourceMemberId))
                        .Select(x => x.SourceMember);
                    break;
            }
            return await PaginatedResult<Member>.PaginationHelper.CreateAsync(result, likeParams.PageNumber, likeParams.PageSize);
        }

        public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId, int pageNumber, int pageSize)
        {
            return await _context.MemberLikes
                .Where(l => l.SourceMemberId == memberId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => l.TargetMemberId)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }   
}