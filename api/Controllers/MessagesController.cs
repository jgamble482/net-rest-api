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
using api.Data;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(LogUserActivity))]
    public class MessagesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUser();

            if (username == createMessageDTO.RecipientUsername) return BadRequest("You can't send a message to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserAsync(username);

            var recipient = await _unitOfWork.UserRepository.GetUserAsync(createMessageDTO.RecipientUsername);

            if (recipient == null) return NotFound("User does not exist");

            var message = new Message
            {
                Sender = sender,
                SenderUsername = sender.UserName,
                Recipient = recipient,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDTO>(message));

            return BadRequest("Failed to send message");



        }

        [HttpGet]

        public async Task<IActionResult> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUser();

            var messages = await _unitOfWork.MessageRepository.GetMessagesForUserAsync(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(messages);
        }


        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteMessage(int id)
        {
            var username = User.GetUser();

            var message = await _unitOfWork.MessageRepository.GetMessageAsync(id);

            if (message.Recipient.UserName != username && message.Sender.UserName != username) return Unauthorized();

            if (message.Sender.UserName == username) message.SenderDeleted = true;

            if (message.Recipient.UserName == username) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted) _unitOfWork.MessageRepository.DeleteMessage(message);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete message");
        }
    }
}
