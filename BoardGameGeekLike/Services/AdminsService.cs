using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Models.Enums;
using BoardGameGeekLike.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Constants = BoardGameGeekLike.Utilities.Constants;

namespace BoardGameGeekLike.Services
{
    public class AdminsService
    {
        private readonly ApplicationDbContext _daoDbContext;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminsService(ApplicationDbContext daoDbContext, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            this._daoDbContext = daoDbContext;
            this._userManager = userManager;
            this._httpContextAccessor = httpContextAccessor;
        }


        #region Board Games (BG)      

        public async Task<(List<AdminsListBoardGamesResponse>?, string)> ListBoardGames(AdminsListBoardGamesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListBoardGames_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var boardGamesDB = await this._daoDbContext
            .BoardGames
                .Select(a => new AdminsListBoardGamesResponse
                {
                    BoardGameId = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    PlayersCount = a.MinPlayersCount == a.MaxPlayersCount ? $"{a.MinPlayersCount}" : $"{a.MinPlayersCount} - {a.MaxPlayersCount}",
                    MinAge = a.MinAge,
                    Category = a.Category!.Name,
                    Mechanics = a.Mechanics!.Select(b => b.Name).ToList(),
                    IsDeleted = a.IsDeleted
                })
                .OrderBy(a => a.IsDeleted)
                .ThenBy(a => a.Name)
                .ToListAsync();

            if (boardGamesDB == null || boardGamesDB.Count == 0)
            {
                return (null, "Error: no board game found");
            }

            return (boardGamesDB, "Board games successfully listed by rate");
        }

        private static (bool, string) ListBoardGames_Validation(AdminsListBoardGamesRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is not null");
            }

            return (true, string.Empty);
        }

