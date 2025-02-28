using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameGeekLike.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> AddBoardGame(AdminsAddBoardGameRequest? request)
        {
            var (content, message) = await this._adminsService.AddBoardGame(request);

            var response = new Response<AdminsAddBoardGameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> EditBoardGame(AdminsEditBoardGameRequest? request)
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
        public async Task<IActionResult> Seed(AdminsSeedRequest? request)
        {
            var (content, message) = await this._adminsService.Seed(request);

            var response = new Response<AdminsSeedResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
    }
}