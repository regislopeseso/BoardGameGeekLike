using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameGeekLike.Controllers
{   
    [AllowAnonymous]
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
        public async Task<IActionResult> FindBoardGame([FromQuery] ExploreFindBoardGameRequest? request)
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
        public async Task<IActionResult> ListBoardGames(ExploreListBoardGamesRequest? request)
        {
            var (content, message) = await this._exploreService.ListBoardGames(request);

            var response = new Response<List<ExploreListBoardGamesResponse>>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ShowBoardGameDetails([FromQuery] ExploreShowBoardGameDetailsRequest? request)
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
