using api.Entities;
using api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly DataContext _context;
        public UserRepo(DataContext context)
        {
            _context = context;  
        }
        public async Task<List<AppUser>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }


        public async Task<AppUser> GetUser(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
