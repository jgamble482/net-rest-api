using api.Data;
using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Helpers;
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

        public async Task<PaginatedList<LikeDTO>> GetUserLikes(LikeParams likeParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            switch (likeParams.Predicate)
            {
                case "liked":
                    likes = likes.Where(l => l.SourceUserId == likeParams.UserId);
                    users = likes.Select(like => like.LikedUser);
                    break;
                case "likedBy":
                    likes = likes.Where(l => l.LikedUserId == likeParams.UserId);
                    users = likes.Select(like => like.SourceUser);
                    break;
                default:
                    break;
            }

            var likeDtos = users.Select(user => new LikeDTO
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(predicate => predicate.IsMain).Url,
                City = user.City,
                Id = user.Id

            });

                return await PaginatedList<LikeDTO>.CreateAsync(likeDtos, likeParams.PageNumber, likeParams.PageSize);

        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(u => u.LikedUsers)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
