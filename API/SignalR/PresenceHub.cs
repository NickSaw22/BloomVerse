using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using API.Extensions;
using API.Services;
using API.Entities;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub(PresenceTracker presenceTracker) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await presenceTracker.UserConnected(GetUserId(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserOnline", GetUserId());

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await presenceTracker.UserDisconnected(GetUserId(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserOffline", GetUserId());
            
            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        

            await base.OnDisconnectedAsync(exception);
        }

        private string GetUserId()
        {
            return Context.User?.GetMemberId()
                ?? throw new InvalidOperationException("User ID not found in claims.");
        }
    }
}