using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
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
        public async Task<IActionResult> FindBoardGame(ExploreFindBoardGameRequest? request)
        {
            var (content, message) = await this._exploreService.FindBoardGame(request);

            var response = new Response<List<ExploreFindBoardGameResponse>>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        [HttpGet]
        public async Task<IActionResult> ShowBoardGameDetails(ExploreShowBoardGameDetailsRequest? request)
        {
            var (content, message) = await this._exploreService.ShowBoardGameDetails(request);

            var response = new Response<ExploreShowBoardGameDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> RatedBoardGames(ExploreRatedBoardGamesRequest? request)
        {
            var (content, message) = await this._exploreService.RatedBoardGames(request);

            var response = new Response<List<ExploreRatedBoardGamesResponse>>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> BoardGamesRankings(ExploreBoardGamesRankingsRequest? request)
        {
            var (content, message) = await this._exploreService.BoardGamesRankings(request);

            var response = new Response<ExploreBoardGamesRankingsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> CategoriesRanking(ExploreCategoriesRankingRequest? request)
        {
            var (content, message) = await this._exploreService.CategoriesRanking(request);

            var response = new Response<ExploreCategoriesRankingResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
    }
}
