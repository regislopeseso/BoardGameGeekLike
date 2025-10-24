using BoardGameGeekLike.Exceptions;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameGeekLike.Controllers
{
    //[Authorize(Roles = "Developer, Administrator, User")]

    [Authorize]
    [ApiController]
    [Route("users/[action]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            this._usersService = usersService;
        }


        #region USER'S DATA

        [HttpGet]
        public async Task<IActionResult> GetProfileDetails([FromBody] UsersGetProfileDetailsRequest? request = null)
        {
            var (content, message) = await this._usersService.GetProfileDetails(request);

            var response = new Response<UsersGetProfileDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> ImportUserData(UsersImportUserDataRequest? request)
        {
            var (content, message) = await this._usersService.ImportUserData(request);

            var response = new Response<UsersImportUserDataResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        #endregion

        #region USER'S PROFILE

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] UsersSignUpRequest? request)
        {
            var (content, message) = await this._usersService.SignUp(request, "User");

            var response = new Response<UsersSignUpResponse>
            {
                Content = content,
                Message = message
            };

            await Task.Delay(1000);

            return new JsonResult(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody] UsersSignInRequest? request)
        {
            await Task.Delay(1000);

            try
            {
                var content = await this._usersService.SignIn(request);
                var response = new Response<UsersSignInResponse>
                {
                    Content = content,
                    Message = $"User signed in successfully"
                };
                return Ok(response);
            }
            catch (ValidationException exception)
            {
                var response = new Response<UsersSignInResponse>
                {
                    Content = null,
                    Message = exception.Message
                };
                return BadRequest(response);
            }
            catch (NotFoundException exception)
            {
                var response = new Response<UsersSignInResponse>
                {
                    Content = null,
                    Message = exception.Message
                };
                return NotFound(response);
            }
            catch (AccountLockedException exception)
            {
                var response = new Response<UsersSignInResponse>
                {
                    Content = null,
                    Message = exception.Message
                };
                return StatusCode(423, response); // 423 Locked
            }
            catch (AccountNotAllowedException exception)
            {
                var response = new Response<UsersSignInResponse>
                {
                    Content = null,
                    Message = exception.Message
                };
                return StatusCode(403, response); // 403 Forbidden
            }
            catch (AuthenticationException exception)
            {
                var content = new UsersSignInResponse
                {
                    RemainingSignInAttempts = exception.RemainingAttempts
                };
                var response = new Response<UsersSignInResponse>
                {
                    Content = content,
                    Message = exception.Message
                };
                return Unauthorized(response);
            }
            catch (Exception exception)
            {
                // Log the exception here
                var response = new Response<UsersSignInResponse>
                {
                    Content = null,
                    Message = $"An unexpected error occurred: {exception.Message}"
                };
                return StatusCode(500, response);
            }
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ValidateStatus(UsersValidateStatusRequest? request = null)
        {
            var (content, message) = await this._usersService.ValidateStatus(request);

            var response = new Response<UsersValidateStatusResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SignOutUser(UsersSignOutUserRequest? request = null)
        {
            var (content, message) = await this._usersService.SignOutUser(request);

            var response = new Response<UsersSignOutUserResponse>
            {
                Content = content,
                Message = message
            };

            await Task.Delay(1000);

            return new JsonResult(response);
        }

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

        [HttpGet]
        public async Task<IActionResult> ExportUserData([FromBody] UsersExportUserDataRequest? request = null)
        {
            var (content, message) = await this._usersService.ExportUserData(request);

            var response = new Response<UsersExportUserDataResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        #endregion

        #region USER'S BOARD GAMES

        // BOARD GAMES        

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

        //--* end of BOARD GAMES *--//

        #endregion

        #region USER'S LIFE COUNTERS

        // LIFE COUNTERS

        // 1� LIFE COUNTER SYNC DATA
        [HttpPost]
        public async Task<IActionResult> SyncLifeCounterData([FromBody] UsersSyncLifeCounterDataRequest? request)
        {
            var (content, message) = await this._usersService.SyncLifeCounterData(request);

            var response = new Response<UsersSyncLifeCounterDataResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        // 2. LIFE COUNTER QUICK START
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


        // 3. LIFE COUNTER TEMPLATES    
        [HttpPost]
        public async Task<IActionResult> CreateLifeCounterTemplate([FromForm] UsersCreateLifeCounterTemplateRequest? request)
        {
            var (content, message) = await this._usersService.CreateLifeCounterTemplate(request);

            var response = new Response<UsersCreateLifeCounterTemplateResponse>
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

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
        public async Task<IActionResult> GetLastLifeCounterTemplateId(UsersGetLastLifeCounterTemplateRequest? request)
        {
            var (content, message) = await this._usersService.GetLastLifeCounterTemplate(request);

            var response = new Response<UsersGetLastLifeCounterTemplateResponse>
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


        [HttpPut]
        public async Task<IActionResult> EditLifeCounterTemplate([FromBody] UsersEditLifeCounterTemplateRequest? request)
        {
            var (content, message) = await this._usersService.EditLifeCounterTemplate(request);

            var response = new Response<UsersEditLifeCounterTemplateResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteLifeCounterTemplate([FromQuery] UsersDeleteLifeCounterTemplateRequest request)
        {
            var (content, message) = await this._usersService.DeleteLifeCounterTemplate(request);

            var response = new Response<UsersDeleteLifeCounterTemplateResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

            return new JsonResult(response);
        }


        // 4. LIFE COUNTER MANAGERS
        [HttpPost]
        public async Task<IActionResult> StartLifeCounterManager([FromForm] UsersStartLifeCounterManagerRequest request)
        {
            var (content, message) = await this._usersService.StartLifeCounterManager(request);

            var response = new Response<UsersStartLifeCounterManagerResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

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

        [HttpGet]
        public async Task<IActionResult> GetLastLifeCounterManager([FromQuery] UsersGetLastLifeCounterManagerRequest? request)
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
        public async Task<IActionResult> ListUnfinishedLifeCounterManagers([FromQuery] UsersListUnfinishedLifeCounterManagersRequest? request)
        {
            var (content, message) = await this._usersService.ListUnfinishedLifeCounterManagers(request);

            var response = new Response<List<UsersListUnfinishedLifeCounterManagersResponse>>
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

        [HttpDelete]
        public async Task<IActionResult> DeleteLifeCounterManager([FromQuery] UsersDeleteLifeCounterManagerRequest request)
        {
            var (content, message) = await this._usersService.DeleteLifeCounterManager(request);

            var response = new Response<UsersDeleteLifeCounterManagerResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

            return new JsonResult(response);
        }


        // 5. LIFE COUNTER PLAYERS
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

        [HttpGet]
        public async Task<IActionResult> GetLifeCounterPlayerDetails([FromQuery] UsersGetLifeCounterPlayerDetailsRequest? request)
        {
            var (content, message) = await this._usersService.GetLifeCounterPlayerDetails(request);

            var response = new Response<UsersGetLifeCounterPlayerDetailsResponse>
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

            // await Task.Delay(300);

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

            // await Task.Delay(300);

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreLifeCounterPlayer([FromForm] UsersRestoreLifeCounterPlayerRequest request)
        {
            var (content, message) = await this._usersService.RestoreLifeCounterPlayer(request);

            var response = new Response<UsersRestoreLifeCounterPlayerResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

            return new JsonResult(response);
        }


        [HttpGet]
        public async Task<IActionResult> GetPlayersCount([FromQuery] UsersGetPlayersCountRequest? request)
        {
            var (content, message) = await this._usersService.GetPlayersCount(request);

            var response = new Response<UsersGetPlayersCountResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePlayerName([FromForm] UsersChangePlayerNameRequest request)
        {
            var (content, message) = await this._usersService.ChangePlayerName(request);

            var response = new Response<UsersChangePlayerNameResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLifeCounterPlayer([FromQuery] UsersDeleteLifeCounterPlayerRequest request)
        {
            var (content, message) = await this._usersService.DeleteLifeCounterPlayer(request);

            var response = new Response<UsersDeleteLifeCounterPlayerResponse>()
            {
                Content = content,
                Message = message
            };

            await Task.Delay(300);

            return new JsonResult(response);
        }


        // 6� LIFE COUNTER STATISTICS
        [HttpGet]
        public async Task<IActionResult> GetLifeCounterStatistics(UsersGetLifeCounterStatisticsRequest? request)
        {
            var (content, message) = await this._usersService.GetLifeCounterStatistics(request);

            var response = new Response<UsersGetLifeCounterStatisticsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        //--* end of LIFE COUNTERS *--//

        #endregion

        #region PLAYABLE GAMES

        // MEDIEVAL AUTO BATTLER (MAB)

        [HttpGet]
        public async Task<IActionResult> MabCheckForExistingCampaign(UsersMabCheckForExistingCampaignRequest? request)
        {
            var (content, message) = await this._usersService.MabCheckForExistingCampaign(request);

            var response = new Response<UsersMabCheckForExistingCampaignResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public IActionResult MabGetCampaignDificultyInfo([FromQuery] UsersMabGetCampaignDificultyInfoRequest? request)
        {
            var (content, message) =  this._usersService.MabGetCampaignDificultyInfo(request);

            var response = new Response<UsersMabGetCampaignDificultyInfoResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabStartCampaign([FromForm] UsersMabStartCampaignRequest? request)
        {
            var (content, message) = await this._usersService.MabStartCampaign(request);

            var response = new Response<UsersMabStartCampaignResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> MabDeleteCampaign(UsersMabDeleteCampaignRequest? request)
        {
            var (content, message) = await this._usersService.MabDeleteCampaign(request);

            var response = new Response<UsersMabDeleteCampaignResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabShowDeckDetails([FromQuery] UsersMabShowDeckDetailsRequest? request)
        {
            var (content, message) = await this._usersService.MabShowDeckDetails(request);

            var response = new Response<UsersMabShowDeckDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> MabEditDeckName([FromBody] UsersMabEditDeckNameRequest? request)
        {
            var (content, message) = await this._usersService.MabEditDeckName(request);

            var response = new Response<UsersMabEditDeckNameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> MabDeleteDeck([FromBody] UsersMabDeleteDeckRequest? request)
        {
            var (content, message) = await this._usersService.DeleteMabDeck(request);

            var response = new Response<UsersMabDeleteDeckResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> MabActivateDeck([FromBody] UsersMabActivateDeckRequest? request)
        {
            var (content, message) = await this._usersService.MabActivateDeck(request);

            var response = new Response<UsersMabActivateDeckResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabListUnassignedPlayerCards([FromQuery] UsersMabListUnassignedPlayerCardsRequest? request)
        {
            var (content, message) = await this._usersService.MabListUnassignedPlayerCards(request);

            var response = new Response<List<UsersMabListUnassignedPlayerCardsResponse>?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabCreateDeck(UsersMabCreateDeckRequest? request)
        {
            var (content, message) = await this._usersService.MabCreateDeck(request);

            var response = new Response<UsersMabCreatePlayerDeckResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabAssignPlayerCard([FromBody] UsersMabAssignPlayerCardRequest? request)
        {
            var (content, message) = await this._usersService.MabAssignPlayerCard(request);

            var response = new Response<UsersMabAssignPlayerCardResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> MabUnassignPlayerCard([FromBody] UsersMabUnassignPlayerCardRequest? request)
        {
            var (content, message) = await this._usersService.MabUnassignPlayerCard(request);

            var response = new Response<UsersMabUnassignPlayerCardResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabShowCampaignStatistics(UsersMabShowCampaignStatisticsRequest? request)
        {
            var (content, message) = await this._usersService.MabShowCampaignStatistics(request);

            var response = new Response<UsersMabShowCampaignStatisticsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> MabEditPlayerNickname([FromBody] UsersMabEditPlayerNicknameRequest? request)
        {
            var (content, message) = await this._usersService.MabEditPlayerNickname(request);

            var response = new Response<UsersMabEditPlayerNickNameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabListDecks(UsersMabListDecksRequest? request)
        {
            var (content, message) = await this._usersService.MabListDecks(request);

            var response = new Response<List<UsersMabListDecksResponse>?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabListQuests(UsersMabListQuestsRequest? request)
        {
            var (content, message) = await this._usersService.MabListQuests(request);

            var response = new Response<List<UsersMabListQuestsResponse>?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabStartBattle([FromForm] UsersMabStartBattleRequest? request)
        {
            var (content, message) = await this._usersService.MabStartBattle(request);

            var response = new Response<UsersMabStartBattleResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabFinishBattle(UsersMabFinishBattleRequest? request)
        {
            var (content, message) = await this._usersService.MabFinishBattle(request);

            var response = new Response<UsersMabFinishBattleResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabStartDuel(UsersMabStartDuelRequest? request)
        {
            var (content, message) = await this._usersService.MabStartDuel(request);

            var response = new Response<UsersMabStartDuelResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
        
        [HttpPost]
        public async Task<IActionResult> MabResolveDuel(UsersMabResolveDuelRequest? request)
        {
            var (content, message) = await this._usersService.MabResolveDuel(request);

            var response = new Response<UsersMabResolveDuelResponse?>
            {
                Content = content,
                Message = message
            };

            await Task.Delay(2000);

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabPlayerAttacks([FromForm] UsersMabPlayerAttacksRequest? request)
        {
            var (content, message) = await this._usersService.MabPlayerAttacks(request);

            var response = new Response<UsersMabPlayerAttacksResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabPlayerRetreats(UsersMabPlayerRetreatsRequest? request)
        {
            var (content, message) = await this._usersService.MabPlayerRetreats(request);

            var response = new Response<UsersMabPlayerRetreatsResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabNpcAttacks(UsersMabNpcAttacksRequest? request)
        {
            var (content, message) = await this._usersService.MabNpcAttacks(request);

            var response = new Response<UsersMabNpcAttacksResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabGetNpcCardFullPower([FromQuery] UsersMabGetNpcCardFullPowerRequest? request)
        {
            var (content, message) = await this._usersService.MabGetNpcCardFullPower(request);

            var response = new Response<UsersMabGetNpcCardFullPowerResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabListPlayerDuellingCards([FromQuery] UsersMabListPlayerDuellingCardsRequest? request)
        {
            var (content, message) = await this._usersService.MabListPlayerDuellingCards(request);

            var response = new Response<List<UsersMabListPlayerDuellingCardsResponse>?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabListNpcPlayedCards(UsersMabListNpcPlayedCardsRequest? request)
        {
            var (content, message) = await this._usersService.MabListNpcPlayedCards(request);

            var response = new Response<List<UsersMabListNpcPlayedCardsResponse>?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabCheckDuelStatus(UsersMabCheckDuelStatusRequest? request)
        {
            var (content, message) = await this._usersService.MabCheckDuelStatus(request);

            var response = new Response<UsersMabCheckDuelStatusResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabManageTurn(UsersMabManageTurnRequest? request)
        {
            var (content, message) = await this._usersService.MabManageTurn(request);

            var response = new Response<UsersMabManageTurnResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabAutoBattle(UsersMabAutoBattleRequest? request)
        {
            var (content, message) = await this._usersService.MabAutoBattle(request);

            var response = new Response<UsersMabAutoBattleResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabShowQuestDetails([FromQuery] UsersMabShowQuestDetailsRequest? request)
        {
            var (content, message) = await this._usersService.MabShowQuestDetails(request);

            var response = new Response<UsersMabShowQuestDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabShowMiningDetails(UsersMabShowMiningDetailsRequest? request)
        {
            var (content, message) = await this._usersService.MabShowMiningDetails(request);

            var response = new Response<UsersMabShowMiningDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabExtractRawMaterial(UsersMabExtractRawMaterialRequest? request)
        {
            var (content, message) = await this._usersService.MabExtractRawMaterial(request);

            var response = new Response<UsersMabExtractRawMaterialResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabListResources(UsersMabListResourcesRequest? request)
        {
            var (content, message) = await this._usersService.MabListResources(request);

            var response = new Response<UsersMabListResourcesResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
        
        [HttpGet]
        public async Task<IActionResult> MabListRawMaterialsPrices(UsersMabListRawMaterialsPricesRequest? request)
        {
            var (content, message) = await this._usersService.MabListRawMaterialsPrices(request);

            var response = new Response<UsersMabListRawMaterialsPricesResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabSellRawMaterial([FromForm] UsersMabSellRawMaterialRequest? request)
        {
            var (content, message) = await this._usersService.MabSellRawMaterial(request);

            var response = new Response<UsersMabSellRawMaterialResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabBuyRawMaterial([FromForm] UsersMabBuyRawMaterialRequest? request)
        {
            var (content, message) = await this._usersService.MabBuyRawMaterial(request);

            var response = new Response<UsersMabBuyRawMaterialResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabShowPlayerCardDetails([FromQuery] UsersMabShowPlayerCardDetailsRequest? request)
        {
            var (content, message) = await this._usersService.MabShowPlayerCardDetails(request);

            var response = new Response<UsersMabShowPlayerCardDetailsResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabForgeCard([FromForm] UsersMabForgeCardRequest? request)
        {
            var (content, message) = await this._usersService.MabForgeCard(request);

            var response = new Response<UsersMabForgeCardResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabSharpenCard([FromForm] UsersMabSharpenCardRequest? request)
        {
            var (content, message) = await this._usersService.MabSharpenCard(request);

            var response = new Response<UsersMabSharpenCardResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabMeltCard([FromForm] UsersMabMeltCardRequest? request)
        {
            var (content, message) = await this._usersService.MabMeltCard(request);

            var response = new Response<UsersMabMeltCardResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> MabGetDeckBoosterDeal(UsersMabGetDeckBoosterDealRequest? request)
        {
            var (content, message) = await this._usersService.MabGetDeckBoosterDeal(request);

            var response = new Response<UsersMabGetDeckBoosterDealResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> MabBuyDeckBooster(UsersMabBuyDeckBoosterRequest? request)
        {
            var (content, message) = await this._usersService.MabBuyDeckBooster(request);

            var response = new Response<List<UsersMabBuyDeckBoosterResponse?>?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        #endregion
    }
}