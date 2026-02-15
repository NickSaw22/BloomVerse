using System;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using API.Helpers;
using System.Collections.Generic;

namespace API.Interfaces
{
    public interface IMemberRepository
    {
        Task<PaginatedResult<Member>> GetMembersAsync(PagingParams pagingParams);
        Task<Member?> GetMemberByIdAsync(string id);
        Task<bool> SaveAllAsync();
        void Update(Member member);
        Task<IReadOnlyList<Photo>> GetPhotosByMemberIdAsync(string memberId);
        Task<Member?> GetMemberForUpdateAsync(string memberId);
    }
}