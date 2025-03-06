using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace BoardGameGeekLike.Services
{
    public class AdminsService
    {
        private readonly ApplicationDbContext _daoDbContext;

        public AdminsService(ApplicationDbContext daoDbContext)
        {
            this._daoDbContext = daoDbContext;
        }

        public async Task<(AdminsAddCategoryResponse?, string)> AddCategory(AdminsAddCategoryRequest? request)
        {
            var (isValid, message) = AddCategory_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var name_exists = await this._daoDbContext
                                        .Categories
                                        .AsNoTracking()
                                        .AnyAsync(a => a.IsDeleted == false && a.Name == request!.CategoryName!.Trim());

            if(name_exists == true)
            {
                return(null, "Error: requested CategoryName is already in use");
            } 

            var newCategory = new Category
            {
                Name = request!.CategoryName!
            };

            await this._daoDbContext.Categories.AddAsync(newCategory);

            await this._daoDbContext.SaveChangesAsync();

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
                return (false, "Error: CategoryName is missing");
            }

            return (true, String.Empty);
        }

        public async Task<(AdminsEditCategoryResponse?, string)> EditCategory(AdminsEditCategoryRequest? request)
        {
            var (isValid, message) = EditCategory_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var name_exists = await this._daoDbContext
                                        .Categories
                                        .AsNoTracking()
                                        .AnyAsync(a => a.Id != request!.CategoryId && 
                                                       a.IsDeleted == false && 
                                                       a.Name == request!.CategoryName!.Trim());
                                        
            if(name_exists == true)
            {
                return(null, "Error: requested category name is already in use");
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

            if(request.CategoryId.HasValue == false)
            {
                return (false, "Error: CategoryId is missing");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName) == true)
            {
                return (false, "Error: Category name is missing");
            }

            return (true, String.Empty);
        }

        public async Task<(AdminsDeleteCategoryResponse?, string)> DeleteCategory(AdminsDeleteCategoryRequest? request)
        {
            var (isValid, message) = DeleteCategory_Validation(request);
            if (isValid == false)
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

            if(request.CategoryId.HasValue == false)
            {
                return(false, "Error: CategoryId is missing");
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

            if (isValid == false)
            {
                return (null, message);
            }

            var name_exists = await this._daoDbContext
                                        .Mechanics
                                        .AsNoTracking()
                                        .AnyAsync(a => a.IsDeleted == false && a.Name == request!.MechanicName!.Trim());
                                        
            if(name_exists == true)
            {
                return(null, "Error: requested mechanic name is already in use");
            } 

            var newMechanic = new Mechanic
            {
                Name = request!.MechanicName!
            };

            await this._daoDbContext.Mechanics.AddAsync(newMechanic);

            await this._daoDbContext.SaveChangesAsync();

            return (null, "Category added successfully");
        }
        
        private static (bool, string) AddMechanic_Validation(AdminsAddMechanicRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.MechanicName) == true)
            {
                return (false, "Error: MechanicName is missing");
            }

            return (true, String.Empty);
        }

        public async Task<(AdminsAddBoardGameResponse?, string)> AddBoardGame(AdminsAddBoardGameRequest? request)
        {
            var (isValid, message) = AddBoardGame_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var name_exists = await this._daoDbContext
                                        .BoardGames
                                        .AsNoTracking()
                                        .AnyAsync(a => a.IsDeleted == false && a.Name == request!.BoardGameName!.Trim());

            if(name_exists == true)
            {
                return(null, "Error: requested board game name is already in use");
            }      

            var newBoardGame = new BoardGame
            {
                Name = request!.BoardGameName!,
                Description = request.BoardGameDescription!,
                MinPlayersCount = request.MinPlayersCount!.Value,
                MaxPlayersCount = request.MaxPlayersCount!.Value,
                MinAge = request.MinAge!.Value,
                CategoryId = request.CategoryId == null ? 1 : request.CategoryId.Value
            };

            var boardGameMechanics = new List<Mechanic>();
            
            if(request.MechanicIds == null || request.MechanicIds.Count == 0)
            {
                var mechnicNotDetermined = await this._daoDbContext
                                                     .Mechanics
                                                     .FindAsync(1);

                boardGameMechanics.Add(mechnicNotDetermined!);            
            }
            else
            {
                var mechanicIdsDB = await this._daoDbContext
                                                 .Mechanics
                                                 .Where(a => a.IsDeleted == false)
                                                 .Select(a => a.Id)
                                                 .ToListAsync();

                var invalidMechanicIds = request.MechanicIds.Except(mechanicIdsDB).ToList();

                if(invalidMechanicIds != null && invalidMechanicIds.Count > 0)
                {
                    message = "Error: requested MechnicId";
                    message += invalidMechanicIds.Count == 1 ?
                              $": {invalidMechanicIds[0]} was" :
                              $"s: {string.Join(", ",invalidMechanicIds)} were ";

                    message += "not found";

                    return (null, message);
                }

                var mechanics = await this._daoDbContext
                                          .Mechanics
                                          .Where(a => request.MechanicIds.Contains(a.Id))
                                          .ToListAsync();

                foreach (var mechanic in mechanics)
                {
                    boardGameMechanics.Add(mechanic);
                }
            }

            newBoardGame.Mechanics = boardGameMechanics;

            await this._daoDbContext.BoardGames.AddAsync(newBoardGame);

            await this._daoDbContext.SaveChangesAsync();

            return (null, "Board game added successfully");
        }

        private static (bool, string) AddBoardGame_Validation(AdminsAddBoardGameRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.BoardGameName) == true)
            {
                return (false, "Error: BoarGameName is missing");
            }

            if(request.MinPlayersCount.HasValue == false)
            {
                return (false, "Error: MinPlayersCount is missing");
            }

            if (request.MinPlayersCount < 1)
            {
                return (false, "Error: MinPlayersCount is less than 1");
            }

            if(request.MaxPlayersCount.HasValue == false)
            {
                return (false, "Error: MaxPlayersCount is missing");
            }

            if (request.MaxPlayersCount < 1)
            {
                return (false, "Error: MaxPlayersCount is less than 1");
            }

            if(request.MinAge.HasValue == false)
            {
                return (false, "Error: MinAge is missing");
            }

            if (request.MinAge < 1)
            {
                return (false, "Error: MinAge is less than 1");
            }

            if (request.CategoryId.HasValue == false) 
            {
                return (false, "Error: CategoryId is missing)");
            }

            if(request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");

            }

            return (true, String.Empty);
        }

        public async Task<(AdminsEditBoardGameResponse?, string)> EditBoardGame(AdminsEditBoardGameRequest? request)
        {
            var (isValid, message) = EditBoardGame_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var boardGameName_exists = await this._daoDbContext
                                                 .BoardGames
                                                 .AsNoTracking()
                                                 .AnyAsync(a => a.Id != request!.BoardGameId && 
                                                                a.IsDeleted == false && 
                                                                a.Name == request.BoardGameName!.Trim());
                                                 

            if(boardGameName_exists == true)
            {
                return(null, "Error: board game name is already in use");
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

            var categoryIdsDB = await this._daoDbContext
                                          .Categories
                                          .Select(a => a.Id)
                                          .ToListAsync();

            if(categoryIdsDB == null || categoryIdsDB.Count == 0)
            {
                return (null, "Error: no categories found");
            }

            var CategoryId_exists = categoryIdsDB.Contains(request.CategoryId!.Value);

            if(CategoryId_exists == false)
            {
                return (null, $"Error: the requested categoryId ({request.CategoryId}) does not exist");
            }


            var boardGameMechanics = new List<Mechanic>();
            
            if(request.MechanicIds == null || request.MechanicIds.Count == 0)
            {
                var mechnicNotDetermined = await this._daoDbContext
                                                     .Mechanics
                                                     .FindAsync(1);

                boardGameMechanics.Add(mechnicNotDetermined!);            
            }
            else
            {
                var mechanicIdsDB = await this._daoDbContext
                                                 .Mechanics
                                                 .Where(a => a.IsDeleted == false)
                                                 .Select(a => a.Id)
                                                 .ToListAsync();

                if(mechanicIdsDB == null || mechanicIdsDB.Count == 0)
                {
                    return (null, "Error: no board game mechanic was found");
                }

                var invalidMechanicIds = request.MechanicIds.Except(mechanicIdsDB).ToList();

                if(invalidMechanicIds != null && invalidMechanicIds.Count > 0)
                {
                    message = "Error: requested MechnicId";
                    message += invalidMechanicIds.Count == 1 ?
                              $": {invalidMechanicIds[0]} was" :
                              $"s: {string.Join(", ",invalidMechanicIds)} were ";

                    message += "not found";

                    return (null, message);
                }

                var mechanics = await this._daoDbContext
                                          .Mechanics
                                          .Where(a => request.MechanicIds.Contains(a.Id))
                                          .ToListAsync();

                foreach (var mechanic in mechanics)
                {
                    boardGameMechanics.Add(mechanic);
                }
            }

            boardGameDB.Name = request.BoardGameName!;
            boardGameDB.Description = request.BoardGameDescription == null ? String.Empty : request.BoardGameDescription;
            boardGameDB.MinPlayersCount = request.MinPlayersCount!.Value;
            boardGameDB.MaxPlayersCount = request.MaxPlayersCount!.Value;
            boardGameDB.MinAge = request.MinAge!.Value;
            boardGameDB.CategoryId = request.CategoryId!.Value;
            boardGameDB.Mechanics = boardGameMechanics;

            await this._daoDbContext.SaveChangesAsync();     

            return (null, "Board game edited successfully");
        }

        private static (bool, string) EditBoardGame_Validation(AdminsEditBoardGameRequest? request)
        {
             if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.BoardGameName) == true)
            {
                return (false, "Error: BoarGameName is missing");
            }

            if(request.MinPlayersCount.HasValue == false)
            {
                return (false, "Error: MinPlayersCount is missing");
            }

            if (request.MinPlayersCount < 1)
            {
                return (false, "Error: MinPlayersCount is less than 1");
            }

            if(request.MaxPlayersCount.HasValue == false)
            {
                return (false, "Error: MaxPlayersCount is missing");
            }

            if (request.MaxPlayersCount < 1)
            {
                return (false, "Error: MaxPlayersCount is less than 1");
            }

            if(request.MinAge.HasValue == false)
            {
                return (false, "Error: MinAge is missing");
            }

            if (request.MinAge < 1)
            {
                return (false, "Error: MinAge is less than 1");
            }

            if (request.CategoryId.HasValue == false) 
            {
                return (false, "Error: CategoryId is missing)");
            }

            if(request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");

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
                                        .FindAsync(request!.BoardGameId);
            
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

            if(request.BoardGameId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, String.Empty);
        }
    }
}