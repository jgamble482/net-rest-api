using api.DTOs;
using api.Entities;
using api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Repositories
{
    public class MessageRepo : IMessageRepo
    {
        public void AddMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public void DeleteMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<Message> GetMessageAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedList<MessageDTO>> GetMessagesForUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MessageDTO>> GetMessageThreadAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}
