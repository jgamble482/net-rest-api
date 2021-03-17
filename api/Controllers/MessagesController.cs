using api.DTOs;
using api.Helpers;
using api.Extensions;
using api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Entities;
using AutoMapper;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(LogUserActivity))]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepo _messageRepo;
        private readonly IUserRepo _userRepo;
        private readonly IMapper _mapper;

        public MessagesController(IMessageRepo messageRepo, IUserRepo userRepo, IMapper mapper)
        {
            _messageRepo = messageRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUser();

            if (username == createMessageDTO.RecipientUsername) return BadRequest("You can't send a message to yourself");

            var sender = await _userRepo.GetUserAsync(username);

            var recipient = await _userRepo.GetUserAsync(createMessageDTO.RecipientUsername);

            if (recipient == null) return NotFound("User does not exist");

            var message = new Message
            {
                Sender = sender,
                SenderUsername = sender.UserName,
                Recipient = recipient,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };

            _messageRepo.AddMessage(message);

            if (await _messageRepo.SaveAllAsync()) return Ok(_mapper.Map<MessageDTO>(message));

            return BadRequest("Failed to send message");



        }

        [HttpGet]

        public async Task<IActionResult> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUser();

            var messages = await _messageRepo.GetMessagesForUserAsync(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(messages);
        }
    }
}
