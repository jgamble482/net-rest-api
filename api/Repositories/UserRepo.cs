using api.Entities;
using api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.DTOs;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using api.Helpers;

namespace api.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepo(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<AppUser> CreateUser(AppUser user)
        {
            _context.Users.Add(user);
            return Task.FromResult(user);
        }

        public async Task<List<AppUser>> GetAll()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public async Task<MemberDTO> GetMemberAsync(string username)
        {
            return await _context.Users
                .Where(u => u.UserName == username)
                .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PaginatedList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users
                .AsQueryable();

            query = query.Where(users => users.UserName != userParams.CurrentUsername);
            query = query.Where(users => users.Gender == userParams.Gender);

            //Setting Date of Birth parameters
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            //Filtering query by age range
            query = query.Where(users => users.DateOfBirth >= minDob && users.DateOfBirth <= maxDob);

            //Ordering the results with a switch statement
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(users => users.Created),

                _ => query.OrderByDescending(users => users.LastActive)
            };
              

            return await PaginatedList<MemberDTO>.CreateAsync(query.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser> GetUserAsync(int id)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<AppUser> GetUserAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users.Where(u => u.UserName == username).Select(u => u.Gender).FirstOrDefaultAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == username.ToLower()))
                return true;

            return false;
        }
    }
}
