using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using API.Extensions;
using API.Interfaces;
using API.DTOs;
using API.Entities;

namespace API.SignalR
{

    [Authorize]
    public class MessageHub(IUnitOfWork _unitOfWork, IHubContext<PresenceHub> presenceHub) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request.Query["userId"].ToString()
             ?? throw new InvalidOperationException("User ID not found in query string.");

            var groupName = GetGroupName(GetUserId(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await AddToGroup(groupName);

            var messages = await _unitOfWork.MessageRepository.GetMessageThread(GetUserId(), otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var sender = await _unitOfWork.MemberRepository.GetMemberByIdAsync(GetUserId());
            var recipient = await _unitOfWork.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);
            if (recipient == null || sender == null || sender.Id == recipient.Id)
                throw new HubException("Cannot send this message.");

            var message = new Message
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.Id, recipient.Id);
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var userInGroup = group != null && group.Connections.Any(x=>x.UserId == message.RecipientId);

            if(userInGroup)
            {
                message.DateRead = DateTime.UtcNow;
            }

            _unitOfWork.MessageRepository.AddMessage(message);
            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.Id);
                if (connections != null && connections.Count > 0 && !userInGroup)
                {
                    await presenceHub.Clients.Clients(connections)
                    .SendAsync("NewMessageReceived", message.ToDto());
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _unitOfWork.MessageRepository.RemoveConnection(Context.ConnectionId);
            await _unitOfWork.Complete();
            await base.OnDisconnectedAsync(exception);
        }

        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, GetUserId());

            if(group == null)
            {
                group = new Group(groupName) { Name = groupName };
                _unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            return await _unitOfWork.Complete();
        }
        private string GetGroupName(string? caller, string? other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        private string GetUserId()
        {
            return Context.User?.GetMemberId()
                ?? throw new InvalidOperationException("User ID not found in claims.");
        }
    }
}