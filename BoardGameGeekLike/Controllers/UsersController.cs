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
        [HttpGet]
        public async Task<IActionResult> ListPlayedBoardGames(UsersListPlayedBoardGamesRequest? request)
        {
            var (content, message) = await this._usersService.ListPlayedBoardGames(request);

            var response = new Response<List<UsersListPlayedBoardGamesResponse>>
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
        public async Task<IActionResult> ListRatedBoardGames([FromQuery] UsersListRatedBoardGamesRequest? request)
        {
            var (content, message) = await this._usersService.ListRatedBoardGames(request);

            var response = new Response<List<UsersListRatedBoardGamesResponse>>
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

        [Authorize(Roles = "Developer, Administrator, User")]
        [HttpDelete]
        public async Task<IActionResult> DeleteRating([FromQuery] UsersDeleteRatingRequest? request)
        {
            var (content, message) = await this._usersService.DeleteRating(request);

            var response = new Response<UsersDeleteRatingResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        //
        // LIFE COUNTERS
        [HttpGet]
        public async Task<IActionResult> ListLifeCounters(UsersListLifeCountersRequest? request)
        {
            var (content, message) = await this._usersService.ListLifeCounters(request);

            var response = new Response<List<UsersListLifeCountersResponse>>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetLastLifeCounterManager(UsersGetLastLifeCounterManagerRequest? request)
        {
            var (content, message) = await this._usersService.GetLastLifeCounterManager(request);

            var response = new Response<UsersGetLastLifeCounterManagerResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> CountLifeCounters(UsersCountLifeCountersRequest? request)
        {
            var (content, message) = await this._usersService.CountLifeCounters(request);

            var response = new Response<UsersCountLifeCountersResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);

        }

        [HttpPost]
        public async Task<IActionResult> NewLifeCounter([FromForm] UsersNewLifeCounterRequest? request)
        {
            var (content, message) = await this._usersService.NewLifeCounter(request);

            var response = new Response<UsersNewLifeCounterResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetLifeCounterDetails([FromQuery] UsersGetLifeCounterDetailsRequest? request)
        {
            var (content, message) = await this._usersService.GetLifeCounterDetails(request);

            var response = new Response<UsersGetLifeCounterDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> StartLifeCounterManager([FromForm] UsersStartLifeCounterManagerRequest request)
        {
            var (content, message) = await this._usersService.StartLifeCounterManager(request);

            var response = new Response<UsersStartLifeCounterManagerResponse>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetLifeCounterPlayersDetails([FromQuery] UsersGetLifeCounterPlayersDetailsRequest? request)
        {
            var (content, message) = await this._usersService.GetLifeCounterPlayersDetails(request);

            var response = new Response<UsersGetLifeCounterPlayersDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> IncreaseLifePoints([FromForm] UsersIncreaseLifePointsRequest request)
        {
            var (content, message) = await this._usersService.IncreaseLifePoints(request);

            var response = new Response<UsersIncreaseLifePointsResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

            return new JsonResult(response);
        }


        [HttpPost]
        public async Task<IActionResult> DecreaseLifePoints([FromForm] UsersDecreaseLifePointsRequest request)
        {
            var (content, message) = await this._usersService.DecreaseLifePoints(request);

            var response = new Response<UsersDecreaseLifePointsResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> CheckForLifeCounterManagerEnd([FromQuery] UsersCheckForLifeCounterManagerEndRequest? request)
        {
            var (content, message) = await this._usersService.CheckForLifeCounterManagerEnd(request);

            var response = new Response<UsersCheckForLifeCounterManagerEndResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> RefreshLifeCounterManager([FromForm] UsersRefreshLifeCounterManagerRequest request)
        {
            var (content, message) = await this._usersService.RefreshLifeCounterManager(request);

            var response = new Response<UsersRefreshLifeCounterManagerResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

            return new JsonResult(response);
        }

    }
}