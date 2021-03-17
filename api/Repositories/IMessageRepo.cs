using api.DTOs;
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

        Task<PaginatedList<MessageDTO>> GetMessagesForUserAsync();

        Task<IEnumerable<MessageDTO>> GetMessageThreadAsync();

        Task<bool> SaveAllAsync();





    }
}
