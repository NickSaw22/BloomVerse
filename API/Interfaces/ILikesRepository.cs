using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<MemberLike> GetMemberLike(string sourceMemberId, string targetMemberId);
        Task<PaginatedResult<Member>> GetMemberLikes(LikeParams likeParams);
        Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId, int pageNumber, int pageSize);
        void DeleteLike(MemberLike like);
        void AddLike(MemberLike like);
        Task<bool> SaveAllAsync();
    }
}