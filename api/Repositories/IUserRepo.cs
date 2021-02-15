using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Entities;

namespace api.Repositories
{
    public interface IUserRepo
    {
        public Task<List<AppUser>> GetAll();

        public Task<AppUser> GetUser(int id);

        public Task<AppUser> CreateUser(AppUser user);

        public Task<bool> UserExists(string username);
    }
}
