using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace api.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _tracker.UserConnected(Context.User.GetUser(), Context.ConnectionId);
            if(isOnline)
             await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUser());

            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconected(Context.User.GetUser(), Context.ConnectionId);

            if(isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUser());


            await base.OnDisconnectedAsync(exception);
        }
    }
}
