using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameGeekLike.Controllers
{
    [ApiController]
    [Route("users/[action]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            this._usersService = usersService;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignUp(UsersSignUpRequest? request)
        {
            var (content, message) = await this._usersService.SignUp(request, "User");

            var response = new Response<UsersSignUpResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignIn(UsersSignInRequest? request)
        {
            var (content, message) = await this._usersService.SignIn(request);

            var response = new Response<UsersSignInResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpPut]
        public async Task<IActionResult> EditProfile(UsersEditProfileRequest? request)
        {
            var (content, message) = await this._usersService.EditProfile(request);

            var response = new Response<UsersEditProfileResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProfile(UsersDeleteProfileRequest? request)
        {
            var (content, message) = await this._usersService.DeleteProfile(request);

            var response = new Response<UsersDeleteProfileResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Rate(UsersRateRequest? request)
        {
            var (content, message) = await this._usersService.Rate(request);

            var response = new Response<UsersRateResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "User")]
        [HttpPut]
        public async Task<IActionResult> EditRating(UsersEditRatingRequest? request)
        {
            var (content, message) = await this._usersService.EditRating(request);

            var response = new Response<UsersEditRatingResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> LogSession(UsersLogSessionRequest? request)
        {
            var (content, message) = await this._usersService.LogSession(request);

            var response = new Response<UsersLogSessionResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "User")]
        [HttpPut]
        public async Task<IActionResult> EditSession(UsersEditSessionRequest? request)
        {
            var (content, message) = await this._usersService.EditSession(request);

            var response = new Response<UsersEditSessionResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSession(UsersDeleteSessionRequest? request)
        {
            var (content, message) = await this._usersService.DeleteSession(request);

            var response = new Response<UsersDeleteSessionResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
    }
}