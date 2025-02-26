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
                Description = request!.BoardGameDescription,
                MinPlayersCount = request.MinPlayersCount,
                MaxPlayersCount = request.MaxPlayersCount,
                MinAge = request.MinAge,
                CategoryId = request.CategoryId == null ? 1 : request.CategoryId.Value
            };

            var boardGameMechanics = new List<BoardGameMechanics>();
            
            if(request.BoardGameMechanicIds == null || request.BoardGameMechanicIds.Count == 0)
            {
                boardGameMechanics.Add(new BoardGameMechanics
                {
                    BoardGameId = newBoardGame.Id,
                    MechanicId = 1
                });
            }
            else
            {
                foreach (var gameMechanicId in request!.BoardGameMechanicIds)
                {
                    boardGameMechanics.Add(new BoardGameMechanics
                    {
                        BoardGameId = newBoardGame.Id,
                        MechanicId = gameMechanicId
                    });
                }
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

            var boardGameName_exists = await this._daoDbContext
                                                .BoardGames
                                                .Where(a => a.Id != request.BoardGameId && a.IsDeleted == false && a.Name == request.BoardGameName.Trim())
                                                .AnyAsync();
            if(boardGameName_exists == true)
            {
                return(null, "Error: board game name already in use");
            }

            var categoryIdsDB = await this._daoDbContext
                                         .Categories
                                         .Select(a => a.Id)
                                         .ToListAsync();

            if(categoryIdsDB == null || categoryIdsDB.Count == 0)
            {
                return (null, "Error: no categories found");
            }

            var CategoryId_exists = categoryIdsDB.Contains(request.CategoryId);

            if(CategoryId_exists == false)
            {
                return (null, $"Error: the requested categoryId ({request.CategoryId}) does not exist");
            }

            var mechanicIdsDB = await this._daoDbContext
                                          .Mechanics
                                          .Select(a => a.Id)
                                          .ToListAsync();

            if(mechanicIdsDB == null || mechanicIdsDB.Count == 0)
            {
                return (null, "Error: no board game mechaniscs found");
            }

            var invalidMechanicIds = request.MechanicIds.Except(mechanicIdsDB).ToList();

            if(invalidMechanicIds.Count != 0)
            {
                message = "Error: the requested ";
                
                message += invalidMechanicIds.Count == 1 ? 
                    $"mechanicId (id = {invalidMechanicIds[0]}) does not exist" :
                    $"mechanicIds (id = {string.Join(", ",invalidMechanicIds)}) do not exist" ;

                return (null, message);
            }

            var boardGameMechanics = new List<BoardGameMechanics>();

            foreach(var mechanicId in request.MechanicIds!)
            {
                boardGameMechanics.Add(new BoardGameMechanics
                {
                    BoardGameId = boardGameDB.Id,
                    MechanicId = mechanicId
                });
            }

            boardGameDB.Name = request.BoardGameName;
            boardGameDB.Description = request.BoardGameDescription;
            boardGameDB.MinPlayersCount = request.MinPlayersCount;
            boardGameDB.MaxPlayersCount = request.MaxPlayersCount;
            boardGameDB.MinAge = request.MinAge;
            boardGameDB.CategoryId = request.CategoryId;
            boardGameDB.BoardGameMechanics = boardGameMechanics;

            await this._daoDbContext.SaveChangesAsync();     

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

            if(request.MechanicIds == null || request.MechanicIds.Count == 0)
            {
                return (false, "Error: MechanicsIds is null");
            }

            return (true, String.Empty);
        }
    
        public async Task<(AdminsDeleteBoardGameResponse?, string)> DeleteBoardGame(AdminsDeleteBoardGameRequest? request)
        {
            var (isValid, message) = DeleteBoardGame_Validation(request);
            if(isValid == false)
            {
                return(null, message);
            }

            var boardGameDb = await this._daoDbContext
                                        .BoardGames
                                        .FirstOrDefaultAsync(a => a.Id == request!.BoardGameId);
            
            if(boardGameDb == null)
            {
                return (null, "Error: board game not found");
            }

            if(boardGameDb.IsDeleted == true)
            {
                return(null, "Error: board game was already deleted");
            }
            
            boardGameDb.IsDeleted = true;
        
            await this._daoDbContext
                      .BoardGames
                      .Where(a => a.Id == request!.BoardGameId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, true));

            return (null, "Board game deleted successfully");
        }

        private static (bool, string) DeleteBoardGame_Validation(AdminsDeleteBoardGameRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, String.Empty);
        }
    }
}