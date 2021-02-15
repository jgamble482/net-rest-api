using api.Data;
using api.Entities;
using api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IUserRepo _userRepo;

        public AccountController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpPost("register")]
        
        public async Task<IActionResult> RegisterUser(string userName, string password)
        {
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = userName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                PasswordSalt = hmac.Key

            };

            return CreatedAtAction("register", await _userRepo.CreateUser(user));

            

            
        }
    }
}
