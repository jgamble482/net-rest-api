using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Repositories;
using api.Entities;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using api.DTOs;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        private readonly IMapper _mapper;

        public UserController(IUserRepo userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepo.GetAll();
            var usersToReturn = _mapper.Map<IEnumerable<MemberDTO>>(users);

            return Ok(usersToReturn);
        }


        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
        {

            return Ok(await _userRepo.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(MemberUpdateDTO memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await  _userRepo.GetUser(username);

            _mapper.Map(memberUpdateDto, user);

            _userRepo.Update(user);

            if (await _userRepo.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");

        }

            

    }
}
