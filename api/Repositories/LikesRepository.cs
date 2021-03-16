using api.Data;
using api.DTOs;
using api.Entities;
using api.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Repositories
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;

        public LikesRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<IEnumerable<LikeDTO>> GetUserLikes(string predicate, int userId)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            switch (predicate)
            {
                case "liked":
                    likes = likes.Where(l => l.SourceUserId == userId);
                    users = likes.Select(like => like.LikedUser);
                    break;
                case "likedBy":
                    likes = likes.Where(l => l.LikedUserId == userId);
                    users = likes.Select(like => like.SourceUser);
                    break;
                default:
                    break;
            }

            return await users.Select(user => new LikeDTO
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(predicate => predicate.IsMain).Url,
                City = user.City,
                Id = user.Id

            }).ToListAsync();

        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(u => u.LikedUsers)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
