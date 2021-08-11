using api.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Data
{
    public interface IUnitOfWork
    {
        public IMessageRepo MessageRepository { get;  }

        public ILikesRepository LikesRepository { get;  }

        public IUserRepo UserRepository { get;  }

        Task<bool> Complete();

        bool HasChanges();


    }
}
