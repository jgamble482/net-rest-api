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
        void AddGroup(Group group);

        void RemoveConnection(Connection connection);

        Task<Connection> GetConnection(string connectionId);

        Task<Group> GetMessageGroup(string groupName);
        void AddMessage(Message message);

        void DeleteMessage(Message message);

        Task<Message> GetMessageAsync(int id);

        Task<PaginatedList<MessageDTO>> GetMessagesForUserAsync(MessageParams messageParams);

        Task<IEnumerable<MessageDTO>> GetMessageThreadAsync(string currentUsername, string recipientUsername);

        Task<bool> SaveAllAsync();





    }
}
