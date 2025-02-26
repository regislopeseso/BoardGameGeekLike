using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Services
{
    public class AdminsService
    {
        private readonly ApplicationDbContext _daoDbContext;

        public AdminsService(ApplicationDbContext daoDbContext)
        {
            _daoDbContext = daoDbContext;
        }

        public async Task<(AdminsAddCategoryResponse?, string)> AddCategory(AdminsAddCategoryRequest? request)
        {
            var (isValid, message) = AddCategory_Validation(request);

            if (!isValid)
            {
                return (null, message);
            }

            var newCategory = new Category
            {
                Name = request!.CategoryName
            };

            await _daoDbContext.Categories.AddAsync(newCategory);

            await _daoDbContext.SaveChangesAsync();

            return (null, "Category added successfully");
        }

        private static (bool, string) AddCategory_Validation(AdminsAddCategoryRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return (false, "Error: name is null or empty");
            }

            return (true, String.Empty);
        }

        public async Task<(AdminsAddMechanicResponse?, string)> AddMechanic(AdminsAddMechanicRequest? request)
        {
            var (isValid, message) = AddMechanic_Validation(request);

            if (!isValid)
            {
                return (null, message);
            }

            var newMechanic = new Mechanic
            {
                Name = request!.MechanicName
            };

            await _daoDbContext.Mechanics.AddAsync(newMechanic);

            await _daoDbContext.SaveChangesAsync();

            return (null, "Category added successfully");
        }

            private static (bool, string) AddMechanic_Validation(AdminsAddMechanicRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.MechanicName))
            {
                return (false, "Error: name is null or empty");
            }

            return (true, String.Empty);
        }



        public async Task<(AdminsAddBoardGameResponse?, string)> AddBoardGame(AdminsAddBoardGameRequest? request)
        {
            var (isValid, message) = AddBoardGame_Validation(request);
            
            if (!isValid)
            {
                return (null, message);
            }

            

            var newBoardGame = new BoardGame
            {
                Name = request!.BoardGameName,
                Description = request.BoardGameDescription,
                MinPlayersCount = request.MinPlayersCount,
                MaxPlayersCount = request.MaxPlayersCount,
                MinAge = request.MinAge,
                CategoryId = request.CategoryId
            };

            var boardGameMechanics = new List<BoardGameMechanics>();
            foreach (var gameMechanicId in request!.BoardGameMechanicIds)
            {
                boardGameMechanics.Add(new BoardGameMechanics
                {
                    BoardGameId = newBoardGame.Id,
                    MechanicId = gameMechanicId
                });
            }

            newBoardGame.BoardGameMechanics = boardGameMechanics;

            await _daoDbContext.BoardGames.AddAsync(newBoardGame);

            await _daoDbContext.SaveChangesAsync();

            return (null, "Board game added successfully");
        }

        private static (bool, string) AddBoardGame_Validation(AdminsAddBoardGameRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.BoardGameName))
            {
                return (false, "Error: name is null or empty");
            }

            if (request.MinPlayersCount < 1)
            {
                return (false, "Error: MinPlayersCount is less than 1");
            }

            if (request.MaxPlayersCount < 1)
            {
                return (false, "Error: MaxPlayersCount is less than 1");
            }

            if (request.MinAge < 1)
            {
                return (false, "Error: MinAge is less than 1");
            }

            if (request.CategoryId < 0)
            {
                return (false, "Error: CategoryId is less than 1");
            }

            if(request.BoardGameMechanicIds == null || request.BoardGameMechanicIds.Count == 0)
            {
                return (false, "Error: BoardGameMechanics is null");
            }

            return (true, String.Empty);
        }

    }
}