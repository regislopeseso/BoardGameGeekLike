using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Models.Enums;
using BoardGameGeekLike.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Constants = BoardGameGeekLike.Utilities.Constants;



namespace BoardGameGeekLike.Services
{
    public class UsersService
    {
        private readonly ApplicationDbContext _daoDbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly Random random = new Random();

        public UsersService(ApplicationDbContext daoDbContext, UserManager<User> userManager, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            this._daoDbContext = daoDbContext;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._httpContextAccessor = httpContextAccessor;
        }

        #region USER'S DATA

        public async Task<(UsersExportUserDataResponse?, string)> ExportUserData(UsersExportUserDataRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ExportUserData_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var sb = new StringBuilder();

            var errorsReport = new List<string>();

            //-*
            // First line: File title
            var time_right_now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            sb.AppendLine($";BGG LIKE USER BACKUP DATA - EXPORT TABLES - {time_right_now}");

            // Second line: empty line
            sb.Append("");

            // Third line: First column ERRORS REPORT
            sb.AppendLine("Errors Report");

            //-*
            // User details table title
            sb.AppendLine(Constants.User_ProfileDetails_TableTitle);

            // User details table header
            sb.AppendLine(Constants.User_ProfileDetails_TableHeaders);

            var userDB = await this._daoDbContext
                .Users
                .AsNoTracking()
                .Where(a => a.Id == userId)
                .Select(a => new DataBackUp_userObj
                {
                    Name = a.Name,
                    Email = a.Email,
                    BirthDate = a.BirthDate,
                    Gender = a.Gender,
                    SignUpDate = a.SignUpDate,
                    LifeCounterTemplateIds = a.LifeCounterTemplates.Select(b => b.Id).ToList(),
                })
                .FirstOrDefaultAsync();

            if (userDB == null)
            {
                return (null, "Error: user not found");
            }
            var userDetails_data = UserData_Validation_ProfileDetails(userDB, errorsReport);

            // User details table row (data)
            sb.AppendLine(userDetails_data);
            //
            //-*


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Life Counter Templates table title
            sb.AppendLine(Constants.User_LifeCounter_Templates_TableTitle);

            // Life Counter Templates table header
            sb.AppendLine(Constants.User_LifeCounter_Templates_TableHeader);

            var lifeCounterTemplatesDB = await this._daoDbContext
           .LifeCounterTemplates
           .AsNoTracking()
           .Where(a => a.UserId == userId)
           .Select(a => new DataBackUp_userObj_lifeCounterTemplate
           {
               LifeCounterTemplateName = a.LifeCounterTemplateName,
               PlayersStartingLifePoints = a.PlayersStartingLifePoints,
               PlayersCount = a.PlayersCount,
               FixedMaxLifePointsMode = a.FixedMaxLifePointsMode,
               PlayersMaxLifePoints = a.PlayersMaxLifePoints,
               AutoDefeatMode = a.AutoDefeatMode,
               AutoEndMode = a.AutoEndMode,
               LifeCounterManagersCount = a.LifeCounterManagers!.Count,
               LifeCounterManagerIds = a.LifeCounterManagers!.Select(b => b.Id).ToList(),
           })
           .ToListAsync();

            if (lifeCounterTemplatesDB != null && lifeCounterTemplatesDB.Count > 0)
            {
                foreach (var template in lifeCounterTemplatesDB)
                {
                    // Clear the errors report for the next table
                    errorsReport = new List<string>();

                    var userLifeCouterTemplate_data = UserData_Validation_LifeCounterTemplates(template, errorsReport);

                    sb.AppendLine(userLifeCouterTemplate_data);
                }
            }
            //
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Life Counter Managers table title
            sb.AppendLine(Constants.User_LifeCounter_Managers_TableTitle);

            // Life Counter Managers table header
            sb.AppendLine(Constants.User_LifeCounter_Managers_TableHeader);


            var lifeCounterManagersDB = await this._daoDbContext
                .LifeCounterManagers
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => new DataBackUp_userObj_lifeCounterManager
                {
                    LifeCounterTemplateName = a.LifeCounterTemplate!.LifeCounterTemplateName,
                    LifeCounterManagerName = a.LifeCounterManagerName,
                    PlayersStartingLifePoints = a.PlayersStartingLifePoints,
                    PlayersCount = a.PlayersCount,
                    FirstPlayerIndex = a.FirstPlayerIndex,
                    FixedMaxLifePointsMode = a.FixedMaxLifePointsMode,
                    PlayersMaxLifePoints = a.PlayersMaxLifePoints,
                    AutoDefeatMode = a.AutoDefeatMode,
                    AutoEndMode = a.AutoEndMode,
                    StartingTime = a.StartingTime,
                    EndingTime = a.EndingTime,
                    Duration_minutes = a.Duration_minutes != 0 ? a.Duration_minutes : null,
                    IsFinished = a.IsFinished,
                    LifeCounterPlayerIds = a.LifeCounterPlayers!.Select(b => b.Id).ToList()
                })
                .ToListAsync();


            if (lifeCounterManagersDB != null && lifeCounterManagersDB.Count > 0)
            {
                foreach (var manager in lifeCounterManagersDB)
                {
                    // Clear the errors report for the next table
                    errorsReport = new List<string>();

                    var userLifeCouterManager_data = UserData_Validation_LifeCounterManagers(manager, userDB.SignUpDate, errorsReport);

                    sb.AppendLine(userLifeCouterManager_data);
                }
            }
            //
            //*-                       


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Life Counter Players table title
            sb.AppendLine(Constants.User_LifeCounter_Players_TableTitle);

            // Life Counter Players table header
            sb.AppendLine(Constants.User_LifeCounter_Players_TableHeader);

            // Flatten all player IDs from all managers
            var allPlayerIds = lifeCounterManagersDB
                .SelectMany(manager => manager.LifeCounterPlayerIds)
                .Distinct()
                .ToList();

            var lifeCounterPlayersDB = await this._daoDbContext
                .LifeCounterPlayers
                .AsNoTracking()
                .Where(player => allPlayerIds.Contains(player.Id))
                .Select(player => new DataBackUp_userObj_lifeCounterPlayer
                {
                    LifeCounterManagerName = player.LifeCounterManager!.LifeCounterManagerName,
                    PlayerName = player.PlayerName,
                    StartingLifePoints = player.StartingLifePoints,
                    CurrentLifePoints = player.CurrentLifePoints,
                    FixedMaxLifePointsMode = player.FixedMaxLifePointsMode,
                    MaxLifePoints = player.MaxLifePoints,
                    AutoDefeatMode = player.AutoDefeatMode,
                    IsDefeated = player.IsDefeated
                })
                .ToListAsync();


            if (lifeCounterPlayersDB != null && lifeCounterPlayersDB.Count > 0)
            {
                foreach (var player in lifeCounterPlayersDB)
                {
                    // Clear the errors report for the next table
                    errorsReport = new List<string>();

                    var userLifeCouterPlayer_data = UserData_Validation_LifeCounterPlayers(player, errorsReport);

                    sb.AppendLine(userLifeCouterPlayer_data);
                }
            }
            //
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Board Games Sessions table title
            sb.AppendLine(Constants.User_BoardGame_Sessions_TableTitle);

            // Logged Board Game Sessions table header
            sb.AppendLine(Constants.User_BoardGame_Sessions_TableHeader);

            var sessionsDB = await this._daoDbContext
                .Sessions
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => new DataBackUp_userObj_Sessions
                {
                    BoardGameName = a.BoardGame.Name,
                    Date = a.Date,
                    PlayersCount = a.PlayersCount,
                    Duration_minutes = a.Duration_minutes,
                    IsDeleted = a.IsDeleted,
                })
                .ToListAsync();


            if (sessionsDB != null && sessionsDB.Count > 0)
            {
                foreach (var session in sessionsDB)
                {
                    // Clear the errors report for the next table
                    errorsReport = new List<string>();

                    var userBoardGameSessions_data = UserData_Validation_BoardGameSessions(session, userDB.SignUpDate, errorsReport);

                    sb.AppendLine(userBoardGameSessions_data);
                }
            }
            //
            //*-


            // Blank line between tables            
            sb.AppendLine("");


            //-*
            // Board Games Ratings table title
            sb.AppendLine(Constants.User_BoardGame_Ratings_TableTitle);

            // Rated Board Games table header
            sb.AppendLine(Constants.User_BoardGame_Ratings_TableHeader);

