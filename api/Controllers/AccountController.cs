using api.Data;
using api.DTOs;
using api.Entities;
using api.Repositories;
using api.Services;
using AutoMapper;
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
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(IUserRepo userRepo, ITokenService tokenService, IMapper mapper)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO info)
        {
            if (await _userRepo.UserExists(info.Username) == true) return BadRequest("Username already exists");
            using var hmac = new HMACSHA512();

            var user = _mapper.Map<AppUser>(info);

            user.UserName = info.Username;
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(info.Password));
            user.PasswordSalt = hmac.Key;
  

            await _userRepo.CreateUser(user);

            return CreatedAtAction(nameof(RegisterUser), new UserDTO {Username = user.UserName, Token = _tokenService.CreateToken(user), KnownAs = user.KnownAs } );

            

            
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<IActionResult> LoginUser([FromBody] LoginDTO login)
        {
            var user = await _userRepo.GetUserAsync(login.Username);

            if (user == null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return Ok(new UserDTO 
            { 
                Username = user.UserName, 
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            });



        }
    }
}
