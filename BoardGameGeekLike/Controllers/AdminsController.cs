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


        #region Board Games  (BG)

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


        #region Medieval Auto Battler  (MAB)

        //MAB CARDS

        [HttpPost]
        public async Task<IActionResult> AddMabCard([FromForm] AdminsAddMabCardRequest request)
        {
            var (content, message) = await this._adminsService.AddMabCard(request);

            var response = new Response<AdminsAddMabCardResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ShowMabCardDetails([FromQuery] AdminsShowMabCardDetailsRequest request)
        {
            var (content, message) = await this._adminsService.ShowMabCardDetails(request);

            var response = new Response<AdminsShowMabCardDetailsResponse>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> EditMabCard([FromBody] AdminsEditMabCardRequest request)
        {
            var (content, message) = await this._adminsService.EditMabCard(request);

            var response = new Response<AdminsEditMabCardResponse>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMabCard([FromBody] AdminsDeleteMabCardRequest? request)
        {
            var (content, message) = await this._adminsService.DeleteMabCard(request);

            var response = new Response<AdminsDeleteMabCardResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreMabCard([FromBody] AdminsRestoreMabCardRequest? request)
        {
            var (content, message) = await this._adminsService.RestoreMabCard(request);

            var response = new Response<AdminsRestoreMabCardResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ListMabCards(AdminsListMabCardsRequest? request)
        {
            var (content, message) = await this._adminsService.ListMabCards(request);

            var response = new Response<List<AdminsListMabCardsResponse>>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ListMabCardIds(AdminsListMabCardIdsRequest? request)
        {
            var (content, message) = await this._adminsService.ListMabCardIds(request);

            var response = new Response<List<AdminsListMabCardIdsResponse>>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        [HttpGet]
        public IActionResult ListMabCardTypes(AdminsListMabCardTypesRequest? request)
        {
            var (content, message) = this._adminsService.ListMabCardTypes(request);

            var response = new Response<List<AdminsListMabCardTypesResponse>>()
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

        // MAB NPCS
        [HttpPost]
        public async Task<IActionResult> AddMabNpc(AdminsAddMabNpcRequest request)
        {
            var (content, message) = await this._adminsService.AddMabNpc(request);

            var response = new Response<AdminsAddMabNpcResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ShowMabNpcDetails([FromQuery] AdminsShowMabNpcDetailsRequest request)
        {
            var (content, message) = await this._adminsService.ShowMabNpcDetails(request);

            var response = new Response<AdminsShowMabNpcDetailsResponse>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> ListMabNpcs(AdminsListMabNpcsRequest? request)
        {
            var (content, message) = await this._adminsService.ListMabNpcs(request);

            var response = new Response<List<AdminsGetNpcsResponse>?>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> EditMabNpc([FromBody] AdminsEditMabNpcRequest request)
        {
            var (content, message) = await this._adminsService.EditMabNpc(request);

            var response = new Response<AdminsEditMabNpcResponse>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }
        
        [HttpPost]
        public async Task<IActionResult> GetMabNpcLvl([FromBody] AdminsGetMabNpcLvlRequest request)
        {
            var (content, message) = await this._adminsService.GetMabNpcLvl(request);

            var response = new Response<AdminsGetMabNpcLvlResponse>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public IActionResult GetDeckSizeLimit(AdminsGetDeckSizeLimitRequest? request)
        {
            var (content, message) = this._adminsService.GetDeckSizeLimit(request);

            var response = new Response<AdminsGetDeckSizeLimitResponse?>()
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMabNpc([FromBody] AdminsDeleteMabNpcRequest? request)
        {
            var (content, message) = await this._adminsService.DeleteMabNpc(request);

            var response = new Response<AdminsDeleteMabNpcResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreMabNpc([FromBody] AdminsRestoreMabNpcRequest? request)
        {
            var (content, message) = await this._adminsService.RestoreMabNpc(request);

            var response = new Response<AdminsRestoreMabNpcResponse?>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }


        #endregion
    }
}