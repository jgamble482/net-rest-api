﻿using api.DTOs;
using api.Entities;
using api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Repositories
{
    public interface IMessageRepo
    {
        void AddMessage(Message message);

        void DeleteMessage(Message message);

        Task<Message> GetMessageAsync(int id);

        Task<PaginatedList<MessageDTO>> GetMessagesForUserAsync(MessageParams messageParams);

        Task<IEnumerable<MessageDTO>> GetMessageThreadAsync(string currentUsername, string recipientUsername);

        Task<bool> SaveAllAsync();





    }
}
