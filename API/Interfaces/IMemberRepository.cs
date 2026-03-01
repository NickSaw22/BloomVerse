using System;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using API.Helpers;
using System.Collections.Generic;

namespace API.Interfaces
{
    public interface IMemberRepository
    {
        Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams);
        Task<Member?> GetMemberByIdAsync(string id);
        void Update(Member member);
        Task<IReadOnlyList<Photo>> GetPhotosByMemberIdAsync(string memberId, bool isCurrentUser);
        Task<Member?> GetMemberForUpdateAsync(string memberId);
    }
}