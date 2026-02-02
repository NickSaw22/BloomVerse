using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController(AppDBContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMembers()
        {
            var users = await context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberById(string id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMember([FromBody] API.Entities.AppUser user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMembers), new { id = user.Id }, user);
        }
    }
}
