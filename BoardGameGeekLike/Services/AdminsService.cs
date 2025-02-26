using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using Microsoft.EntityFrameworkCore;

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

        public async Task<(AdminsEditCategoryResponse?, string)> EditCategory(AdminsEditCategoryRequest? request)
        {
            var (isValid, message) = EditCategory_Validation(request);

            if (!isValid)
            {
                return (null, message);
            }

            var categoryDB = await this._daoDbContext.Categories
                                       .FindAsync(request!.CategoryId);

            if (categoryDB == null)
            {
                return (null, "Error: category not found");
            }

            if(categoryDB.IsDeleted == true)
            {
                return (null, "Error: category is deleted");
            }

            await this._daoDbContext
                      .Categories
                      .Where(a => a.Id == request.CategoryId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.Name, request.CategoryName));

            return (null, "Category edited successfully");
        }

        private static (bool, string) EditCategory_Validation(AdminsEditCategoryRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return (false, "Error: name is null or empty");
            }

            return (true, String.Empty);
        }

        public async Task<(AdminsDeleteCategoryResponse?, string)> DeleteCategory(AdminsDeleteCategoryRequest? request)
        {
            var (isValid, message) = DeleteCategory_Validation(request);
            if (!isValid)
            {
                return (null, message);
            }

            var categoryDB = await this._daoDbContext
                                       .Categories
                                       .FindAsync(request!.CategoryId);

            if (categoryDB == null)
            {
                return (null, "Error: category not found");
            }

            if(categoryDB.IsDeleted == true)
            {
                return (null, "Error: category was already deleted");
            }

            await this._daoDbContext
                      .Categories
                      .Where(a => a.Id == request.CategoryId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, true));

            return (null, "Category deleted successfully");
        }

        private static (bool, string) DeleteCategory_Validation(AdminsDeleteCategoryRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");
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

        public async Task<(AdminsEditBoardGameResponse?, string)> EditBoardGame(AdminsEditBoardGameRequest? request)
        {
            var (isValid, message) = EditBoardGame_Validation(request);
            if (!isValid)
            {
                return (null, message);
            }

            var boardGameDB = await this._daoDbContext
                                        .BoardGames
                                        .FindAsync(request!.BoardGameId);

            if (boardGameDB == null)
            {
                return (null, "Error: board game not found");
            }

            if(boardGameDB.IsDeleted == true)
            {
                return (null, "Error: board game is deleted");
            }

            var mechanicIdsDB = await this._daoDbContext
                                          .Mechanics
                                          .Select(a => a.Id)
                                          .ToListAsync();

            var invalidMechanicIds = mechanicIdsDB.Except(request.MechanicsIds);

            foreach(var mechanicId in request.MechanicsIds!)
            {
                var mechanicDB = await this._daoDbContext
                                          .Mechanics
                                          .FindAsync(mechanicId);

                if (mechanicDB == null)
                {
                    return (null, "Error: mechanic not found");
                }
            }
        

            

            return (null, "Board game edited successfully");
        }

        private static (bool, string) EditBoardGame_Validation(AdminsEditBoardGameRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            if (string.IsNullOrWhiteSpace(request.BoardGameName))
            {
                return (false, "Error: name is null or empty");
            }

            if (request.MinPlayersCount < 1)
            {
                return (false, "Error: invalid MinPlayersCount (is less than 1");
            }

            if (request.MaxPlayersCount < 1)
            {
                return (false, "Error: invalid MaxPlayersCount (is less than 1)");
            }

            if (request.MinAge < 1)
            {
                return (false, "Error: invalid MinAge (is less than 1)");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");
            }

            if(request.MechanicsIds == null || request.MechanicsIds.Count == 0)
            {
                return (false, "Error: MechanicsIds is null");
            }

            return (true, String.Empty);
        }
    }
}