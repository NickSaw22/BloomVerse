using System;
using API.Helpers;
using API.Entities;
using API.DTOs;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message?> GetMessage(string id);
        Task<PaginatedResult<MessageDto>> GetMessagesForMember(MessageParams messageParams);
        Task<IReadOnlyList<MessageDto>> GetMessageThread(string currentUserId, string recipientId);
        void AddGroup(Group group);
        Task RemoveConnection(string connectionId);
        Task<Connection?> GetConnection(string connectionId);
        Task<Group?> GetMessageGroup(string groupName);
        Task<Group?> GetGroupForConnection(string connectionId);
    }
}