            var ratingsDB = await this._daoDbContext
                .Ratings
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => new DataBackUp_userObj_Ratings
                {
                    BoardGameName = a.BoardGame.Name,
                    Rate = a.Rate
                })
                .ToListAsync();

            if (ratingsDB != null && ratingsDB.Count > 0)
            {
                foreach (var rating in ratingsDB)
                {
                    var userBoardGameRatings_data = UserData_Validation_BoardGameRatings(rating, errorsReport);

                    sb.AppendLine(userBoardGameRatings_data);
                }
            }
            //
            //*-


            //-*
            // Blank line between at the end to follow the logic for importing data
            sb.AppendLine("");
            //
            //*-


            // Encode CSV with UTF-8 BOM to avoid Excel encoding issues
            var csvString = sb.ToString();
            var bom = Encoding.UTF8.GetPreamble();
            var bytes = bom.Concat(Encoding.UTF8.GetBytes(csvString)).ToArray();
            var base64 = Convert.ToBase64String(bytes);

            var response = new UsersExportUserDataResponse
            {
                FileName = "bgg_like_user_data.csv",
                Base64Data = base64,
                ContentType = "text/csv"
            };

            return (response, string.Empty);
        }
        private static (bool, string) ExportUserData_Validation(UsersExportUserDataRequest? request)
        {
            if (request != null)
            {
                return (false, "Request must be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersImportUserDataResponse?, string)> ImportUserData(UsersImportUserDataRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return (null, "Error: User is not authenticated");

            var (isValid, message) = ImportUserData_Validation(request);

            if (!isValid)
                return (null, message);

            byte[] csvBytes;
            try
            {
                csvBytes = Convert.FromBase64String(request!.Base64CsvData);
            }
            catch
            {
                return (null, "Error: Invalid Base64 CSV data.");
            }

            var encoding = Encoding.UTF8;
            if (csvBytes.Length >= 3 && csvBytes[0] == 0xEF && csvBytes[1] == 0xBB && csvBytes[2] == 0xBF)
            {
                csvBytes = csvBytes[3..]; // Remove BOM
            }

            var sections = new Dictionary<int, List<string>>();
            int currentTable = -1;

            using (var reader = new StreamReader(new MemoryStream(csvBytes), encoding))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) continue;

                    // Normalize the line before checking if it's a table title
                    var trimmedLine = line.TrimStart(';').Trim();

                    if (line.TrimStart().StartsWith(";TABLE #"))
                    {
                        var numberMatch = Regex.Match(line, @"TABLE\s+#(\d+)");
                        if (numberMatch.Success)
                        {
                            currentTable = int.Parse(numberMatch.Groups[1].Value);
                            sections[currentTable] = new List<string>();
                        }
                        continue;
                    }

                    if (currentTable != -1)
                    {
                        sections[currentTable].Add(line);
                    }
                }
            }

            var anyErrors = false;

            //-*
            var sb = new StringBuilder();
            // First line: File title
            var time_right_now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            sb.AppendLine($";BGG LIKE USER BACKUP - DATA IMPORT TABLES - {time_right_now}");
            // Second line: empty line
            sb.Append("");
            // Third line: First column ERRORS REPORT
            sb.AppendLine("Errors Report");
            //
            //-*


            //*-
            // Dealing with user'a Profile Details
            if (sections.TryGetValue(1, out var userDetailsSection) == false || userDetailsSection.Count < 2)
            {
                return (null, "Error: Missing or invalid user section (Table #1)");
            }

            // User's Profile Details table title
            sb.AppendLine(Constants.User_ProfileDetails_TableTitle);

            // User's Details table header
            sb.AppendLine(Constants.User_ProfileDetails_TableHeaders);

            var importedUserDetails = new DataBackUp_userObj();
            var profileDetails_Table_errorsReport = new List<string>();

            (importedUserDetails, profileDetails_Table_errorsReport) = ParseUser(userDetailsSection, profileDetails_Table_errorsReport);

            if (importedUserDetails != null)
            {           
                var rowData_user_profileDetails = UserData_Validation_ProfileDetails(importedUserDetails, profileDetails_Table_errorsReport);

                if (profileDetails_Table_errorsReport.Count > 1 || rowData_user_profileDetails[0] != ';')
                {
                    anyErrors = true;
                }

                sb.AppendLine(rowData_user_profileDetails);
            }
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Dealing with the user'a Life Counter TEMPLATES
            if (!sections.TryGetValue(2, out var userLifeCounterTemplatesSection) || userLifeCounterTemplatesSection.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #2)");

            // User's Life Counter Templates table title
            sb.AppendLine(Constants.User_LifeCounter_Templates_TableTitle);

            // User's Life Counter Templates table header
            sb.AppendLine(Constants.User_LifeCounter_Templates_TableHeader);

            var importedLifeCounterTemplates = new List<DataBackUp_userObj_lifeCounterTemplate>();
            var lifeCounter_templatesTable_errorsReport = new List<string>();

            (importedLifeCounterTemplates, lifeCounter_templatesTable_errorsReport) = ParseLifeCounterTemplates(userLifeCounterTemplatesSection, lifeCounter_templatesTable_errorsReport);


            if (importedLifeCounterTemplates != null || importedLifeCounterTemplates.Count > 0)
            {

                foreach (var template in importedLifeCounterTemplates)
                {
                    var isTemplateNameUnavailable = await this._daoDbContext
                        .LifeCounterTemplates
                        .AsNoTracking()
                        .Where(a => a.LifeCounterTemplateName == template.LifeCounterTemplateName && a.UserId == userId)
                        .AnyAsync();

                    if (isTemplateNameUnavailable == true)
                    {
                        lifeCounter_templatesTable_errorsReport.Add("#Error: Life Counter Template Name already registered, please check for duplicates!");
                    }

                    var rowData_user_lifeCounterTemplates = UserData_Validation_LifeCounterTemplates(template, lifeCounter_templatesTable_errorsReport);

                    if (lifeCounter_templatesTable_errorsReport.Count > 1 || rowData_user_lifeCounterTemplates[0] != ';')
                    {
                        anyErrors = true;
                    }

                    sb.AppendLine(rowData_user_lifeCounterTemplates);
                }
            }
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Dealing with the user'a Life Counter MANAGERS
            if (!sections.TryGetValue(3, out var userLifeCounterManagersSection) || userLifeCounterManagersSection.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #3)");

            // User's Life Counter Managers table title
            sb.AppendLine(Constants.User_LifeCounter_Managers_TableTitle);

            // User's Life Counter Managers table header
            sb.AppendLine(Constants.User_LifeCounter_Managers_TableHeader);

            var importedLifeCounterManagers = new List<DataBackUp_userObj_lifeCounterManager>();
            var lifeCounter_managersTable_errorsReport = new List<string>();

            (importedLifeCounterManagers, lifeCounter_managersTable_errorsReport) = ParseLifeCounterManagers(userLifeCounterManagersSection, lifeCounter_managersTable_errorsReport);

            if (importedLifeCounterManagers != null || importedLifeCounterManagers.Count > 0)
            {
                foreach (var manager in importedLifeCounterManagers)
                {
                    var isManagerNameUnavailable = await this._daoDbContext
                        .LifeCounterManagers
                        .AsNoTracking()
                        .Where(a => a.LifeCounterManagerName == manager.LifeCounterManagerName && a.UserId == userId)
                        .AnyAsync();

                    if (isManagerNameUnavailable == true)
                    {
                        lifeCounter_managersTable_errorsReport.Add("#Error: Life Counter Manager Name already registered, please check for duplicates!");
                    }

                    var rowData_user_lifeCounterManagers = UserData_Validation_LifeCounterManagers(manager, importedUserDetails.SignUpDate, lifeCounter_managersTable_errorsReport);

                    if (lifeCounter_managersTable_errorsReport.Count > 1 || rowData_user_lifeCounterManagers[0] != ';')
                    {
                        anyErrors = true;
                    }

                    sb.AppendLine(rowData_user_lifeCounterManagers);
                }
            }
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Dealing with the user'a Life Counter PLAYERS
            if (sections.TryGetValue(4, out var userLifeCounterPlayersSection) == false || userLifeCounterPlayersSection.Count < 2)
            {
                return (null, "Error: Missing or invalid user section (Table #4)");
            }

            // User's Life Counter Players table title
            sb.AppendLine(Constants.User_LifeCounter_Players_TableTitle);

            // User's Life Counter Players table header
            sb.AppendLine(Constants.User_LifeCounter_Players_TableHeader);

            var importedLifeCounterPlayers = new List<DataBackUp_userObj_lifeCounterPlayer>();
            var lifeCounter_playersTable_errorsReport = new List<string>();

            (importedLifeCounterPlayers, lifeCounter_playersTable_errorsReport) = ParseLifeCounterPlayers(userLifeCounterPlayersSection, lifeCounter_playersTable_errorsReport);

            if (importedLifeCounterPlayers != null && importedLifeCounterPlayers.Count > 0)
            {
                foreach (var player in importedLifeCounterPlayers)
                {
                    var isPlayerNameUnavailable = await this._daoDbContext
                        .LifeCounterPlayers
                        .AsNoTracking()
                        .Include(a => a.LifeCounterManager)
                        .Where(a => a.LifeCounterManager.UserId == userId && a.LifeCounterManager.LifeCounterManagerName == player.LifeCounterManagerName && a.PlayerName == player.PlayerName)
                        .AnyAsync();

                    if (isPlayerNameUnavailable == true)
                    {
                        lifeCounter_playersTable_errorsReport.Add("#Error: Life Counter Player Name already registered for the respective life counter manager, please check for duplicates!");
                    }

                    var rowData_user_lifeCounterPlayers = UserData_Validation_LifeCounterPlayers(player, lifeCounter_playersTable_errorsReport);

                    if (lifeCounter_playersTable_errorsReport.Count > 1 || rowData_user_lifeCounterPlayers[0] != ';')
                    {
                        anyErrors = true;
                    }

                    sb.AppendLine(rowData_user_lifeCounterPlayers);
                }
            }
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Dealing with THE user'a BOARD GAME SESSIONS
            if (!sections.TryGetValue(5, out var userBoardGameSessions) || userBoardGameSessions.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #5)");

            //-*
            // User's Board Games Sessions table title
            sb.AppendLine(Constants.User_BoardGame_Sessions_TableTitle);

            // User's Board Game Sessions table header
            sb.AppendLine(Constants.User_BoardGame_Sessions_TableHeader);

            var importedBoarGameSessions = new List<DataBackUp_userObj_Sessions>();
            var boardGame_sessionsTable_errorsReport = new List<string>();
            (importedBoarGameSessions, boardGame_sessionsTable_errorsReport) = ParseBoardGameSessions(userBoardGameSessions, boardGame_sessionsTable_errorsReport);

            if (importedBoarGameSessions != null && importedBoarGameSessions.Count > 0)
            {
                foreach (var session in importedBoarGameSessions)
                {
                    var doesBoardGameStillExist = await this._daoDbContext
                        .BoardGames
                        .AsNoTracking()
                        .Where(a => a.Name == session.BoardGameName)
                        .AnyAsync();

                    if (doesBoardGameStillExist == false)
                    {
                        boardGame_sessionsTable_errorsReport.Add("#Error: Board Game does not exist anymore, this row must be removed!");
                    }


                    var isSessionDuplicated = await this._daoDbContext
                        .Sessions
                        .Include(a => a.BoardGame)
                        .AsNoTracking()
                        .Where(a => a.BoardGame.Name == session.BoardGameName & a.Date == session.Date & a.UserId == userId && a.Date == session.Date & a.PlayersCount == session.PlayersCount && a.Duration_minutes == session.Duration_minutes)
                        .AnyAsync();

                    if (isSessionDuplicated == true)
                    {
                        boardGame_sessionsTable_errorsReport.Add("#Error: This Board Game Session already registered, please check for duplicates!");
                    }

                    var rowData_user_boardGameSessions = UserData_Validation_BoardGameSessions(session, importedUserDetails.SignUpDate, boardGame_sessionsTable_errorsReport);

                    if (boardGame_sessionsTable_errorsReport.Count > 1 || rowData_user_boardGameSessions[0] != ';')
                    {
                        anyErrors = true;
                    }

                    sb.AppendLine(rowData_user_boardGameSessions);
                }

            }
            //*-


            // Blank line between tables
            sb.AppendLine("");


            //-*
            // Dealing with THE user'a BOARD GAME RATINGS
            if (!sections.TryGetValue(6, out var userBoardGameRatings) || userBoardGameRatings.Count < 2)
                return (null, "Error: Missing or invalid user section (Table #6)");

            // User's Board Games Ratings table title
            sb.AppendLine(Constants.User_BoardGame_Ratings_TableTitle);

            // User's Rated Board Games table header
            sb.AppendLine(Constants.User_BoardGame_Ratings_TableHeader);

            var importedBoardGameRatings = new List<DataBackUp_userObj_Ratings>();
            var boardGame_ratingsTable_errorsReport = new List<string>();

            (importedBoardGameRatings, boardGame_ratingsTable_errorsReport) = ParseBoardGameRatings(userBoardGameRatings, boardGame_ratingsTable_errorsReport);

            if (importedBoardGameRatings != null && importedBoardGameRatings.Count > 0)
            {
                foreach (var rating in importedBoardGameRatings)
                {

                    var doesBoardGameStillExist = await this._daoDbContext
                        .BoardGames
                        .AsNoTracking()
                        .Where(a => a.Name == rating.BoardGameName)
                        .AnyAsync();

                    if (!doesBoardGameStillExist)
                    {
                        boardGame_ratingsTable_errorsReport.Add("#Error: Board Game does not exist anymore, this row must be removed!");
                    }


                    var isRatingDuplicated = await this._daoDbContext
                        .Ratings
                        .Include(a => a.BoardGame)
                        .AsNoTracking()
                        .Where(a => a.BoardGame.Name == rating.BoardGameName && a.UserId == userId && a.Rate == rating.Rate)
                        .AnyAsync();

                    if (isRatingDuplicated == true)
                    {
                        boardGame_ratingsTable_errorsReport.Add("#Error: This Board Game Rating already registered, please check for duplicates!");
                    }

                    var rowData_user_boardGameRatings = UserData_Validation_BoardGameRatings(rating, boardGame_ratingsTable_errorsReport);

                    if (boardGame_ratingsTable_errorsReport.Count > 0 || rowData_user_boardGameRatings[0] != ';')
                    {
                        anyErrors = true;
                    }

                    sb.AppendLine(rowData_user_boardGameRatings);
                }
            }

            // Blank line between at the end to follow the logic for importing data
            sb.AppendLine("");
            //
            //*-


            var userDB = await _daoDbContext
               .Users
               .FirstOrDefaultAsync(u => u.Id == userId);

            userDB.SignUpDate = importedUserDetails.SignUpDate.Value;

            var response = new UsersImportUserDataResponse();

            var newTemplates = importedLifeCounterTemplates.Select(template => new LifeCounterTemplate
            {
                UserId = userId,
                LifeCounterTemplateName = template.LifeCounterTemplateName,
                PlayersStartingLifePoints = template.PlayersStartingLifePoints,
                PlayersCount = template.PlayersCount,
                FixedMaxLifePointsMode = template.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = template.PlayersMaxLifePoints,
                AutoDefeatMode = template.AutoDefeatMode,
                AutoEndMode = template.AutoEndMode,
                LifeCounterManagers = new List<LifeCounterManager>(),
            }).ToList();

            for (int i = 0; i < newTemplates.Count; i++)
            {
                for (int j = 0; j < importedLifeCounterManagers.Count; j++)
                {
                    if (importedLifeCounterManagers[j].LifeCounterTemplateName == newTemplates[i].LifeCounterTemplateName)
                    {
                        var newManager = new LifeCounterManager
                        {
                            UserId = userId,
                            LifeCounterTemplateId = newTemplates[i].Id,
                            LifeCounterManagerName = importedLifeCounterManagers[j].LifeCounterManagerName,
                            PlayersStartingLifePoints = importedLifeCounterManagers[j].PlayersStartingLifePoints,
                            PlayersCount = importedLifeCounterManagers[j].PlayersCount,
                            FirstPlayerIndex = importedLifeCounterManagers[j].FirstPlayerIndex,
                            FixedMaxLifePointsMode = importedLifeCounterManagers[j].FixedMaxLifePointsMode,
                            PlayersMaxLifePoints = importedLifeCounterManagers[j].PlayersMaxLifePoints,
                            AutoDefeatMode = importedLifeCounterManagers[j].AutoDefeatMode,
                            AutoEndMode = importedLifeCounterManagers[j].AutoEndMode,
                            StartingTime = importedLifeCounterManagers[j].StartingTime,
                            EndingTime = importedLifeCounterManagers[j].EndingTime,
                            Duration_minutes = importedLifeCounterManagers[j].Duration_minutes,
                            IsFinished = importedLifeCounterManagers[j].IsFinished!.Value,
                            LifeCounterPlayers = new List<LifeCounterPlayer>()
                        };

                        foreach (var player in importedLifeCounterPlayers)
                        {
                            if (player.LifeCounterManagerName == newManager.LifeCounterManagerName)
                            {
                                newManager.LifeCounterPlayers.Add(new LifeCounterPlayer
                                {
                                    PlayerName = player.PlayerName,
                                    StartingLifePoints = player.StartingLifePoints,
                                    CurrentLifePoints = player.CurrentLifePoints,
                                    FixedMaxLifePointsMode = player.FixedMaxLifePointsMode!.Value,
                                    MaxLifePoints = player.MaxLifePoints,
                                    AutoDefeatMode = player.AutoDefeatMode,
                                    IsDefeated = player.IsDefeated!.Value
                                });
                            }
                        }

                        newTemplates[i].LifeCounterManagers.Add(newManager);
                        newTemplates[i].LifeCounterManagersCount++;
                    }
                }
            }

            this._daoDbContext.LifeCounterTemplates.AddRange(newTemplates);

            var boardGameNames = importedBoarGameSessions!
                .Select(b => b.BoardGameName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            var playedBGs = await _daoDbContext
                .BoardGames
                .Include(a => a.Sessions)
                .Include(a => a.Ratings)
                .Where(a => boardGameNames.Contains(a.Name))
                .ToListAsync();

            foreach (var playedBG in playedBGs)
            {
                var sessions = importedBoarGameSessions
                    .Where(a => a.BoardGameName.ToLower() == playedBG.Name.ToLower())
                    .Select(a => new Session
                    {
                        UserId = userId,
                        Date = a.Date,
                        PlayersCount = a.PlayersCount!.Value,
                        Duration_minutes = a.Duration_minutes!.Value,
                        IsDeleted = a.IsDeleted!.Value
                    })
                    .ToList();


                playedBG.Sessions.AddRange(sessions);


                var ratings = importedBoardGameRatings
                  .Where(a => a.BoardGameName.ToLower() == playedBG.Name.ToLower())
                  .Select(a => new Rating
                  {
                      UserId = userId,
                      BoardGameId = playedBG.Id,
                      Rate = a.Rate,
                  })
                  .ToList();

                if (ratings.Count > 0)
                {
                    playedBG.Ratings.AddRange(ratings);
                }

            }

            if (anyErrors == true)
            {
                // Encode CSV with UTF-8 BOM to avoid Excel encoding issues
                var csvString = sb.ToString();
                var bom = Encoding.UTF8.GetPreamble();
                var bytes = bom.Concat(Encoding.UTF8.GetBytes(csvString)).ToArray();
                var base64 = Convert.ToBase64String(bytes);

                response = new UsersImportUserDataResponse
                {
                    FileName = "bgg_like_user_data_errors.csv",
                    Base64Data = base64,
                    ContentType = "text/csv"
                };

                return (response, "Errors found in the data to be imported please refer to the prompted data sheet");
            }

            await _daoDbContext.SaveChangesAsync();

            return (new UsersImportUserDataResponse(), "User data imported successfully."); ;
        }
        private static (bool, string) ImportUserData_Validation(UsersImportUserDataRequest? request)
        {
            if (request == null)
            {
                return (false, "Request is null!");
            }

            return (true, string.Empty);
        }


        private static string UserData_Validation_ProfileDetails(DataBackUp_userObj? userDB, List<string> errorsReport)
        {
            var userName = userDB.Name;
            if (string.IsNullOrWhiteSpace(userName) == true)
            {
                errorsReport.Add("#Error: user name is missing!");
            }
            else
            {
                userName = userName.Trim().ToUpper();

                if (userName.Length > 30)
                {
                    errorsReport.Add("#Error: user name cannot be longer than 30 characters!");
                }

                var forbiddenCharacters = @"["";,]";
                bool isUserNameInvalid = Regex.IsMatch(userName, forbiddenCharacters);

                if (isUserNameInvalid == true)
                {
                    errorsReport.Add("\"Error: user name may contain " +
                     "neither \"(double quotes) nor ,(commas) nor ;(semi-colons)!\"");
                }
            }

            var userEmail = userDB.Email;
            if (string.IsNullOrWhiteSpace(userEmail) == true)
            {
                errorsReport.Add("#Error: user email is missing!");
            }
            else
            {
                userEmail = userEmail.Trim().ToUpper();

                if (userEmail.Length > 50)
                {
                    errorsReport.Add("#Error: user email cannot be longer than 50 characters!");
                }

                var invalidEmailFormat = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                bool isUserEmailValid = Regex.IsMatch(userEmail, invalidEmailFormat);

                if (isUserEmailValid == false)
                {
                    errorsReport.Add("#Error: user email format is invalid!");
                }
            }

            var userBirthDate_DateOnly = userDB.BirthDate;
            if (userBirthDate_DateOnly == null)
            {
                errorsReport.Add("#Error: user birth date is missing!");
            }
            else
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                int age = today.Year - userBirthDate_DateOnly.Value.Year;
                if (userBirthDate_DateOnly > today.AddYears(-age)) age--; // adjust if birthday hasn't occurred yet this year

                if (age < 12 || age > 90)
                {
                    errorsReport.Add("#Error: user age must be between 12 and 90 years!");
                }
            }
            var userBirthDate = userBirthDate_DateOnly == null ? "" : userBirthDate_DateOnly.Value.ToString("yyyy-MM-dd");

            var userGender = userDB.Gender.Value;
            if (userGender == null)
            {
                errorsReport.Add("#Error: user gender is missing!");
            }
            if (Enum.IsDefined(userGender) == false)
            {
                errorsReport.Add("#Error: user gender must be either male (0) or female (1)!");
            }

            var userSignUpDate_DateOnly = userDB.SignUpDate;
            if (userSignUpDate_DateOnly == null)
            {
                errorsReport.Add("#Error: user sign up date is missing!");
            }
            var userSignUpDate = userSignUpDate_DateOnly.Value.ToString("yyyy-MM-dd");

            if (errorsReport == null || errorsReport.Count == 0)
            {
                errorsReport.Add("");
            }

            return $"{string.Join(" | ", errorsReport)};\"{userName}\";{userEmail};{userBirthDate};{userGender};{userSignUpDate}";
        }
        private static string UserData_Validation_LifeCounterTemplates(DataBackUp_userObj_lifeCounterTemplate? template, List<string> errorsReport)
        {
            var templateName = template.LifeCounterTemplateName;
            if (string.IsNullOrWhiteSpace(templateName) == true)
            {
                errorsReport.Add("#Error: Life Counter Template Name is missing!...");
            }
            else
            {
                templateName = templateName.Trim().ToUpper();
                if (templateName.Length > 30)
                {
                    errorsReport.Add("#Error: Life Counter Template Name cannot be longer than 30 characters!...");
                }

                var forbiddenCharacters = @"["";,]";

                bool isTemplateNameInvalid = Regex.IsMatch(templateName, forbiddenCharacters);

                if (isTemplateNameInvalid == true)
                {
                    errorsReport.Add("\"Error: Life Counter Template Name may contain " +
                        "neither  \"(double quotes) nor ,(commas) nor ;(semi-colons)!\"");
                }
            }

            var templatePlayersStartingLifePoints = template.PlayersStartingLifePoints;
            if (templatePlayersStartingLifePoints == null || templatePlayersStartingLifePoints < 1)
            {
                errorsReport.Add("#Error: Players Starting Life Points must be greater than 0!...");
            }

            var templatePlayersCount = template.PlayersCount;
            if (templatePlayersCount == null || templatePlayersCount < 1)
            {
                errorsReport.Add("#Error: Players Count must be greater than 0!...");
            }

            var templateFixedMaxLifePointsMode = template.FixedMaxLifePointsMode;
            if (templateFixedMaxLifePointsMode == null)
            {
                errorsReport.Add("#Error: Fixed Max Life Points Mode is missing!...");
            }

            var templatePlayersMaxLifePoints = template.PlayersMaxLifePoints;
            if (templatePlayersMaxLifePoints != null && templateFixedMaxLifePointsMode == false)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is FALSE then Max Life Points cell must be empty!...");
            }
            if (templatePlayersMaxLifePoints != null && templateFixedMaxLifePointsMode == true && templatePlayersMaxLifePoints >= templatePlayersStartingLifePoints)
            {
                errorsReport.Add("#Error: Max Life Points may not be greater than nor equal to Players Starting Life Points!...");
            }
            if (templatePlayersMaxLifePoints == null && templateFixedMaxLifePointsMode == true)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is TRUE then Max Life Points cell must contain a value!...");
            }

            var templateAutoDefeatMode = template.AutoDefeatMode;
            if (templateAutoDefeatMode == null)
            {
                errorsReport.Add("#Error: Auto Defeat Mode is missing!...");
            }

            var templateAutoEndMode = template.AutoEndMode;
            if (templateAutoEndMode == null)
            {
                errorsReport.Add("#Error: Auto End Mode is missing!...");
            }
            if (templateAutoEndMode == true && templateAutoDefeatMode == false)
            {
                errorsReport.Add("#Error: Auto End Mode cannot be TRUE if Auto Defeat Mode is FALSE!...");
            }

            var templateLifeCounterManagersCount = template.LifeCounterManagersCount;
            if (templateLifeCounterManagersCount == null || templateLifeCounterManagersCount < 0)
            {
                errorsReport.Add("#Error: Life Counter Managers Count must be greater than or equal to 0!...");
            }

            return $"{string.Join(" | ", errorsReport)};\"{templateName}\";{templatePlayersStartingLifePoints};{templatePlayersCount};{Helper.BoolToEnabledDisabled(templateFixedMaxLifePointsMode)};{templatePlayersMaxLifePoints};{Helper.BoolToEnabledDisabled(templateAutoDefeatMode)};{Helper.BoolToEnabledDisabled(templateAutoEndMode)}  ;{templateLifeCounterManagersCount}";
        }
        private static string UserData_Validation_LifeCounterManagers(DataBackUp_userObj_lifeCounterManager? manager, DateOnly? signUpDate, List<string> errorsReport)
        {
            var templateName = manager.LifeCounterTemplateName;
            if (string.IsNullOrWhiteSpace(templateName) == true)
            {
                errorsReport.Add("#Error: Life Counter Manager Name is missing!...");
            }
            else
            {
                templateName = templateName.Trim().ToUpper();
                if (templateName.Length > 60)
                {
                    errorsReport.Add("#Error: Life Counter Manager Name cannot be longer than 60 characters!...");
                }

                var forbiddenCharacters = @"["";,]";

                bool isTemplateNameInvalid = Regex.IsMatch(templateName, forbiddenCharacters);

                if (isTemplateNameInvalid == true)
                {
                    errorsReport.Add("\"Error: Life Counter Template Name may contain " +
                        "neither \"(double quotes) nor ,(commas) nor ;(semi-colons)!\"");
                }
            }

            var managerName = manager.LifeCounterManagerName;
            if (string.IsNullOrWhiteSpace(managerName) == true)
            {
                errorsReport.Add("#Error: Life Counter Manager Name is missing!...");
            }
            else
            {
                managerName = managerName.Trim().ToUpper();
                if (managerName.Length > 60)
                {
                    errorsReport.Add("#Error: Life Counter Manager Name cannot be longer than 60 characters!...");
                }

                var forbiddenCharacters = @"["";,]";

                bool isManagerNameInvalid = Regex.IsMatch(managerName, forbiddenCharacters);

                if (isManagerNameInvalid == true)
                {
                    errorsReport.Add("\"Error: Life Counter Manager Name may contain " +
                        "neither \"(double quotes) nor ,(commas) nor ;(semi-colons)!\"");
                }
            }

            var managerPlayerStartingLifePoints = manager.PlayersStartingLifePoints;
            if (managerPlayerStartingLifePoints == null || managerPlayerStartingLifePoints < 1)
            {
                errorsReport.Add("#Error: Players Starting Life Points must be greater than 0!...");
            }

            var managerPlayersCount = manager.PlayersCount;
            if (managerPlayersCount == null || managerPlayersCount < 1)
            {
                errorsReport.Add("#Error: Players Count must be greater than 0!...");
            }

            var managerFirstPlayerIndex = manager.FirstPlayerIndex;
            if (managerFirstPlayerIndex == null || managerFirstPlayerIndex < 0 || managerFirstPlayerIndex > 5)
            {
                errorsReport.Add("#Error: First Player Index must be a value between 0 and 5!...");
            }

            var managerFixedMaxLifePointsMode = manager.FixedMaxLifePointsMode;
            if (managerFixedMaxLifePointsMode == null)
            {
                errorsReport.Add("#Error: Fixed Max Life Points Mode is missing!...");
            }

            var managerPlayersMaxLifePoints = manager.PlayersMaxLifePoints;
            if (managerPlayersMaxLifePoints != null && managerFixedMaxLifePointsMode == false)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is FALSE then Max Life Points cell must be empty!...");
            }
            if (managerPlayersMaxLifePoints != null && managerFixedMaxLifePointsMode == true && managerPlayersMaxLifePoints <= managerPlayerStartingLifePoints)
            {
                errorsReport.Add("#Error: Max Life Points may NOT be greater than NOR equal to Players Starting Life Points!...");
            }
            if (managerPlayersMaxLifePoints == null && managerFixedMaxLifePointsMode == true)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is TRUE then Max Life Points cell must contain a value!...");
            }

            var managerAutoDefeatMode = manager.AutoDefeatMode;
            if (managerAutoDefeatMode == null)
            {
                errorsReport.Add("#Error: Auto Defeat Mode is missing!...");
            }

            var managerAutoEndMode = manager.AutoEndMode;
            if (managerAutoEndMode == null)
            {
                errorsReport.Add("#Error: Auto End Mode is missing!...");
            }
            if (managerAutoEndMode == true && managerAutoEndMode == false)
            {
                errorsReport.Add("#Error: Auto End Mode cannot be TRUE if Auto Defeat Mode is FALSE!...");
            }

            var managerStartingTime_Ticks = manager.StartingTime;
            DateTime managerStartingTime_DateTime = new DateTime(managerStartingTime_Ticks!.Value);
            DateOnly managerStartingTime_DateOnly = DateOnly.FromDateTime(managerStartingTime_DateTime);
            if (managerStartingTime_Ticks == null || managerStartingTime_DateOnly < signUpDate)
            {
                errorsReport.Add("#Error: Invalid Starting Time mark!...");
            }
            var managerStartingTime = managerStartingTime_DateTime.ToString("dd/MM/yyyy HH:mm:ss");

            var managerEndingTime_Ticks = manager.EndingTime;
            var managerEndingTime = "";
            if (managerEndingTime_Ticks != null && managerEndingTime_Ticks > 0)
            {
                DateTime managerEndingTime_DateTime = new DateTime(managerEndingTime_Ticks!.Value);
                DateTime today_and_now = DateTime.Today;
                DateOnly managerEndingTime_DateOnly = DateOnly.FromDateTime(managerEndingTime_DateTime);

                if (managerStartingTime_Ticks != null && managerEndingTime_Ticks < 0 || managerEndingTime_DateTime > managerStartingTime_DateTime || managerEndingTime_DateTime > today_and_now)
                {
                    errorsReport.Add("#Error: Invalid Ending Time mark!...");
                }
                managerEndingTime = managerEndingTime_Ticks != null && managerEndingTime_Ticks > 0 ? managerEndingTime_DateTime.ToString("yyyy-MM-dd HH:mm:ss") : "";
            }

            var managerDuration_minutes = manager.Duration_minutes;
            if (managerDuration_minutes != null && (managerEndingTime_Ticks == null || managerEndingTime == ""))
            {
                errorsReport.Add("#Error: If Ending Time cell is empty Duration must be empty also!...");
            }
            if (managerDuration_minutes == null && (managerEndingTime_Ticks != null || managerEndingTime_Ticks > 0))
            {
                errorsReport.Add("#Error: Duration must be greater than or equal to 0 minutes!...");
            }

            var managerIsFinished = manager.IsFinished;
            if (managerIsFinished == null)
            {
                errorsReport.Add("#Error: Is Finished data is missing!...");
            }
            if ((managerIsFinished != null && managerIsFinished == true) && (managerEndingTime_Ticks == null || managerDuration_minutes == null))
            {
                errorsReport.Add("#Error: If Ending Time cell is empty and/or Duration cell is also empty then Is Finished cell must be empty as well!...");
            }
            if ((managerIsFinished == null || managerIsFinished == false) && (managerEndingTime_Ticks != null || managerDuration_minutes != null))
            {
                errorsReport.Add("#Error: If neither Ending Time cell is empty NOR Duration cell then Is Finished cell may not be empty nor false!...");
            }

            if (errorsReport == null || errorsReport.Count == 0)
            {
                errorsReport.Add("");
            }

            return $"{string.Join(" | ", errorsReport)};\"{templateName}\";\"{managerName}\";{managerPlayerStartingLifePoints};{managerPlayersCount};{managerFirstPlayerIndex};{Helper.BoolToEnabledDisabled(managerFixedMaxLifePointsMode)};{managerPlayersMaxLifePoints};{Helper.BoolToEnabledDisabled(managerAutoDefeatMode)};{Helper.BoolToEnabledDisabled(managerAutoEndMode)};{managerStartingTime};{managerEndingTime};{managerDuration_minutes};{Helper.BoolToEnabledDisabled(managerIsFinished)}";
        }
        private static string UserData_Validation_LifeCounterPlayers(DataBackUp_userObj_lifeCounterPlayer? player, List<string> errorsReport)
        {
            var managerName = player.LifeCounterManagerName;
            if (string.IsNullOrWhiteSpace(managerName) == true)
            {
                errorsReport.Add("#Error: Life Counter Template Name is missing!...");
            }
            else
            {
                managerName = managerName.Trim().ToUpper();
                if (managerName.Length > 60)
                {
                    errorsReport.Add("#Error: Life Counter Template Name cannot be longer than 60 characters!...");
                }

                var forbiddenCharacters = @"["";,]";

                bool isPlayerNameInvalid = Regex.IsMatch(managerName, forbiddenCharacters);

                if (isPlayerNameInvalid == true)
                {
                    errorsReport.Add("\"Error: Life Counter Player Name may contain " +
                        "neither \"(double quotes) nor ,(commas) nor ;(semi-colons)!\"");
                }
            }

            var lifeCounterPlayer_Name = player.PlayerName;
            if (string.IsNullOrWhiteSpace(lifeCounterPlayer_Name) == true)
            {
                errorsReport.Add("#Error: Life Counter Template Name is missing!...");
            }
            else
            {
                lifeCounterPlayer_Name = lifeCounterPlayer_Name.Trim().ToUpper();
                if (lifeCounterPlayer_Name.Length > 30)
                {
                    errorsReport.Add("#Error: Life Counter Template Name cannot be longer than 30 characters!...");
                }

                var forbiddenCharacters = @"["";,]";

                bool isPlayerNameInvalid = Regex.IsMatch(lifeCounterPlayer_Name, forbiddenCharacters);

                if (isPlayerNameInvalid == true)
                {
                    errorsReport.Add("\"Error: Life Counter Player Name may contain " +
                        "neither \"(double quotes) nor ,(commas) nor ;(semi-colons)!\"");
                }
            }

            var lifeCounterPlayer_StartingLifePoints = player.StartingLifePoints;
            if (lifeCounterPlayer_StartingLifePoints == null || lifeCounterPlayer_StartingLifePoints < 1)
            {
                errorsReport.Add("#Error: Player Starting Life Points must be greater than 0!");
            }

            var lifeCounterPlayer_CurrentLifePoints = player.CurrentLifePoints;
            if (lifeCounterPlayer_CurrentLifePoints == null)
            {
                errorsReport.Add("#Error: Player Current Life Points is missing!");
            }

            var lifeCounterPlayer_FixedMaxLifePointsMode = player.FixedMaxLifePointsMode;
            if (lifeCounterPlayer_FixedMaxLifePointsMode == null)
            {
                errorsReport.Add("#Error: Fixed Max Life Points Mode is missing!");
            }

            var lifeCounterPlayer_MaxLifePoints = player.MaxLifePoints;
            if (lifeCounterPlayer_MaxLifePoints != null && lifeCounterPlayer_FixedMaxLifePointsMode == false)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is FALSE then Max Life Points cell must be empty!");
            }
            if (lifeCounterPlayer_MaxLifePoints != null && lifeCounterPlayer_FixedMaxLifePointsMode == true && lifeCounterPlayer_MaxLifePoints <= lifeCounterPlayer_MaxLifePoints)
            {
                errorsReport.Add("#Error: Max Life Points may NOT be lesser than NOR equal to Starting Life Points!");
            }
            if (lifeCounterPlayer_MaxLifePoints == null && lifeCounterPlayer_FixedMaxLifePointsMode == true)
            {
                errorsReport.Add("#Error: If Fixed Max Life mode is TRUE then Max Life Points cell must contain a value!");
            }

            var lifeCounterPlayer_AutoDefeatMode = player.AutoDefeatMode;
            if (lifeCounterPlayer_AutoDefeatMode == null)
            {
                errorsReport.Add("#Error: Auto Defeat Mode is missing!");
            }
            if (lifeCounterPlayer_AutoDefeatMode != null && lifeCounterPlayer_AutoDefeatMode == true && lifeCounterPlayer_CurrentLifePoints < 0)
            {
                errorsReport.Add("#Error: If Auto Defeat Mode cell value is true then Current Life Points must be zero!");
            }

            var lifeCounterPlayer_IsDefeated = player.IsDefeated;
            if (lifeCounterPlayer_IsDefeated == null)
            {
                errorsReport.Add("#Error: Is Defeated data is missing!");
            }
            if (lifeCounterPlayer_IsDefeated == true && lifeCounterPlayer_AutoDefeatMode == false)
            {
                errorsReport.Add("#Error: If Auto Defeat Mode cell has the value false then Is Defeated cell value MUST be false!");
            }
            if (lifeCounterPlayer_IsDefeated == false && lifeCounterPlayer_AutoDefeatMode == true && lifeCounterPlayer_CurrentLifePoints <= 0)
            {
                errorsReport.Add("#Error: If Auto Defeat Mode cell has the value true and Current Life Points is less or equal to zero then Is Defeated cell value MUST be true!");
            }

            if (errorsReport == null || errorsReport.Count == 0)
            {
                errorsReport.Add("");
            }

            return $"{string.Join(" | ", errorsReport)};\"{managerName}\";\"{lifeCounterPlayer_Name}\";{lifeCounterPlayer_StartingLifePoints};{lifeCounterPlayer_CurrentLifePoints};{Helper.BoolToEnabledDisabled(lifeCounterPlayer_FixedMaxLifePointsMode)};{lifeCounterPlayer_MaxLifePoints};{Helper.BoolToEnabledDisabled(lifeCounterPlayer_AutoDefeatMode)};{Helper.BoolToEnabledDisabled(lifeCounterPlayer_IsDefeated)}";
        }
        private static string UserData_Validation_BoardGameSessions(DataBackUp_userObj_Sessions? session, DateOnly? signUpDate, List<string> errorsReport)
        {
            var boardGameName = session!.BoardGameName;
            if (string.IsNullOrWhiteSpace(boardGameName) == true)
            {
                errorsReport.Add("#Error: user name is missing!");
            }
            else
            {
                boardGameName = boardGameName.Trim().ToUpper();

                if (boardGameName.Length > 30)
                {
                    errorsReport.Add("#Error: user name cannot be longer than 30 characters!");
                }

                var forbiddenCharacters = @"["";,]";
                bool isUserNameInvalid = Regex.IsMatch(boardGameName, forbiddenCharacters);

                if (isUserNameInvalid == true)
                {
                    errorsReport.Add("\"Error: user name may contain " +
                     "neither \"(double quotes) nor ,(commas) nor ;(semi-colons)!\"");
                }
            }


            var date = session.Date;
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (date == null || date > today)
            {
                errorsReport.Add("#Error: Date is missing or invalid!...");
            }

            var playersCount = session.PlayersCount;
            if (playersCount == null || playersCount < 1 || playersCount > 6)
            {
                errorsReport.Add("#Error: Players Count must be a value between 1 nad 6!...");
            }

            var duration = session.Duration_minutes;
            if (duration == null || duration < 0)
            {
                errorsReport.Add("#Error: Duration must be greater than or equal to 0 minutes!...");
            }
            if (duration > 1440)
            {
                errorsReport.Add("#Error: Duration must be less than or equal to 1440 minutes (24 hours)!...");
            }

            var isDeleted = session.IsDeleted;
            if (isDeleted == null)
            {
                errorsReport.Add("#Error: Is Deleted data is missing!...");
            }

            return $"{string.Join(" | ", errorsReport)};\"{boardGameName}\";{date};{playersCount};{duration};{Helper.BoolToEnabledDisabled(isDeleted)}";
        }
        private static string UserData_Validation_BoardGameRatings(DataBackUp_userObj_Ratings? rating, List<string> errorsReport)
        {
            var boardGameName = rating.BoardGameName;
            if (string.IsNullOrWhiteSpace(boardGameName) == true)
            {
                errorsReport.Add("#Error: user name is missing!");
            }
            else
            {
                boardGameName = boardGameName.Trim().ToUpper();

                if (boardGameName.Length > 30)
                {
                    errorsReport.Add("#Error: user name cannot be longer than 30 characters!");
                }

                var forbiddenCharacters = @"["";,]";
                bool isUserNameInvalid = Regex.IsMatch(boardGameName, forbiddenCharacters);

                if (isUserNameInvalid == true)
                {
                    errorsReport.Add("\"Error: user name may contain " +
                     "neither \"(double quotes) nor ,(commas) nor ;(semi-colons)!\"");
                }
            }

            var rate = rating.Rate;
            if (rate == null || rate < 0 || rate > 5)
            {
                errorsReport.Add("#Error: Rate must be between 0 and 5!...");
            }

            return $"{string.Join(" | ", errorsReport)};\"{boardGameName}\";\"{rate}\"";
        }


        private static (DataBackUp_userObj, List<string>) ParseUser(List<string> lines, List<string> profileDetails_Table_errorsReport)
        {
            if (lines == null || lines.Count < 2)
            {
                throw new Exception("Invalid user section");
            }

            var header = lines[0]; // skip or validate

            var data = lines[1].Split(';');

            if (data.Length < 5)
            {
                profileDetails_Table_errorsReport.Add("#Error: User data row is incomplete.!");
            }

            var importedUser = new DataBackUp_userObj
            {
                Name = data[1].Trim('"'),
                Email = data[2].Trim('"'),
                BirthDate = DateOnly.Parse(data[3]),
                Gender = Enum.Parse<Gender>(data[4].Trim('"')),
                SignUpDate = DateOnly.Parse(data[5])
            };

            return (importedUser, profileDetails_Table_errorsReport);
        }
        private static (List<DataBackUp_userObj_lifeCounterTemplate>, List<string>) ParseLifeCounterTemplates(List<string>? lines, List<string> lifeCounter_templatesTable_errorsReport)
        {
            if (lines == null || lines.Count < 2)
            {
                throw new Exception("Invalid user section");
            }

            var header = lines[0]; // skip or validate           

            var importedLifeCounterTemplates = new List<DataBackUp_userObj_lifeCounterTemplate>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {         
                var data = lines[i].Split(';');

                if (data.Length < 8)
                {
                    lifeCounter_templatesTable_errorsReport.Add("#Error: User's life counter templates data is incomplete!");
                    continue;
                }

                importedLifeCounterTemplates.Add(new DataBackUp_userObj_lifeCounterTemplate
                {
                    LifeCounterTemplateName = data[1].Trim('"'),
                    PlayersStartingLifePoints = int.Parse(data[2]),
                    PlayersCount = int.Parse(data[3]),
                    FixedMaxLifePointsMode = Helper.ParseEnabledDisabledToBool(data[4]),
                    PlayersMaxLifePoints = int.TryParse(data[5], out int playersMaxLifePoints) ? playersMaxLifePoints : null,
                    AutoDefeatMode = Helper.ParseEnabledDisabledToBool(data[6]),
                    AutoEndMode = Helper.ParseEnabledDisabledToBool(data[7]),
                    LifeCounterManagersCount = int.Parse(data[8])
                });
            }

            return (importedLifeCounterTemplates, lifeCounter_templatesTable_errorsReport);
        }
        private static (List<DataBackUp_userObj_lifeCounterManager>, List<string>) ParseLifeCounterManagers(List<string>? lines, List<string> lifeCounter_managersTable_errorsReport)
        {
            if (lines == null || lines.Count < 2)
            {
                throw new Exception("Invalid user section");
            }

            var header = lines[0]; // skip or validate         

            var importedLifeCounterManagers = new List<DataBackUp_userObj_lifeCounterManager>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {               
                var data = lines[i].Split(';');

                if (data.Length < 13)
                {
                    lifeCounter_managersTable_errorsReport.Add("#Error: User's life counter managers data is incomplete!");
                    continue;
                }

                long? startingTime = string.IsNullOrWhiteSpace(data[10]) == true ?
                    null :
                    DateTime.ParseExact(data[10], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).Ticks;

                long? endingTime = string.IsNullOrWhiteSpace(data[11]) == true ?
                    null:
                    DateTime.ParseExact(data[11], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).Ticks;
               
                importedLifeCounterManagers.Add(new DataBackUp_userObj_lifeCounterManager
                {
                    LifeCounterTemplateName = data[1].Trim('"'),
                    LifeCounterManagerName = data[2].Trim('"'),
                    PlayersStartingLifePoints = int.Parse(data[3]),
                    PlayersCount = int.Parse(data[4]),
                    FirstPlayerIndex = int.Parse(data[5]),
                    FixedMaxLifePointsMode = Helper.ParseEnabledDisabledToBool(data[6]),
                    PlayersMaxLifePoints = int.TryParse(data[7], out int playersMaxLifePoints) ? playersMaxLifePoints : null,
                    AutoDefeatMode = Helper.ParseEnabledDisabledToBool(data[8]),
                    AutoEndMode = Helper.ParseEnabledDisabledToBool(data[9]),
                    StartingTime = startingTime,
                    EndingTime = endingTime,
                    Duration_minutes = double.TryParse(data[12], out double duration) ? duration : null,
                    IsFinished = Helper.ParseEnabledDisabledToBool(data[13]),
                });
            }

            return (importedLifeCounterManagers, lifeCounter_managersTable_errorsReport);
        }
        private static (List<DataBackUp_userObj_lifeCounterPlayer>, List<string>) ParseLifeCounterPlayers(List<string>? lines, List<string> lifeCounter_playersTable_errorsReport)
        {
            if (lines == null || lines.Count < 2)
            {
                throw new Exception("Invalid user section");
            }

            var header = lines[0]; // skip or validate

            

            var importedLifeCounterPlayers = new List<DataBackUp_userObj_lifeCounterPlayer>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {                               
                var data = lines[i].Split(';');

                if (data.Length < 8)
                {
                    lifeCounter_playersTable_errorsReport.Add("#Error: User's life counter players data is incomplete!");
                    continue;
                }

                importedLifeCounterPlayers.Add(new DataBackUp_userObj_lifeCounterPlayer
                {
                    LifeCounterManagerName = data[1].Trim('"'),
                    PlayerName = data[2].Trim('"'),
                    StartingLifePoints = int.Parse(data[3]),
                    CurrentLifePoints = int.Parse(data[4]),
                    FixedMaxLifePointsMode = Helper.ParseEnabledDisabledToBool(data[5]),
                    MaxLifePoints = int.TryParse(data[6], out int maxLifePoints) ? maxLifePoints : null,
                    AutoDefeatMode = Helper.ParseEnabledDisabledToBool(data[7]),
                    IsDefeated = Helper.ParseEnabledDisabledToBool(data[8]),
                });
            }

            return (importedLifeCounterPlayers, lifeCounter_playersTable_errorsReport);
        }
        private static (List<DataBackUp_userObj_Sessions>, List<string>) ParseBoardGameSessions(List<string>? lines, List<string> boardGame_sessionsTable_errorsReport)
        {
            if (lines == null || lines.Count < 2)
            {
                throw new Exception("Invalid user section");
            }

            var header = lines[0]; // skip or validate           

            var importedBoarGameSessions = new List<DataBackUp_userObj_Sessions>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {
                var data = lines[i].Split(';');

                if (data.Length < 5)
                {
                    boardGame_sessionsTable_errorsReport.Add("#Error: User's board game sessions data is incomplete!");
                }

                data = lines[i].Split(';');

                importedBoarGameSessions.Add(new DataBackUp_userObj_Sessions
                {
                    BoardGameName = data[1].Trim('"'),
                    Date = DateOnly.TryParse(data[2].Trim('"'), out var date) ? date : null,
                    PlayersCount = int.Parse(data[3]),
                    Duration_minutes = int.Parse(data[4]),
                    IsDeleted = Helper.ParseEnabledDisabledToBool(data[5]),
                });
            }

            return (importedBoarGameSessions, boardGame_sessionsTable_errorsReport);
        }
        private static (List<DataBackUp_userObj_Ratings>, List<string>) ParseBoardGameRatings(List<string>? lines, List<string> boardGame_ratingsTable_errorsReport)
        {
            if (lines == null || lines.Count < 2)
            {
                throw new Exception("Invalid user section");
            }

            var header = lines[0]; // skip or validate          

            var importedBoarGameRatings = new List<DataBackUp_userObj_Ratings>();

            // Skip header line at index 0
            for (int i = 1; i < lines.Count - 1; i++)
            {
                var data = lines[i].Split(';');

                if (data.Length < 2)
                {
                    boardGame_ratingsTable_errorsReport.Add("#Error: User's board game ratings data is incomplete!");
                    continue;
                }

                importedBoarGameRatings.Add(new DataBackUp_userObj_Ratings
                {
                    BoardGameName = data[1].Trim('"'),
                    Rate = decimal.Parse(data[2].Trim('"'), new CultureInfo("pt-BR"))
                });
            }

            return (importedBoarGameRatings, boardGame_ratingsTable_errorsReport);
        }


        #endregion


        #region USER'S PROFILE

        public async Task<(UsersSignUpResponse?, string)> SignUp(UsersSignUpRequest? request, string? userRole)
        {
            var (isValid, message) = SignUp_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }     
           
            var userEmail_exists = await this._daoDbContext
                .Users
                .AsNoTracking()
                .AnyAsync(a => a.Email == request!.Email && a.IsDeleted == false);

            if (userEmail_exists == true)
            {
                return (null, "Error: requested emal is already in use");
            }            

            var parsedDate = DateOnly.ParseExact(request!.UserBirthDate!, "yyyy-MM-dd");

            var user = new User
            {
                Name = request.Name!,
                UserName = request.Email!,
                Email = request.Email!.ToLower(),
                BirthDate = parsedDate,
                Gender = request.Gender
            };

            var signUpAttempt = await _userManager.CreateAsync(user, request.Password!);

            if(signUpAttempt.Succeeded == false)
            {
                var errors = string.Join("; ", signUpAttempt.Errors.Select(e => e.Description));
                return (null, $"Error creating user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "User");

            return (new UsersSignUpResponse(), "User signed up successfully");
        }
        private static (bool, string) SignUp_Validation(UsersSignUpRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.Name) == true)
            {
                return (false, "Error: Name is null or empty");
            }

            if (string.IsNullOrWhiteSpace(request.Email)== true)
            {
                return (false, "Error: UserEmail is missing");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.Email, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return (false, "Error: password is null");
            }

            if (request.Password.Trim().Length < 6)
            {
                return (false, "Error: password must have at leat 6 digits");
            }
            
            if(string.IsNullOrEmpty(request.Password) == true)
            {
                return (false, "Error: UserPassword is missing");
            }
            
            if (string.IsNullOrWhiteSpace(request.UserBirthDate) == true)
            {
                return (false, "Error: UserBirthDate is missing");
            }

            string birthDatePattern = @"^(19|20)\d{2}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$";

            if (Regex.IsMatch(request.UserBirthDate, birthDatePattern) == false)
            {
                return (false, "Error: invalid birth date format. Expected format: yyyy-MM-dd");
            }

            // Convert string to DateOnly
            if (DateOnly.TryParseExact(request.UserBirthDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
            {
                return (false, "Error: invalid birth date");
            }

            int age = DateTime.Now.Year - parsedDate.Year;

            if (parsedDate.Month > DateTime.Now.Month || 
                    (parsedDate.Month == DateTime.Now.Month && parsedDate.Day > DateTime.Now.Day))
            {
                age--;
            }

            if (age < 12)
            {
                return (false, "Error: the minimum age for signing up is 12");
            }

            if (age > 90)
            {
                return (false, "Error: invalid birth date");
            }          

            if (Enum.IsDefined(request.Gender) == false)
            {
                var validOption = string.Join(", ", Enum.GetValues(typeof(Gender))
                                       .Cast<Gender>()
                                       .Select(gender => $"{gender} ({(int)gender})"));

                return (false, $"Error: invalid Gender. It must be one of the following: {validOption}");
            }

            return (true, string.Empty);
        }


        private async Task<(int, string)>ValidatePassword(User userDB, string userPassword)
        {
            var isPasswordValid = await _userManager.CheckPasswordAsync(userDB, userPassword);

            var countFailedAttempts = await this._userManager
                .GetAccessFailedCountAsync(userDB);

            var maxAllowedAttempts = this._userManager
                .Options
                .Lockout
                .MaxFailedAccessAttempts;

            var remainingAttempts = maxAllowedAttempts - countFailedAttempts;

            var isUserLocked = await this._userManager.IsLockedOutAsync(userDB);

            if (isUserLocked == true)
            {
                return (-1, "Error: account temporarily locked duo to multiple failed attempts");
            }
         
            if (isPasswordValid == false)
            {
                remainingAttempts--;
                await _userManager.AccessFailedAsync(userDB);
                return (remainingAttempts, $"Invalid Password. You have {remainingAttempts} attempts remaining");
            }

            await this._userManager.ResetAccessFailedCountAsync(userDB);

            return (remainingAttempts, string.Empty);
        }
       

        public async Task<(UsersSignInResponse?, string)> SignIn(UsersSignInRequest? request)
        {
            var (isValid, message) = SignIn_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                .Users
                .FirstOrDefaultAsync(a => a.Email == request!.Email);          

            if(userDB == null)
            {
                return (null, "Requested user does not exist");
            }
            if(userDB.IsDeleted == true)
            {
                return (null, "Requested user has been deleted");
            }

            var result = await this._signInManager
                .PasswordSignInAsync(request!.Email!, request.Password!, false, true);

            if (result.IsLockedOut == true)
            {
                return (null, "Error: account temporarily locked due to multiple failed attempts");
            }

            if (result.IsNotAllowed == true)
            {
                return (null, "Error: account is not allowed to sign in (e.g. email not confirmed)");
            }

            var countFailedAttempts = await this._userManager
                .GetAccessFailedCountAsync(userDB);

            var maxAllowedAttempts = this._userManager
                .Options
                .Lockout
                .MaxFailedAccessAttempts;

            var remainingAttempts = maxAllowedAttempts - countFailedAttempts;
    

            if (result.Succeeded == false)
            {               
                var response = new UsersSignInResponse
                {
                    RemainingSignInAttempts = remainingAttempts
                };

                return (response, $"Error: email or password is incorrect. You have {remainingAttempts} attempts remaining");
            }

            await this._userManager.ResetAccessFailedCountAsync(userDB);

            return (new UsersSignInResponse(), $"User: {userDB.Name} signed in successfully");
        }
        private static (bool, string) SignIn_Validation(UsersSignInRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }       

            if (string.IsNullOrWhiteSpace(request.Email) == true)
            {
                return (false, "Error: Email is missing");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.Email, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if (string.IsNullOrEmpty(request.Password) == true)
            {
                return (false, "Error: UserPassword is missing");
            }

            return (true, string.Empty);
        }


        public (UsersValidateStatusResponse?, string) ValidateStatus()
        {         
            if (this._httpContextAccessor.HttpContext?.User.Identity != null &&
        this._httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
            {
                return (new UsersValidateStatusResponse {IsUserLoggedIn = true  } , "User is authenticated.");
            }
            else
            {
                return (new UsersValidateStatusResponse { IsUserLoggedIn = false },  "User is not authenticated.");
            }
        }


        public async Task<(UsersGetRoleResponse?, string)> GetRole()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
            if (user == null)
            {
                return (null, "Error: User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault(); // assuming 1 role per user

            if (string.IsNullOrEmpty(userRole))
            {
                return (null, "Error: User has no role assigned");
            }

            return (new UsersGetRoleResponse
            {
                Role = userRole
            }, $"User role: {userRole}" );
        }


        public async Task<(UsersSignOutResponse?, string)> SignOut()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext!.User);
            if (user == null)
            {
                return (null, "Error: User not found");
            }

            if (user.IsDeleted == true)
            {
                return (null, "Error: User has been deleted");
            }   

            try
            {
                await _signInManager.SignOutAsync();

                return (new UsersSignOutResponse { IsUserSignOut = true }, "User signed out successfully");
            }
            catch (Exception ex)
            {               
                return (new UsersSignOutResponse { IsUserSignOut = false }, $"Error: Failed to sign out user. {ex.Message}");
            }
        }


        public async Task<(UsersEditProfileResponse?, string)> EditProfile(UsersEditProfileRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditProfile_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            } 

            var userEmail_exists = await this._daoDbContext
                                                .Users
                                                .AsNoTracking()
                                                .AnyAsync(a => a.Id != userId &&
                                                               a.Email == request!.NewEmail &&
                                                               a.IsDeleted == false);

            if(userEmail_exists == true)
            {
                return (null, "Error: requested Email is already in use");
            }

            var userDB = await this._daoDbContext
                                   .Users
                                   .FindAsync(userId);

            if(userDB == null)
            {
                return (null, "Error: user not found");
            }

            if(userDB.IsDeleted == true)
            {
                return (null, "Error: user is deleted");
            }
            
            var parsedDate = DateOnly.ParseExact(request.NewBirthDate!, "yyyy-MM-dd");

           

            userDB.Name = request.NewName;
            userDB.Email = request.NewEmail!.ToLower();
            userDB.UserName = request.NewEmail;
            userDB.BirthDate = parsedDate;
            
            var updateResult = await _userManager.UpdateAsync(userDB);

            if (!updateResult.Succeeded)
            {
                return (null, "Error: failed to update user profile");
            }

            return (null, "User's profile edited successfully");
        }
        private static (bool, string) EditProfile_Validation(UsersEditProfileRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.NewName) == true)
            {
                return (false, "Error: Name is missing");
            }

            if (string.IsNullOrWhiteSpace(request.NewEmail) == true)
            {
                return (false, "Error: UserEmail is missing");
            }

            string emailPattern = @"(^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,10})?$)";

            if (Regex.IsMatch(request.NewEmail, emailPattern) == false)
            {
                return (false, "Error: invalid email format");
            }

            if (string.IsNullOrWhiteSpace(request.NewBirthDate) == true)
            {
                return (false, "Error: UserBirthDate is missing");
            }

            string birthDatePattern = @"^(19|20)\d{2}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$";

            if (Regex.IsMatch(request.NewBirthDate, birthDatePattern) == false)
            {
                return (false, "Error: invalid birth date format. Expected format: yyyy-MM-dd");
            }

            // Convert string to DateOnly
            if (DateOnly.TryParseExact(request.NewBirthDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
            {
                return (false, "Error: invalid birth date");
            }

            int age = DateTime.Now.Year - parsedDate.Year;

            if (parsedDate.Month > DateTime.Now.Month || 
                    (parsedDate.Month == DateTime.Now.Month && parsedDate.Day > DateTime.Now.Day))
            {
                age--;
            }

            if (age < 12)
            {
                return (false, "Error: the minimum age for signing up is 12");
            }

            if (age > 90)
            {
                return (false, "Error: birth date is too old");
            }

            return(true, string.Empty);
        }


        public async Task<(UsersChangePasswordResponse?, string)> ChangePassword(UsersChangePasswordRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ChangePassword_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                                   .Users
                                   .FindAsync(userId);

            if (userDB == null)
            {
                return (null, "Error: user not found");
            }

            if (userDB.IsDeleted == true)
            {
                return (null, "Error: user is deleted");
            }

            var (remainingAttempts, text) = await this.ValidatePassword(userDB, request!.CurrentPassword!);
            
            if (remainingAttempts < 3)
            {
                return (new UsersChangePasswordResponse
                {
                    RemainingPasswordAttempts = remainingAttempts
                }, text);
            }
            
            // Remove current password
            var removeResult = await _userManager.RemovePasswordAsync(userDB);
            if (removeResult.Succeeded == false)
            {
                return (null, "Error: failed to remove old password");
            }

            // Sets new password
            var addPasswordResult = await _userManager.AddPasswordAsync(userDB, request!.NewPassword!);
            if (addPasswordResult.Succeeded == false)
            {
                return (null, "Error: failed to set new password");
            }

            return (new UsersChangePasswordResponse(), "Password changed successfully");
        }
        public static (bool, string) ChangePassword_Validation(UsersChangePasswordRequest? request)
        {
            if(request == null)
            {
                return (false, "Error: null request");
            }

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return (false, "Error: password is null");
            }

            if (request.CurrentPassword.Trim().Length < 6)
            {
                return (false, "Error: password must have at leat 6 digits");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return (false, "Error: password is null");
            }

            if (request.NewPassword.Trim().Length < 6)
            {
                return (false, "Error: password must have at leat 6 digits");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersDeleteProfileResponse?, string)> DeleteProfile(UsersDeleteProfileRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }                        

            var (isValid, message) = DeleteProfile_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                .Users                               
                .Include(a => a.Sessions)
                .Include(a => a.Ratings)
                .Include(a => a.LifeCounterTemplates)
                .Include(a => a.LifeCounterManagers!)
                .ThenInclude(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a => a.Id == userId);

            if(userDB == null)
            {
                return (null, "Error: user not found");
            }

            if(userDB.IsDeleted == true)
            {
                return (null, "Error: this user's profile was already deleted");
            }

            var (remainingAttempts, text) = await this.ValidatePassword(userDB, request!.Password!);

            if (remainingAttempts < 3)
            {
                return (new UsersDeleteProfileResponse
                {
                    RemainingPasswordAttempts = remainingAttempts
                }, text);
            }

            this._daoDbContext.Sessions.RemoveRange(userDB.Sessions!);

            this._daoDbContext.Ratings.RemoveRange(userDB.Ratings!);

            this._daoDbContext.LifeCounterPlayers.RemoveRange(userDB.LifeCounterManagers!.SelectMany(a => a.LifeCounterPlayers!));   

            this._daoDbContext.LifeCounterManagers.RemoveRange(userDB.LifeCounterManagers!);

            this._daoDbContext.LifeCounterTemplates.RemoveRange(userDB.LifeCounterTemplates!);

            userDB.IsDeleted = true;

            await this._daoDbContext.SaveChangesAsync();        

            await this.SignOut();

            return (new UsersDeleteProfileResponse(), "User's profile deleted successfully");
        }
        private static (bool, string) DeleteProfile_Validation(UsersDeleteProfileRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }
            
            if(string.IsNullOrWhiteSpace(request.Password) == true)
            {
                return (false, "Error: requested password is empty");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersGetProfileDetailsResponse?, string)> GetProfileDetails(UsersGetProfileDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetProfileDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var userDB = await this._daoDbContext
                .Users
                .FindAsync(userId);

            if(userDB == null)
            {
                return (null, "Error: User not found");
            }
            
            if (userDB.IsDeleted == true)
            {
                return (null, "Error: User has been deleted");
            }

            var countSessionsDB = await this._daoDbContext
                .Sessions
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.IsDeleted == false)
                .CountAsync();

            var countRatedBgDB = await this._daoDbContext
                .Ratings
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .CountAsync();

            var treatmentTitle = userDB.Gender == 0 ? "Mr." : "Mrs.";

            return (new UsersGetProfileDetailsResponse
            {
                TreatmentTitle = treatmentTitle,
                Name = userDB.Name,
                Email = userDB.Email,
                BirthDate = userDB.BirthDate,
                SignUpDate = userDB.SignUpDate,
                SessionsCount = countSessionsDB,
                RatedBgCount = countRatedBgDB
            }, "User details loaded successfully");

        }
        private static (bool, string) GetProfileDetails_Validation(UsersGetProfileDetailsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null. It must be null!");
            }         

            return (true, string.Empty);
        }

        #endregion

        
        #region USER'S  BOARD GAMES

        public async Task<(UsersLogSessionResponse?, string)> LogSession(UsersLogSessionRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = LogSession_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var user_exists = await this._daoDbContext
                                        .Users
                                        .AsNoTracking()
                                        .AnyAsync(a => a.Id == userId && a.IsDeleted == false);

            if(user_exists == false)
            {
                return (null, "Error: user not found");
            }

            var boardgameDB = await this._daoDbContext
                                             .BoardGames
                                             .FindAsync(request!.BoardGameId);
            
            if(boardgameDB == null)
            {
                return (null, "Error: board game not found");
            }

            if(boardgameDB.IsDeleted == true)
            {
                return (null, "Error: board game is deleted");
            }        

            var newSession = new Session
            {
                UserId = userId!,
                BoardGameId = request.BoardGameId!.Value,
                PlayersCount = request.PlayersCount!.Value,
                Duration_minutes = request.Duration_minutes!.Value
            };

            boardgameDB.SessionsCount++;

            if(request.Date != null)
            {
                newSession.Date = DateOnly.ParseExact(request.Date!, "yyyy-MM-dd");
            }

            await this._daoDbContext.Sessions.AddAsync(newSession);

            var newAvgDuration = (boardgameDB.AvgDuration_minutes * boardgameDB.SessionsCount + request.Duration_minutes!.Value) / (boardgameDB.SessionsCount + 1);

            await this._daoDbContext
                .BoardGames
                .Where(a => a.Id == boardgameDB.Id)
                .ExecuteUpdateAsync(a => a.SetProperty(b => b.AvgDuration_minutes, newAvgDuration));    

            await this._daoDbContext.SaveChangesAsync();

            return (null, $"Session logged successfully, new Avg.Duration is: {newAvgDuration} minutes");
        }
        private static (bool, string) LogSession_Validation(UsersLogSessionRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.BoardGameId.HasValue == false)
            {
                return(false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoarGameId (is less than 1)");
            }

            if(string.IsNullOrWhiteSpace(request.Date) == false)
            {
                string datePattern = @"^(19|20)\d{2}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$";

                if (Regex.IsMatch(request.Date, datePattern) == false)
                {
                    return (false, "Error: invalid date format. Expected format: yyyy-MM-dd");
                }

                // Convert string to DateOnly
                if (DateOnly.TryParseExact(request.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
                {
                    return (false, "Error: invalid date");
                }

                var thisYear = DateTime.Now.Year;
                
                var requestedYear = parsedDate.Year;
                
                if(thisYear - requestedYear < 0)
                {
                    return (false, "Error: invalid date. Requested date is a future date");
                }

                if (thisYear - requestedYear > 100)
                {
                    return (false, "Error: invalid date. Minimum date allowed is 100 years ago");
                }
            }

            if(request.PlayersCount.HasValue == false)
            {
                return (false, "Error: PlayersCount is missing");
            }

            if(request.PlayersCount < 1)
            {
                return (false, "Error: invalid PlayersCount (is less than 1)");
            }

            if(request.Duration_minutes.HasValue == false)
            {
                return (false, "Error: Daration_minutes is missing");
            }

            if(request.Duration_minutes < 0)
            {
                return (false, "Error: invalid Duration_minutes (is negative)");
            }

            if(request.Duration_minutes > 1440)
            {
                return (false, "Error: invalid Duration_minutes (the maximum duration allowed is 1440 minutes = 1 day)");
            }

            return (true, string.Empty);           
        }
        

        public async Task<(List<UsersListPlayedBoardGamesResponse>?, string)> ListPlayedBoardGames(UsersListPlayedBoardGamesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListPlayedBoardGames_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = await this._daoDbContext
                .BoardGames
                .Where(a => a.IsDeleted == false && a.Sessions!.Any(b => b.UserId == userId && b.IsDeleted == false))
                .Select(a => new UsersListPlayedBoardGamesResponse
                {
                    BoardGameId = a.Id,
                    BoardGameName = a.Name
                })
                .ToListAsync();
           

            if (content == null || content.Count == 0)
            {
                return (null, "No sessions logged by user yet");
            }      
        
            return (content, "Board Games played by user listed successfully");
        }
        private static (bool, string) ListPlayedBoardGames_Validation(UsersListPlayedBoardGamesRequest? request)
        {
            if (request == null)
            {
                return (true, string.Empty);
            }
            
            return (true, string.Empty);
        }


        public async Task<(UsersGetSessionsResponse?, string)> GetSessions(UsersGetSessionsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetSessions_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var bgExists = await this._daoDbContext
                .BoardGames
                .AsNoTracking()
                .AnyAsync(a => a.Id == request!.BoardGameId );

            if(bgExists == false)
            {
                return (null, "Error: requested board game was not found");
            }
            

            var sessionsDB = await this._daoDbContext
                .Sessions
                .Where(a => a.BoardGameId == request!.BoardGameId && a.UserId == userId && a.IsDeleted == false)
                .ToListAsync();

            if(sessionsDB == null)
            {
                return (null, "Error: no sessions found for the requested board game");
            }
            
            return (new UsersGetSessionsResponse { Sessions = sessionsDB},"Seessions loaded successfully");
        }
        private static (bool, string) GetSessions_Validation(UsersGetSessionsRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error, request failed, request is null == {request == null}");
            }
            
            if(request.BoardGameId.HasValue == false)
            {
                return (false, $"Error, BoardGameId Id request failed, request.BoardGameId.HasValue == {request.BoardGameId.HasValue}");
            }

            if (request.BoardGameId.HasValue == true & request.BoardGameId.Value <= 0)
            {
                return (false, $"Error, BoardGameId Id request failed, request.BoardGameId.Value == {request.BoardGameId.Value}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersEditSessionResponse?, string)> EditSession(UsersEditSessionRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditSession_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var boardgameDB = await this._daoDbContext
                .BoardGames
                .FindAsync(request!.BoardGameId);
            
            if(boardgameDB == null)
            {
                return (null, "Error: board game not found");
            }

            if(boardgameDB.IsDeleted == true)
            {
                return (null, "Error: board game is deleted");
            }

            var sessionDB = await this._daoDbContext
                .Sessions               
                .FirstOrDefaultAsync(a => a.Id == request.SessionId && a.UserId == userId);
            
            if(sessionDB == null)
            {
                return (null, "Error: board game session not found");
            }

            if(sessionDB.IsDeleted == true)
            {
                return (null, "Error: this session is deleted");
            }

            if(boardgameDB.Id != request.BoardGameId)
            {
                await this._daoDbContext
                    .BoardGames
                    .Where(a => a.Id == boardgameDB.Id)
                    .ExecuteUpdateAsync(a => a.SetProperty(b => b.SessionsCount, b => b.SessionsCount - 1));

                await this._daoDbContext
                    .BoardGames
                    .Where(a => a.Id == request.BoardGameId)
                    .ExecuteUpdateAsync(a => a.SetProperty(b => b.SessionsCount, b => b.SessionsCount + 1));
            }

            sessionDB.BoardGameId = request.BoardGameId!.Value;
            sessionDB.PlayersCount = request.NewPlayersCount!.Value;
            sessionDB.Duration_minutes = request.NewDuration_minutes!.Value;

            if(request.NewDate != null)
            {
                sessionDB.Date = DateOnly.ParseExact(request.NewDate!, "yyyy-MM-dd");
            }

            await this._daoDbContext.SaveChangesAsync();

            return (null, "Session edited successfully");
        }
        private static (bool, string) EditSession_Validation(UsersEditSessionRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.SessionId.HasValue == false)
            {
                return (false, "Error: SessionId is missing");
            }

            if(request.SessionId < 1)
            {
                return (false, "Error: invalid SessionId (is less than 1)");
            }

            if(request.BoardGameId.HasValue == false)
            {
                return(false, "Error: BoardGameId is missing");
            }

            if (request.BoardGameId < 1)
            {
                return (false, "Error: invalid BoarGameId (is less than 1)");
            }

            if(string.IsNullOrWhiteSpace(request.NewDate) == false)
            {
                string datePattern = @"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$";

                if (Regex.IsMatch(request.NewDate, datePattern) == false)
                {
                    return (false, "Error: invalid date format. Expected format: yyyy-MM-dd");
                }

                // Convert string to DateOnly
                if (DateOnly.TryParseExact(request.NewDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly parsedDate) == false)
                {
                    return (false, "Error: invalid date");
                }

                var thisYear = DateTime.Now.Year;
                
                var requestedYear = parsedDate.Year;
                
                if(thisYear - requestedYear < 0)
                {
                    return (false, "Error: invalid date. Requested date is a future date");
                }
            }

            if(request.NewPlayersCount.HasValue == false)
            {
                return (false, "Error: PlayersCount is missing");
            }

            if(request.NewPlayersCount < 1)
            {
                return (false, "Error: invalid PlayersCount (is less than 1)");
            }

            if(request.NewDuration_minutes.HasValue == false)
            {
                return (false, "Error: Daration_minutes is missing");
            }

            if(request.NewDuration_minutes < 0)
            {
                return (false, "Error: invalid Duration_minutes (is negative)");
            }

            if(request.NewDuration_minutes > 1440)
            {
                return (false, "Error: invalid Duration_minutes (the maximum duration allowed is 1440 minutes)");
            }

            return (true, string.Empty);
        }
    

        public async Task<(UsersDeleteSessionResponse?, string)> DeleteSession(UsersDeleteSessionRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteSession_Validation(request);

            if(isValid == false)
            {
                return (null, message);
            }

            var sessionDB = await this._daoDbContext
                .Sessions
                .FirstOrDefaultAsync(a => a.Id == request!.SessionId && a.UserId == userId);

            if (sessionDB == null || sessionDB.UserId != userId)
            {
                return (null, "Error: session not found");
            }

            if(sessionDB.IsDeleted == true)
            {
                return (null, "Error: session was already deleted");
            }

            var boardGameId = sessionDB.BoardGameId;

            await this._daoDbContext
                .BoardGames
                .Where(a => a.Id == boardGameId)
                .ExecuteUpdateAsync(a => a.SetProperty(b => b.SessionsCount, b => b.SessionsCount - 1));

            await this._daoDbContext
                .Sessions
                .Where(a => a.Id == request!.SessionId)
                .ExecuteUpdateAsync(a => a.SetProperty(b => b.IsDeleted, true));

            return (null, "Session deleted successfully");
        }
        private static (bool, string) DeleteSession_Validation(UsersDeleteSessionRequest? request)
        {
            if(request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.SessionId.HasValue == false)
            {
                return (false, "Error: SessionId is missing");   
            }

            if(request.SessionId < 1)
            {
                return (false, "Error: invalid SessionId (is less than 1)");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersRateResponse?, string)> Rate(UsersRateRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = Rate_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }           

            var boardgameDB = await this._daoDbContext
                .BoardGames
                .FindAsync(request!.BoardGameId);

            if (boardgameDB == null)
            {
                return (null, "Error: board game not found");
            }

            if (boardgameDB.IsDeleted == true)
            {
                return (null, "Error: board game is deleted");
            }

            var rate_exists = await this._daoDbContext
                .Ratings
                .AsNoTracking()
                .AnyAsync(a => a.UserId == userId && a.BoardGameId == request.BoardGameId);

            if (rate_exists == true)
            {
                return (null, "Error: the request board game was already rated by this user");
            }

            var newRate = new Rating
            {
                Rate = request.Rate!.Value,
                UserId = userId,
                BoardGameId = request.BoardGameId!.Value
            };

            await this._daoDbContext.Ratings.AddAsync(newRate);

            var newAverageRating =
            (
                ((boardgameDB.AverageRating * boardgameDB.RatingsCount) + newRate.Rate) / (boardgameDB.RatingsCount + 1)
            );

            boardgameDB.AverageRating = newAverageRating;
            boardgameDB.RatingsCount++;



            await this._daoDbContext.SaveChangesAsync();

            return (new UsersRateResponse(), $"Board game rated successfully, its new average rating is: {newAverageRating:F1}");
        }
        private static (bool, string) Rate_Validation(UsersRateRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Rate.HasValue == false)
            {
                return (false, "Error: Rate is missing");
            }

            if (request.Rate.HasValue == true && (request.Rate < 0 || request.Rate > 5) == true)
            {
                return (false, "Error: invalid rate. It must be a value between 0 and 5");
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
     

        public async Task<(List<UsersListRatedBoardGamesResponse>?, string)> ListRatedBoardGames(UsersListRatedBoardGamesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListRatedBoardGames_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = new List<UsersListRatedBoardGamesResponse>();

            if (request!.BoardGameName! == null || string.IsNullOrWhiteSpace(request.BoardGameName) == true)
            {
                var ratedBoardgamesDB = await this._daoDbContext
                    .Ratings
                    .AsNoTracking()
                    .Where(a => a.UserId == userId)
                    .Select(a => new UsersListRatedBoardGamesResponse 
                    { 
                        BoardGameId = a.BoardGameId, 
                        BoardGameName = a.BoardGame!.Name,
                        RatingId = a.Id,
                        Rate = (int)Math.Ceiling(a.Rate)
                    })
                    .ToListAsync();

                content = ratedBoardgamesDB;
            }
            else
            {
                var ratedBoardgamesDB = await this._daoDbContext
                    .Ratings
                    .AsNoTracking()
                    .Where(a => a.UserId == userId && a!.BoardGame!.Name.ToLower().Contains(request!.BoardGameName!.ToLower()))
                    .Select(a => new UsersListRatedBoardGamesResponse
                    {
                        BoardGameId = a.BoardGameId,
                        BoardGameName = a.BoardGame!.Name,
                        RatingId = a.Id,
                        Rate = (int)Math.Ceiling(a.Rate)
                    })
                    .ToListAsync();

                content = ratedBoardgamesDB;
            }

            if (content == null)
            {
                return (null, "Error: no board games found");
            }
             
            return (content, "Board Games listed found successfully");
        }
        private static (bool, string) ListRatedBoardGames_Validation(UsersListRatedBoardGamesRequest? request)
        {
            if (request == null)
            {
                return (false, "Error request is null");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersEditRatingResponse?, string)> EditRating(UsersEditRatingRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditRating_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }            

            var ratingDB = await this._daoDbContext
                .Ratings
                .Include(a => a.BoardGame)
                .FirstOrDefaultAsync(a => a.Id == request!.RatingId && a.UserId == userId);

            if (ratingDB == null)
            {
                return (null, "Error: requested rating not found");
            }
                     
            var oldRate = ratingDB.Rate;

            ratingDB.Rate = request!.Rate!.Value;

            var boardgameDB = ratingDB.BoardGame;

            if(boardgameDB == null)
            {
                return (null, "Error: requested rated board game not found");
            }

            var newAverageRating =
            (
                (boardgameDB.AverageRating * boardgameDB.RatingsCount - oldRate + request.Rate!.Value) / (boardgameDB.RatingsCount)
            );

            boardgameDB.AverageRating = newAverageRating;

            await this._daoDbContext.SaveChangesAsync();

            return (null, $"Rating edited successfully, its new average rating is: {newAverageRating:F1}");
        }
        private static (bool, string) EditRating_Validation(UsersEditRatingRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Rate.HasValue == false)
            {
                return (false, "Error: Rate is missing");
            }

            if (request.Rate.HasValue == true && (request.Rate < 0 || request.Rate > 5) == true)
            {
                return (false, "Error: invalid rate. It must be a value between 0 and 5");
            }       

            if (request.RatingId.HasValue == false)
            {
                return (false, "Error: BoardGameId is missing");
            }

            if (request.RatingId < 1)
            {
                return (false, "Error: invalid BoardGameId (is less than 1)");
            }

            return (true, string.Empty);
        }
        

        public async Task<(UsersDeleteRatingResponse?, string)> DeleteRating(UsersDeleteRatingRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteRating_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var ratingDB = await this._daoDbContext
                .Ratings
                .Include(a => a.BoardGame)
                .FirstOrDefaultAsync(a => a.Id == request!.RateId && a.UserId == userId);

            if (ratingDB == null || ratingDB.UserId != userId)
            {
                return (null, "Error: user rating not found");
            }

            var boardgameDB = ratingDB.BoardGame;

            if (boardgameDB == null || boardgameDB.IsDeleted == true)
            {
                return (null, "Error: rated board game not found");
            }

            var ratingsCount = boardgameDB.RatingsCount;
            var oldAvgRating = boardgameDB.AverageRating;

            
            if (ratingsCount < 2)
            {
                boardgameDB.AverageRating = 0;
                
                boardgameDB.RatingsCount = 0;

                this._daoDbContext.Ratings.Remove(ratingDB);

                await this._daoDbContext.SaveChangesAsync();

                return (null, $"Board game rating deleted successfully");
            }
                 
            var newAverageRating = (oldAvgRating * ratingsCount - ratingDB.Rate) / (ratingsCount - 1); 

            boardgameDB.AverageRating = newAverageRating;
            boardgameDB.RatingsCount = ratingsCount - 1;    

            this._daoDbContext.Ratings.Remove(ratingDB);
            
            await this._daoDbContext.SaveChangesAsync();

            return (null, $"Board game rating deleted successfully, its new average rating is: {newAverageRating:F1}");
        }
        private static (bool, string) DeleteRating_Validation(UsersDeleteRatingRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.RateId.HasValue == false)
            {
                return (false, "Error: RateId is missing");
            }

            if (request.RateId < 1)
            {
                return (false, "Error: invalid RateId (is less than 1)");
            }

            return (true, string.Empty);
        }

        #endregion


        #region USER'S  LIFE COUNTERS

        #region 1º LIFE COUNTER QUICK START      
        public async Task<(UsersSyncLifeCounterDataResponse?, string)> SyncLifeCounterData(UsersSyncLifeCounterDataRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = SyncLifeCounterData_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var templates = request!.LifeCounterTemplates;

            foreach (var template in templates!)
            {
                var templateName = template.LifeCounterTemplateName!.Trim();


                var templateDB = await this._daoDbContext
                    .LifeCounterTemplates
                    .Include(a => a.LifeCounterManagers!)
                    .ThenInclude(a => a.LifeCounterPlayers)
                    .FirstOrDefaultAsync(a => a.UserId == userId &&
                                         a.LifeCounterTemplateName!.Trim() == templateName);


                var newManagers = new List<LifeCounterManager>();

                foreach (var manager in template.LifeCounterManagers!)
                {
                    var managerName = manager.LifeCounterManagerName!.Trim();

                    var managerDB = await this._daoDbContext
                    .LifeCounterManagers
                    .FirstOrDefaultAsync(a => a.UserId == userId &&
                                        a.LifeCounterManagerName!.Trim() == managerName);

                    var importedPlayers = manager.LifeCounterPlayers;

                    var newPlayers = new List<LifeCounterPlayer>();

                    foreach (var player in importedPlayers!)
                    {
                        newPlayers.Add(new LifeCounterPlayer
                        {
                            PlayerName = player.PlayerName,
                            CurrentLifePoints = player.CurrentLifePoints,
                            IsDefeated = player.IsDefeated,

                            StartingLifePoints = manager.PlayersStartingLifePoints,
                            FixedMaxLifePointsMode = manager.FixedMaxLifePointsMode!.Value,
                            MaxLifePoints = manager.PlayersMaxLifePoints,
                            AutoDefeatMode = manager.AutoDefeatMode,
                        });
                    }

                    if (managerDB == null)
                    {
                        var newManager = new LifeCounterManager
                        {
                            LifeCounterManagerName = manager.LifeCounterManagerName,
                            PlayersStartingLifePoints = manager.PlayersStartingLifePoints,
                            PlayersCount = manager.PlayersCount,
                            FirstPlayerIndex = manager.FirstPlayerIndex,
                            FixedMaxLifePointsMode = manager.FixedMaxLifePointsMode,
                            PlayersMaxLifePoints = manager.PlayersMaxLifePoints,
                            AutoDefeatMode = manager.AutoDefeatMode,
                            AutoEndMode = manager.AutoEndMode,
                            StartingTime = manager.StartingTime,
                            EndingTime = manager.EndingTime,
                            Duration_minutes = manager.Duration_minutes,
                            IsFinished = manager.IsFinished,

                            LifeCounterPlayers = newPlayers,

                            UserId = userId,
                        };

                        newManagers.Add(newManager);
                    }
                    else
                    {
                        if (managerDB.LifeCounterPlayers == null || managerDB.LifeCounterPlayers.Count < 1)
                        {
                            managerDB.LifeCounterPlayers = newPlayers;
                            managerDB.PlayersCount = newPlayers.Count;
                        }
                        else
                        {
                            foreach (var newPlayer in newPlayers)
                            {
                                var newPlayerName = newPlayer.PlayerName!.Trim();

                                bool alreadyExists = managerDB.LifeCounterPlayers
                                    .Any(p => p.PlayerName!.Trim() == newPlayerName);

                                if (alreadyExists == false)
                                {
                                    managerDB.LifeCounterPlayers.Add(newPlayer);

                                    if (managerDB.LifeCounterPlayers.Count == 6)
                                    {
                                        break;
                                    }
                                }
                            }
                            
                            managerDB.PlayersCount = managerDB.LifeCounterPlayers.Count;
                        }
                    }

                    if (templateDB == null)
                    {
                        var newTemplate = new LifeCounterTemplate
                        {
                            LifeCounterTemplateName = template.LifeCounterTemplateName,
                            PlayersStartingLifePoints = template.PlayersStartingLifePoints,
                            PlayersCount = template.PlayersCount,
                            FixedMaxLifePointsMode = template.FixedMaxLifePointsMode,
                            PlayersMaxLifePoints = template.PlayersMaxLifePoints,
                            AutoDefeatMode = template.AutoDefeatMode,
                            AutoEndMode = template.AutoEndMode,
                            LifeCounterManagersCount = template.LifeCounterManagersCount,

                            LifeCounterManagers = newManagers,

                            UserId = userId
                        };

                        this._daoDbContext.LifeCounterTemplates.Add(newTemplate);
                    }
                    else
                    {
                        if (templateDB.LifeCounterManagers == null || templateDB.LifeCounterManagers.Count < 1)
                        {
                            templateDB.LifeCounterManagers = newManagers;
                        }
                        else
                        {
                            for (int i = 0; i < newManagers.Count; i++)
                            {
                                var newMangerName = newManagers[i].LifeCounterManagerName!.Trim();

                                var managersToBeAdded = new List<LifeCounterManager>() { };

                                foreach (var existingManager in templateDB.LifeCounterManagers!)
                                {
                                    var existingManagerName = existingManager.LifeCounterManagerName!.Trim();

                                    if (newMangerName != existingManagerName)
                                    {
                                        managersToBeAdded.Add(newManagers[i]);
                                    }
                                }
                                templateDB.LifeCounterManagers.AddRange(managersToBeAdded);
                            }

                        }
                    }
                }
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersSyncLifeCounterDataResponse(), "Life Counter Data sucessfully synced");
        }
        private static (bool, string) SyncLifeCounterData_Validation(UsersSyncLifeCounterDataRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request is null.");
            }

            if(request.LifeCounterTemplates == null || request.LifeCounterTemplates.Count == 0)
            {
                return (false, $"Error: requested life counter templates are null.");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersQuickStartLifeCounterResponse?, string)> QuickStartLifeCounter(UsersQuickStartLifeCounterRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = QuickStartLifeCounter_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            //  => Search for any existing life counter MANAGER
            var doesAnyLifeCounterManagerExists = await this._daoDbContext
                .LifeCounterManagers
                .AnyAsync(a => a.UserId == userId);

            var text = "";


            int? lifeCounterTemplateId = 0;
            string? lifeCounterTemplateName = "";
            int? lifeCounterManagerId = 0;          

            if (doesAnyLifeCounterManagerExists == false)
            {
                // No life counter MANAGER was found, THEN:
                // => Search for ANY existing life counter TEMPLATE
                var doesAnyLifeCounterTemplateExists = await this._daoDbContext
                .LifeCounterTemplates
                .AnyAsync(a => a.UserId == userId);

                

                if (doesAnyLifeCounterTemplateExists == false)
                {
                    //  No life counter player exists AND no life counter TEMPLATE exists, THEN:
                    //  => Call Create LifeCounter Template and Create Life Counter Manager

                    // Creating a new default Life Counter TEMPLATE:
                    var (createTemplate_reponse_content, createTemplate_response_message) = await this.CreateLifeCounterTemplate();

                    if (createTemplate_reponse_content == null)
                    {
                        return (null, $"Error: request to create a new Default Life Counter TEMPLATE failed: {createTemplate_response_message}");
                    }

                    // Default Life Counter Template NpcId:
                    lifeCounterTemplateId = createTemplate_reponse_content.LifeCounterTemplateId;
                    lifeCounterTemplateName = createTemplate_reponse_content.LifeCounterTemplateName;

                    text = "New Default life counter TEMPLATE and new Default life counter MANAGER were created successfully";
                }
                else
                {
                    //  No life counter player exists BUT at least one life counter TEMPLATE exists, THEN:
                    //  => Call Create Life Counter Manager and create one for the newest created template

                    // Fechting the id of the most recently life counter TEMPLATE created:
                    var (getLastTemplate_reponse_content, getLastTemplateId_response_message) = await this.GetLastLifeCounterTemplate();

                    if (getLastTemplate_reponse_content == null)
                    {
                        return (null,
                            $"Error: failed to fetch the ID of the most recently created Life Counter TEMPLATE: {getLastTemplateId_response_message}");
                    }

                    // Most recently created Life Counter Template NpcId:
                    lifeCounterTemplateId = getLastTemplate_reponse_content.LastLifeCounterTemplateId;
                    lifeCounterTemplateName = getLastTemplate_reponse_content.LastLifeCounterTemplateName;

                    text = "New Default life counter MANAGER started successfully, belonging to the last created life counter Template";
                }

                // Starting a new Life Counter MANAGER
                var startLifeCounterManager_request = new UsersStartLifeCounterManagerRequest
                {
                    LifeCounterTemplateId = lifeCounterTemplateId
                };

                var (createManager_response_content, createManager_response_message) = await this.StartLifeCounterManager(startLifeCounterManager_request);

                if (createManager_response_content == null)
                {
                    return (null, $"Error: request to create a new Default Life Counter MANAGER failed: {createManager_response_message}");
                }                

                lifeCounterManagerId = createManager_response_content.LifeCounterManagerId;     
                
            }
            else
            {
                // At least one life counter MANAGER was found, THEN:               
                // => fetch the most recently started life counter MANAGER..
                var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterTemplate)
                .Include(a => a.LifeCounterPlayers)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a => a.UserId == userId);

                if (lifeCounterManagerDB == null)
                {
                    return (null, "Error: attempt to fetch the last life counter MANAGER failed, returning null");
                }

                lifeCounterTemplateId = lifeCounterManagerDB.LifeCounterTemplateId;
                lifeCounterTemplateName = lifeCounterManagerDB.LifeCounterTemplate!.LifeCounterTemplateName;

                var playersDB = lifeCounterManagerDB.LifeCounterPlayers;

                if (playersDB == null || playersDB.Count < 1)
                {
                    return (null, "Error: the fetched most recently started life counter MANAGER faled to return its players: null or less than 1 player");
                }

                if (lifeCounterManagerDB.IsFinished == false)
                {
                    // The most recently started life counter MANAGER found is NOT yet finished, THEN:
                    // => reload the life counter MANAGER found 
                    lifeCounterManagerId = lifeCounterManagerDB.Id;                

                    text = "Most recently not yet finished Life Counter Manager loaded successfully";
                }
                else
                {
                    // No life counter MANAGER that was not yet finished was found to be reloaded, THEN:
                    //
                    // 1st => start a new life counter MANAGER of the template 
                    var startNewManagerCopy_request = new UsersStartLifeCounterManagerRequest
                    {
                        LifeCounterTemplateId = lifeCounterManagerDB.LifeCounterTemplateId,
                    };

                    var (startNewManagerCopy_response_content, startNewManagerCopy_response_message) = await this.StartLifeCounterManager(startNewManagerCopy_request);

                    if (startNewManagerCopy_response_content == null)
                    {
                        return (null,
                            $"Error trying to start a life counter MANAGER relative to the most recently finished one: {startNewManagerCopy_response_message}");
                    }
                    ;

                    var newLifeCounterManagerId = startNewManagerCopy_response_content.LifeCounterManagerId;

                    // 2nd => overwrite via EDIT endpoint the data of started life counter MANAGER with the data of the most recently finished life counter MANAGER                    
                    var editManager_request = new UsersEditLifeCounterManagerRequest
                    {
                        LifeCounterManagerId = newLifeCounterManagerId,
                        NewLifeCounterManagerName = lifeCounterManagerDB.LifeCounterManagerName,
                        NewPlayersCount = lifeCounterManagerDB.PlayersCount,
                        NewPlayersStartingLifePoints = lifeCounterManagerDB.PlayersStartingLifePoints,
                        FixedMaxLifePointsMode = lifeCounterManagerDB.FixedMaxLifePointsMode,
                        NewPlayersMaxLifePoints = lifeCounterManagerDB.PlayersMaxLifePoints,
                        AutoDefeatMode = lifeCounterManagerDB.AutoDefeatMode,
                        AutoEndMode = lifeCounterManagerDB.AutoEndMode,
                    };

                    var (editManager_response_content, editManager_response_message) = await this.EditLifeCounterManager(editManager_request);

                    if (editManager_response_content == null)
                    {
                        return (null,
                            $"Error trying to edit the new a life counter MANAGER started " +
                            $"that mirrors the most recently finished one: {editManager_response_message}");
                    }
                    ;

                    lifeCounterManagerId = editManager_response_content!.LifeCounterManagerId;

                    // 3rd => fetch the players belonging to the new life counter MANAGER started in order to edit its players...
                    var newPlayers = await this._daoDbContext
                        .LifeCounterPlayers
                        .Where(a => a.LifeCounterManagerId == newLifeCounterManagerId)
                        .ToListAsync();

                    // 4th => overwite the new players data of the started life counter MANAGER with the data of the most recently finished life counter MANAGER (except for the PlayersCurrentLifePoints and isDefeated fields)...
                    for (int i = 0; i < playersDB.Count; i++)
                    {
                        newPlayers[i].PlayerName = playersDB[i].PlayerName;
                        newPlayers[i].CurrentLifePoints = lifeCounterManagerDB.PlayersStartingLifePoints;
                        newPlayers[i].AutoDefeatMode = lifeCounterManagerDB.AutoDefeatMode;
                        newPlayers[i].IsDefeated = false;
                    }

                    text = "Most recently FINISHED Life Counter Manager has been copied and started successfully";
                }
            }

            var content = new UsersQuickStartLifeCounterResponse
            {
                LifeCounterTemplateId = lifeCounterTemplateId,
                LifeCounterTemplateName = lifeCounterTemplateName,
                LifeCounterManagerId = lifeCounterManagerId,            
            };

            await this._daoDbContext.SaveChangesAsync();

            return (content, text);
        }
        private static (bool, string) QuickStartLifeCounter_Validation(UsersQuickStartLifeCounterRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is not null however it must be null");
            }

            return (true, string.Empty);
        }

        #endregion


        #region 2º LIFE COUNTER TEMPLATES
        public async Task<(UsersCreateLifeCounterTemplateResponse?, string)> CreateLifeCounterTemplate(UsersCreateLifeCounterTemplateRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var lifeCounterTemplateName = "";
            
            
            var lastTemplateId = await this._daoDbContext
            .LifeCounterTemplates        
            .OrderByDescending(a => a.Id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();
                
            if(lastTemplateId + 1 < 10)
            {
                lifeCounterTemplateName = $"Life Counter Template #0{lastTemplateId + 1}";          
            } else
            {
                lifeCounterTemplateName = $"Life Counter Template #{lastTemplateId + 1}";
            }

                var newLifeCounterTemplate = new LifeCounterTemplate
                {
                    LifeCounterTemplateName = lifeCounterTemplateName,
                    PlayersStartingLifePoints = 10,
                    PlayersCount = 1,
                    FixedMaxLifePointsMode = false,
                    PlayersMaxLifePoints = null,
                    AutoDefeatMode = false,
                    AutoEndMode = false,

                    UserId = userId,
                };

            this._daoDbContext.Add(newLifeCounterTemplate);

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersCreateLifeCounterTemplateResponse { 
                LifeCounterTemplateId = newLifeCounterTemplate.Id, 
                LifeCounterTemplateName = newLifeCounterTemplate.LifeCounterTemplateName
                }, "New LifeCounterTemplate created successfully");
        }
        public static (bool, string) CreateLifeCounterTemplate_Validation(UsersCreateLifeCounterTemplateRequest? request)
        {
            if(request != null)
            {
                return (false, "Error: request is not null however it MUST be null");
            }          

            return (true, string.Empty);
        }


        public async Task<(UsersCountLifeCounterTemplatesResponse?, string)> CountLifeCountersTemplates(UsersCountLifeCounterTemplatesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = CountLifeCounterTemplates_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifecountersCountDB = await this._daoDbContext
                .LifeCounterTemplates
                .Where(a => a.UserId == userId)
                .CountAsync();

            return (new UsersCountLifeCounterTemplatesResponse { LifeCounterTemplatesCount = lifecountersCountDB }, "User's LifeCounterTemplates counted successfully");
        }
        private static (bool, string) CountLifeCounterTemplates_Validation(UsersCountLifeCounterTemplatesRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is not null: {request}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersGetLastLifeCounterTemplateResponse?, string)> GetLastLifeCounterTemplate(UsersGetLastLifeCounterTemplateRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLastLifeCounterTemplate_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterTemplateDB = await this._daoDbContext
                .LifeCounterTemplates
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (lifeCounterTemplateDB == null)
            {
                return (null, $"Error: life counter template request failed: {lifeCounterTemplateDB}");
            }           

            return (new UsersGetLastLifeCounterTemplateResponse
            {
                LastLifeCounterTemplateId = lifeCounterTemplateDB.Id,
                LastLifeCounterTemplateName = lifeCounterTemplateDB.LifeCounterTemplateName
            }, "Last LifeCounterTemplateId fetched successfully");
        }
        private static (bool, string) GetLastLifeCounterTemplate_Validation(UsersGetLastLifeCounterTemplateRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is not null, it must be null!");
            }            

            return (true, string.Empty);
        }


        public async Task<(List<UsersListLifeCounterTemplatesResponse>?, string)> ListLifeCounterTemplates(UsersListLifeCounterTemplatesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListLifeCounterTemplates_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = await this._daoDbContext
                .LifeCounterTemplates
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.LifeCounterTemplateName)
                .Select(a => new UsersListLifeCounterTemplatesResponse
                {
                    LifeCounterTemplateId = a.Id,
                    LifeCounterTemplateName = a.LifeCounterTemplateName!
                })
                .ToListAsync();


            if (content == null || content.Count == 0)
            {
                return (null, "No life counter templates were created by this user yet");
            }

            return (content, "User's LifeCounterTemplates listed successfully");
        }
        private static (bool, string) ListLifeCounterTemplates_Validation(UsersListLifeCounterTemplatesRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is NOT null but it MUST be null!");
            }

            return (true, string.Empty);
        }

        
        public async Task<(UsersGetLifeCounterTemplateDetailsResponse?, string)> GetLifeCounterTemplateDetails(UsersGetLifeCounterTemplateDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterTemplateDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterDB = await this._daoDbContext
                .LifeCounterTemplates
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == request.LifeCounterTemplateId);

            if (lifeCounterDB == null)
            {
                return (null, $"Error: life counter template request failed: {lifeCounterDB}");
            }

            var response = new UsersGetLifeCounterTemplateDetailsResponse
            {
                LifeCounterTemplateName = lifeCounterDB.LifeCounterTemplateName,
                PlayersStartingLifePoints = lifeCounterDB.PlayersStartingLifePoints,
                PlayersCount = lifeCounterDB.PlayersCount,
                FixedMaxLifePointsMode = lifeCounterDB.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = lifeCounterDB.PlayersMaxLifePoints,
                AutoDefeatMode = lifeCounterDB.AutoDefeatMode,
                AutoEndMode = lifeCounterDB.AutoEndMode,
                LifeCounterManagersCount = lifeCounterDB.LifeCounterManagersCount
            };

            return (response, "LifeCounterTemplate details fetched successfully");
        }
        private static (bool, string) GetLifeCounterTemplateDetails_Validation(UsersGetLifeCounterTemplateDetailsRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterTemplateId == null || request.LifeCounterTemplateId.HasValue == false)
            {
                return (false, $"Error: requested LifeCounterTemplateId failed: {request.LifeCounterTemplateId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersEditLifeCounterTemplateResponse?, string)> EditLifeCounterTemplate(UsersEditLifeCounterTemplateRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditLifeCounterTemplate_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterTemplateDB = await this._daoDbContext
                .LifeCounterTemplates
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == request!.LifeCounterTemplateId);

            if (lifeCounterTemplateDB == null)
            {
                return (null, $"Error: life counter template request failed: {lifeCounterTemplateDB}");
            }

            var nameAlreadyInUse = await this._daoDbContext
                .LifeCounterTemplates
                .AnyAsync(a => a.UserId == userId && 
                               a.LifeCounterTemplateName!.ToLower().Trim() == request!.NewLifeCounterTemplateName!.ToLower().Trim() && 
                               a.Id != request.LifeCounterTemplateId);

            if (nameAlreadyInUse == true)
            {
                return (null, $"Error: requested Life Counter Template NAME is already in use, please choose a different name: {request!.NewLifeCounterTemplateName}");
            }

            lifeCounterTemplateDB.LifeCounterTemplateName = request!.NewLifeCounterTemplateName;
            lifeCounterTemplateDB.PlayersStartingLifePoints = request.NewPlayersStartingLifePoints;
            lifeCounterTemplateDB.PlayersCount = request.NewPlayersCount;
            lifeCounterTemplateDB.FixedMaxLifePointsMode = request.FixedMaxLifePointsMode;
            lifeCounterTemplateDB.PlayersMaxLifePoints = request.NewPlayersMaxLifePoints;
            lifeCounterTemplateDB.AutoDefeatMode = request.AutoDefeatMode;
            lifeCounterTemplateDB.AutoEndMode = request.AutoEndMode;

            await this._daoDbContext.SaveChangesAsync();    

            return (new UsersEditLifeCounterTemplateResponse
            {
                LifeCounterTemplateId = lifeCounterTemplateDB.Id,
            }, "LifeCounterTemplate edited successfully");
        }
        private static (bool, string) EditLifeCounterTemplate_Validation(UsersEditLifeCounterTemplateRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterTemplateId == null || request.LifeCounterTemplateId.HasValue == false || request.LifeCounterTemplateId < 1)
            {
                return (false, $"Error: requested LifeCounterTemplateId failed: {request.LifeCounterTemplateId}");
            }

            if (string.IsNullOrWhiteSpace(request!.NewLifeCounterTemplateName) == true)
            {
                return (false,
                   "Error: LifeCounterTemplateName is null or empty! " +
                   $"request.LifeCounterTemplateName: {request.NewLifeCounterTemplateName}");
            }

            if (request.NewPlayersStartingLifePoints.HasValue == false || request.NewPlayersStartingLifePoints == null)
            {
                return (false,
                   "Error: StartingLifePoints is null! " +
                   $"request.PlayersStartingLifePoints: {request.NewPlayersStartingLifePoints}");
            }

            if (request.NewPlayersStartingLifePoints < 0)
            {
                return (false,
                    "Error: StartingLifePoints cannot be less than 0! " +
                    $"request.PlayersStartingLifePoints: {request.NewPlayersStartingLifePoints}");
            }

            if (request.NewPlayersCount.HasValue == false || request.NewPlayersCount == null)
            {
                return (false,
                   "Error: PlayersCount is null! " +
                   $"request.PlayersCount: {request.NewPlayersCount}");
            }

            if (request.NewPlayersCount < 1 || request.NewPlayersCount > 6)
            {
                return (false,
                    "Error: PlayersCount cannot be less than 1 or more than 6! " +
                    $"request.PlayersCount: {request.NewPlayersCount}");
            }

            if (request.FixedMaxLifePointsMode.HasValue == false)
            {
                return (false,
                   "Error: FixedMaxLifePointsMode is null! " +
                   $"request.FixedMaxLifePointsMode.HasValue == false: {request.FixedMaxLifePointsMode.HasValue == false}");
            }

            if (request.FixedMaxLifePointsMode == true && (request.NewPlayersStartingLifePoints.HasValue == false || request.NewPlayersStartingLifePoints == null))
            {
                return (false,
                   "Error: PlayersMaxLifePoints is null! " +
                   $"request.PlayersMaxLifePoints: {request.NewPlayersMaxLifePoints}");
            }

            if (request.NewPlayersMaxLifePoints < 1 || request.NewPlayersMaxLifePoints > 999)
            {
                return (false,
                    "Error: PlayersMaxLifePoints cannot be less than 1 or more than 999! " +
                    $"request.PlayersMaxLifePoints: {request.NewPlayersMaxLifePoints}");
            }

            if (request.AutoDefeatMode.HasValue == false || request.AutoDefeatMode == null)
            {
                return (false,
                   "Error: AutoDefeatMode is null! " +
                   $"request.AutoDefeatMode: {request.AutoDefeatMode}");
            }

            if (request.AutoEndMode.HasValue == false || request.AutoEndMode == null)
            {
                return (false,
                   "Error: AutoEndMatch is null! " +
                   $"request.AutoEndMatch: {request.AutoEndMode}");
            }


            return (true, string.Empty);
        }


        public async Task<(UsersDeleteLifeCounterTemplateResponse?, string)> DeleteLifeCounterTemplate(UsersDeleteLifeCounterTemplateRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteLifeCounterTemplate_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var attemptToDeleteLifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Where(a =>
                    a.UserId == userId &
                    a.LifeCounterTemplateId == request!.LifeCounterTemplateId)
                 .ExecuteDeleteAsync();

            if (attemptToDeleteLifeCounterManagerDB <= 0)
            {
                return (null, "Error: request to delete LIFE COUNTER MANAGER failed");
            }

            var attemptToDeleteLifeCounterTemplateDB = await this._daoDbContext
                .LifeCounterTemplates
                .Where(a =>
                    a.UserId == userId &
                    a.Id == request!.LifeCounterTemplateId)
                 .ExecuteDeleteAsync();

            if (attemptToDeleteLifeCounterTemplateDB <= 0)
            {
                return (null, "Error: request to delete LIFE COUNTER MANAGER failed");
            }


            return (new UsersDeleteLifeCounterTemplateResponse
            {

            }, $"Life Counter Template deleted successfully,");

        }
        private static (bool, string) DeleteLifeCounterTemplate_Validation(UsersDeleteLifeCounterTemplateRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.LifeCounterTemplateId == null || request.LifeCounterTemplateId < 1)
            {
                return (false, $"Error: LifeCounterManagerId request failed: {request.LifeCounterTemplateId}");
            }

            return (true, string.Empty);
        }

        #endregion

        
        #region 3º LIFE COUNTER MANAGERS
        public async Task<(UsersStartLifeCounterManagerResponse?, string)> StartLifeCounterManager(UsersStartLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = StartLifeCounterManager_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterTemplateDB = await this._daoDbContext
                .LifeCounterTemplates
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == request!.LifeCounterTemplateId);

            if (lifeCounterTemplateDB == null)
            {
                return (null, $"Error, invalid requested Life Counter, returning: {lifeCounterTemplateDB}");
            }          

            var newPlayers = new List<LifeCounterPlayer>();

            for (int playersCount = 1; playersCount <= lifeCounterTemplateDB.PlayersCount; playersCount++)
            {
                var name = $"Player #{playersCount}";

                newPlayers.Add
                (
                    new LifeCounterPlayer
                    {
                        PlayerName = name,
                        StartingLifePoints = lifeCounterTemplateDB.PlayersStartingLifePoints,
                        CurrentLifePoints = lifeCounterTemplateDB.PlayersStartingLifePoints,
                        FixedMaxLifePointsMode = lifeCounterTemplateDB.FixedMaxLifePointsMode!.Value,
                        MaxLifePoints = lifeCounterTemplateDB.PlayersMaxLifePoints,
                        AutoDefeatMode = lifeCounterTemplateDB.AutoDefeatMode!.Value,
                    }
                );
            }

            var startMark = DateTime.UtcNow.ToLocalTime().Ticks;
            var random = new Random();
            int firstPlayerIndex = random.Next(0, lifeCounterTemplateDB.PlayersCount!.Value);

            var lifeCounterManagerName = $"{lifeCounterTemplateDB.LifeCounterTemplateName}-M{lifeCounterTemplateDB.LifeCounterManagersCount + 1}";

            var newLifeCounterManager = new LifeCounterManager
            {
                LifeCounterTemplateId = request!.LifeCounterTemplateId,
                
                LifeCounterManagerName = lifeCounterManagerName,
                LifeCounterPlayers = newPlayers,
                PlayersCount = lifeCounterTemplateDB.PlayersCount,
                FirstPlayerIndex = firstPlayerIndex,
                PlayersStartingLifePoints = lifeCounterTemplateDB.PlayersStartingLifePoints,
                FixedMaxLifePointsMode = lifeCounterTemplateDB.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = lifeCounterTemplateDB.PlayersMaxLifePoints,
                AutoDefeatMode = lifeCounterTemplateDB.AutoDefeatMode!.Value,
                AutoEndMode = lifeCounterTemplateDB.AutoEndMode!.Value,
                
                StartingTime = startMark,
                
                UserId = userId,
            };

            lifeCounterTemplateDB.LifeCounterManagers ??= [];

            lifeCounterTemplateDB.LifeCounterManagers.Add(newLifeCounterManager);

            lifeCounterTemplateDB.LifeCounterManagersCount++;

            await this._daoDbContext.SaveChangesAsync();

            var content = new UsersStartLifeCounterManagerResponse
            {
                LifeCounterTemplateId = lifeCounterTemplateDB.Id,
                LifeCounterTemplateName = lifeCounterTemplateDB.LifeCounterTemplateName,

                LifeCounterManagerId = newLifeCounterManager.Id,
                LifeCounterManagerName = newLifeCounterManager.LifeCounterManagerName,

                PlayersStartingLifePoints = newLifeCounterManager.PlayersStartingLifePoints,
                PlayersCount = newLifeCounterManager.PlayersCount,
                FirstPlayerIndex = newLifeCounterManager.FirstPlayerIndex,
                LifeCounterPlayers = [],

                FixedMaxLifePointsMode = newLifeCounterManager.FixedMaxLifePointsMode,
                PlayersMaxLifePoints = newLifeCounterManager.PlayersMaxLifePoints,

                AutoDefeatMode = newLifeCounterManager.AutoDefeatMode,
                AutoEndMode = newLifeCounterManager.AutoEndMode,
                StartingTime = startMark

            };

            foreach (var newPlayer in newPlayers)
            {
                content.LifeCounterPlayers.Add(new UsersStartLifeCounterManagerResponse_players
                {
                    LifeCounterPlayerId = newPlayer.Id,
                    PlayerName = newPlayer.PlayerName,
                    PlayerStartingLifePoints = newPlayer.CurrentLifePoints,
                });
            }          

            return (content, $"New {lifeCounterTemplateDB.LifeCounterTemplateName} instance started with {newPlayers.Count} players");
        }
        private static (bool, string) StartLifeCounterManager_Validation(UsersStartLifeCounterManagerRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterTemplateId == null || request.LifeCounterTemplateId.HasValue == false)
            {
                return (false, $"Error: requested LifeCounterManagerId failed:" +
                    $" request.LifeCounterTemplateId == null -> {request.LifeCounterTemplateId == null} or  " +
                    $"request.LifeCounterTemplateId.HasValue == false -> {request.LifeCounterTemplateId.HasValue == false}");
            }          

            return (true, string.Empty);
        }


        public async Task<(UsersEditLifeCounterManagerResponse?, string)> EditLifeCounterManager(UsersEditLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditLifeCounterManager_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &
                    a.Id == request!.LifeCounterManagerId);

            if (lifeCounterManagerDB == null)
            {
                return (null, "Error: LifeCounterManager request failed");
            }           

            lifeCounterManagerDB.LifeCounterManagerName = request!.NewLifeCounterManagerName;
            lifeCounterManagerDB.PlayersStartingLifePoints = request!.NewPlayersStartingLifePoints;

            var playersDB = lifeCounterManagerDB.LifeCounterPlayers;

            if (playersDB == null)
            {
                return (null, "Error: Life counter manager players request failed");
            }

            foreach (var player in playersDB)
            {
                player.StartingLifePoints = request.NewPlayersStartingLifePoints;
                player.FixedMaxLifePointsMode = request.FixedMaxLifePointsMode!.Value;
                player.MaxLifePoints = request.NewPlayersMaxLifePoints;
                player.AutoDefeatMode = request.AutoDefeatMode!.Value;
            }

            if (request.NewPlayersCount > lifeCounterManagerDB.PlayersCount)
            {
                for (var i = lifeCounterManagerDB.PlayersCount; i < request.NewPlayersCount; i++)
                {
                    var newPlayerName = $"Player #{i+1}";
                    playersDB.Add(new LifeCounterPlayer
                    {
                        PlayerName = newPlayerName,
                        StartingLifePoints = request.NewPlayersStartingLifePoints,
                        CurrentLifePoints = request.NewPlayersStartingLifePoints,
                        FixedMaxLifePointsMode = request.FixedMaxLifePointsMode!.Value,
                        MaxLifePoints = request.NewPlayersMaxLifePoints,
                        AutoDefeatMode = request.AutoDefeatMode!.Value, 
                    });
                }
            } else if(request.NewPlayersCount < lifeCounterManagerDB.PlayersCount)
            {
                for (var i = lifeCounterManagerDB.PlayersCount.Value - 1; i >= request.NewPlayersCount; i--)
                {
                    playersDB.RemoveAt(i);
                }

            }

            lifeCounterManagerDB.PlayersCount = request.NewPlayersCount;

            lifeCounterManagerDB.FirstPlayerIndex = request.FirstPlayerIndex;

            lifeCounterManagerDB.FixedMaxLifePointsMode = request.FixedMaxLifePointsMode;
            
            if(request.FixedMaxLifePointsMode == true)
            {
                lifeCounterManagerDB.PlayersMaxLifePoints = request.NewPlayersMaxLifePoints;
            }

            lifeCounterManagerDB.AutoDefeatMode = request.AutoDefeatMode;

            foreach (var player in playersDB)
            {
                if(request.AutoDefeatMode == false) player.IsDefeated = false;
                if(request.AutoDefeatMode == true && player.CurrentLifePoints == 0) player.IsDefeated = true;
            }         

            lifeCounterManagerDB.AutoEndMode = request.AutoEndMode!.Value;

            if(lifeCounterManagerDB.AutoDefeatMode == false || lifeCounterManagerDB.AutoEndMode == false)
            {
                lifeCounterManagerDB.Duration_minutes = 0;
                lifeCounterManagerDB.EndingTime = null;
                lifeCounterManagerDB.IsFinished = false;
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersEditLifeCounterManagerResponse
            {
                LifeCounterManagerId = lifeCounterManagerDB.Id,
            }, $"Life Counter Manager {lifeCounterManagerDB.LifeCounterManagerName} edited successfully");
        }
        private static (bool, string) EditLifeCounterManager_Validation(UsersEditLifeCounterManagerRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.LifeCounterManagerId == null || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: LifeCounterManagerId request failed: {request.LifeCounterManagerId}");
            }

            if (string.IsNullOrWhiteSpace(request.NewLifeCounterManagerName) == true)
            {
                return (false, $"Error: NewLifeCounterName request failed: {request.NewLifeCounterManagerName}");
            }

            if (request.NewPlayersCount == null || request.NewPlayersCount < 1)
            {
                return (false, $"Error: NewPlayersCount request failed: {request.NewPlayersCount}");
            }

            if (request.NewPlayersStartingLifePoints == null || request.NewPlayersStartingLifePoints < 1)
            {
                return (false, $"Error: NewPlayersStartingLifePoints request failed: {request.NewPlayersStartingLifePoints}");
            }

            if (request.FixedMaxLifePointsMode == null)
            {
                return (false, $"Error: FixedMaxLifePointsMode request failed: {request.FixedMaxLifePointsMode}");
            }

            if (request.FixedMaxLifePointsMode == true && 
                (request.NewPlayersMaxLifePoints == null || request.NewPlayersMaxLifePoints < 1))
            {
                return (false, $"Error: NewPlayersMaxLifePoints request failed: {request.NewPlayersMaxLifePoints}");
            }

            if (request.AutoDefeatMode == null)
            {
                return (false, $"Error: AutoDefeatMode request failed: {request.AutoDefeatMode}");
            }

            if (request.AutoEndMode == null)
            {
                return (false, $"Error: AutoEndMode request failed: {request.AutoEndMode}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersGetLifeCounterManagerDetailsResponse?, string)> GetLifeCounterManagerDetails(UsersGetLifeCounterManagerDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterManagerDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var response = await this._daoDbContext
                .LifeCounterManagers
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.Id == request!.LifeCounterManagerId)
                .Select(a => new UsersGetLifeCounterManagerDetailsResponse
                {
                    LifeCounterTemplateId = a.LifeCounterTemplate!.Id,
                    LifeCounterTemplateName = a.LifeCounterTemplate!.LifeCounterTemplateName,

                    LifeCounterManagerName = a.LifeCounterManagerName,
                    PlayersCount = a.PlayersCount,
                    FirstPlayerIndex = a.FirstPlayerIndex,
                    LifeCounterPlayers = a.LifeCounterPlayers.Select(b => new UsersGetLifeCounterManagerDetailsResponse_players
                    {
                        PlayerId = b.Id,
                        PlayerName = b.PlayerName,
                        CurrentLifePoints = b.CurrentLifePoints,
                        IsDefeated = b.IsDefeated,
                    }).ToList(),
                    PlayersStartingLifePoints = a.PlayersStartingLifePoints,
                    FixedMaxLifePointsMode = a.FixedMaxLifePointsMode,
                    PlayersMaxLifePoints = a.PlayersMaxLifePoints,
                    AutoDefeatMode = a.AutoDefeatMode,
                    AutoEndMode = a.AutoEndMode,
                    StartingTime = a.StartingTime,
                    EndingTime = a.EndingTime,
                    Duration_minutes = a.Duration_minutes,
                    IsFinished = a.IsFinished,
                })
                .FirstOrDefaultAsync();

            if (response == null)
            {
                return (null, "Error: requested LifeCounterManagerDB failed");
            }     

            if (response.LifeCounterTemplateId == null || string.IsNullOrWhiteSpace(response.LifeCounterTemplateName) == true)
            {
                return (null, "Error: requested LifeCounterTemplateDB failed");
            }         
      

            if (response.LifeCounterPlayers == null || response.LifeCounterPlayers.Count < 1)
            {
                return (null, "Error: requested LifeCounterPlayersDB failed");
            }

            return (response, "Life counter details fetched successfully");
        }
        private static (bool, string) GetLifeCounterManagerDetails_Validation(UsersGetLifeCounterManagerDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId == null || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: requested LifeCounterManagerId failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersGetLastLifeCounterManagerResponse?, string)> GetLastLifeCounterManager(UsersGetLastLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLastLifeCounterManager_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers              
                .Include(a => a.LifeCounterPlayers)             
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.LifeCounterTemplateId == request!.LifeCounterTemplateId && a.IsFinished == false);

            var response = new UsersGetLastLifeCounterManagerResponse();

         

            if (lifeCounterManagerDB == null)
            {
                var startLifeCounterManager_request = new UsersStartLifeCounterManagerRequest
                {
                    LifeCounterTemplateId = request!.LifeCounterTemplateId
                };

                var (createManager_response_content, createManager_response_message) = await this.StartLifeCounterManager(startLifeCounterManager_request);

                if (createManager_response_content == null)
                {
                    return (null, $"Error: request to create a new Default Life Counter MANAGER failed: {createManager_response_message}");
                }
          
                response.LifeCounterManagerId = createManager_response_content.LifeCounterManagerId;
                response.LifeCounterManagerName = createManager_response_content.LifeCounterManagerName;
                response.PlayersCount = createManager_response_content.PlayersCount;
                response.FirstPlayerIndex = createManager_response_content.FirstPlayerIndex;
                
                if(createManager_response_content.LifeCounterPlayers == null || createManager_response_content.LifeCounterPlayers.Count == 0)
                {
                    return (null, "Error, failed to fetch life counter PLAYERS while creating new life counter manager");
                }

                response.LifeCounterPlayers = [];

                foreach (var player in createManager_response_content.LifeCounterPlayers)
                {
                    response.LifeCounterPlayers.Add(new UsersGetLastLifeCounterManagerResponse_players
                    {
                        LifeCounterPlayerId = player.LifeCounterPlayerId,
                        LifeCounterPlayerName = player.PlayerName,
                        CurrentLifePoints = player.PlayerStartingLifePoints,
                        IsDefeated = false
                    });
                }

                response.PlayersStartingLifePoints = createManager_response_content.PlayersStartingLifePoints;
                response.AutoDefeatMode = createManager_response_content.AutoDefeatMode;
                response.AutoEndMode = createManager_response_content.AutoEndMode;
                response.StartingTime = createManager_response_content.StartingTime;

                return (response, "New life counter manager started successfully for the seleced life counter template.");
            }
      
            response.LifeCounterManagerId = lifeCounterManagerDB.Id;
            response.LifeCounterManagerName = lifeCounterManagerDB.LifeCounterManagerName;
            response.PlayersCount = lifeCounterManagerDB.PlayersCount;
            response.FirstPlayerIndex = lifeCounterManagerDB.FirstPlayerIndex;

            if (lifeCounterManagerDB.LifeCounterPlayers == null || lifeCounterManagerDB.LifeCounterPlayers.Count == 0)
            {
                return (null, "Error, failed to fetch life counter PLAYERS of the unfinished life counter manager");
                }

            response.LifeCounterPlayers = [];

            foreach (var player in lifeCounterManagerDB.LifeCounterPlayers)
            {
                response.LifeCounterPlayers.Add(new UsersGetLastLifeCounterManagerResponse_players
                {
                    LifeCounterPlayerId = player.Id,
                    LifeCounterPlayerName = player.PlayerName,
                    CurrentLifePoints = player.CurrentLifePoints,
                    IsDefeated = false
                });
            }

            response.PlayersStartingLifePoints = lifeCounterManagerDB.PlayersStartingLifePoints;
            response.AutoDefeatMode = lifeCounterManagerDB.AutoDefeatMode;
            response.AutoEndMode = lifeCounterManagerDB.AutoEndMode;
            response.StartingTime = lifeCounterManagerDB.StartingTime;

            return (response, "Unfinished life counter manager reloaded successfully for the seleced life counter template.");
        }
        private static (bool, string) GetLastLifeCounterManager_Validation(UsersGetLastLifeCounterManagerRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterTemplateId.HasValue == false || request.LifeCounterTemplateId == null || request.LifeCounterTemplateId < 1)
            {
                return (false, $"Error: requested LifeCounterManagerId failed: {request.LifeCounterTemplateId}");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersListUnfinishedLifeCounterManagersResponse>?, string)> ListUnfinishedLifeCounterManagers(UsersListUnfinishedLifeCounterManagersRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListUnfinishedLifeCounterManagers_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var unfinishedLifeCounterManagersDB = await this._daoDbContext
                .LifeCounterManagers
                .Where(a => a.UserId == userId && a.LifeCounterTemplateId == request.LifeCounterTemplateId && a.IsFinished == false)
                .OrderByDescending(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.LifeCounterManagerName,
                    a.StartingTime 
                })
                .ToListAsync();

            if (unfinishedLifeCounterManagersDB == null || unfinishedLifeCounterManagersDB.Count < 1)
            {
                return (null, "Error: requested list of unfinished Life Counter Managers returned null or with 0 elements");
            }

            var content = unfinishedLifeCounterManagersDB
                .Select(a => new UsersListUnfinishedLifeCounterManagersResponse
                {
                    LifeCounterManagerId = a.Id,
                    LifeCounterManagerName = a.LifeCounterManagerName,
                    StartingDate = DateOnly
                        .FromDateTime(new DateTime(a.StartingTime!.Value))
                        .ToString("dd/MM/yyyy")
                })
                .ToList();         

            return (content, "Unfinished Life Counter Managers listed successfully");
        }
        private static (bool, string) ListUnfinishedLifeCounterManagers_Validation(UsersListUnfinishedLifeCounterManagersRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request is null!");
            }

            if(request.LifeCounterTemplateId == null || request.LifeCounterTemplateId < 0)
            {
                return (false, "Requested LifeCounterTemplateId failed");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersRefreshLifeCounterManagerResponse?, string)> RefreshLifeCounterManager(UsersRefreshLifeCounterManagerRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = RefreshLifeCounterManager_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .Where(a => a.Id == request.LifeCounterManagerId &&
                    a.UserId == userId)
                .FirstOrDefaultAsync();

            if (lifeCounterManagerDB == null)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterManagerDB}");
            }

            

            lifeCounterManagerDB.StartingTime = DateTime.UtcNow.ToLocalTime().Ticks;

            lifeCounterManagerDB.Duration_minutes = 0;

            lifeCounterManagerDB.EndingTime = null;

            lifeCounterManagerDB.IsFinished = false;

            var lifeCounterPlayers = lifeCounterManagerDB.LifeCounterPlayers;

            if (lifeCounterPlayers == null || lifeCounterPlayers.Count <= 0)
            {
                return (null, $"Error: life counter players request failed: {lifeCounterPlayers}");
            }

            var random = new Random();
            lifeCounterManagerDB.FirstPlayerIndex = random.Next(0, lifeCounterPlayers.Count);

            foreach (var player in lifeCounterPlayers)
            {
                player.CurrentLifePoints = player.StartingLifePoints;
                player.IsDefeated = false;
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersRefreshLifeCounterManagerResponse
            {
                IsLifeCounterAlreadyFinished = false,
            }, "Life Counter Manager refreshed successfully!");
        }
        private static (bool, string) RefreshLifeCounterManager_Validation(UsersRefreshLifeCounterManagerRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersCheckForLifeCounterManagerEndResponse?, string)> CheckForLifeCounterManagerEnd(UsersCheckForLifeCounterManagerEndRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = CheckForLifeCounterManagerEnd_Validation(request);

            if (!requestIsValid)
            {
                return (null, message);
            }

            var lifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Include(a => a.LifeCounterPlayers)
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.Id == request!.LifeCounterManagerId &&
                    a.AutoEndMode == true);

            if (lifeCounterManagerDB == null)
            {
                return (null, "Error: LifeCounterManager request failed");
            }

            if (!lifeCounterManagerDB.StartingTime.HasValue)
            {
                return (null, "Error: StartingTime is not set");
            }

            long rawDurationTicks;
            double roundedDuration;

            if (lifeCounterManagerDB.IsFinished == true)
            {
                rawDurationTicks = lifeCounterManagerDB.EndingTime!.Value - lifeCounterManagerDB.StartingTime.Value;
                roundedDuration = Math.Round((double)(rawDurationTicks) / 600_000_000, 2);

                return (new UsersCheckForLifeCounterManagerEndResponse
                {
                    IsFinished = true,
                    Duration_minutes = roundedDuration,
                }, "This life counter manager is finished");
            }

            var lifeCounterPlayersDB = lifeCounterManagerDB.LifeCounterPlayers;

            if (lifeCounterPlayersDB == null || lifeCounterPlayersDB.Count <= 0)
            {
                return (null, "Error: lifeCounterPlayersDB request failed");
            }

            if (!lifeCounterManagerDB.PlayersCount.HasValue)
            {
                return (null, "Error: PlayersCount is not set");
            }

            var playersCount = lifeCounterManagerDB.PlayersCount.Value;
            var defeatedPlayers = lifeCounterPlayersDB.Count(a => a.IsDefeated == true);

            var isLifeCounterManagerEnded = (playersCount - defeatedPlayers) <= 1;

            if (!isLifeCounterManagerEnded)
            {
                return (new UsersCheckForLifeCounterManagerEndResponse(), "This life counter manager is NOT finished");
            }

            var currentTimeMark = DateTime.UtcNow.ToLocalTime().Ticks;

            lifeCounterManagerDB.IsFinished = true;
            lifeCounterManagerDB.EndingTime = currentTimeMark;

            rawDurationTicks = currentTimeMark - lifeCounterManagerDB.StartingTime.Value;
            roundedDuration = Math.Round((double)rawDurationTicks / 600_000_000, 2);
            lifeCounterManagerDB.Duration_minutes = roundedDuration;

            foreach (var player in lifeCounterPlayersDB)
            {
                if (player.CurrentLifePoints <= 0)
                {
                    player.IsDefeated = true;
                }
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersCheckForLifeCounterManagerEndResponse
            {
                IsFinished = true,
                Duration_minutes = roundedDuration,
            }, "This life counter manager is finished");
        }
        private static (bool, string) CheckForLifeCounterManagerEnd_Validation(UsersCheckForLifeCounterManagerEndRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersDeleteLifeCounterManagerResponse?, string)> DeleteLifeCounterManager(UsersDeleteLifeCounterManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteLifeCounterManager_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var attemptToDeleteLifeCounterManagerDB = await this._daoDbContext
                .LifeCounterManagers
                .Where(a =>
                    a.UserId == userId &
                    a.Id == request!.LifeCounterManagerId)
                 .ExecuteDeleteAsync();

            if(attemptToDeleteLifeCounterManagerDB <= 0)
            {
                return (null, "Error: request to delete LIFE COUNTER MANAGER failed");
            }         

            return (new UsersDeleteLifeCounterManagerResponse
            {

            }, $"Life Counter Template deleted successfully,");
                
        }
        private static (bool, string) DeleteLifeCounterManager_Validation(UsersDeleteLifeCounterManagerRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.LifeCounterManagerId == null || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: LifeCounterManagerId request failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }

        #endregion


        #region 4º LIFE COUNTER PLAYERS    
        public async Task<(UsersGetLifeCounterPlayersDetailsResponse?, string)> GetLifeCounterPlayersDetails(UsersGetLifeCounterPlayersDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterPlayersDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayersDB = await this._daoDbContext
                .LifeCounterPlayers
                .Where(a => a.LifeCounterManagerId == request!.LifeCounterManagerId && 
                            a.LifeCounterManager!.UserId == userId && 
                            a.LifeCounterManager.IsFinished == false)
                .Select(a => new 
                { 
                    a.Id, 
                    a.PlayerName,
                    a.CurrentLifePoints,
                    a.MaxLifePoints,
                    a.FixedMaxLifePointsMode,
                    a.IsDefeated
                })
                .OrderBy(a => a.Id)
                .ToListAsync();

            if (lifeCounterPlayersDB == null)
            {
                return (null, $"Error: life counter manager request failed: {lifeCounterPlayersDB}");
            }

            var players = new List<UsersGetLifeCounterPlayersDetailsResponse_players>();
           
            foreach (var player in lifeCounterPlayersDB) {
                players.Add(new UsersGetLifeCounterPlayersDetailsResponse_players
                {
                    PlayerId = player.Id,
                    PlayerName = player.PlayerName,
                    PlayerMaxLifePoints = player.MaxLifePoints,
                    PlayerCurrentLifePoints = player.CurrentLifePoints,
                    IsMaxLifePointsFixed = player.FixedMaxLifePointsMode,
                    IsDefeated = player.IsDefeated
                });
             }

            return (new UsersGetLifeCounterPlayersDetailsResponse
            {
                LifeCounterPlayers = players
            }, "Life counter players details loaded successfully");
        }
        private static (bool, string) GetLifeCounterPlayersDetails_Validation(UsersGetLifeCounterPlayersDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request is not null: {request}");
            }

            if(request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: LifeCounterManagerId request failed: {request.LifeCounterManagerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersGetLifeCounterPlayerDetailsResponse?, string)> GetLifeCounterPlayerDetails(UsersGetLifeCounterPlayerDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterPlayerDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .FirstOrDefaultAsync(a => a.Id == request!.LifeCounterPlayerId &&
                            a.LifeCounterManager!.UserId == userId);              

            if (lifeCounterPlayerDB == null)
            {
                return (null, $"Error: lifeCounterPlayerDB request failed, lifeCounterPlayerDB == null: {lifeCounterPlayerDB}");
            }

 
            return (new UsersGetLifeCounterPlayerDetailsResponse
            {
                LifeCounterManagerId = lifeCounterPlayerDB.LifeCounterManagerId,
                LifeCounterPlayerName = lifeCounterPlayerDB.PlayerName,
                PlayerStartingLifePoints = lifeCounterPlayerDB.StartingLifePoints,
                PlayerCurrentLifePoints = lifeCounterPlayerDB.CurrentLifePoints,
                FixedMaxLifePointsMode = lifeCounterPlayerDB.FixedMaxLifePointsMode,
                PlayerMaxLifePoints = lifeCounterPlayerDB.MaxLifePoints,
                AutoDefeatMode = lifeCounterPlayerDB.AutoDefeatMode,
            }, "Life counter players details loaded successfully");
        }
        private static (bool, string) GetLifeCounterPlayerDetails_Validation(UsersGetLifeCounterPlayerDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request is not null: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false || request.LifeCounterPlayerId < 1)
            {
                return (false, $"Error: LifeCounterPlayerId request failed: {request.LifeCounterPlayerId}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersIncreaseLifePointsResponse?, string)> IncreaseLifePoints(UsersIncreaseLifePointsRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = IncreaseLifePoints_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .Include(a => a.LifeCounterManager)
                .Where(a => a.Id == request.LifeCounterPlayerId && 
                    a.LifeCounterManager!.UserId == userId)                   
                .FirstOrDefaultAsync();

            if (lifeCounterPlayerDB == null)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterPlayerDB}");
            }

            if (lifeCounterPlayerDB.LifeCounterManager!.IsFinished == true)
            {
                return (null, "Error: this life counter manager was already finished");
            }

            if (lifeCounterPlayerDB.IsDefeated == true) 
            {
                return (null, $"Error: {lifeCounterPlayerDB.PlayerName} has been already defeated");
            }
            
            int? updatedLifePoints = 0;
            var text = "";
          
            if (lifeCounterPlayerDB.FixedMaxLifePointsMode == true &&
                (lifeCounterPlayerDB.CurrentLifePoints == lifeCounterPlayerDB.MaxLifePoints ||
                lifeCounterPlayerDB.CurrentLifePoints + request.LifePointsToIncrease > lifeCounterPlayerDB.MaxLifePoints))
            {
                updatedLifePoints = lifeCounterPlayerDB.MaxLifePoints;

                text = $"{lifeCounterPlayerDB.PlayerName} life points updated successfully. Max life points reached: {updatedLifePoints}";
            }
            else
            {
                updatedLifePoints = lifeCounterPlayerDB.CurrentLifePoints + request.LifePointsToIncrease;
                text = $"{lifeCounterPlayerDB.PlayerName} life points updated successfully. Current life points: {updatedLifePoints}";
            }

            lifeCounterPlayerDB.CurrentLifePoints = updatedLifePoints;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersIncreaseLifePointsResponse { UpdatedLifePoints = updatedLifePoints }, text);
        }
        private static (bool, string) IncreaseLifePoints_Validation(UsersIncreaseLifePointsRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }
            
            if(request.LifeCounterPlayerId.HasValue == false || request.LifeCounterPlayerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterPlayerId}");
            }

            if (request.LifePointsToIncrease.HasValue == false )
            {
                return (false, $"Error: invalid increase amount request: {request.LifePointsToIncrease}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersDecreaseLifePointsResponse?, string)> DecreaseLifePoints(UsersDecreaseLifePointsRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = DecreaseLifePoints_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .Include(a => a.LifeCounterManager)
                .Where(a => a.Id == request.LifeCounterPlayerId &&
                    a.LifeCounterManager!.UserId == userId)
                .FirstOrDefaultAsync();

            if (lifeCounterPlayerDB == null)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterPlayerDB}");
            }

            if (lifeCounterPlayerDB.LifeCounterManager!.IsFinished == true)
            {
                return (null, "Error: this life counter manager was already finished");
            }

            if (lifeCounterPlayerDB.AutoDefeatMode == true && lifeCounterPlayerDB.IsDefeated == true)
            {
                return (null, $"Error: {lifeCounterPlayerDB.PlayerName} has been already defeated");
            }            

            var updatedLifePoints = lifeCounterPlayerDB.CurrentLifePoints - request.LifePointsToDecrease;

            lifeCounterPlayerDB.CurrentLifePoints = updatedLifePoints;

            var text = $"{lifeCounterPlayerDB.PlayerName} life points updated successfully. Current life points: {updatedLifePoints}";

            if (lifeCounterPlayerDB.CurrentLifePoints <= 0 && lifeCounterPlayerDB.AutoDefeatMode == true)
            {
                lifeCounterPlayerDB.CurrentLifePoints = 0; 
                lifeCounterPlayerDB.IsDefeated = true;

                text = $"{lifeCounterPlayerDB.PlayerName} has been defeated!";
            }

            await this._daoDbContext.SaveChangesAsync();
            
            return (new UsersDecreaseLifePointsResponse { UpdatedLifePoints = updatedLifePoints }, text);
        }
        private static (bool, string) DecreaseLifePoints_Validation(UsersDecreaseLifePointsRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false || request.LifeCounterPlayerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterPlayerId}");
            }

            if (request.LifePointsToDecrease.HasValue == false)
            {
                return (false, $"Error: invalid decrease amount request: {request.LifePointsToDecrease}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersRestoreLifeCounterPlayerResponse?, string)> RestoreLifeCounterPlayer(UsersRestoreLifeCounterPlayerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = RestoreLifeCounterPlayer_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .Include(a => a.LifeCounterManager)
                .Where(a => a.Id == request!.LifeCounterPlayerId &&
                    a.LifeCounterManager!.UserId == userId)
                .AnyAsync();

            if (lifeCounterPlayerDB == false)
            {
                return (null, $"Error, invalid requested LifeCounterPlayer, returning: {lifeCounterPlayerDB}");
            }


            await this._daoDbContext
                .LifeCounterPlayers
                .Where(a => a.Id == request!.LifeCounterPlayerId &&
                            a.LifeCounterManager!.UserId == userId)
                .ExecuteUpdateAsync(a => a
                    .SetProperty(b => b.CurrentLifePoints, 1)
                    .SetProperty(b => b.IsDefeated, false));

            return (new UsersRestoreLifeCounterPlayerResponse {  }, "Player restored successfully");
        }
        private static (bool, string) RestoreLifeCounterPlayer_Validation(UsersRestoreLifeCounterPlayerRequest? request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false || request.LifeCounterPlayerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterPlayerId}");
            }        

            return (true, string.Empty);
        }


        public async Task<(UsersGetPlayersCountResponse?, string)> GetPlayersCount(UsersGetPlayersCountRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = GetPlayersCount_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var playersCountDB = await this._daoDbContext
                .LifeCounterManagers               
                .Where(a => a.UserId == userId &&
                           a.Id == request.LifeCounterManagerId)
                .Select(a => a.PlayersCount)
                .FirstOrDefaultAsync();

            if (playersCountDB == null || playersCountDB < 0)
            {
                return (null, $"Error, PlayersCount request failed, returning: {playersCountDB}");
            }
    
            return (new UsersGetPlayersCountResponse { PlayersCount = playersCountDB }, "Players count fetched successfully)");
        }
        private static (bool, string) GetPlayersCount_Validation(UsersGetPlayersCountRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterManagerId.HasValue == false || request.LifeCounterManagerId < 1)
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: {request.LifeCounterManagerId}");
            }        

            return (true, string.Empty);
        }



        public async Task<(UsersChangePlayerNameResponse?, string)> ChangePlayerName(UsersChangePlayerNameRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = ChangePlayerName_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB_isValid = await this._daoDbContext
                .LifeCounterPlayers
                .AnyAsync(a => a.LifeCounterManager!.UserId == userId && a.Id == request.LifeCounterPlayerId);                         

            if (lifeCounterPlayerDB_isValid == false)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterPlayerDB_isValid}");
            }

            var isNewNameValid = await this._daoDbContext
                .LifeCounterPlayers
                .AnyAsync(a =>
                     a.PlayerName!.ToLower().Trim() == request.PlayerNewName!.ToLower().Trim());

            if(isNewNameValid == true)
            {
                return (null, "Error: requested name is already in use!");
            }

            await this._daoDbContext
                .LifeCounterPlayers
                .Where(a => a.LifeCounterManager!.UserId == userId && a.Id == request.LifeCounterPlayerId)
                .ExecuteUpdateAsync(a => a.SetProperty(b => b.PlayerName, request.PlayerNewName));

            return (new UsersChangePlayerNameResponse { }, "Life counter player name changed successfully.");
        }
        private static (bool, string) ChangePlayerName_Validation(UsersChangePlayerNameRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false ||
                (request.LifeCounterPlayerId.HasValue == true && request.LifeCounterPlayerId.Value < 1))
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: request.LifeCounterPlayerId == {request.LifeCounterPlayerId}");

            }

            if (string.IsNullOrWhiteSpace(request.PlayerNewName) == true)
            {
                return (false, $"Error: requested PlayerNewName failed: String.IsNullOrWhiteSpace(request.PlayerNewName) == true? {string.IsNullOrWhiteSpace(request.PlayerNewName) == true}");
            }

            return (true, string.Empty);
        }

        public async Task<(UsersDeleteLifeCounterPlayerResponse?, string)> DeleteLifeCounterPlayer(UsersDeleteLifeCounterPlayerRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (requestIsValid, message) = DeleteLifeCounterPlayer_Validation(request);

            if (requestIsValid == false)
            {
                return (null, message);
            }

            var lifeCounterPlayerDB = await this._daoDbContext
                .LifeCounterPlayers
                .Include(a => a.LifeCounterManager)
                .FirstOrDefaultAsync(a => a.LifeCounterManager!.UserId == userId && a.Id == request.LifeCounterPlayerId);

            if (lifeCounterPlayerDB == null)
            {
                return (null, $"Error, invalid requested LifeCounterManager, returning: {lifeCounterPlayerDB}");
            }

            
            var lifeCounterManagerDB = lifeCounterPlayerDB.LifeCounterManager;

            if (lifeCounterManagerDB == null)
            {
                return (null, $"Error, LifeCounterManager request failed, returning: {lifeCounterManagerDB}");
            }
           
            if( lifeCounterManagerDB.PlayersCount <= 1)
            {
                return (null, "Error: a life counter manager must host at least one player");
            }
            
            this._daoDbContext.LifeCounterPlayers.Remove(lifeCounterPlayerDB);

            lifeCounterManagerDB.PlayersCount--;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersDeleteLifeCounterPlayerResponse { }, "Life counter player name changed successfully.");
        }
        private static (bool, string) DeleteLifeCounterPlayer_Validation(UsersDeleteLifeCounterPlayerRequest request)
        {
            if (request == null)
            {
                return (false, $"Error: request failed: {request}");
            }

            if (request.LifeCounterPlayerId.HasValue == false ||
                (request.LifeCounterPlayerId.HasValue == true && request.LifeCounterPlayerId.Value < 1))
            {
                return (false, $"Error: requested LifeCounterPlayerId failed: request.LifeCounterPlayerId == {request.LifeCounterPlayerId}");

            }           

            return (true, string.Empty);
        }

        #endregion
        

        #region 5º LIFE COUNTER STATISTICS
        public async Task<(UsersGetLifeCounterStatisticsResponse?, string)> GetLifeCounterStatistics(UsersGetLifeCounterStatisticsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = GetLifeCounterStatistics_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var lifeCounterTemplatesDB = await this._daoDbContext
                .LifeCounterTemplates
                .Include(a => a.LifeCounterManagers)
                .Where(a => a.UserId == userId)            
                .ToListAsync();

            if (lifeCounterTemplatesDB == null || lifeCounterTemplatesDB.Count < 0)
            {
                return (null, "Error: requested list of Life Counter returned null or with 0 elements");
            }

            var mustUsedLifeCounter = "";
            var lifeCountersCreated = 0;
            var lifeCountersStarted = 0;
            var unfinishedLifeCounters = 0;
            var favoritePlayersCount = 0;

            if (lifeCounterTemplatesDB.Count == 0)
            {
                return (new UsersGetLifeCounterStatisticsResponse
                {
                    MostUsedLifeCounter = "No Life Counter created yet",
                    LifeCountersCreated = lifeCountersCreated,
                    LifeCountersStarted = lifeCountersStarted,
                    UnfinishedLifeCounters = unfinishedLifeCounters,
                    FavoritePlayersCount = favoritePlayersCount
                }, "Statistics fetched successfully");

            }

            mustUsedLifeCounter = lifeCounterTemplatesDB
                .OrderByDescending(a => a.LifeCounterManagers!.Count)
                .FirstOrDefault()?.LifeCounterTemplateName;

            lifeCountersCreated = lifeCounterTemplatesDB.Count;

            var lifeCounterManagersDB = lifeCounterTemplatesDB
                .SelectMany(a => a.LifeCounterManagers!)
                .Where(a => a.UserId == userId)
                .ToList();

            if (lifeCounterManagersDB == null || lifeCounterManagersDB.Count <= 0)
            {
                var avgTemplatePlayersCount =  (lifeCounterManagersDB!.Select(a => a.PlayersCount!).DefaultIfEmpty(0).Average() ?? 0);

                return (new UsersGetLifeCounterStatisticsResponse
                {
                    MostUsedLifeCounter = mustUsedLifeCounter,
                    LifeCountersCreated = lifeCountersCreated,
                    LifeCountersStarted = 0,
                    UnfinishedLifeCounters = 0,
                    FavoritePlayersCount = (int?)Math.Ceiling(avgTemplatePlayersCount!)
                }, "Statistics fetched successfully");

            }

            lifeCountersStarted = lifeCounterTemplatesDB
                .SelectMany(a => a.LifeCounterManagers!)
                .Count();

            unfinishedLifeCounters = lifeCounterTemplatesDB
                .SelectMany(a => a.LifeCounterManagers!)
                .Where(a => a.IsFinished == false)
                .Count();

            var avgPlayersCount = lifeCounterTemplatesDB
                .SelectMany(a => a.LifeCounterManagers!)
                .Select(a => a.PlayersCount!)
                .Average();

            var content = new UsersGetLifeCounterStatisticsResponse
            {
                MostUsedLifeCounter = mustUsedLifeCounter,
                LifeCountersCreated = lifeCountersCreated,
                LifeCountersStarted = lifeCountersStarted,
                UnfinishedLifeCounters = unfinishedLifeCounters,
                FavoritePlayersCount = (int?)Math.Ceiling(avgPlayersCount!.Value)
            };

            return (content, "Statistics fetched successfully");
        }
        private static (bool, string) GetLifeCounterStatistics_Validation(UsersGetLifeCounterStatisticsRequest? request)
        {
            if (request != null)
            {
                return (false, $"Error: request is NOT null but it MUST be null!");
            }

            return (true, string.Empty);
        }

        #endregion

        #endregion


        #region PLAYABLE GAMES

        public async Task<(UsersStartMabCampaignResponse?, string)> StartMabCampaign(UsersStartMabCampaignRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = StartMabCampaign_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var anyUserUnfinishedCampaign = await this._daoDbContext
                .MabPlayerCampaigns
                .AnyAsync(a => a.UserId == userId && a.IsDeleted == false);
            
            if(anyUserUnfinishedCampaign == true)
            {
                return (null, "Error: there is already a campaign started by this user!");
            }

            var userNewMabCampaign = new MabPlayerCampaign
            {
                UserId = userId,
                MabPlayerCardCopies = new List<MabPlayerCardCopy>(),
                MabPlayerDecks = new List<MabPlayerDeck>()
            };               

            var difficultyLevelEvaluation = EvaluateDifficultyLevel(request.MabCampaignDifficulty!.Value);

            var startingGoldStash = difficultyLevelEvaluation[0];

            var maxCardLevel = difficultyLevelEvaluation[1];

            var startingCardsCount = difficultyLevelEvaluation[2];

            (var mabPlayerInitialCardCopies, message) = await GetMabPlayerInicialCardCopies(userNewMabCampaign.Id, maxCardLevel, startingCardsCount);

            if (mabPlayerInitialCardCopies == null || mabPlayerInitialCardCopies.Count == 0)
            {
                return (null, message);
            }

            (var mabPlayerInitialAssignedCardCopies, message) = GetMabPlayerInitialAssignedCardCopies(mabPlayerInitialCardCopies);

            if (mabPlayerInitialAssignedCardCopies == null || mabPlayerInitialCardCopies.Count == 0)
            {
                return (null, message);
            }

            var mabPlayerNewDeck = new MabPlayerDeck
            {
                Name = "Inicial Deck",
                MabPlayerAssignedCardCopies = mabPlayerInitialAssignedCardCopies,
                IsActive = true
            };
            
            userNewMabCampaign.MabPlayerNickName = request!.MabPlayerNickName;
            userNewMabCampaign.MabPlayerLevel = 0;
            userNewMabCampaign.Difficulty = request.MabCampaignDifficulty;
            userNewMabCampaign.GoldStash = startingGoldStash;
            userNewMabCampaign.MabPlayerCardCopies = mabPlayerInitialCardCopies;
            userNewMabCampaign.MabPlayerDecks.Add(mabPlayerNewDeck);            

            this._daoDbContext.MabPlayerCampaigns.Add(userNewMabCampaign);

            var isCampainStartedSuccessfully = await this._daoDbContext.SaveChangesAsync();

            return (new UsersStartMabCampaignResponse
            {
                MabCampainId = userNewMabCampaign.Id,
            }, "New Medieval Auto Battler Campain started successfully!");
        }
        private static (bool, string) StartMabCampaign_Validation(UsersStartMabCampaignRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.MabPlayerNickName) == true)
            {
                return (false, "Error: MabPlayerNickName is null or empty");
            }         

            return (true, string.Empty);
        }
        private async Task<(List<MabPlayerCardCopy>?, string)> GetMabPlayerInicialCardCopies(int mabCampaignId, int maxCardLevel, int startingCardsCount)
        {
            var validInitialMabCardsDB = await _daoDbContext
                .MabCards
                .Where(a => a.Level <= maxCardLevel && a.IsDeleted == false)
                .Select(a => a.Id)
                .ToListAsync();

            if (validInitialMabCardsDB == null || validInitialMabCardsDB.Count == 0)
            {
                return (null, "Error: no mab cards having power + upper hand < 5 were found");
            }

            var random = new Random();
            var randomCardIds = new List<int>();

            while (randomCardIds.Count < startingCardsCount)
            {
                randomCardIds.Add(validInitialMabCardsDB[random.Next(validInitialMabCardsDB.Count)]);
            }

            var mabPlayerInitialCardCopies = new List<MabPlayerCardCopy>();

            foreach (var cardId in randomCardIds)
            {
                if (cardId == 0)
                {
                    return (null, "Error: invalid mab cardId for initial Mab Player Card Copies");
                }

                mabPlayerInitialCardCopies.Add(new MabPlayerCardCopy
                {
                    MabPlayerCampaignId = mabCampaignId,
                    MabCardId = cardId,                  
                });
            }

            return (mabPlayerInitialCardCopies, string.Empty);
        }
        private (List<MabPlayerAssignedCardCopy>?, string) GetMabPlayerInitialAssignedCardCopies(List<MabPlayerCardCopy> mabPlayerInicialCardCopies)
        {
            var mabPlayerInicialAssignedCardCopies = new List<MabPlayerAssignedCardCopy>();

            for (var i = 0; i < Constants.DeckSize; i++)
            {
                mabPlayerInicialAssignedCardCopies.Add(new MabPlayerAssignedCardCopy
                {
                    MabCardCopy = mabPlayerInicialCardCopies[i]
                });
            }

            if (mabPlayerInicialAssignedCardCopies.Count == 0)
            {
                return (null, "Error while processing mab player card copies into assigned ones");
            }

            return (mabPlayerInicialAssignedCardCopies, string.Empty);
        }
        private List<int> EvaluateDifficultyLevel(MabCampaignDifficulty difficultyLevel)
        {
            int startingGoldStash;

            int maxCardLevel;

            int startingCardsCount;

            switch (difficultyLevel)
            {
                case MabCampaignDifficulty.Easy:
                    startingGoldStash = 2 * Constants.BoosterPrice;
                    maxCardLevel = Constants.MinCardLevel + 2;
                    startingCardsCount = Constants.DeckSize * 3;
                    break;
                case MabCampaignDifficulty.Medium:
                    startingGoldStash = Constants.BoosterPrice;
                    maxCardLevel = Constants.MinCardLevel + 1;
                    startingCardsCount = Constants.DeckSize * 2;
                    break;
                case MabCampaignDifficulty.Hard:
                    startingGoldStash = 0;
                    maxCardLevel = Constants.MinCardLevel;
                    startingCardsCount = Constants.DeckSize;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(difficultyLevel));                 
            }   
     

            return new List<int> { startingGoldStash, maxCardLevel, startingCardsCount };
        }

        public async Task<(UsersShowMabCampaignStatisticsResponse?, string)> ShowMabCampaignStatistics(UsersShowMabCampaignStatisticsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ShowMabCampaignStatistics_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var content = await this._daoDbContext
                .MabPlayerCampaigns
                .Where(a => a.UserId == userId && a.IsDeleted == false)
                .Select(a => new UsersShowMabCampaignStatisticsResponse
                {
                    MabPlayerNickName = a.MabPlayerNickName!,

                    MabCampaignDifficulty = a.Difficulty!.Value,

                    Goldstash = a.GoldStash!.Value,

                    CountMatches = a.CountMatches!.Value,

                    CountVictories = a.CountVictories!.Value,

                    CountDefeats = a.CountDefeats!.Value,

                    CountBoosters = a.CountBoosters!.Value,

                    PlayerLevel = a.MabPlayerLevel!.Value,

                    DecksOwned = a.MabPlayerDecks!.Count,

                    AllCardsCollectedTrophy = a.AllCardsCollectedTrophy!.Value,

                    AllNpcsDefeatedTrophy = a.AllNpcsDefeatedTrophy!.Value
                })
                .FirstOrDefaultAsync();

            if (content == null)
            {
                return (null, "Error: Mab Campaign not found");
            }           

            return (content, "Mab Campaign Statistics fetched successfully!");
        }
        private static (bool, string) ShowMabCampaignStatistics_Validation(UsersShowMabCampaignStatisticsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersEditMabPlayerNickNameResponse?, string)> EditMabPlayerNickName(UsersEditMabPlayerNickNameRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditMabPlayerNickName_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabPlayerCampaigns
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId);

            if (mabCampaignDB == null)
            {
                return (null, "Error: Mab Campaign not found");
            }

            if (mabCampaignDB.IsDeleted == true)
            {
                return (null, "Error: requested Mab Campaign is already finished");
            }

            mabCampaignDB.MabPlayerNickName = request!.NewMabPlayerNickname!;


            await this._daoDbContext.SaveChangesAsync();

            return (new UsersEditMabPlayerNickNameResponse(), "Mab Player Nickname updated successfully!");
        }
        private static (bool, string) EditMabPlayerNickName_Validation(UsersEditMabPlayerNickNameRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }


            if (string.IsNullOrWhiteSpace(request.NewMabPlayerNickname) == true)
            {
                return (false, $"Error: NewMabPlayerNickname request failed: {request.NewMabPlayerNickname}");
            }



            return (true, string.Empty);
        }


        public async Task<(UsersShowMabCardDetailsResponse?, string)> ShowMabCardDetails(UsersShowMabCardDetailsRequest? request)
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
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request!.MabCardId);

            if (mabCardDB == null)
            {
                return (null, "Error: Mab Card not found!");
            }

            return (new UsersShowMabCardDetailsResponse
            {
                MabCardName = mabCardDB.Name,
                MabCardPower = mabCardDB.Power,
                MabCardUpperHand = mabCardDB.UpperHand,
                MabCardLevel = mabCardDB.Level,
                MabCardType = mabCardDB.Type,
            }, "Mab Card details fetched successfully!");
        }
        private static (bool, string) ShowMabCardDetails_Validation(UsersShowMabCardDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is not null");
            }

            if (request.MabCardId <= 0)
            {
                return (false, "Error: invalid medieval auto battler MabCardId");
            }

            return (true, string.Empty);
        }

      
        public async Task<(UsersShowMabDeckDetailsResponse?, string)> ShowMabDeckDetails(UsersShowMabDeckDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ShowMabPlayerDeckDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }


           
            if (request!.MabDeckId == null)
            {
                var activeDeckIdDB = await this._daoDbContext
                    .MabPlayerDecks
                      .AsNoTracking()                                      
                      .FirstOrDefaultAsync(a => a.MabPlayerCampaign!.UserId == userId && a.MabPlayerCampaign.IsDeleted == false && a.IsActive == true);

                if (activeDeckIdDB == null)
                {
                    var lastMabDeckAdded = await this._daoDbContext
                        .MabPlayerDecks 
                        .AsNoTracking()
                        .OrderByDescending(a => a.Id)                                                           
                        .FirstOrDefaultAsync(a => a.MabPlayerCampaign!.UserId == userId && a.MabPlayerCampaign.IsDeleted == false);

                    if(lastMabDeckAdded == null)
                    {
                        return (null, "Error: no mab player decks found for this user!");
                    }

                    lastMabDeckAdded.IsActive = true;

                    request.MabDeckId = lastMabDeckAdded.Id;
                }
                else
                {
                    request.MabDeckId = activeDeckIdDB.Id;
                }           

            }
          
            var deckData = await this._daoDbContext
                .MabPlayerDecks
                .AsNoTracking()
                .Where(a => a.MabPlayerCampaign!.UserId == userId &&
                            a.MabPlayerCampaign.IsDeleted!.Value == false &&
                            a.Id == request.MabDeckId)
                .Select(a => new
                {
                    DeckId = a.Id,
                    DeckName = a.Name,
                    AssignedCardCopies = a.MabPlayerAssignedCardCopies
                        .Where(b => b.MabCardCopy != null)
                        .Select(b => new UsersShowMabDeckDetailsResponse_assignedCardCopy
                        {
                            AssignedMabCardCopyId = b.Id,
                            MabCardName = b.MabCardCopy.MabCard.Name,
                            MabCardLevel = b.MabCardCopy.MabCard.Level,
                            MabCardPower = b.MabCardCopy.MabCard.Power,
                            MabCardUpperHand = b.MabCardCopy.MabCard.UpperHand,
                            MabCardType = b.MabCardCopy.MabCard.Type
                        })
                        .OrderByDescending(b => b.MabCardLevel)
                        .ThenByDescending(b => b.MabCardPower)
                        .ThenByDescending(b => b.MabCardUpperHand)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (deckData == null)
            {
                return (null, "Error: failed to fetch mab player active deck");
            }

           
            var deckLevel = deckData.AssignedCardCopies.Count > 0 ? Helper.GetPlayerDeckLevel(deckData.AssignedCardCopies.Select(a => a.MabCardLevel).ToList()) : 0;

            var content = new UsersShowMabDeckDetailsResponse
            {
                ActiveMabDeckId = deckData.DeckId,
                ActiveMabDeckName = deckData.DeckName,
                DeckLevel = deckLevel,
                MabCardCopies = deckData.AssignedCardCopies
            };

            return (content, "Mab Player Current Deck fetched successfully!");
        }
        private static (bool, string) ShowMabPlayerDeckDetails_Validation(UsersShowMabDeckDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }      
     
            return (true, string.Empty);
        }


        public async Task<(UsersEditMabDeckNameResponse?, string)> EditMabDeckName(UsersEditMabDeckNameRequest? request) {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = EditMabDeckName_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var activeMabDeck = await this._daoDbContext
                .MabPlayerDecks
                .FirstOrDefaultAsync(a => a.MabPlayerCampaign!.UserId == userId && a.Id == request!.MabDeckId);

            if(activeMabDeck == null)
            {
                return (null, "Error: requested active mab deck not found!");
            }

            activeMabDeck.Name = request!.MabDeckName;

            var wasNameEditionSuccessfull =  await this._daoDbContext.SaveChangesAsync();

            if(wasNameEditionSuccessfull < 1)
            {
                return (null, "Warning: no changes have been effected!");
            }

            return (new UsersEditMabDeckNameResponse(), "Active Mab Deck name updated successfully!");
        }
        private static (bool, string) EditMabDeckName_Validation(UsersEditMabDeckNameRequest? request) {

            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.MabDeckId.HasValue == false || request.MabDeckId < 1)
            {
                return (false, "Error: invalid or missing MabDeckId");
            }

            if (string.IsNullOrWhiteSpace(request.MabDeckName) == true)
            {
                return (false, "Error: invalid or missing MabDeckName");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersAssignMabCardCopyToDeckResponse?, string)> AssignMabCardCopy(UsersAssignMabCardCopyRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = AssignMabCardCopy_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mablayerDeckDB = await this._daoDbContext
              .MabPlayerDecks
              .Include(a => a.MabPlayerAssignedCardCopies)
              .FirstOrDefaultAsync(a => a.MabPlayerCampaign.UserId == userId && a.MabPlayerCampaign.IsDeleted == false && a.Id == request.MabDeckId);

            if (mablayerDeckDB == null)
            {
                return (null, "Error: requested mab card copy not found!");
            }

            mablayerDeckDB.MabPlayerAssignedCardCopies.Add(new MabPlayerAssignedCardCopy
            {
                MabCardCopyId = request.MabCardCopyId,
                MabPlayerDeckId = mablayerDeckDB.Id
            });

            var wereChangesEffected = await this._daoDbContext.SaveChangesAsync();

            if (wereChangesEffected < 1)
            {
                return (null, "Error: attempt to add mab card copy to deck failed!");
            }

            return (new UsersAssignMabCardCopyToDeckResponse(), "Mab card copy was successfully assigned to deck!");
        }
        private static (bool, string) AssignMabCardCopy_Validation(UsersAssignMabCardCopyRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.MabCardCopyId == null || request.MabCardCopyId < 1)
            {
                return (false, "Error: invalid or missing MabCardCopyId");
            }

            if (request.MabDeckId == null || request.MabDeckId < 1)
            {
                return (false, "Error: invalid or missing ActiveMabDeckId");
            }


            return (true, string.Empty);
        }


        public async Task<(UsersUnassignMabCardCopyResponse?, string)> UnassignMabCardCopy(UsersUnassignMabCardCopyRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = UnassignMabCardCopy_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var wasDeleteSuccessfull = await this._daoDbContext
                .MabPlayerAssignedCardCopies
                .Where(a => a.MabPlayerDeck.MabPlayerCampaign.UserId == userId && a.Id == request.AssignedMabCardCopyId)
                .ExecuteDeleteAsync();
   
            if (wasDeleteSuccessfull < 1)
            {
                return (null, "Error: attempt to inactivate mab card copy failed!");
            }

            return (new UsersUnassignMabCardCopyResponse(), "Mab card copy was successfully INACTIVATED!");
        }
        private static (bool, string) UnassignMabCardCopy_Validation(UsersUnassignMabCardCopyRequest? request) 
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.AssignedMabCardCopyId == null || request.AssignedMabCardCopyId < 1)
            {
                return (false, "Error: invalid or missing MabCardCopyId");
            }

            return (true, string.Empty);
        }
 

        public async Task<(List<UsersListUnassignedMabCardCopiesResponse>?, string)> ListUnassignedMabCardCopies(UsersListListUnassignedMabCardCopiesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListInactiveMabCardCopies_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            // Step 1: Get counts per card from DB
            var groupedCards = await _daoDbContext
                .MabPlayerCardCopies
                .AsNoTracking()
                .Where(a => a.MabPlayerAssignedCardCopies!
                    .Any(b => b.MabPlayerDeckId == request!.MabDeckId) == false)                
                .GroupBy(a => new
                {
                    a.MabCardId,                    
                })
                .Select(a => new
                {
                    MabCard = a.Select(b => new
                    {
                        b.Id,
                        b.MabCard!.Name,
                        b.MabCard.Level,
                        b.MabCard.Power,
                        b.MabCard.UpperHand
                    }).FirstOrDefault(),
                  
                    Qty = a.Count()
                })
                .ToListAsync(); // materialize in memory

            // Step 2: Build final MabPlayerTurn_response with string formatting in memory
            var content = groupedCards
                .Select(a => new UsersListUnassignedMabCardCopiesResponse
                {
                    MabCardCopyId = a.MabCard!.Id!,
                    MabCardDescription = $"{a.MabCard.Name}({a.Qty})-{a.MabCard.Level}/{a.MabCard.Power}/{a.MabCard.UpperHand}"
                })
                .OrderBy(x => x.MabCardDescription)
                .ToList();


            return (content, "Mab Player Current Deck fetched successfully!");
        }
        private static (bool, string) ListInactiveMabCardCopies_Validation(UsersListListUnassignedMabCardCopiesRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.MabDeckId < 1)
            {
                return (false, "Error: request is ");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersListMabPlayerDecksResponse>?, string)> ListMabPlayerDecks(UsersListMabPlayerDecksRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListMabPlayerDecks_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabPlayerDecks = await this._daoDbContext
                .MabPlayerDecks
                .Where(a => a.MabPlayerCampaign!.UserId == userId && a.IsActive == false)
                .Select(a => new UsersListMabPlayerDecksResponse
                {
                    MabDeckId = a.Id,
                    MabDeckDescription = a.Name
                })
                .ToListAsync();


            return (mabPlayerDecks, "Player's mab decks listed successfully");
        }
        private static (bool, string) ListMabPlayerDecks_Validation(UsersListMabPlayerDecksRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it must be null");
            }
                       
            return (true, string.Empty);
        }


        public async Task<(UsersAddMabPlayerDeckResponse?, string)> AddMabPlayerDeck(UsersAddMabCardCopyToDeckRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = AddMabPlayerDeck_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabPlayerCampaigns
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Select(a => new
                {
                    MabCampaignID = a.Id,
                    MabDecksCount = a.MabPlayerDecks.Count
                })
                .FirstOrDefaultAsync();

            if(mabCampaignDB == null)
            {
                return (null, "Error: mab campaign not found");
            }

            var newMabDeck = new MabPlayerDeck
            {
                Name = $"New Deck #{mabCampaignDB.MabDecksCount + 1}",
                IsActive = false,
                MabPlayerCampaignId = mabCampaignDB.MabCampaignID
            };

            this._daoDbContext.MabPlayerDecks.Add(newMabDeck);

            var isNewMabDeckAddedSuccessfully = await this._daoDbContext.SaveChangesAsync();

            if(isNewMabDeckAddedSuccessfully < 1)
            {
                return (null, "No changes effected while trying to add a new mab deck");
            }

            return (new UsersAddMabPlayerDeckResponse
            {
                NewMabDeckId = newMabDeck.Id,
                NewMabDeckName = newMabDeck.Name,
            }, "Mab Player card copies listed successfully!");
        }
        private static (bool, string) AddMabPlayerDeck_Validation(UsersAddMabCardCopyToDeckRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersDeleteMabDeckResponse?, string)> DeleteMabDeck(UsersDeleteMabDeckRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = DeleteMabDeck_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabDeckDB = await this._daoDbContext
                .MabPlayerDecks
                .FirstOrDefaultAsync(a => a.MabPlayerCampaign!.UserId == userId && a.Id == request!.MabDeckId);

            if(mabDeckDB == null)
            {
                return (null, "Error: requested mab deck could not be found");
            }

            var mabAssignedCardCopies = await this._daoDbContext
                .MabPlayerAssignedCardCopies
                .Where(a => a.MabPlayerDeck!.MabPlayerCampaign!.UserId == userId && a.MabPlayerDeckId == request.MabDeckId)
                .ToListAsync();

            if(mabAssignedCardCopies.Count > 0)
            {
                this._daoDbContext.MabPlayerAssignedCardCopies.RemoveRange(mabAssignedCardCopies);
            }

            this._daoDbContext.MabPlayerDecks.Remove(mabDeckDB);

            var wereChangesEffect = await this._daoDbContext.SaveChangesAsync();

            if(wereChangesEffect < 1)
            {
                return (null, "Warning: no changes were effected");
            }

            return (new UsersDeleteMabDeckResponse(), "Mab deck deleted successfully");
        }
        private static (bool, string) DeleteMabDeck_Validation(UsersDeleteMabDeckRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if(request.MabDeckId.HasValue == false || request.MabDeckId < 1)
            {
                return (false, "Error: MabDeckId is invalid or missing");
            }

            return (true, string.Empty);

        }


        public async Task<(UsersActivateMabDeckResponse?, string)> ActivateMabDeck(UsersActivateMabDeckRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ActivateMabDeck_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var currentActiveMabDeckDB = await this._daoDbContext
                .MabPlayerDecks
                .FirstOrDefaultAsync(a => a.MabPlayerCampaign!.UserId == userId && a.IsActive == true);

            if(currentActiveMabDeckDB != null)
            {
                currentActiveMabDeckDB!.IsActive = false;
            }

            var mabDeckDB = await this._daoDbContext
                .MabPlayerDecks
                .FirstOrDefaultAsync(a => a.MabPlayerCampaign!.UserId == userId && a.Id == request!.MabDeckId);

            if (mabDeckDB == null)
            {
                return (null, "Error: requested mab deck could not be found");
            }

            mabDeckDB.IsActive = true;

            var wereChangesEffect = await this._daoDbContext.SaveChangesAsync();

            if (wereChangesEffect < 1)
            {
                return (null, "Warning: no changes were effected");
            }

            return (new UsersActivateMabDeckResponse(), mabDeckDB.Name!);
        }
        private static (bool, string) ActivateMabDeck_Validation(UsersActivateMabDeckRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.MabDeckId.HasValue == false || request.MabDeckId < 1)
            {
                return (false, "Error: MabDeckId is invalid or missing");
            }

            return (true, string.Empty);

        }


        public async Task<(UsersStartMabBattleResponse?, string)> StartMabBattle(UsersStartMabBattleRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = StartMabBattle_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabPlayerCampaigns!
                .Include(a => a.MabBattles!)
                .ThenInclude(b => b.Npc)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDeleted == false);

            if (mabCampaignDB == null)
            {
                return (null, "Error: request mab campaign could not be found!");
            }
            
            var anyOngoingBattle = mabCampaignDB.MabBattles!.Any(a => a.IsFinished == false);

            if(anyOngoingBattle == true)
            {
                return (null, "Error: an ongoing mab battle is already in progress, please continue it or finish it first!");
            }

            var randomNpc = await this._daoDbContext
                .MabNpcs
                .AsNoTracking()
                .Where(a => !a.IsDeleted && a.Level <= mabCampaignDB.MabPlayerLevel + 1)
                .OrderBy(a => Guid.NewGuid())   // random order at SQL side
                .Select(a => new { a.Id, a.Name })
                .FirstOrDefaultAsync();

            if (randomNpc == null)
            {
                return (null, "Error: no valid mab NPC was found for this battle");
            }

            var doesPlayerGoFirst = RandomFirstPlayer();

            var newMabBattleTurns = new List<MabBattleTurn>();
            newMabBattleTurns.Add(new MabBattleTurn
            {
                MabBattleRoundNumber = 1,
            });            

            mabCampaignDB.MabBattles!.Add(new MabBattle
            {
                IsPlayerTurn = doesPlayerGoFirst,
                IsFinished = false,
                MabCampaignId = mabCampaignDB.Id,
                NpcId = randomNpc!.Id,   
                MabBattleTurns = newMabBattleTurns
            });            

            await _daoDbContext.SaveChangesAsync();          

            return (
                new UsersStartMabBattleResponse
                {         
                    MabPlayerNickName = mabCampaignDB.MabPlayerNickName!,
                    MabNpcName = randomNpc.Name,
                    DoesPlayerGoFirst = doesPlayerGoFirst,
                    MabBattleRoundNumber = 1,
                },
                "A new battle started successfully");
        }
        private static (bool, string) StartMabBattle_Validation(UsersStartMabBattleRequest? request)
        {
            if(request == null)
            {
                return ( false, "Error: request is null!");
            }

            if(request.MabQuestId.HasValue == false || request.MabQuestId < 1)
            {
                return (false, "Error: MabQuestId is invalid or missing");
            }   

            return (true, string.Empty);    
        }
        private static bool RandomFirstPlayer()
        {
            return random.Next(0, 2) == 0;
        }

        public async Task<(UsersContinueMabBattleResponse?, string)> ContinueMabBattle(UsersContinueMabBattleRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ContinueMabBattle_Validation(request);
            
            if (isValid == false)
            {
                return (null, message);
            }

            var onGoingMabBattleDB = await this._daoDbContext
                .MabBattles
                .AsNoTracking()
                .Where(a => a.MabPlayerCampaign!.UserId == userId && a.MabPlayerCampaign.IsDeleted == false && a.IsFinished == false)
                .Select(a => new
                {
                    MabPlayerNickName = a.MabPlayerCampaign.MabPlayerNickName,
                    MabNpcName = a.Npc.Name,
                    IsPlayerTurn = a.IsPlayerTurn,
                    MabBattleTurnsCount = a.MabBattleTurns!.Count,
                    LastMabNpcDeckEntryId = a.MabBattleTurns!.OrderByDescending(b => b.Id).FirstOrDefault()!.MabNpcDeckEntryId,
                    MabBattlePoints = a.MabBattlePoints
                })
                .FirstOrDefaultAsync();

            if(onGoingMabBattleDB == null)
            {
                return (null, "Error: no ongoing mab battle was found!");
            }


            var mabPlayerNickName = onGoingMabBattleDB.MabPlayerNickName;
            var mabNpcName = onGoingMabBattleDB.MabNpcName;
            var isPlayerTurn = onGoingMabBattleDB.IsPlayerTurn;
            var mabBattleRoundNumber = onGoingMabBattleDB.MabBattleTurnsCount;
            var mabNpcDeckEntryId = onGoingMabBattleDB.LastMabNpcDeckEntryId;
            var mabBattlePoints = onGoingMabBattleDB.MabBattlePoints;

            //It is no the Player's turn...
            if (isPlayerTurn == false)
            {
                // Npc has not played yet, so we need to make its turn now...
                if (mabNpcDeckEntryId == null)
                {
                    (var mabNpcDuellingCard, message) = await this.MabNpcTurn();

                    if(mabNpcDuellingCard == null)
                    {
                        return (null, message);
                    }                    

                    return (
                        new UsersContinueMabBattleResponse
                        {
                            MabPlayerNickName = mabPlayerNickName,
                            MabNpcName = mabNpcName,
                            IsPlayerTurn = isPlayerTurn,
                            MabBattleRoundNumber = mabBattleRoundNumber,
                            MabBattlePoints = mabBattlePoints,
                            MabNpcCard = new UsersContinueMabBattleResponse_MabNpcCard
                            {
                                MabNpcCardId = mabNpcDuellingCard.MabNpcCardId,
                                MabCardName = mabNpcDuellingCard.MabCardName,
                                MabCardLevel = mabNpcDuellingCard.MabCardLevel,
                                MabCardPower = mabNpcDuellingCard.MabCardPower,
                                MabCardUpperHand = mabNpcDuellingCard.MabCardUpperHand,
                                MabCardType = mabNpcDuellingCard.MabCardType,
                            }
                        }
                        ,"Mab Battle loaded successfully!");
                }
                //It is the NPC'S turn, and he has already played, fetching its card data...
                var mabNpcCard = await this._daoDbContext
                        .MabNpcCard
                        .Where(a => a.Id == mabNpcDeckEntryId)
                        .Select(a => new UsersContinueMabBattleResponse_MabNpcCard
                        {
                            MabNpcCardId = a.Id,
                            MabCardName = a.Card.Name,
                            MabCardLevel = a.Card.Level,
                            MabCardPower = a.Card.Power,
                            MabCardUpperHand = a.Card.UpperHand,
                            MabCardType = a.Card.Type,
                        })
                        .FirstOrDefaultAsync();

                if (mabNpcCard == null)
                {
                    return (null, "Error: failed to fetch MabNpcCard data");
                }

                return (new UsersContinueMabBattleResponse
                {
                    MabPlayerNickName = mabPlayerNickName,
                    MabNpcName = mabNpcName,
                    IsPlayerTurn = isPlayerTurn,
                    MabBattleRoundNumber = mabBattleRoundNumber,
                    MabBattlePoints = mabBattlePoints,
                    MabNpcCard = mabNpcCard
                }, "Mab Battle loaded successfully!");


            }        
    
            return (new UsersContinueMabBattleResponse
            {
                MabPlayerNickName = mabPlayerNickName,
                MabNpcName = mabNpcName,
                IsPlayerTurn = isPlayerTurn,
                MabBattleRoundNumber = mabBattleRoundNumber,
                MabBattlePoints = mabBattlePoints,
                MabNpcCard = null
            }, "Mab Battle loaded successfully!");
        }
        private static (bool, string) ContinueMabBattle_Validation(UsersContinueMabBattleRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it must be null!");
            }           

            return (true, string.Empty);
        }


        public async Task<(UsersMabBattleTurnManagerResponse?, string)> MabBattleTurnManager(UsersMabBattleTurnManagerRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabBattleTurnManager_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabBattlesDB = await this._daoDbContext
                .MabBattles
                .AsNoTracking()
                .Where(a => a.MabPlayerCampaign!.UserId == userId && a.IsFinished == false)
                .OrderByDescending(a => a.Id)
                .Select(a => a.IsPlayerTurn)
                .ToListAsync();

            if (mabBattlesDB.Count != 1)
            {
                return (null, "Error: more than one ongoing battle were found!");
            }

            var IsPlayerTurn = mabBattlesDB.FirstOrDefault();

            switch (IsPlayerTurn)
            {
                case true:
                    (var MabPlayerTurn_response, message) = await this.MabPlayerTurn(
                        new UsersMabPlayerTurnRequest
                        {
                            MabCardCopyId = request!.MabPlayerCardId
                        });

                    if(MabPlayerTurn_response == null)
                    {
                        return (null, message);
                    }

                    return (
                        new UsersMabBattleTurnManagerResponse
                        {
                            MabNpcCard = null
                        }, 
                        "Mab Player Turn executed successfully");
                case false:
                    (var mabNpcTurn_response, message) = await this.MabNpcTurn();

                    if (mabNpcTurn_response == null)
                    {
                        return (null, message);
                    }

                    return (new UsersMabBattleTurnManagerResponse
                    {
                        MabNpcCard = mabNpcTurn_response
                    }, "Mab NPC Turn executed successfully");
                default:
                    return (null, "Error: failed to compute whose turn it is!");
            }
        }
        private static (bool, string) MabBattleTurnManager_Validation(UsersMabBattleTurnManagerRequest? request)
        {
            if (request != null && request.MabPlayerCardId.HasValue == false)
            {
                return (false, "Error: request.MabPlayerCardId is missing or invalid!");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersListMabAvailableRoundCardCopiesResponse>?, string)> ListMabAvailableRoundCardCopies(UsersListMabAvailableRoundCardCopiesRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ListMabPlayerRoundCardCopies_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabPlayerAssignedCardCopyIds = await this._daoDbContext
                .MabPlayerAssignedCardCopies
                .Where(a => a.MabPlayerDeck!.MabPlayerCampaign!.UserId == userId && a.MabPlayerDeck.IsActive == true)
                .Select(a => a.MabCardCopyId)
                .ToListAsync();           

            if (mabPlayerAssignedCardCopyIds == null)
            {
                return (null, "Error: no mab player assigned cards were found for this battle!");
            }

            var mabPlayerUnavailableCardCopyIds = await _daoDbContext
                .MabBattles
                .Where(b => b.MabPlayerCampaign.UserId == userId)
                .SelectMany(b => b.MabBattleTurns!)
                .Where(t => t.MabPlayerCardCopyId != null)
                .Select(t => t.MabPlayerCardCopyId!.Value)
                .ToListAsync();

            var avalailableMabPlayerCards =
                mabPlayerAssignedCardCopyIds
                .Where(a => mabPlayerUnavailableCardCopyIds.Contains((int)a!) == false)
                .ToList();

            
            var content = await this._daoDbContext
                .MabPlayerCardCopies
                .AsNoTracking()
                .Where(a => avalailableMabPlayerCards.Contains(a.Id))
                .Select(a => new UsersListMabAvailableRoundCardCopiesResponse
                {
                    MabCardCopyId = a.Id,
                    MabCardName = a.MabCard.Name,
                    MabCardPower = a.MabCard.Power,
                    MabCardUpperHand = a.MabCard.UpperHand,
                    MabCardLevel = a.MabCard.Level,
                    MabCardType = a.MabCard.Type,
                })
                .OrderBy(a => a.MabCardLevel)
                .ToListAsync();

            

            return (content,
                "Mab Player Card Copies available for this round have been listed successfully!");
        }
        private static (bool, string) ListMabPlayerRoundCardCopies_Validation(UsersListMabAvailableRoundCardCopiesRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null!, however it MUST be null");
            }         

            return (true, string.Empty);
        }


        public async Task<(UsersMabPlayerTurnResponse?, string)> MabPlayerTurn(UsersMabPlayerTurnRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabPlayerTurn_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var isMabCardCopyIdValid = await this._daoDbContext
                .MabPlayerAssignedCardCopies
                .AnyAsync(a => a.MabCardCopyId == request!.MabCardCopyId &&
                               a.MabPlayerDeck!.MabPlayerCampaign!.UserId == userId &&
                               a.MabPlayerDeck.IsActive == true);

            if (isMabCardCopyIdValid == false)
            {
               return (null, "Error: MabCardCopyId is invalid or not assigned to the active deck!");
            }

            var isMabBattleUnique = await this._daoDbContext
                .MabBattles                
                .CountAsync(a => a.IsFinished == false && a.MabPlayerCampaign!.UserId == userId);

            if(isMabBattleUnique != 1)
            {
                return (null, "Error: more than one ongoing battle were found!");
            }

            var mabBattleDB = await this._daoDbContext
                .MabBattles
                .OrderByDescending(a => a.Id)
                .Include(a => a.MabBattleTurns)
                .FirstOrDefaultAsync(a => a.IsFinished == false && a.MabPlayerCampaign!.UserId == userId);

            if(mabBattleDB == null)
            {
               return (null, "Error: no mab battle found for this campaign!");
            }

            if (mabBattleDB.IsPlayerTurn == false)
            {
                return (null, "Error: its not the players turn!");
            }

            var isMabCardCopyAvailable = mabBattleDB
                .MabBattleTurns!
                .Any(a => a.MabPlayerCardCopyId == request!.MabCardCopyId) == false;

            if(isMabCardCopyAvailable == false)
            {
                return (null, "Error: MabCardCopyId has already been used in this battle!");
            }

            var mabBattleLastTurn = mabBattleDB
                .MabBattleTurns?              
                .FirstOrDefault(a => a.MabPlayerCardCopyId == null);                

            if (mabBattleLastTurn == null)
            {
                return (null, "Error: something went wrong!");
            }
                          
            mabBattleLastTurn.MabPlayerCardCopyId = request!.MabCardCopyId;
            
            mabBattleDB.IsPlayerTurn = false;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabPlayerTurnResponse(),"Mab Player Battle Round ended successfully");
        }
        private static (bool, string) MabPlayerTurn_Validation(UsersMabPlayerTurnRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null!");
            }

            if (request.MabCardCopyId.HasValue == false || request.MabCardCopyId < 1)
            {
                return (false, "Error: MabCardCopyId is invalid or missing");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabNpcTurnResponse?, string)> MabNpcTurn(UsersMabNpcTurnRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabNpcTurn_Validation(request);
            if (!isValid)
            {
                return (null, message);
            }

            var isMabBattleUnique = await this._daoDbContext
              .MabBattles
              .CountAsync(a => a.IsFinished == false && a.MabPlayerCampaign!.UserId == userId);

            if (isMabBattleUnique != 1)
            {
                return (null, "Error: more than one ongoing battle were found!");
            }

            var mabBattleDB = await this._daoDbContext
                .MabBattles
                .Include(a => a.MabBattleTurns)
                .Include(a => a.Npc)
                .ThenInclude(a => a.MabNpcCards)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a => a.IsFinished == false && a.MabPlayerCampaign!.UserId == userId);

            if (mabBattleDB == null)
            {
                return (null, "Error: no mab battle found for this campaign!");
            }

            if(mabBattleDB.IsPlayerTurn == true)
            {
                return (null, "Error: it is not the NPC's turn!");
            }

            var mabNpcDB = mabBattleDB.Npc;
            if (mabNpcDB == null)
            {
                return (null, "Error: no mab NPC found for this battle!");
            }

            var mabNpcCardsDB = mabNpcDB.MabNpcCards;
            if (mabNpcCardsDB == null || mabNpcCardsDB.Count < Constants.DeckSize)
            {
                return (null, "Error: NPC cards missing or insufficient!");
            }

            // already used entries
            var usedNpcDeckEntryIds = mabBattleDB
                .MabBattleTurns!
                .Where(a => a.MabNpcDeckEntryId != null)
                .Select(a => a.MabNpcDeckEntryId!.Value)
                .ToList();

            // available entries (copies not yet used)
            var availableNpcDeckEntries = mabNpcCardsDB
                .Where(a => !usedNpcDeckEntryIds.Contains(a.Id))
                .ToList();

            if (availableNpcDeckEntries.Any() == false)
            {
                return (null, "Error: NPC has no more available cards!");
            }

            var randomNpcEntry = availableNpcDeckEntries.OrderBy(_ => random.Next()).First();
            var npcDeckEntryId = randomNpcEntry.Id;

            mabBattleDB.IsPlayerTurn = true;
           
            var mabLastBattleTurn = mabBattleDB
                .MabBattleTurns!
                .FirstOrDefault(a => a.MabNpcDeckEntryId == null);

            if (mabLastBattleTurn == null)
            {
                mabBattleDB.MabBattleTurns!.Add(new MabBattleTurn
                {
                    MabNpcDeckEntryId = npcDeckEntryId,
                });
            }
            else
            {
                mabLastBattleTurn.MabNpcDeckEntryId = npcDeckEntryId;
            }

            var mabCardDB = await this._daoDbContext
                .MabNpcCard
                .AsNoTracking()
                .Where(a => a.Id == npcDeckEntryId)
                .Select(a => new
                {
                    NpcCardId = a.Id,
                    CardName = a.Card.Name,
                    CardLevel = a.Card.Level,
                    CardPower = a.Card.Power,
                    CardUpperHand = a.Card.UpperHand,
                    CardType = a.Card.Type,
                })
                .FirstOrDefaultAsync();

            if (mabCardDB == null)
            {
                return (null, "Error: failed to fecth mab card data of the card used by this npc");
            }
            
            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabNpcTurnResponse
            {
                MabNpcCardId = mabCardDB.NpcCardId,
                MabCardName = mabCardDB.CardName,
                MabCardLevel = mabCardDB.CardLevel,
                MabCardPower = mabCardDB.CardPower,
                MabCardUpperHand = mabCardDB.CardUpperHand,
                MabCardType = mabCardDB.CardType
            },
            "Round ended, winner evaluated successfully!");
        }
        private static (bool, string) MabNpcTurn_Validation(UsersMabNpcTurnRequest? request) 
        { 
            if(request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersCheckForMabDuelResponse?, string)> CheckForMabDuel(UsersCheckForMabDuelRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = CheckForMabDuel_Validation(request);
            if (!isValid)
            {
                return (null, message);
            }

            var isLastMabDuelFinished = await this._daoDbContext
                .MabBattles                         
                .Where(a => 
                    a.MabPlayerCampaign.UserId == userId && 
                    a.MabPlayerCampaign.IsDeleted == false && 
                    a.IsFinished == false)
                .Select(a => a.MabBattleTurns!.OrderByDescending(b => b.Id).Select(b => b.IsRoundFinished!.Value).FirstOrDefault())
                .FirstOrDefaultAsync();                    

            if(isLastMabDuelFinished == true)
            {
                (var results, message) = await this.ResolveMabBattleDuel(new());

                return (null, message);
            }

            return (null, string.Empty);
        }
        private static (bool, string) CheckForMabDuel_Validation(UsersCheckForMabDuelRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }



        public async Task<(UsersResolveMabBattleDuelResponse?, string)> ResolveMabBattleDuel(UsersResolveMabBattleDuelRequest request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = ResolveMabBattleDuel_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabBattleDB = await this._daoDbContext
                .MabBattles     
                .Include(a => a.MabBattleTurns)
                .FirstOrDefaultAsync(a => a.MabPlayerCampaign!.UserId == userId && a.IsFinished == false);

            if(mabBattleDB == null)
            {
                return (null, "Error: no ongoing battle found for this campaign!");
            }

            var mabBattleDuels = mabBattleDB.MabBattleTurns;

            if(mabBattleDuels == null || mabBattleDuels.Count == 0)
            {
                return (null, "Error: no duels found for this battle!");
            }

            var isBattleFinished = mabBattleDuels.Count == 5 && mabBattleDuels.All(a => a.IsRoundFinished == true);
            if(isBattleFinished == true)
            {
                return (new UsersResolveMabBattleDuelResponse(), "This mab battle is now finished!");
            }

            var lastBattleDuel = mabBattleDB
                .MabBattleTurns!
                .FirstOrDefault(a => a.MabNpcDeckEntryId != null && a.MabPlayerCardCopyId != null && a.IsRoundFinished == false);

            if(lastBattleDuel == null)
            {
                return (null, "Error: no valid duel found to be resolved!");
            }

            var mabPlayerDuellingCardId = lastBattleDuel.MabPlayerCardCopyId!.Value;

            var mabNpcDuellingCardId = lastBattleDuel.MabNpcDeckEntryId!.Value;

            var duellingCards = await this._daoDbContext
            .MabCards
            .AsNoTracking()
            .Where(a => !a.IsDeleted &&
                        (a.MabPlayerCardCopies!.Any(b => b.Id == mabPlayerDuellingCardId) || a.Id == mabNpcDuellingCardId))
            .Select(a => new
            {
                a.Id,
                a.Power,
                a.UpperHand,
                a.Type,
                IsPlayer = a.MabPlayerCardCopies!.Any(b => b.Id == mabPlayerDuellingCardId),
                IsNpc = a.Id == mabNpcDuellingCardId
            })
            .ToListAsync();

            var mabPlayerDuellingCard = duellingCards.FirstOrDefault(c => c.IsPlayer);

            var mabNpcDuellingCard = duellingCards.FirstOrDefault(c => c.IsNpc);

            var mabPlayerDuellingCard_FullPower = Helper.GetCardFullPower(
                mabPlayerDuellingCard!.Power,
                mabPlayerDuellingCard.UpperHand, 
                (int)mabPlayerDuellingCard.Type!, 
                (int)mabNpcDuellingCard.Type!);

            var mabNpcDuellingCard_FullPower = Helper.GetCardFullPower(
                mabNpcDuellingCard!.Power,
                mabNpcDuellingCard.UpperHand, 
                (int)mabNpcDuellingCard.Type!, 
                (int)mabPlayerDuellingCard.Type!);

            var mabBattleRoundPoints = Helper.GetDuelingPoints(mabPlayerDuellingCard_FullPower, mabNpcDuellingCard_FullPower);

            switch (Math.Sign(mabBattleRoundPoints))
            {
                case 1: // player wins
                    lastBattleDuel.HasPlayerWon = true;
                    break;
                case -1: // npc wins
                    lastBattleDuel.HasPlayerWon = false;
                    break;
                case 0: // draw
                    lastBattleDuel.HasPlayerWon = false;
                    break;
                default:
                    return (null, "Error: unexpected error while trying to resolve mab battle duel!");
            }

            lastBattleDuel.IsRoundFinished = true;

            var newTurn = new MabBattleTurn
            {
                MabBattleRoundNumber = mabBattleDuels.Count + 1,       
            };

            mabBattleDB.MabBattleTurns!.Add(newTurn);

            await this._daoDbContext.SaveChangesAsync();

            var content = new UsersResolveMabBattleDuelResponse
            {
                MabPlayerDuellingCardCopyFullPower = mabPlayerDuellingCard_FullPower,
                MabNpcDuellingDeckEntryFullPower = mabNpcDuellingCard_FullPower,
                MabBattleRoundPoints = mabBattleRoundPoints,
                NextRoundNumber = newTurn.MabBattleRoundNumber.Value
            };

            return (content, "Mab Battle resolved successfully!");
        }
        private static (bool, string) ResolveMabBattleDuel_Validation(UsersResolveMabBattleDuelRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }






        #endregion
    }
}