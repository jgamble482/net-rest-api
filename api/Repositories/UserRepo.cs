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

        public async Task<AppUser> CreateUser(AppUser user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<AppUser>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }


        public async Task<AppUser> GetUser(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}
