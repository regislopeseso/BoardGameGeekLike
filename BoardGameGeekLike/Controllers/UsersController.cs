using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
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

        [HttpPost]
        public async Task<IActionResult> SignUp(UsersSignUpRequest? request)
        {
            var (content, message) = await this._usersService.SignUp(request);

            var response = new Response<UsersSignUpResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


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


        [HttpPost]
        public async Task<IActionResult> RateBoardGame(UsersRateBoardGameRequest? request)
        {
            var (content, message) = await this._usersService.RateBoardGame(request);

            var response = new Response<UsersRateBoardGameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> LogPlay(UsersLogPlayRequest? request)
        {
            var (content, message) = await this._usersService.LogPlay(request);

            var response = new Response<UsersLogPlayResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
    }
}