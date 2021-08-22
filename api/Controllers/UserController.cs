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
using api.Extensions;
using api.Services;
using api.Helpers;
using api.Data;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogUserActivity))]
    
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UserController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
            
        }

        [HttpGet]
        [Authorize (Roles = "Admin , Member")]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUser());

            userParams.CurrentUsername = User.GetUser();

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }


        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
        {

            return Ok(await _unitOfWork.UserRepository.GetMemberAsync(username));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(MemberUpdateDTO memberUpdateDto)
        {
            

            var user = await  _unitOfWork.UserRepository.GetUserAsync(User.GetUser());

            _mapper.Map(memberUpdateDto, user);

            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");

        }

        [HttpPost("add-photo")]

        public async Task<IActionResult> AddPhoto(IFormFile file)
        {
            var user = await _unitOfWork.UserRepository.GetUserAsync(User.GetUser());

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _unitOfWork.Complete()) return CreatedAtAction(nameof(this.AddPhoto), _mapper.Map<PhotoDTO>(photo));

            return BadRequest("There was an issue uploading a photo");


        }

        [HttpPut("set-main-photo/{photoId}")]

        public async Task<IActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserAsync(User.GetUser());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentMain != null)
            {
                currentMain.IsMain = false;
            }

            photo.IsMain = true;
            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserAsync(User.GetUser());

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return NotFound("Photo does not exist");

            if (photo.IsMain) return BadRequest("You can't delete your main photo");

            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("There was a problem deleting the photo");
        }


            

    }
}
