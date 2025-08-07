using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGameGeekLike.Controllers
{ 
    [Authorize(Roles = "Developer")]
    [ApiController]
    [Route("devs/[action]")]
    public class DevsController : ControllerBase
    {
        private readonly DevsService _devsService;

        public DevsController(DevsService devsService)
        {
            this._devsService = devsService;     
        }

        #region  BOARD GAMES

        [HttpPost]
        public async Task<IActionResult> BoardGamesSeed(DevsBoardGamesSeedRequest? request)
        {
            var (content, message) = await this._devsService.BoardGamesSeed(request);

            var response = new Response<DevsBoardGamesSeedResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> BoardGamesDeleteSeed(DevsBoardGamesDeleteSeedRequest? request)
        {
            var (content, message) = await this._devsService.BoardGamesDeleteSeed(request);

            var response = new Response<DevsBoardGamesDeleteSeedResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        #endregion

        #region PLAYABLE GAMES
        
        // Medieval Auto Battler (M.A.B.)

        [HttpPost]
        public async Task<IActionResult> MedievalAutoBattlerSeed(DevsMedievalAutoBattlerSeedRequest request)
        {
            var (content, message) = await this._devsService.MedievalAutoBattlerSeed(request);

            var response = new Response<DevsMedievalAutoBattlerSeedResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }

        [HttpDelete]
        public async Task<IActionResult> MedievalAutoBattlerDeleteSeed(DevsMedievalAutoBattlerDeleteSeedRequest? request)
        {
            var (content, message) = await this._devsService.MedievalAutoBattlerDeleteSeed(request);

            var response = new Response<DevsMedievalAutoBattlerDeleteSeedResponse>
            {
                Content = content,
                Message = message
            };

            return new JsonResult(response);
        }



        #endregion

    }
}