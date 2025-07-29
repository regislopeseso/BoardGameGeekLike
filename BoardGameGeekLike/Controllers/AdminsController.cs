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


        #region Board Games 

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
        public async Task<IActionResult> DeleteBoardGame([FromBody] AdminsDeleteBoardGameRequest? request)
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
        public async Task<IActionResult> RestoreBoardGame([FromBody] AdminsRestoreBoardGameRequest? request)
        {
            var (content, message) = await this._adminsService.RestoreBoardGame(request);

            var response = new Response<AdminsRestoreBoardGameResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }        

        //
        //  CATEGORIES
        //

        [HttpGet]
        public async Task<IActionResult> ListCategories(AdminsListCategoriesRequest? request)
        {
            var (content, message) = await this._adminsService.ListCategories(request);

            var response = new Response<List<AdminsListCategoriesResponse>>
            {
                Content = content,
                Message = message
            };

            await Task.Delay(1000);

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromForm] AdminsAddCategoryRequest? request)
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
        public async Task<IActionResult> ShowCategoryDetails([FromQuery] AdminsShowCategoryDetailsRequest? request)
        {
            var (content, message) = await this._adminsService.ShowCategoryDetails(request);

            var response = new Response<AdminsShowCategoryDetailsResponse>
            {
                Content = content,
                Message = message
            };
            await Task.Delay(1000);
            return new JsonResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> EditCategory([FromBody] AdminsEditCategoryRequest? request)
        {
            var (content, message) = await this._adminsService.EditCategory(request);

            var response = new Response<AdminsEditCategoryResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCategory([FromBody] AdminsDeleteCategoryRequest? request)
        {
            var (content, message) = await this._adminsService.DeleteCategory(request);

            var response = new Response<AdminsDeleteCategoryResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreCategory([FromBody] AdminsRestoreCategoryRequest? request)
        {
            var (content, message) = await this._adminsService.RestoreCategory(request);

            var response = new Response<AdminsRestoreCategoryResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        //
        //  MECHANICS
        //

        [HttpGet]
        public async Task<IActionResult> ListMechanics(AdminsListMechanicsRequest? request)
        {
            var (content, message) = await this._adminsService.ListMechanics(request);

            var response = new Response<List<AdminsListMechanicsResponse>>
            {
                Content = content,
                Message = message
            };

            await Task.Delay(1000);

            return new JsonResult(response);
        }


        [HttpPost]
        public async Task<IActionResult> AddMechanic([FromForm] AdminsAddMechanicRequest? request)
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
        public async Task<IActionResult> ShowMechanicDetails([FromQuery] AdminsShowMechanicDetailsRequest? request)
        {
            var (content, message) = await this._adminsService.ShowMechanicDetails(request);

            var response = new Response<AdminsShowMechanicDetailsResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);

        }

        [HttpPut]
        public async Task<IActionResult> EditMechanic([FromBody] AdminsEditMechanicRequest? request)
        {
            var (content, message) = await this._adminsService.EditMechanic(request);

            var response = new Response<AdminsEditMechanicResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMechanic([FromBody] AdminsDeleteMechanicRequest? request)
        {
            var (content, message) = await this._adminsService.DeleteMechanic(request);

            var response = new Response<AdminsDeleteMechanicResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreMechanic([FromBody] AdminsRestoreMechanicRequest? request)
        {
            var (content, message) = await this._adminsService.RestoreMechanic(request);

            var response = new Response<AdminsRestoreMechanicResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        #endregion


        #region Medieval Auto Battler
        [HttpPost]
        public async Task<IActionResult> CreateCard(AdminsCreateCardRequest request)
        {
            var (content, message) = await this._adminsService.CreateCard(request);

            var response = new Response<AdminsCreateCardResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> FilterCards(AdminsFilterCardsRequest request)
        {
            var (content, message) = await this._adminsService.FilterCards(request);

            var response = new Response<List<AdminsFilterCardsResponse>>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCards(AdminsGetAllCardsRequest request)
        {
            var (content, message) = await this._adminsService.GetAllCards(request);

            var response = new Response<List<AdminsGetAllCardsResponse>>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }




        #endregion
    }
}