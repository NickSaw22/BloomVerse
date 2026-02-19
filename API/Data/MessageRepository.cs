using System;
using API.Entities;
using API.Interfaces;
using API.Extensions;
using API.Helpers;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace API.Data
{
    public class MessageRepository(AppDBContext context) : IMessageRepository
    {
        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Message?> GetMessage(string id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<PaginatedResult<MessageDto>> GetMessagesForMember(MessageParams messageParams)
        {
            var query = context.Messages
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();
            
            query = messageParams.Container switch
            {
                "Outbox" => query.Where(m => m.SenderId == messageParams.MemberId && !m.SenderDeleted),
                _ => query.Where(m => m.RecipientId == messageParams.MemberId && !m.RecipientDeleted)
            };

            var messageQuery = query.Select(MessageExtensions.ToDtoProjection());

            return await PaginatedResult<MessageDto>.PaginationHelper.CreateAsync(messageQuery, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IReadOnlyList<MessageDto>> GetMessageThread(string currentUserId, string recipientId)
        {
            await context.Messages
                .Where(m => m.RecipientId == currentUserId && m.SenderId == recipientId && m.DateRead == null)
                .ExecuteUpdateAsync(m => m.SetProperty(msg => msg.DateRead, DateTime.UtcNow));
            
            var messages = await context.Messages
                .Where(m => (m.RecipientId == currentUserId && m.SenderId == recipientId && !m.RecipientDeleted) ||
                            (m.RecipientId == recipientId && m.SenderId == currentUserId && !m.SenderDeleted))
                .OrderBy(m => m.MessageSent)
                .Select(MessageExtensions.ToDtoProjection())
                .ToListAsync();
            return messages;
        }

        // public async Task<Group?> GetMessageGroup(string groupName)
        // {
        //     return await context.Groups
        //         .Include(g => g.Connections)
        //         .FirstOrDefaultAsync(g => g.Name == groupName);
        // }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}