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
                .MabCards
                .AsNoTracking()
                .AnyAsync(a =>
                    a.Mab_CardName.Trim().ToLower() == request.CardName.Trim().ToLower() && 
                    a.Mab_IsCardDeleted == false);

            if (exists == true)
            {
                return (null, $"Error: requested name is already taken, please choose another!");
            }

            var mabCardsCount = await this._daoDbContext
                .MabCards
                .AsNoTracking()
                .CountAsync();

            var mabCardCode = mabCardsCount < 10 ?
                            "MAB00" + mabCardsCount :
                            (mabCardsCount >= 10 && mabCardsCount < 100) ?
                            "MAB0" + mabCardsCount :
                            "MAB" + mabCardsCount;

            var newCard = new MabCard
            {
                Mab_CardCode = mabCardCode,
                Mab_CardName = request.CardName,
                Mab_CardPower = request.CardPower,
                Mab_CardUpperHand = request.CardUpperHand,
                Mab_CardLevel = Helper.MabGetCardLevel(request.CardPower, request.CardUpperHand),
                Mab_CardType = request.CardType,
                Mab_IsCardDeleted = false,
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
                .MabCards
                .FindAsync(request!.CardId);

            if( mabCardDB == null )
            {
                return (null, "Error: Medieval Auto Battler Card not found!");
            }                

            return (new AdminsShowMabCardDetailsResponse
            {
                CardCode = mabCardDB.Mab_CardCode,
                CardName = mabCardDB.Mab_CardName,
                CardPower = mabCardDB.Mab_CardPower,
                CardUpperHand = mabCardDB.Mab_CardUpperHand,
                CardLevel = mabCardDB.Mab_CardLevel,
                CardType = mabCardDB.Mab_CardType,
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
                                   .MabCards
                                   .AnyAsync(a => a.Mab_CardName.ToLower() == request.CardName.Trim().ToLower());

            if (exist == true)
            {
                return (null, $"Error: invalid CardName: ->{request.CardName}<- .A card with this name already exists");
            }

            var cardDB = await this._daoDbContext
                                   .MabCards
                                   .FirstOrDefaultAsync(a => a.Id == request.CardId && a.Mab_IsCardDeleted == false);

            if (cardDB == null)
            {
                return (null, $"Error: card not found");
            }

            cardDB.Mab_CardName = request.CardName;
            cardDB.Mab_CardPower = request.CardPower;
            cardDB.Mab_CardUpperHand = request.CardUpperHand;
            cardDB.Mab_CardLevel = Helper.MabGetCardLevel(request.CardPower, request.CardUpperHand);
            cardDB.Mab_CardType = request.CardType;

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
                .MabCards
                .FindAsync(request!.CardId);

            if (cardDB == null)
            {
                return (null, "Error: card not found");
            }

            cardDB.Mab_IsCardDeleted = true;

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
                .MabCards
                .FindAsync(request!.CardId);

            if (cardDB == null)
            {
                return (null, "Error: card not found");
            }

            cardDB.Mab_IsCardDeleted = false;

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
                .MabCards
                .AsNoTracking()                
                .Select(a => new AdminsListMabCardsResponse
                {
                    CardId = a.Id,
                    CardCode = a.Mab_CardCode,
                    CardName = a.Mab_CardName,
                    CardPower = a.Mab_CardPower,
                    CardUpperHand = a.Mab_CardUpperHand,
                    CardLevel = a.Mab_CardLevel,
                    CardType = a.Mab_CardType,
                    IsDeleted = a.Mab_IsCardDeleted,
                })
                .OrderBy(a => a.IsDeleted)
                .ThenBy(a => a.CardCode)
                .ThenBy(a => a.CardName)
                .ThenBy(a => a.CardLevel)
                .ThenBy(a => a.CardType)
                .ThenBy(a => a.CardPower)
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
                .MabCards
                .AsNoTracking()
                .Select(a => new AdminsListMabCardIdsResponse
                {
                    CardId = a.Id,
                    CardName = a.Mab_CardName + a.Mab_CardCode,                 
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
                .MabNpcs
                .AnyAsync(a => a.Mab_NpcName == request.Name && a.Mab_IsNpcDeleted == false);

            if (exists == true)
            {
                return (null, $"Error: this Mab NPC already exists - {request.Name}");
            }

            var (newNpcDeckEntries, ErrorMessage) = await GenerateRandomDeck(request.CardIds);

            if (newNpcDeckEntries == null || newNpcDeckEntries.Count != 5)
            {
                return (null, ErrorMessage);
            }

            var newNpc = new MabNpc
            {
                Mab_NpcName = request.Name,
                Mab_NpcDescription = request.Description,
                Mab_NpcCards = newNpcDeckEntries,
                Mab_NpcLevel = Helper.MabGetNpcLevel(newNpcDeckEntries.Select(a => a.Mab_Card.Mab_CardLevel).ToList()),
                Mab_IsNpcDeleted = false
            };

            this._daoDbContext.Add(newNpc);

            var savingSucceded = await this._daoDbContext.SaveChangesAsync();

            if (savingSucceded == 0)
            {
                return (null, "Error: saving DB modifications failed.");
            }

            return (new AdminsAddMabNpcResponse
            {
                Level = newNpc.Mab_NpcLevel,
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
                .MabNpcs
                .Where(a => a.Id == request!.NpcId)
                .Select(a => new AdminsShowMabNpcDetailsResponse
                {
                    NpcName = a.Mab_NpcName,
                    Description = a.Mab_NpcDescription,
                    Level = a.Mab_NpcLevel,                    
                    Cards = a
                        .Mab_NpcCards
                        .Select(b => new AdminsShowMabNpcDetailsResponse_card
                        {
                            CardId = b.Mab_Card.Id,
                            CardName = b.Mab_Card.Mab_CardName,
                            CardPower = b.Mab_Card.Mab_CardPower,
                            CardUpperHand = b.Mab_Card.Mab_CardUpperHand,
                            CardType = b.Mab_Card.Mab_CardType,
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

            var (isValid, message) = ListMabNpcs_Validation(request);

            if (!isValid)
            {
                return (null, message);
            }

            var npcsDB = await this._daoDbContext
                .MabNpcs
                .AsNoTracking()
                .Select(a => new AdminsGetNpcsResponse
                {
                    NpcId = a.Id,
                    NpcName = a.Mab_NpcName,
                    NpcDescription = a.Mab_NpcDescription,
                    NpcLevel = a.Mab_NpcLevel,
                    NpcIsDeleted = a.Mab_IsNpcDeleted,
                    Deck = a.Mab_NpcCards.Select(b => new AdminsGetNpcsResponse_Deck
                    {
                        Name = b.Mab_Card.Mab_CardName,
                        Power = b.Mab_Card.Mab_CardPower,
                        UpperHand = b.Mab_Card.Mab_CardUpperHand,
                        Level = b.Mab_Card.Mab_CardLevel,
                        Type = b.Mab_Card.Mab_CardType,
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
        public (bool, string) ListMabNpcs_Validation(AdminsListMabNpcsRequest? request)
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
                .MabNpcs
                .Include(a => a.Mab_NpcCards)
                .ThenInclude(b => b.Mab_Card)
                .FirstOrDefaultAsync(a => a.Id == request.NpcId);

            if (npcDB == null)
            {
                return (null, $"Error: NPC not found!");
            }

            if (npcDB.Mab_IsNpcDeleted == true)
            {
                return (null, $"Error: NPC has been already deleted!");
            }

            var availableCardIds = _daoDbContext
                .MabCards
                .Where(a => a.Mab_IsCardDeleted == false)
                .Select(a => a.Id)
                .ToList();

            if (request.CardIds!.All(id => availableCardIds.Contains(id)) == false)
            {
                return (null, "Error: the cardId or cardIds provided lead to non-existing cards");
            }

            var oldCardIds = npcDB
                .Mab_NpcCards
                .Select(a => a.Id)
                .ToList();

            await this._daoDbContext
                .MabNpcCards
                .Where(a => oldCardIds.Contains(a.Mab_CardId) && a.Mab_NpcId == request.NpcId)
                .ExecuteDeleteAsync();
            

            var (newNpcDeckEntries, ErrorMessage) = await GenerateRandomDeck(request.CardIds!);

            if (newNpcDeckEntries == null || newNpcDeckEntries.Count != 5)
            {
                return (null, ErrorMessage);
            }

            npcDB.Mab_NpcName = request.NpcName!;
            npcDB.Mab_NpcDescription = request.NpcDescription!;
            npcDB.Mab_NpcCards = newNpcDeckEntries;
            npcDB.Mab_NpcLevel = Helper.MabGetNpcLevel(newNpcDeckEntries.Select(a => a.Mab_Card.Mab_CardLevel).ToList());

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
                .MabCards
                .Where(a => request.MabCardIds.Contains(a.Id))
                .Select(a => new
                {
                    CardId = a.Id,
                    CardLvl = a.Mab_CardLevel,
                    IsDeleted = a.Mab_IsCardDeleted
                })
                .ToListAsync();

            if (cardsDB == null || cardsDB.Count < 1)
            {
                return (null, "Error: cards not found!");
            }

            // Dictionary for fast lookup
            var cardLookup = cardsDB.ToDictionary(a => a.CardId, a => new { a.CardLvl, a.IsDeleted });

            // Reconstruct the full list of card powers based on the original request (with duplicates)
            var cardPowers = new List<int>();

            foreach (var cardId in request.MabCardIds!)
            {
                if (!cardLookup.TryGetValue(cardId, out var cardInfo) || cardInfo.IsDeleted == true)
                {
                    return (null, "Error: not all requested cards could be found!");
                }

                cardPowers.Add(cardInfo.CardLvl);
            }

            if (cardPowers.Count != Constants.DeckSize)
            {
                return (null, "Error: deck size is invalid!");
            }

            int? calculatedNMabNpcLevel = Helper.MabGetNpcLevel(cardPowers);

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


        public (AdminsGetDeckSizeLimitResponse?, string) GetDeckSizeLimit(AdminsGetDeckSizeLimitRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetDeckSizeLimit_Validation(request);

            if (!isValid)
            {
                return (null, message);
            } 

            return (new AdminsGetDeckSizeLimitResponse(), "Deck sized limit fetchted successfully");
        }
        public (bool, string) GetDeckSizeLimit_Validation(AdminsGetDeckSizeLimitRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null however it MUST be null!");
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
                .MabNpcs
                .FindAsync(request!.NpcId);

            if (mabNpcDB == null)
            {
                return (null, "Error: npc not found");
            }

            mabNpcDB.Mab_IsNpcDeleted = true;

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
                .MabNpcs
                .FindAsync(request!.NpcId);

            if (mabNpcDB == null)
            {
                return (null, "Error: npc not found");
            }

            mabNpcDB.Mab_IsNpcDeleted = false;

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

        private async Task<(List<MabNpcCard>?, string)> GenerateRandomDeck(List<int> mabCardIds)
        {
            var cardsDB = await _daoDbContext
                .MabCards
                .Where(a => mabCardIds.Contains(a.Id) && a.Mab_IsCardDeleted == false)
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

            var newDeck = new List<MabNpcCard>();

            foreach (var id in mabCardIds)
            {
                var newCard = cardsDB.FirstOrDefault(a => a.Id == id);

                if (newCard != null)
                {
                    newDeck.Add(new MabNpcCard()
                    {
                        Mab_Card = newCard,
                    });
                }
            }

            return (newDeck, string.Empty);
        }

        // Mab Quests
        public async Task<(AdminsMabAddQuestResponse?, string)> MabAddQuest(AdminsMabAddQuestRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabAddQuest_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var doesMabQuestAlreadyExists = await _daoDbContext
                .MabQuests
                .AsNoTracking()
                .AnyAsync(quest => 
                    quest.Mab_QuestTitle == request.Mab_QuestTitle &&
                    quest.Mab_IsDeleted == false);

            if (doesMabQuestAlreadyExists == true)
            {
                return (null, $"Error: MabAddQuest failed! Request Mab_QuestTitle already in use!");
            }    

            var mabNpcsDB = await _daoDbContext
                .MabNpcs
                .Where(npc => request.Mab_NpcIds!.Any(npcId => npcId == npc.Id))
                .ToListAsync();
            

            var newMabQuest = new MabQuest
            {
                Mab_QuestTitle = request.Mab_QuestTitle,
                Mab_QuestDescription = request.Mab_QuestDescription,
                Mab_QuestLevel = request.Mab_QuestLevel,
                Mab_GoldBounty = request.Mab_GoldBounty,
                Mab_XpReward =request.Mab_XpReward,
                Mab_Npcs = mabNpcsDB
            };

            return (new AdminsMabAddQuestResponse() 
            , "Mab Quest created successfully");
        }
        public (bool, string) MabAddQuest_Validation(AdminsMabAddQuestRequest request)
        {
            if (request == null)
            {
                return (false, "Error: MabAddQuest failed! Request is null!");
            }

            if (string.IsNullOrWhiteSpace(request.Mab_QuestTitle) == true)
            {
                return (false, "Error: MabAddQuest failed! Mab_QuestTitle is missing!");
            }

            if (string.IsNullOrEmpty(request.Mab_QuestDescription) == true)
            {
                return (false, "Error: MabAddQuest failed! Mab_QuestDescription is missing!");
            }

            if (request.Mab_QuestLevel.HasValue == false || request.Mab_QuestLevel < 1)
            {
                return (false, $"Error: MabAddQuest failed! Mab_QuestLevel is missing or invalid!");
            }

            if (request.Mab_GoldBounty.HasValue == false || request.Mab_GoldBounty < 1)
            {
                return (false, $"Error: MabAddQuest failed! Mab_GoldBounty is missing or invalid!");
            }

            if (request.Mab_XpReward.HasValue == false || request.Mab_XpReward < 1)
            {
                return (false, $"Error: MabAddQuest failed! Mab_XpReward is missing or invalid!");
            }

            if (request.Mab_NpcIds == null || request.Mab_NpcIds.Count < 1)
            {
                return (false, $"Error: MabAddQuest failed! Mab_NpcIds is empty or null!");
            }

            return (true, string.Empty);
        }



        #endregion

    }
}