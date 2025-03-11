using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameGeekLike.Controllers
{
    [ApiController]
    [Route("explore/[action]")]
    public class ExploreController : ControllerBase
    {
        private readonly ExploreService _exploreService;

        public ExploreController(ExploreService exploreService)
        {
            this._exploreService = exploreService;
        }

        [HttpGet]
        public async Task<IActionResult> FindBoardGame(UsersFindBoardGameRequest? request)
        {
            var (content, message) = await this._exploreService.FindBoardGame(request);

            var response = new Response<List<UsersFindBoardGameResponse>>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        [HttpGet]
        public async Task<IActionResult> ShowBoardGameDetails(UsersShowBoardGameDetailsRequest? request)
        {
            var (content, message) = await this._exploreService.ShowBoardGameDetails(request);

            var response = new Response<UsersShowBoardGameDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> BoardGamesRanking(UsersBoardGamesRankingRequest? request)
        {
            var (content, message) = await this._exploreService.BoardGamesRanking(request);

            var response = new Response<UsersBoardGamesRankingResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> CategoriesRanking(UsersCategoriesRankingRequest? request)
        {
            var (content, message) = await this._exploreService.CategoriesRanking(request);

            var response = new Response<UsersCategoriesRankingResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
    }
}
