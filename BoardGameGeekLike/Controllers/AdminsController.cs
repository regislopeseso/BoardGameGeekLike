using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameGeekLike.Controllers
{
    [Authorize(Roles = "Developer, Administrator")]
    [ApiController]
    [Route("admins/[action]")]
    public class AdminsController : ControllerBase
    {
        private readonly AdminsService _adminsService;

        public AdminsController(AdminsService adminsService)
        {
            this._adminsService = adminsService;
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(AdminsAddCategoryRequest? request)
        {
            var (content, message) = await this._adminsService.AddCategory(request);

            var response = new Response<AdminsAddCategoryResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ShowCategories(AdminsShowCategoriesRequest? request)
        {
            var (content, message) = await this._adminsService.ShowCategories(request);

            var response = new Response<List<AdminsShowCategoriesResponse>>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);

        }

        [HttpPost]
        public async Task<IActionResult> AddMechanic(AdminsAddMechanicRequest? request)
        {
            var (content, message) = await this._adminsService.AddMechanic(request);

            var response = new Response<AdminsAddMechanicResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ShowMechanics(AdminsShowMechanicsRequest? request)
        {
            var (content, message) = await this._adminsService.ShowMechanics(request);

            var response = new Response<List<AdminsShowMechanicsResponse>>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);

        }
        
        [HttpPost]
        public async Task<IActionResult> AddBoardGame([FromForm] AdminsAddBoardGameRequest? request)
        {
            var (content, message) = await this._adminsService.AddBoardGame(request);

            var response = new Response<AdminsAddBoardGameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ShowBoardGameDetails([FromQuery] AdminsShowBoardGameDetailsRequest? request)
        {
            var (content, message) = await this._adminsService.ShowBoardGameDetails(request);

            var response = new Response<AdminsShowBoardGameDetailsResponse>
            {
                Content = content,
                Message = message
            };
            await Task.Delay(1000);
            return new JsonResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> EditBoardGame([FromBody] AdminsEditBoardGameRequest? request)
        {
            var (content, message) = await this._adminsService.EditBoardGame(request);

            var response = new Response<AdminsEditBoardGameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBoardGame(AdminsDeleteBoardGameRequest? request)
        {
            var (content, message) = await this._adminsService.DeleteBoardGame(request);

            var response = new Response<AdminsDeleteBoardGameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreBoardGame([FromForm]  AdminsRestoreBoardGameRequest? request)
        {
            var (content, message) = await this._adminsService.RestoreBoardGame(request);

            var response = new Response<AdminsRestoreBoardGameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ListBoardGames(AdminsListBoardGamesRequest? request)
        {
            var (content, message) = await this._adminsService.ListBoardGames(request);

            var response = new Response<List<AdminsListBoardGamesResponse>>
            {
                Content = content,
                Message = message
            };

            await Task.Delay(1000);

            return new JsonResult(response);
        }
    }
}