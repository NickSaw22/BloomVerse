using System;
using Microsoft.AspNetCore.Mvc;
using API.Entities;

namespace API.Interfaces
{
    public interface IMemberRepository
    {
        Task<IReadOnlyList<Member>> GetMembersAsync();
        Task<Member?> GetMemberByIdAsync(string id);
        Task<bool> SaveAllAsync();
        void Update(Member member);
        Task<IReadOnlyList<Photo>> GetPhotosByMemberIdAsync(string memberId);
    }
}