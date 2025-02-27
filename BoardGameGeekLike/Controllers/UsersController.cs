using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
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

        [HttpPost]
        public async Task<IActionResult> SignUp(UsersSignUpRequest request)
        {
            var (content, message) = await this._usersService.SignUp(request);

            var response = new Response<UsersSignUpResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

    }
}