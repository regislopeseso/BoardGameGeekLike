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
        
        // USER'S PROFILE
        //
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
        //
        //--* end of USER'S PROFILE *--//       
           


        // BOARD GAMES
        //
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
        //--* end of BOARD GAMES *--//



        // LIFE COUNTERS
        //
        // 1º LIFE COUNTER QUICK START
        [HttpPost]
        public async Task<IActionResult> QuickStartLifeCounter([FromBody] UsersQuickStartLifeCounterRequest? request)
        {
            var (content, message) = await this._usersService.QuickStartLifeCounter(request);

            var response = new Response<UsersQuickStartLifeCounterResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        //
        // 2º LIFE COUNTER TEMPLATES
        [HttpPost]
        public async Task<IActionResult> CreateLifeCounterTemplate([FromForm] UsersCreateLifeCounterTemplateRequest? request)
        {
            var (content, message) = await this._usersService.CreateLifeCounterTemplate(request);

            var response = new Response<UsersCreateLifeCounterTemplateResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
        
        [HttpGet]
        public async Task<IActionResult> CountLifeCounterTemplates(UsersCountLifeCounterTemplatesRequest? request)
        {
            var (content, message) = await this._usersService.CountLifeCountersTemplates(request);

            var response = new Response<UsersCountLifeCounterTemplatesResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);

        }

        [HttpGet]
        public async Task<IActionResult> GetLastLifeCounterTemplateId(UsersGetLastLifeCounterTemplateIdRequest? request)
        {
            var (content, message) = await this._usersService.GetLastLifeCounterTemplateId(request);

            var response = new Response<UsersGetLastLifeCounterTemplateIdResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ListLifeCounterTemplates(UsersListLifeCounterTemplatesRequest? request)
        {
            var (content, message) = await this._usersService.ListLifeCounterTemplates(request);

            var response = new Response<List<UsersListLifeCounterTemplatesResponse>>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetLifeCounterTemplateDetails([FromQuery] UsersGetLifeCounterTemplateDetailsRequest? request)
        {
            var (content, message) = await this._usersService.GetLifeCounterTemplateDetails(request);

            var response = new Response<UsersGetLifeCounterTemplateDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
        
        //
        // 3º LIFE COUNTER MANAGERS
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
        
        [HttpPut]
        public async Task<IActionResult> EditLifeCounterManager([FromBody] UsersEditLifeCounterManagerRequest? request)
        {
            var (content, message) = await this._usersService.EditLifeCounterManager(request);

            var response = new Response<UsersEditLifeCounterManagerResponse>
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

        [HttpGet]
        public async Task<IActionResult> GetLifeCounterManagerDetails([FromQuery] UsersGetLifeCounterManagerDetailsRequest? request)
        {
            var (content, message) = await this._usersService.GetLifeCounterManagerDetails(request);

            var response = new Response<UsersGetLifeCounterManagerDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        //
        // 4º LIFE COUNTER PLAYERS
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
        
        //
        //--* end of LIFE COUNTERS *--//
    }
}