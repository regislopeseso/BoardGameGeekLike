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
        public async Task<IActionResult> SignUp([FromForm] UsersSignUpRequest? request)
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
        public async Task<IActionResult> SignIn([FromForm] UsersSignInRequest? request)
        {
            var (content, message) = await this._usersService.SignIn(request);

            var response = new Response<UsersSignInResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ValidateStatus()
        {
            var (content, message) = this._usersService.ValidateStatus();

            var response = new Response<UsersValidateStatusResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
        
        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpGet]
        public async Task<IActionResult> GetRole()
        {
            var (content, message) = await this._usersService.GetRole();

            var response = new Response<UsersGetRoleResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            var (content, message) = await this._usersService.SignOut();

            var response = new Response<UsersSignOutResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpPut]
        public async Task<IActionResult> EditProfile([FromBody] UsersEditProfileRequest? request)
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
        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromBody] UsersChangePasswordRequest? request)
        {
            var (content, message) = await this._usersService.ChangePassword(request);

            var response = new Response<UsersChangePasswordResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProfile([FromBody] UsersDeleteProfileRequest? request)
        {
            var (content, message) = await this._usersService.DeleteProfile(request);

            var response = new Response<UsersDeleteProfileResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
       
        
        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpGet]
        public async Task<IActionResult> GetProfileDetails([FromQuery] UsersGetProfileDetailsRequest? request)
        {
            var (content, message) = await this._usersService.GetProfileDetails(request);

            var response = new Response<UsersGetProfileDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpPost]
        public async Task<IActionResult> LogSession([FromForm] UsersLogSessionRequest? request)
        {
            var (content, message) = await this._usersService.LogSession(request);

            var response = new Response<UsersLogSessionResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpGet]
        public async Task<IActionResult> GetSessions([FromQuery] UsersGetSessionsRequest? request)
        {
            var (content, message) = await this._usersService.GetSessions(request);

            var response = new Response<UsersGetSessionsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpPut]
        public async Task<IActionResult> EditSession([FromBody] UsersEditSessionRequest? request)
        {
            var (content, message) = await this._usersService.EditSession(request);

            var response = new Response<UsersEditSessionResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSession([FromQuery] UsersDeleteSessionRequest? request)
        {
            var (content, message) = await this._usersService.DeleteSession(request);

            var response = new Response<UsersDeleteSessionResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpPost]
        public async Task<IActionResult> Rate([FromForm] UsersRateRequest? request)
        {
            var (content, message) = await this._usersService.Rate(request);

            var response = new Response<UsersRateResponse>
            {
                Content = content,
                Message = message
            };
           
            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpGet]
        public async Task<IActionResult> GetRate([FromQuery] UsersGetRateRequest? request)
        {
            var (content, message) = await this._usersService.GetRate(request);

            var response = new Response<UsersGetRateResponse>
            {
                Content = content,
                Message = message
            };
            
            return new JsonResult(response);
        }

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpPut]
        public async Task<IActionResult> EditRating([FromBody] UsersEditRatingRequest? request)
        {
            var (content, message) = await this._usersService.EditRating(request);

            var response = new Response<UsersEditRatingResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

    }
}