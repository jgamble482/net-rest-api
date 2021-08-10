using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepo _messageRepo;
        private readonly IMapper _mapper;
        private readonly IUserRepo _userRepo;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;

        public MessageHub(IMessageRepo messageRepo, IMapper mapper, IUserRepo userRepo, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
        {
            _messageRepo = messageRepo;
            _mapper = mapper;
            _userRepo = userRepo;
            _presenceHub = presenceHub;
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var otherUser = httpContext.Request.Query["user"].ToString();

            var groupName = GetGroupName(Context.User.GetUser(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _messageRepo.GetMessageThreadAsync(Context.User.GetUser(), otherUser);


            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);

            
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

            var group = await RemomveFromMessageGroup();

            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
            var username = Context.User.GetUser();

            if (username == createMessageDTO.RecipientUsername) throw new HubException("You can't send a message to yourself");

            var sender = await _userRepo.GetUserAsync(username);

            var recipient = await _userRepo.GetUserAsync(createMessageDTO.RecipientUsername);

            if (recipient == null) throw new HubException("User does not exist");

            var message = new Message
            {
                Sender = sender,
                SenderUsername = sender.UserName,
                Recipient = recipient,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _messageRepo.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName)) 
                message.DateRead = DateTime.UtcNow;
            else
            {
                var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
                if(connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _messageRepo.AddMessage(message);

            if (await _messageRepo.SaveAllAsync())
            {
                
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDTO>(message));
            }

        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _messageRepo.GetMessageGroup(groupName);

            var connection = new Connection(Context.ConnectionId, Context.User.GetUser());

            if(group == null)
            {
                group = new Group(groupName);
                _messageRepo.AddGroup(group);
            }

            group.Connections.Add(connection);

            if(await _messageRepo.SaveAllAsync()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemomveFromMessageGroup()
        {
            var group = await _messageRepo.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            _messageRepo.RemoveConnection(connection);

            if(await _messageRepo.SaveAllAsync()) return group;

            throw new HubException("Failed to remove from group");
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            var groupName = stringCompare ? $"{caller}--{other}" : $"{other}--{caller}";
            return groupName;
        }
    }
}
