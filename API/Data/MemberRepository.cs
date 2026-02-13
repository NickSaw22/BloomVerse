using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MemberRepository: IMemberRepository
    {
        private readonly AppDBContext _context;

        public MemberRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Member?> GetMemberByIdAsync(string id)
        {
            return await _context.Members.FindAsync(id);
        }

        public async Task<IReadOnlyList<Member>> GetMembersAsync()
        {
            return await _context.Members
                            .Include(x=> x.Photos)
                            .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IReadOnlyList<Photo>> GetPhotosByMemberIdAsync(string memberId)
        {
            return await _context.Members.Where(m => m.Id == memberId)
                                         .SelectMany(m => m.Photos)
                                         .ToListAsync(); 
            //await _context.Photos.Where(p => p.MemberId == memberId).ToListAsync();
        }

        public void Update(Member member)
        {
            _context.Entry(member).State = EntityState.Modified;
        }

        public async Task<Member?> GetMemberForUpdateAsync(string memberId)
        {
            return await _context.Members
                                 .Include(m => m.User)
                                 .Include(m => m.Photos)
                                 .SingleOrDefaultAsync(m => m.Id == memberId);
        }

        public async Task AddMemberAsync(Member member)
        {
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
        }
    }
}