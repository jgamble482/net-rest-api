using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Entities;
using api.Helpers;

namespace api.Repositories
{
    public interface IUserRepo
    {
        public Task<List<AppUser>> GetAll();

        public Task<AppUser> GetUserAsync(int id);

        public Task<AppUser> GetUserAsync(string username);


        public Task<AppUser> CreateUser(AppUser user);

        public Task<bool> UserExists(string username);

        public void Update(AppUser user);

        public Task<MemberDTO> GetMemberAsync(string username);

        public Task<PaginatedList<MemberDTO>> GetMembersAsync(UserParams userParams);

        
    }
}
