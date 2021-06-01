using api.Data;
using api.DTOs;
using api.Entities;
using api.Repositories;
using api.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(ITokenService tokenService, IMapper mapper, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO info)
        {
            if (await UserExists(info.Username) == true) return BadRequest("Username already exists");

            var user = _mapper.Map<AppUser>(info);

            user.UserName = info.Username.ToLower();


            var result = await _userManager.CreateAsync(user, info.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);



            return CreatedAtAction(nameof(RegisterUser), new UserDTO {Username = user.UserName, Token = await _tokenService.CreateTokenAsync(user), KnownAs = user.KnownAs, Gender = user.Gender } );

            

            
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<IActionResult> LoginUser([FromBody] LoginDTO login)
        {
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(u => u.UserName == login.Username.ToLower());

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);

            if (!result.Succeeded) return Unauthorized();
                

            if (user == null) return Unauthorized("Invalid Username");


            return Ok(new UserDTO
            {
                Username = user.UserName,
                Token = await _tokenService.CreateTokenAsync(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            });



        }
        [HttpGet]
        public async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(u => u.UserName == username.ToLower());
        }
    }
}
