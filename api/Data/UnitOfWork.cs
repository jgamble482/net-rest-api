using api.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UnitOfWork(DataContext context, IMapper mapper )
        {
            _context = context;
            _mapper = mapper;
        }
        public IMessageRepo MessageRepository => new MessageRepo(_context, _mapper);

        public ILikesRepository LikesRepository => new LikesRepository(_context);

        public IUserRepo UserRepository => new UserRepo(_context, _mapper);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}
