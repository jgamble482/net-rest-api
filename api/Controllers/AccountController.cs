using api.Data;
using api.DTOs;
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
        
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO info)
        {
            if (await _userRepo.UserExists(info.Username) == true) return BadRequest("Username already exists");
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = info.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(info.Password)),
                PasswordSalt = hmac.Key

            };

            return CreatedAtAction(nameof(RegisterUser), await _userRepo.CreateUser(user));

            

            
        }
    }
}
