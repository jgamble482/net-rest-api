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

        public MessageHub(IMessageRepo messageRepo, IMapper mapper, IUserRepo userRepo)
        {
            _messageRepo = messageRepo;
            _mapper = mapper;
            _userRepo = userRepo;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var otherUser = httpContext.Request.Query["user"].ToString();

            var groupName = GetGroupName(Context.User.GetUser(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var messages = await _messageRepo.GetMessageThreadAsync(Context.User.GetUser(), otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);

            await AddToGroup(Context, groupName);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            await RemomveFromMessageGroup(Context.ConnectionId);
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

            if (group.Connections.Any(x => x.Username == recipient.UserName)) message.DateRead = DateTime.UtcNow;

            _messageRepo.AddMessage(message);

            if (await _messageRepo.SaveAllAsync())
            {
                
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDTO>(message));
            }

        }

        private async Task<bool> AddToGroup(HubCallerContext context, string groupName)
        {
            var group = await _messageRepo.GetMessageGroup(groupName);

            var connection = new Connection(context.ConnectionId, context.User.GetUser());

            if(group == null)
            {
                group = new Group(groupName);
                _messageRepo.AddGroup(group);
            }

            group.Connections.Add(connection);

            return await _messageRepo.SaveAllAsync();
        }

        private async Task RemomveFromMessageGroup(string connectionId)
        {
            var connection = await _messageRepo.GetConnection(connectionId);

            _messageRepo.RemoveConnection(connection);

            await _messageRepo.SaveAllAsync();
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            var groupName = stringCompare ? $"{caller}--{other}" : $"{other}--{caller}";
            return groupName;
        }
    }
}