        public async Task<(AdminsAddBoardGameResponse?, string)> AddBoardGame(AdminsAddBoardGameRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = AddBoardGame_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var name_exists = await this._daoDbContext
                .BoardGames
                .AsNoTracking()
                .AnyAsync(a => a.IsDeleted == false &&
                               a.Name == request!.BoardGameName!.Trim());

            if (name_exists == true)
            {
                return (null, "Error: requested board game name is already in use");
            }

            var categoryDB = await this._daoDbContext
                .Categories
                .FindAsync(request!.CategoryId);

            if (categoryDB == null)
            {
                return (null, "Error: requested category not found");
            }

            if (categoryDB.IsDeleted == true)
            {
                return (null, "Error: requested category was deleted");
            }

            var mechanicIdsDB = await this._daoDbContext
                .Mechanics
                .Select(a => a.Id)
                .ToListAsync();

            if (mechanicIdsDB == null || mechanicIdsDB.Count < 1)
            {
                return (null, "Error: no mechanic found");
            }

            var invalidMechanicIds = request.MechanicIds!.Except(mechanicIdsDB).ToList();

            if (invalidMechanicIds != null && invalidMechanicIds.Count > 0)
            {
                message = "Error: requested MechnicId";
                message += invalidMechanicIds.Count == 1 ?
                          $": {invalidMechanicIds[0]} was " :
                          $"s: {string.Join(", ", invalidMechanicIds)} were ";

                message += "not found";

                return (null, message);
            }

            var mechanicsDB = await this._daoDbContext
                .Mechanics
                .Where(a => request.MechanicIds!.Contains(a.Id))
                .ToListAsync();

            var deletedMechanics = new List<Mechanic>() { };

            var boardGameMechanics = new List<Mechanic>();

            foreach (var mechanic in mechanicsDB)
            {
                if (mechanic.IsDeleted == true)
                {
                    deletedMechanics.Add(mechanic);
                }

                boardGameMechanics.Add(mechanic);
            }

            if (deletedMechanics != null && deletedMechanics.Count > 0)
            {
                message = "Error: the requested mechanic";
                message += deletedMechanics.Count == 1 ? $" (MechanicId: {deletedMechanics[0].Id + " - " + deletedMechanics[0].Name}) was deleted" :
                                                         $"s (MechanicIds: " +
                                                         $"{string.Join(", ", deletedMechanics
                                                                .Select(a => a.Id)
                                                                .ToList()) +
                                                            " - " +
                                                            string.Join(", ", deletedMechanics
                                                                .Select(a => a.Name)
                                                                .ToList())}) were deleted";
                return (null, message);
            }

            var newBoardGame = new BoardGame
            {
                Name = request!.BoardGameName!,
                Description = request.BoardGameDescription!,
                MinPlayersCount = request.MinPlayersCount!.Value,
                MaxPlayersCount = request.MaxPlayersCount!.Value,
                MinAge = request.MinAge!.Value,
                Category = categoryDB,
                Mechanics = mechanicsDB
            };

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

            if (request.MinPlayersCount.HasValue == false)
            {
                return (false, "Error: MinPlayersCount is missing");
            }

            if (request.MinPlayersCount < 1)
            {
                return (false, "Error: MinPlayersCount is less than 1");
            }

            if (request.MaxPlayersCount.HasValue == false)
            {
                return (false, "Error: MaxPlayersCount is missing");
            }

            if (request.MaxPlayersCount < 1)
            {
                return (false, "Error: MaxPlayersCount is less than 1");
            }

            if (request.MinAge.HasValue == false)
            {
                return (false, "Error: MinAge is missing");
            }

            if (request.MinAge < 1)
            {
                return (false, "Error: MinAge is less than 1");
            }

            if (request.CategoryId.HasValue == false)
            {
                return (false, "Error: no CategoryId informed");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");

            }

            if (request.MechanicIds == null || request.MechanicIds.Count < 1)
            {
                return (false, "Error: no MechanicId(s) informed");
            }

            if (request.MechanicIds != null && request.MechanicIds.Count > 0 && request.MechanicIds.Any(a => a < 0))
            {
                var invalidMechanicIds = request.MechanicIds.Where(a => a < 0).ToList();

                var message = "Error: invalid MechanicId";

                message += invalidMechanicIds.Count == 1 ? " (is less than 1)" :
                                                          $"s ({invalidMechanicIds.Count} negative values were requested)";

                return (false, message);
            }

            return (true, string.Empty);
        }

        public async Task<(AdminsShowBoardGameDetailsResponse?, string)> ShowBoardGameDetails(AdminsShowBoardGameDetailsRequest? request)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ShowBoardGameDetails_Validation(request);
            if (!isValid)
            {
                return (null, message);
            }

            var boardgameDB = await _daoDbContext
                .BoardGames
                .Include(a => a.Category)
                .Include(a => a.Mechanics)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request!.BoardGameId);

            if (boardgameDB == null)
            {
                return (null, "Error: Requested BoardGame not found");
            }

            if (boardgameDB?.Category == null)
            {
                return (null, "Error: BoardGame category not found");
            }

            var categoryName = boardgameDB.Category.Id!;

            if (boardgameDB.Mechanics == null || boardgameDB.Mechanics.Count <= 0)
            {
                return (null, "Error: No BoardGame mechanics found");
            }

            var mechanicsNames = boardgameDB.Mechanics.Select(a => a.Id).ToList();

            var content = new AdminsShowBoardGameDetailsResponse
            {
                BoardGameId = boardgameDB.Id,
                BoardGameName = boardgameDB.Name,
                BoardGameDescription = boardgameDB.Description,
                MinPlayersCount = boardgameDB.MinPlayersCount,
                MaxPlayerCount = boardgameDB.MaxPlayersCount,
                MinAge = boardgameDB.MinAge,
                Category = categoryName!,
                Mechanics = mechanicsNames,
                IsDeleted = boardgameDB.IsDeleted
            };

            return (content, "Board game details listed successfully");
        }

        private static (bool, string) ShowBoardGameDetails_Validation(AdminsShowBoardGameDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.BoardGameId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (requested BoardGameId is zero or a negative number)");
            }

            return (true, string.Empty);
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
                    a.Name == request.BoardGameName!.Trim());


            if (boardGameName_exists == true)
            {
                return (null, "Error: board game name is already in use");
            }

            var boardGameDB = await this._daoDbContext
                .BoardGames
                .Include(a => a.Mechanics)
                .FirstOrDefaultAsync(a => a.Id == request!.BoardGameId);

            if (boardGameDB == null)
            {
                return (null, "Error: board game not found");
            }

            var categoryIdsDB = await this._daoDbContext
                .Categories
                .AsNoTracking()
                .Select(a => a.Id)
                .ToListAsync();

            if (categoryIdsDB == null || categoryIdsDB.Count == 0)
            {
                return (null, "Error: no categories found");
            }

            var CategoryId_exists = categoryIdsDB.Contains(request.CategoryId!.Value);

            if (CategoryId_exists == false)
            {
                return (null, "Error: requested category not found");
            }

            var mechanicsDB = await this._daoDbContext
                .Mechanics
                .ToListAsync();

            if (mechanicsDB == null || mechanicsDB.Count == 0)
            {
                return (null, "Error: no mechanic found");
            }

            var availableMechanicIds = mechanicsDB.Select(a => a.Id).ToList();

            var invalidMechanicIds = request.MechanicIds!.Except(availableMechanicIds).ToList();

            if (invalidMechanicIds != null && invalidMechanicIds.Count > 0)
            {
                message = "Error: requested MechnicId";
                message += invalidMechanicIds.Count == 1 ?
                          $": {invalidMechanicIds[0]} was" :
                          $"s: {string.Join(", ", invalidMechanicIds)} were ";

                message += "not found";

                return (null, message);
            }

            var foundMechanics = mechanicsDB.Where(a => request.MechanicIds!.Contains(a.Id)).ToList();

            var deletedMechanics = new List<Mechanic>() { };

            var boardGameMechanics = new List<Mechanic>();

            foreach (var mechanic in foundMechanics)
            {
                if (mechanic.IsDeleted == true)
                {
                    deletedMechanics.Add(mechanic);
                }

                boardGameMechanics.Add(mechanic);
            }

            if (deletedMechanics != null && deletedMechanics.Count > 0)
            {
                message = "Error: the requested mechanic";
                message += deletedMechanics.Count == 1 ? $" (MechanicId: {deletedMechanics[0].Id + " - " + deletedMechanics[0].Name}) was deleted" :
                                                         $"s (MechanicIds: " +
                                                         $"{string.Join(", ", deletedMechanics
                                                                .Select(a => a.Id)
                                                                .ToList()) +
                                                            " - " +
                                                            string.Join(", ", deletedMechanics
                                                                .Select(a => a.Name)
                                                                .ToList())}) were deleted";
                return (null, message);
            }


            boardGameDB.Name = request.BoardGameName!;
            boardGameDB.Description = request.BoardGameDescription == null ? string.Empty : request.BoardGameDescription;
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

            if (request.MinPlayersCount.HasValue == false)
            {
                return (false, "Error: MinPlayersCount is missing");
            }

            if (request.MinPlayersCount < 1)
            {
                return (false, "Error: MinPlayersCount is less than 1");
            }

            if (request.MaxPlayersCount.HasValue == false)
            {
                return (false, "Error: MaxPlayersCount is missing");
            }

            if (request.MaxPlayersCount < 1)
            {
                return (false, "Error: MaxPlayersCount is less than 1");
            }

            if (request.MinAge.HasValue == false)
            {
                return (false, "Error: MinAge is missing");
            }

            if (request.MinAge < 1)
            {
                return (false, "Error: MinAge is less than 1");
            }

            if (request.CategoryId.HasValue == false)
            {
                return (false, "Error: no CategoryId informed");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");

            }

            if (request.MechanicIds == null || request.MechanicIds.Count < 1)
            {
                return (false, "Error: no MechanicId(s) informed");
            }

            if (request.MechanicIds != null && request.MechanicIds.Count > 0 && request.MechanicIds.Any(a => a < 0))
            {
                var invalidMechanicIds = request.MechanicIds.Where(a => a < 0).ToList();

                var message = "(Error: invalid MechanicId";

                message += invalidMechanicIds.Count == 1 ? " (is less than 1)" :
                                                          $"s ({invalidMechanicIds.Count} negative values were requested)";

                return (false, message);
            }

            return (true, string.Empty);
        }

        public async Task<(AdminsDeleteBoardGameResponse?, string)> DeleteBoardGame(AdminsDeleteBoardGameRequest? request)
        {
            var (isValid, message) = DeleteBoardGame_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var boardGameDb = await this._daoDbContext
                                        .BoardGames
                                        .FindAsync(request!.BoardGameId);

            if (boardGameDb == null)
            {
                return (null, "Error: board game not found");
            }

            if (boardGameDb.IsDeleted == true)
            {
                return (null, "Error: board game was already deleted");
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

            if (request.BoardGameId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, string.Empty);
        }

        public async Task<(AdminsRestoreBoardGameResponse?, string)> RestoreBoardGame(AdminsRestoreBoardGameRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = RestoreBoardGame_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var boardGameDb = await this._daoDbContext
                                        .BoardGames
                                        .FindAsync(request!.BoardGameId);

            if (boardGameDb == null)
            {
                return (null, "Error: board game not found");
            }

            if (boardGameDb.IsDeleted == false)
            {
                return (null, "Error: board game was already restored");
            }

            boardGameDb.IsDeleted = true;

            await this._daoDbContext
                      .BoardGames
                      .Where(a => a.Id == request!.BoardGameId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, false));

            return (new AdminsRestoreBoardGameResponse(), "Board game restored successfully");
        }

        private static (bool, string) RestoreBoardGame_Validation(AdminsRestoreBoardGameRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.BoardGameId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, string.Empty);
        }

        //
        //  CATEGORIES
        //
        public async Task<(List<AdminsListCategoriesResponse>?, string)> ListCategories(AdminsListCategoriesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListCategories_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var categoryDB = await this._daoDbContext
            .Categories
                .Select(a => new AdminsListCategoriesResponse
                {
                    CategoryId = a.Id,
                    Name = a.Name,
                    IsDeleted = a.IsDeleted
                })
                .OrderBy(a => a.IsDeleted)
                .ThenBy(a => a.Name)
                .ToListAsync();

            if (categoryDB == null || categoryDB.Count == 0)
            {
                return (null, "Error: no category found");
            }

            return (categoryDB, "Categories listed successfully");
        }

        private static (bool, string) ListCategories_Validation(AdminsListCategoriesRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is not null");
            }

            return (true, string.Empty);
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

            if (name_exists == true)
            {
                return (null, "Error: requested CategoryName is already in use");
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

            return (true, string.Empty);
        }

        public async Task<(AdminsShowCategoryDetailsResponse?, string)> ShowCategoryDetails(AdminsShowCategoryDetailsRequest? request)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ShowCategoryDetails_Validation(request);
            if (!isValid)
            {
                return (null, message);
            }

            var categoryDB = await _daoDbContext
                .Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request!.CategoryId);

            if (categoryDB == null)
            {
                return (null, "Error: Requested Category not found");
            }

            var content = new AdminsShowCategoryDetailsResponse
            {
                CategoryId = categoryDB.Id,
                CategoryName = categoryDB.Name,
                IsDeleted = categoryDB.IsDeleted
            };

            return (content, "Category details listed successfully");
        }

        private static (bool, string) ShowCategoryDetails_Validation(AdminsShowCategoryDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.CategoryId.HasValue == false)
            {
                return (false, "Error: CategoryId is missing");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (requested CategoryId is zero or a negative number)");
            }

            return (true, string.Empty);
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

            if (name_exists == true)
            {
                return (null, "Error: requested category name is already in use");
            }

            var categoryDB = await this._daoDbContext
                                       .Categories
                                       .FindAsync(request!.CategoryId);

            if (categoryDB == null)
            {
                return (null, "Error: category not found");
            }

            if (categoryDB.IsDeleted == true)
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

            if (request.CategoryId.HasValue == false)
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

            return (true, string.Empty);
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

            if (categoryDB.IsDeleted == true)
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

            if (request.CategoryId.HasValue == false)
            {
                return (false, "Error: CategoryId is missing");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsRestoreCategoryResponse?, string)> RestoreCategory(AdminsRestoreCategoryRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = RestoreCategory_Validation(request);

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

            if (categoryDB.IsDeleted == false)
            {
                return (null, "Error: category was already restored");
            }

            categoryDB.IsDeleted = true;

            await this._daoDbContext
                      .Categories
                      .Where(a => a.Id == request!.CategoryId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, false));

            return (new AdminsRestoreCategoryResponse(), "Category restored successfully");
        }

        private static (bool, string) RestoreCategory_Validation(AdminsRestoreCategoryRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.CategoryId.HasValue == false)
            {
                return (false, "Error: CategoryId is missing");
            }

            if (request.CategoryId < 1)
            {
                return (false, "Error: invalid CategoryId (is less than 1)");
            }

            return (true, string.Empty);
        }

        //
        //  MECHANICS
        //
        public async Task<(List<AdminsListMechanicsResponse>?, string)> ListMechanics(AdminsListMechanicsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListMechanics_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mechanicDB = await this._daoDbContext
            .Mechanics
                .Select(a => new AdminsListMechanicsResponse
                {
                    MechanicId = a.Id,
                    Name = a.Name,
                    IsDeleted = a.IsDeleted
                })
                .OrderBy(a => a.IsDeleted)
                .ThenBy(a => a.Name)
                .ToListAsync();

            if (mechanicDB == null || mechanicDB.Count == 0)
            {
                return (null, "Error: no mechanic found");
            }

            return (mechanicDB, "Mechanics listed successfully");
        }

        private static (bool, string) ListMechanics_Validation(AdminsListMechanicsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is not null");
            }

            return (true, string.Empty);
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

            if (name_exists == true)
            {
                return (null, "Error: requested mechanic name is already in use");
            }

            var newMechanic = new Mechanic
            {
                Name = request!.MechanicName!
            };

            await this._daoDbContext.Mechanics.AddAsync(newMechanic);

            await this._daoDbContext.SaveChangesAsync();

            return (null, "Mechanic added successfully");
        }

        private static (bool, string) AddMechanic_Validation(AdminsAddMechanicRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.MechanicName))
            {
                return (false, "Error: MechanicName is missing");
            }

            return (true, string.Empty);
        }

        public async Task<(AdminsShowMechanicDetailsResponse?, string)> ShowMechanicDetails(AdminsShowMechanicDetailsRequest? request)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ShowMechanicDetails_Validation(request);
            if (!isValid)
            {
                return (null, message);
            }

            var mechanicDB = await _daoDbContext
                .Mechanics
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request!.MechanicId);

            if (mechanicDB == null)
            {
                return (null, "Error: Requested Mechanic not found");
            }

            var content = new AdminsShowMechanicDetailsResponse
            {
                MechanicId = mechanicDB.Id,
                MechanicName = mechanicDB.Name,
                IsDeleted = mechanicDB.IsDeleted
            };

            return (content, "Mechanic details listed successfully");
        }
        private static (bool, string) ShowMechanicDetails_Validation(AdminsShowMechanicDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.MechanicId.HasValue == false)
            {
                return (false, "Error: MechanicId is missing");
            }

            if (request.MechanicId < 1)
            {
                return (false, "Error: invalid MechanicId (requested MechanicId is zero or a negative number)");
            }

            return (true, string.Empty);
        }

        public async Task<(AdminsEditMechanicResponse?, string)> EditMechanic(AdminsEditMechanicRequest? request)
        {
            var (isValid, message) = EditMechanic_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var name_exists = await this._daoDbContext
                                        .Mechanics
                                        .AsNoTracking()
                                        .AnyAsync(a => a.Id != request!.MechanicId &&
                                                       a.IsDeleted == false &&
                                                       a.Name == request!.MechanicName!.Trim());

            if (name_exists == true)
            {
                return (null, "Error: requested mechanic name is already in use");
            }

            var mechanicDB = await this._daoDbContext
                                       .Mechanics
                                       .FindAsync(request!.MechanicId);

            if (mechanicDB == null)
            {
                return (null, "Error: mechanic not found");
            }

            if (mechanicDB.IsDeleted == true)
            {
                return (null, "Error: mechanic is deleted");
            }

            await this._daoDbContext
                      .Mechanics
                      .Where(a => a.Id == request.MechanicId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.Name, request.MechanicName));

            return (null, "Mechanic edited successfully");
        }

        private static (bool, string) EditMechanic_Validation(AdminsEditMechanicRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.MechanicId.HasValue == false)
            {
                return (false, "Error: MechanicId is missing");
            }

            if (request.MechanicId < 1)
            {
                return (false, "Error: invalid MechanicId (is less than 1)");
            }

            if (string.IsNullOrWhiteSpace(request.MechanicName) == true)
            {
                return (false, "Error: Mechanic name is missing");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsDeleteMechanicResponse?, string)> DeleteMechanic(AdminsDeleteMechanicRequest? request)
        {
            var (isValid, message) = DeleteMechanic_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mechanicDB = await this._daoDbContext
                                       .Mechanics
                                       .FindAsync(request!.MechanicId);

            if (mechanicDB == null)
            {
                return (null, "Error: mechanic not found");
            }

            if (mechanicDB.IsDeleted == true)
            {
                return (null, "Error: mechanic was already deleted");
            }

            await this._daoDbContext
                      .Mechanics
                      .Where(a => a.Id == request.MechanicId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, true));

            return (null, "Mechanic deleted successfully");
        }

        private static (bool, string) DeleteMechanic_Validation(AdminsDeleteMechanicRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.MechanicId.HasValue == false)
            {
                return (false, "Error: MechanicId is missing");
            }

            if (request.MechanicId < 1)
            {
                return (false, "Error: invalid MechanicId (is less than 1)");
            }

            return (true, string.Empty);
        }

        public async Task<(AdminsRestoreMechanicResponse?, string)> RestoreMechanic(AdminsRestoreMechanicRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = RestoreMechanic_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mechanicDB = await this._daoDbContext
                                        .Mechanics
                                        .FindAsync(request!.MechanicId);

            if (mechanicDB == null)
            {
                return (null, "Error: mechanic not found");
            }

            if (mechanicDB.IsDeleted == false)
            {
                return (null, "Error: mechanic was already restored");
            }

            mechanicDB.IsDeleted = true;

            await this._daoDbContext
                      .Mechanics
                      .Where(a => a.Id == request!.MechanicId)
                      .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, false));

            return (new AdminsRestoreMechanicResponse(), "Mechanic restored successfully");
        }

        private static (bool, string) RestoreMechanic_Validation(AdminsRestoreMechanicRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.MechanicId.HasValue == false)
            {
                return (false, "Error: MechanicId is missing");
            }

            if (request.MechanicId < 1)
            {
                return (false, "Error: invalid MechanicId (is less than 1)");
            }

            return (true, string.Empty);
        }


        #endregion


        #region Medieval Auto Battler (MAB)

        // MAB CARDS

        public async Task<(AdminsAddMabCardResponse?, string)> AddMabCard(AdminsAddMabCardRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = AddMabCard_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var exists = await this._daoDbContext
                .Cards
                .AnyAsync(a =>
                    a.Name.Trim().ToLower() == request.CardName.Trim().ToLower() && 
                    a.IsDeleted == false);

            if (exists == true)
            {
                return (null, $"Error: requested name is already taken, please choose another!");
            }

            var newCard = new Card
            {
                Name = request.CardName,
                Power = request.CardPower,
                UpperHand = request.CardUpperHand,
                Level = Helper.GetCardLevel(request.CardPower, request.CardUpperHand),
                Type = request.CardType,
                IsDeleted = false,
            };

            this._daoDbContext.Add(newCard);

            var savingSucceded = await this._daoDbContext.SaveChangesAsync();

            if(savingSucceded == 0)
            {
                return (null, "Error: saving DB modifications failed.");
            }

            return (new AdminsAddMabCardResponse(), "New Mab Card added successfully");
        }
        private static (bool, string) AddMabCard_Validation(AdminsAddMabCardRequest request)
        {
            if (request == null)
            {
                return (false, "Error: no information provided");
            }

            if (string.IsNullOrWhiteSpace(request.CardName) == true)
            {
                return (false, "Error: invalid CardName");
            }

            if (request.CardPower < Constants.MinCardPower || request.CardPower > Constants.MaxCardPower)
            {
                return (false, $"Error: invalid CardPowe. It must be between {Constants.MinCardPower} and {Constants.MaxCardPower}");
            }

            if (request.CardUpperHand < Constants.MinCardUpperHand || request.CardUpperHand > Constants.MaxCardUpperHand)
            {
                return (false, $"Error: invalid CardUpperHand. It must be between {Constants.MinCardUpperHand} and {Constants.MaxCardUpperHand}");
            }

            if (Enum.IsDefined(request.CardType) == false)
            {
                var validTypes = string
                    .Join(", ", Enum.GetValues(typeof(MabCardType))
                    .Cast<MabCardType>()
                    .Select(cardType => $"{cardType} ({(int)cardType})"));

                return (false, $"Error: invalid CardType. It must be one of the following: {validTypes}");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsShowMabCardDetailsResponse?, string)> ShowMabCardDetails(AdminsShowMabCardDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ShowMabCardDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCardDB = await this._daoDbContext
                .Cards
                .FindAsync(request!.CardId);

            if( mabCardDB == null )
            {
                return (null, "Error: Medieval Auto Battler Card not found!");
            }                

            return (new AdminsShowMabCardDetailsResponse
            {
                CardName = mabCardDB.Name,
                CardPower = mabCardDB.Power,
                CardUpperHand = mabCardDB.UpperHand,
                CardType = mabCardDB.Type,
            }, "Mab Card details fetched successfully!");
        }
        private static (bool, string) ShowMabCardDetails_Validation(AdminsShowMabCardDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is not null");
            }

            if(request.CardId <= 0)
            {
                return (false, "Error: invalid medieval auto battler CardId");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsEditMabCardResponse?, string)> EditMabCard(AdminsEditMabCardRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditMabCard_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var exist = await this._daoDbContext
                                   .Cards
                                   .AnyAsync(a => a.Name.ToLower() == request.CardName.Trim().ToLower());

            if (exist == true)
            {
                return (null, $"Error: invalid CardName: ->{request.CardName}<- .A card with this name already exists");
            }

            var cardDB = await this._daoDbContext
                                   .Cards
                                   .FirstOrDefaultAsync(a => a.Id == request.CardId && a.IsDeleted == false);

            if (cardDB == null)
            {
                return (null, $"Error: card not found");
            }

            cardDB.Name = request.CardName;
            cardDB.Power = request.CardPower;
            cardDB.UpperHand = request.CardUpperHand;
            cardDB.Type = request.CardType;

            var savingSucceded = await this._daoDbContext.SaveChangesAsync();

            if (savingSucceded == 0)
            {
                return (null, "Error: saving DB modifications failed.");
            }

            return (new AdminsEditMabCardResponse(), "Mab Card updated successfully");
        }
        private static (bool, string) EditMabCard_Validation(AdminsEditMabCardRequest request)
        {
            if (request == null)
            {
                return (false, "Error: no information provided");
            }

            if (string.IsNullOrWhiteSpace(request.CardName) == true)
            {
                return (false, "Error: invalid CardName");
            }

            if (request.CardPower < Constants.MinCardPower || request.CardPower > Constants.MaxCardPower)
            {
                return (false, $"Error: invalid CardPower. It must be between {Constants.MinCardPower} and {Constants.MaxCardPower}");
            }

            if (request.CardUpperHand < Constants.MinCardUpperHand || request.CardUpperHand > Constants.MaxCardUpperHand)
            {
                return (false, $"Error: invalid CardUpperHand. It must be between {Constants.MinCardUpperHand} and {Constants.MaxCardUpperHand}");
            }

            if (Enum.IsDefined(request.CardType) == false)
            {
                var validTypes = string.Join(", ", Enum.GetValues(typeof(MabCardType))
                                       .Cast<MabCardType>()
                                       .Select(cardType => $"{cardType} ({(int)cardType})"));

                return (false, $"Error: invalid CardType. It must be one of the following: {validTypes}");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsDeleteMabCardResponse?, string)> DeleteMabCard(AdminsDeleteMabCardRequest? request)
        {
            var (isValid, message) = DeleteMabCard_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var cardDB = await this._daoDbContext
                .Cards
                .FindAsync(request!.CardId);

            if (cardDB == null)
            {
                return (null, "Error: card not found");
            }

            cardDB.IsDeleted = true;

            var savingSucceded = await this._daoDbContext.SaveChangesAsync();

            if (savingSucceded == 0)
            {
                return (null, "Error: saving DB modifications failed.");
            }

            return (new AdminsDeleteMabCardResponse(), "Mab Card deleted successfully");
        }
        private static (bool, string) DeleteMabCard_Validation(AdminsDeleteMabCardRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.CardId.HasValue == false)
            {
                return (false, "Error: CardId is missing");
            }

            if (request.CardId < 1)
            {
                return (false, "Error: invalid CardId (is less than 1)");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsRestoreMabCardResponse?, string)> RestoreMabCard(AdminsRestoreMabCardRequest? request)
        {
            var (isValid, message) = RestoreMabCard_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var cardDB = await this._daoDbContext
                .Cards
                .FindAsync(request!.CardId);

            if (cardDB == null)
            {
                return (null, "Error: card not found");
            }

            cardDB.IsDeleted = false;

            var savingSucceded = await this._daoDbContext.SaveChangesAsync();

            if (savingSucceded == 0)
            {
                return (null, "Error: saving DB modifications failed.");
            }

            return (new AdminsRestoreMabCardResponse(), "Mab Card restored successfully");
        }
        private static (bool, string) RestoreMabCard_Validation(AdminsRestoreMabCardRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.CardId.HasValue == false)
            {
                return (false, "Error: CardId is missing");
            }

            if (request.CardId < 1)
            {
                return (false, "Error: invalid CardId (is less than 1)");
            }

            return (true, string.Empty);
        }


        public async Task<(List<AdminsListMabCardsResponse>?, string)> ListMabCards(AdminsListMabCardsRequest? request)
        {
            var (isValid, message) = ListMabCards_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = await this._daoDbContext
                .Cards
                .AsNoTracking()                
                .Select(a => new AdminsListMabCardsResponse
                {
                    CardId = a.Id,
                    CardName = a.Name,
                    CardPower = a.Power,
                    CardUpperHand = a.UpperHand,
                    CardLevel = a.Level,
                    CardType = a.Type,
                    IsDeleted = a.IsDeleted,
                })
                .OrderBy(a => a.IsDeleted)
                .ThenBy(a => a.CardPower)
                .ThenBy(a => a.CardName)
                .ThenBy(a => a.CardType)
                .ThenBy(a => a.CardUpperHand)
                .ToListAsync();

            if (content == null || content.Count == 0)
            {
                return (null, "Error: no cards were found");
            }

            return (content, "All cards listed successfully");
        }
        private static (bool, string) ListMabCards_Validation(AdminsListMabCardsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(List<AdminsListMabCardIdsResponse>?, string)> ListMabCardIds(AdminsListMabCardIdsRequest? request)
        {
            var (isValid, message) = ListMabCardIds_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = await this._daoDbContext
                .Cards
                .AsNoTracking()
                .Select(a => new AdminsListMabCardIdsResponse
                {
                    CardId = a.Id,
                    CardName = a.Name,                 
                })
                .OrderBy(a => a.CardName)                
                .ToListAsync();

            if (content == null || content.Count == 0)
            {
                return (null, "Error: no cards were found");
            }

            return (content, "All card ids listed successfully");
        }
        private static (bool, string) ListMabCardIds_Validation(AdminsListMabCardIdsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public (List<AdminsListMabCardTypesResponse>?, string) ListMabCardTypes(AdminsListMabCardTypesRequest? request)
        {
            var (isValid, message) = ListMabCardTypes_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var cardTypesDB = Enum.GetValues(typeof(MabCardType))
                .Cast<MabCardType>()
                .Select(a => new AdminsListMabCardTypesResponse
                {
                    CardTypeValue = (int)a,
                    CardTypeName = a.ToString()
                })
                .ToList();

            var content = cardTypesDB;

            if (content == null || content.Count == 0)
            {
                return (null, "Error: no card types were found");
            }

            return (content, "All card types listed successfully");
        }
        private static (bool, string) ListMabCardTypes_Validation(AdminsListMabCardTypesRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }          

            return (true, string.Empty);
        }

        // Provavelmente esse endpoint será desnecessário...
        public async Task<(List<AdminsFilterCardsResponse>?, string)> FilterCards(AdminsFilterCardsRequest request)
        {
            var (filterIsValid, message) = FilterIsValid(request);

            if (filterIsValid == false)
            {
                return (null, message);
            }

            var contentQueriable = this._daoDbContext
                                       .Cards
                                       .AsNoTracking()
                                       .Where(a => a.IsDeleted == false);

            message = "All cards listed successfully";

            #region Filtering by CardName           
            if (string.IsNullOrWhiteSpace(request.CardName) == false)
            {
                contentQueriable = contentQueriable.Where(a => a.Name.ToLower().Contains(request.CardName.ToLower()));

                message = $"The card ->{request.CardName}<- has been successfully filtered";
            }
            #endregion

            #region Filtering by CardId           
            if (request.StartCardId.HasValue && request.EndCardId.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.Id >= request.StartCardId && a.Id <= request.EndCardId);

                message = $"The cards ranging from id = {request.StartCardId} to id = {request.EndCardId} have been successfully filtered";

                if (request.StartCardId == request.EndCardId)
                {
                    message = $"The card of id = {request.StartCardId} has been successfully filtered";
                }
            }
            #endregion

            #region Filtering by CardPower
            if (request.CardPower.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.Power == request.CardPower);

                message = $"The cards having power = {request.CardPower} have been successfully filtered";
            }
            #endregion

            #region Filtering by CardUpperHand           
            if (request.CardUpperHand.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.UpperHand == request.CardUpperHand);

                message = $"The cards having upper hand = {request.CardUpperHand} have been successfully filtered";
            }
            #endregion

            #region Filtering By CardLevel
            if (request.CardLevel.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.Level == request.CardLevel);

                message = $"The cards of level = {request.CardLevel} have been successfully filtered";
            }
            #endregion

            #region Filtering by CardType          
            if (request.CardType.HasValue == true)
            {
                contentQueriable = contentQueriable.Where(a => a.Type == request.CardType);

                message = $"The cards of type = {request.CardType} have been successfully filtered";
            }
            #endregion

            var content = await contentQueriable
                                    .Select(a => new AdminsFilterCardsResponse
                                    {
                                        CardId = a.Id,
                                        CardName = a.Name,
                                        CardPower = a.Power,
                                        CardUpperHand = a.UpperHand,
                                        CardLevel = a.Level,
                                        CardType = a.Type,
                                    })
                                    .OrderBy(a => a.CardId)
                                    .ThenBy(a => a.CardName)
                                    .ToListAsync();

            if (content == null || content.Count == 0)
            {
                return (null, "Error: nothing found");
            }

            return (content, message);
        }
        public (bool, string) FilterIsValid(AdminsFilterCardsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CardName) == true &&
               request.StartCardId == null && request.EndCardId == null &&
               request.CardPower == null &&
               request.CardUpperHand == null &&
               request.CardLevel == null &&
               request.CardType == null)
            {
                return (false, "Error: no filter added querying the cards");
            }

            if (string.IsNullOrWhiteSpace(request.CardName) == false && request.CardName.Trim().Length < 3)
            {
                return (false, "Error: invalid CardName. Filtering by name requires at least 3 characters");
            }

            if (request.StartCardId > request.EndCardId)
            {
                return (false, "Error: invalid CardIds. StartCardId cannot be greater than EndCardId");
            }

            if (request.StartCardId.HasValue == true && request.EndCardId.HasValue == false)
            {
                request.EndCardId = request.StartCardId + 99;
            }

            if (request.StartCardId.HasValue == false && request.EndCardId.HasValue == true)
            {
                request.StartCardId = request.EndCardId - 99;
            }

            if (request.EndCardId - request.StartCardId > 100)
            {
                return (false, "Error: invalid range. Only 100 cards can be loaded per query");
            }

            if (request.CardPower < Constants.MinCardPower || request.CardPower > Constants.MaxCardPower)
            {
                return (false, $"Error: invalid CardPower. It must be between {Constants.MinCardPower} and {Constants.MaxCardPower}");
            }

            if (request.CardUpperHand < Constants.MinCardUpperHand || request.CardUpperHand > Constants.MaxCardUpperHand)
            {
                return (false, $"Error: invalid CardUpperHand. It must be between {Constants.MinCardUpperHand} and {Constants.MaxCardUpperHand}");
            }

            if (request.CardLevel < Constants.MinCardLevel || request.CardLevel > Constants.MaxCardLevel)
            {
                return (false, $"Error: invalid CardLevel. It must be between {Constants.MinCardLevel} and {Constants.MaxCardLevel}");
            }

            if (request.CardType.HasValue == true && Enum.IsDefined(request.CardType.Value) == false)
            {
                var validTypes = string.Join(", ", Enum.GetValues(typeof(MabCardType))
                                       .Cast<MabCardType>()
                                       .Select(cardType => $"{cardType} ({(int)cardType})"));

                return (false, $"Error: invalid CardType. It must be one of the following: {validTypes}");
            }

            return (true, string.Empty);
        }

        // MAB NPCS

        public async Task<(AdminsAddMabNpcResponse?, string)> AddMabNpc(AdminsAddMabNpcRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = AddMabNpc_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var exists = await _daoDbContext
                .Npcs
                .AnyAsync(a => a.Name == request.Name && a.IsDeleted == false);

            if (exists == true)
            {
                return (null, $"Error: this Mab NPC already exists - {request.Name}");
            }

            var (newNpcDeckEntries, ErrorMessage) = await GenerateRandomDeck(request.CardIds);

            if (newNpcDeckEntries == null || newNpcDeckEntries.Count != 5)
            {
                return (null, ErrorMessage);
            }

            var newNpc = new Npc
            {
                Name = request.Name,
                Description = request.Description,
                Deck = newNpcDeckEntries,
                Level = Helper.GetNpcLevel(newNpcDeckEntries.Select(a => a.Card.Level).ToList()),
                IsDeleted = false
            };

            this._daoDbContext.Add(newNpc);

            var savingSucceded = await this._daoDbContext.SaveChangesAsync();

            if (savingSucceded == 0)
            {
                return (null, "Error: saving DB modifications failed.");
            }

            return (new AdminsAddMabNpcResponse
            {
                Level = newNpc.Level,
                CountCards = newNpcDeckEntries.Count,
            }, "Mab NPC created successfully");
        }
        public (bool, string) AddMabNpc_Validation(AdminsAddMabNpcRequest request)
        {
            if (request == null)
            {
                return (false, "Error: no information was provided");
            }

            if (string.IsNullOrWhiteSpace(request.Name) == true)
            {
                return (false, "Error: the NPC's name is mandatory");
            }

            if (string.IsNullOrEmpty(request.Description) == true)
            {
                return (false, "Error: the NPC's description is mandatory");
            }

            if (request.CardIds == null || request.CardIds.Count != Constants.DeckSize)
            {
                return (false, $"Error: the NPC's deck can neither be empty nor contain fewer nor more than {Constants.DeckSize} cards");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsShowMabNpcDetailsResponse?, string)> ShowMabNpcDetails(AdminsShowMabNpcDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ShowMabNpcDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabNpcDB = await this._daoDbContext
                .Npcs
                .Select(a => new AdminsShowMabNpcDetailsResponse
                {
                    NpcName = a.Name,
                    Description = a.Description,
                    Level = a.Level,
                    DeckSize = Constants.DeckSize,
                    Cards = a
                        .Deck
                        .Select(b => new AdminsShowMabNpcDetailsResponse_card
                        {
                            CardId = b.Card.Id,
                            CardName = b.Card.Name,
                            CardPower = b.Card.Power,
                            CardUpperHand = b.Card.UpperHand,
                            CardType = b.Card.Type,
                        })                        
                        .ToList()
                })
                .FirstOrDefaultAsync();
        

            if (mabNpcDB == null)
            {
                return (null, "Error: Medieval Auto Battler Npc not found!");
            }

            return (mabNpcDB, "Mab Npc details fetched successfully!");
        }
        private static (bool, string) ShowMabNpcDetails_Validation(AdminsShowMabNpcDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is not null");
            }

            if (request.NpcId <= 0)
            {
                return (false, "Error: invalid medieval auto battler CardId");
            }

            return (true, string.Empty);
        }


        public async Task<(List<AdminsGetNpcsResponse>?, string)> ListMabNpcs(AdminsListMabNpcsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var npcsDB = await this._daoDbContext
                .Npcs
                .AsNoTracking()
                .Select(a => new AdminsGetNpcsResponse
                {
                    NpcId = a.Id,
                    NpcName = a.Name,
                    NpcDescription = a.Description,
                    NpcLevel = a.Level,
                    NpcIsDeleted = a.IsDeleted,
                    Deck = a.Deck.Select(b => new AdminsGetNpcsResponse_Deck
                    {
                        Name = b.Card.Name,
                        Power = b.Card.Power,
                        UpperHand = b.Card.UpperHand,
                        Level = b.Card.Level,
                        Type = b.Card.Type,
                    })
                    .ToList()
                })
                .OrderBy(a => a.NpcLevel)
                .ThenBy(a => a.NpcName)
                .ToListAsync();

            if (npcsDB == null || npcsDB.Count == 0)
            {
                return (null, "Error: no NPCS found!");
            }        

            return (npcsDB, "NPCs listed successfully");
        }
        public (bool, string) GetNpcs_Validation(AdminsListMabNpcsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsEditMabNpcResponse?, string)> EditMabNpc(AdminsEditMabNpcRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditMabNpc_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var npcDB = await _daoDbContext
                .Npcs
                .Include(a => a.Deck)
                .ThenInclude(b => b.Card)
                .FirstOrDefaultAsync(a => a.Id == request.NpcId);

            if (npcDB == null)
            {
                return (null, $"Error: NPC not found!");
            }

            if (npcDB.IsDeleted == true)
            {
                return (null, $"Error: NPC has been already deleted!");
            }

            var availableCardIds = _daoDbContext
                .Cards
                .Where(a => a.IsDeleted == false)
                .Select(a => a.Id)
                .ToList();

            if (request.CardIds!.All(id => availableCardIds.Contains(id)) == false)
            {
                return (null, "Error: the cardId or cardIds provided lead to non-existing cards");
            }

            var oldCardIds = npcDB
                .Deck
                .Select(a => a.Id)
                .ToList();

            await this._daoDbContext
                .NpcDeckEntries
                .Where(a => oldCardIds.Contains(a.CardId) && a.NpcId == request.NpcId)
                .ExecuteDeleteAsync();
            

            var (newNpcDeckEntries, ErrorMessage) = await GenerateRandomDeck(request.CardIds!);

            if (newNpcDeckEntries == null || newNpcDeckEntries.Count != 5)
            {
                return (null, ErrorMessage);
            }

            npcDB.Name = request.NpcName!;
            npcDB.Description = request.NpcDescription!;
            npcDB.Deck = newNpcDeckEntries;
            npcDB.Level = Helper.GetNpcLevel(newNpcDeckEntries.Select(a => a.Card.Level).ToList());

            await this._daoDbContext.SaveChangesAsync();

            return (new AdminsEditMabNpcResponse(), "NPC updated successfully");
        }
        private static (bool, string) EditMabNpc_Validation(AdminsEditMabNpcRequest request)
        {
            if (request == null)
            {
                return (false, "Error: no information provided");
            }

            if (string.IsNullOrWhiteSpace(request.NpcName) == true)
            {
                return (false, "Error: invalid NpcName");
            }

            if (string.IsNullOrWhiteSpace(request.NpcDescription) == true)
            {
                return (false, "Error: invalid NpcName");
            }

            if (request.CardIds == null || request.CardIds.Count != Constants.DeckSize)
            {
                return (false, $"Error: the NPC's deck can neither be empty nor contain fewer or more than {Constants.DeckSize} cards");
            }         

            return (true, string.Empty);
        }

        public async Task<(AdminsGetMabNpcLvlResponse?, string)> GetMabNpcLvl(AdminsGetMabNpcLvlRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetMabNpcLvl_Validation(request);

            if (!isValid)
            {
                return (null, message);
            }

            // Load unique cards from DB
            var cardsDB = await _daoDbContext
                .Cards
                .Where(a => request.MabCardIds.Contains(a.Id))
                .Select(a => new
                {
                    CardId = a.Id,
                    CardPower = a.Power,
                    IsDeleted = a.IsDeleted
                })
                .ToListAsync();

            if (cardsDB == null || cardsDB.Count < 1)
            {
                return (null, "Error: cards not found!");
            }

            // Dictionary for fast lookup
            var cardLookup = cardsDB.ToDictionary(a => a.CardId, a => new { a.CardPower, a.IsDeleted });

            // Reconstruct the full list of card powers based on the original request (with duplicates)
            var cardPowers = new List<int>();

            foreach (var cardId in request.MabCardIds)
            {
                if (!cardLookup.TryGetValue(cardId, out var cardInfo) || cardInfo.IsDeleted == true)
                {
                    return (null, "Error: not all requested cards could be found!");
                }

                cardPowers.Add(cardInfo.CardPower);
            }

            if (cardPowers.Count != Constants.DeckSize)
            {
                return (null, "Error: deck size is invalid!");
            }

            int? calculatedNMabNpcLevel = Helper.GetNpcLevel(cardPowers);

            if (calculatedNMabNpcLevel == null || calculatedNMabNpcLevel < 0 || calculatedNMabNpcLevel > 9)
            {
                return (null, "Error: something went wrong while calculating NPC level!");
            }

            return (new AdminsGetMabNpcLvlResponse
            {
                MabNpcLvl = calculatedNMabNpcLevel
            }, "Mab NPC level calculated successfully");
        }
        private static (bool, string) GetMabNpcLvl_Validation(AdminsGetMabNpcLvlRequest request)
        {
            if (request == null)
            {
                return (false, "Error: no information provided");
            }              

            if (request.MabCardIds == null || request.MabCardIds.Count != Constants.DeckSize)
            {
                return (false, $"Error: the number of cards necessary for calculating the mab npc level must be: {Constants.DeckSize}");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsDeleteMabNpcResponse?, string)> DeleteMabNpc(AdminsDeleteMabNpcRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteMabNpc_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var mabNpcDB = await this._daoDbContext
                .Npcs
                .FindAsync(request!.NpcId);

            if (mabNpcDB == null)
            {
                return (null, "Error: npc not found");
            }

            mabNpcDB.IsDeleted = true;

            var savingSucceded = await this._daoDbContext.SaveChangesAsync();

            if (savingSucceded == 0)
            {
                return (null, "Error: saving DB modifications failed.");
            }

            return (new AdminsDeleteMabNpcResponse(), "Mab Npc deleted successfully");
        }
        private static (bool, string) DeleteMabNpc_Validation(AdminsDeleteMabNpcRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.NpcId.HasValue == false)
            {
                return (false, "Error: NpcId is missing");
            }

            if (request.NpcId < 1)
            {
                return (false, "Error: invalid NpcId (is less than 1)");
            }

            return (true, string.Empty);
        }


        public async Task<(AdminsRestoreMabNpcResponse?, string)> RestoreMabNpc(AdminsRestoreMabNpcRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = RestoreMabNpc_Validation(request);
           
            if (isValid == false)
            {
                return (null, message);
            }

            var mabNpcDB = await this._daoDbContext
                .Npcs
                .FindAsync(request!.NpcId);

            if (mabNpcDB == null)
            {
                return (null, "Error: npc not found");
            }

            mabNpcDB.IsDeleted = false;

            var savingSucceded = await this._daoDbContext.SaveChangesAsync();

            if (savingSucceded == 0)
            {
                return (null, "Error: saving DB modifications failed.");
            }

            return (new AdminsRestoreMabNpcResponse(), "Mab Npc restored successfully");
        }
        private static (bool, string) RestoreMabNpc_Validation(AdminsRestoreMabNpcRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.NpcId.HasValue == false)
            {
                return (false, "Error: NpcId is missing");
            }

            if (request.NpcId < 1)
            {
                return (false, "Error: invalid NpcId (is less than 1)");
            }

            return (true, string.Empty);
        }

        private async Task<(List<NpcDeckEntry>?, string)> GenerateRandomDeck(List<int> mabCardIds)
        {
            var cardsDB = await _daoDbContext
                .Cards
                .Where(a => mabCardIds.Contains(a.Id) && a.IsDeleted == false)
                .ToListAsync();

            if (cardsDB == null || cardsDB.Count == 0)
            {
                return (null, "Error: invalid card Ids");
            }

            var uniqueCardIds = mabCardIds
                .Distinct()
                .ToList()
                .Count;

            if (uniqueCardIds != cardsDB.Count)
            {
                var notFoundIds = mabCardIds
                    .Distinct()
                    .ToList()
                    .Except(cardsDB.Select(a => a.Id).ToList());

                return (null, $"Error: invalid cardId: {string.Join(" ,", notFoundIds)}");
            }

            var newDeck = new List<NpcDeckEntry>();

            foreach (var id in mabCardIds)
            {
                var newCard = cardsDB.FirstOrDefault(a => a.Id == id);

                if (newCard != null)
                {
                    newDeck.Add(new NpcDeckEntry()
                    {
                        Card = newCard,
                    });
                }
            }

            return (newDeck, string.Empty);
        }


        #endregion

    }
}