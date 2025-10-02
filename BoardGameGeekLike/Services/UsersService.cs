using BoardGameGeekLike.Models;
using BoardGameGeekLike.Models.Dtos.Request;
using BoardGameGeekLike.Models.Dtos.Response;
using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Models.Enums;
using BoardGameGeekLike.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
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

                        // Normalize the line before checking if it's campaign table title
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
                // Dealing with user'campaign Profile Details
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
                // Dealing with the user'campaign Life Counter TEMPLATES
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
                // Dealing with the user'campaign Life Counter MANAGERS
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
                // Dealing with the user'campaign Life Counter PLAYERS
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
                // Dealing with THE user'campaign BOARD GAME SESSIONS
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
                // Dealing with THE user'campaign BOARD GAME RATINGS
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

        #region 1� LIFE COUNTER QUICK START      
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

                    // Creating campaign new default Life Counter TEMPLATE:
                    var (createTemplate_reponse_content, createTemplate_response_message) = await this.CreateLifeCounterTemplate();

                    if (createTemplate_reponse_content == null)
                    {
                        return (null, $"Error: request to create a new Default Life Counter TEMPLATE failed: {createTemplate_response_message}");
                    }

                    // Default Life Counter Template Mab_NpcId:
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

                    // Most recently created Life Counter Template Mab_NpcId:
                    lifeCounterTemplateId = getLastTemplate_reponse_content.LastLifeCounterTemplateId;
                    lifeCounterTemplateName = getLastTemplate_reponse_content.LastLifeCounterTemplateName;

                    text = "New Default life counter MANAGER started successfully, belonging to the last created life counter Template";
                }

                // Starting campaign new Life Counter MANAGER
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
                    // 1st => start campaign new life counter MANAGER of the template 
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


        #region 2� LIFE COUNTER TEMPLATES
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

        
        #region 3� LIFE COUNTER MANAGERS
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


        #region 4� LIFE COUNTER PLAYERS    
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
        

        #region 5� LIFE COUNTER STATISTICS
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
        public async Task<(UsersMabCheckForExistingCampaignResponse?, string)> MabCheckForExistingCampaign(UsersMabCheckForExistingCampaignRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabBuyDeckBooster failed! User is not authenticated");
            }

            var (isValid, message) = MabCheckForExistingCampaign_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabAnyExisitngCampaign = await this._daoDbContext
                .MabCampaigns
                .AnyAsync(campaign =>
                    campaign.Mab_IsCampaignDeleted == false && campaign.UserId == userId);

            if (mabAnyExisitngCampaign == null)
            {
                return (null, "Error: MabBuyDeckBooster failed! No Mab Campaign found!");
            }           


            return (new UsersMabCheckForExistingCampaignResponse
            {
                Mab_AnyExistingCampaign = mabAnyExisitngCampaign
            }, "Successfully check for any existing campaign");
        }
        private static (bool, string) MabCheckForExistingCampaign_Validation(UsersMabCheckForExistingCampaignRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabBuyDeckBooster failed! Request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }

        public (UsersMabGetCampaignDificultyInfoResponse?, string) MabGetCampaignDificultyInfo(UsersMabGetCampaignDificultyInfoRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabGetCampaignDificultyInfo failed! User is not authenticated");
            }

            var (isValid, message) = MabGetCampaignDificultyInfo_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDifficultyEvaluation = MabEvaluateCampaignDifficultyLevel(request!.Mab_CampaignDifficulty!.Value);

            var startingCoinsStash = mabCampaignDifficultyEvaluation[0];

            var startingCardsMaxLevel = mabCampaignDifficultyEvaluation[1];

            var startingCardsCount = mabCampaignDifficultyEvaluation[2];

            var questsBaseGoldBounty = mabCampaignDifficultyEvaluation[3];

            var questsBaseXpReward = mabCampaignDifficultyEvaluation[4];

            return (new UsersMabGetCampaignDificultyInfoResponse
            {
                Mab_StartingCoinsStash = startingCoinsStash,
                Mab_StartingCardsMaxLevel = startingCardsMaxLevel,
                Mab_StartingCardsCount = startingCardsCount,
                Mab_QuestsBaseGoldBounty = questsBaseGoldBounty,
                Mab_QuestsBaseXpReward = questsBaseGoldBounty,
            }, "Mab Campaign Info fetched successfully!");
        }
        private static (bool, string) MabGetCampaignDificultyInfo_Validation(UsersMabGetCampaignDificultyInfoRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabGetCampaignDificultyInfo_Validation failed! Request is null");
            }

            if (request.Mab_CampaignDifficulty == null || request.Mab_CampaignDifficulty.HasValue == false)
            {
                return (false, "Error: MabGetCampaignDificultyInfo_Validation failed! Mab_CampaignDifficulty is null invalid");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabStartCampaignResponse?, string)> MabStartCampaign(UsersMabStartCampaignRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabStartCampaign failed! User is not authenticated");
            }

            var (isValid, message) = MabStartCampaign_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var anyUnfinishedMabCampaign = await this._daoDbContext
                .MabCampaigns
                .AsNoTracking()
                .AnyAsync(a => a.UserId == userId && a.Mab_IsCampaignDeleted == false);            
            if(anyUnfinishedMabCampaign == true)
            {
                return (null, "Error: MabStartCampaign failed! An ongoing mab CAMPAIGN has been found!");
            }

            var newMabCampaign = new MabCampaign
            {
                UserId = userId,
                Mab_PlayerCards = new List<MabPlayerCard>(),
                Mab_Decks = new List<MabDeck>()
            };               

            var mabCampaignDifficultyEvaluation = MabEvaluateCampaignDifficultyLevel(request!.MabCampaignDifficulty!.Value);

            var startingMabCoinsStash = mabCampaignDifficultyEvaluation[0];

            var maxMabCardLevel = mabCampaignDifficultyEvaluation[1];

            var startingMabCardsCount = mabCampaignDifficultyEvaluation[2];

            (var mabPlayerInitialCards, message) = await MabGetPlayerInicialCards(newMabCampaign.Id, maxMabCardLevel, startingMabCardsCount);

            if (mabPlayerInitialCards == null || mabPlayerInitialCards.Count == 0)
            {
                return (null, message);
            }

            (var mabPlayerInitialAssignedCards, message) = MabGetPlayerInitialAssignedCards(mabPlayerInitialCards);

            if (mabPlayerInitialAssignedCards == null || mabPlayerInitialCards.Count == 0)
            {
                return (null, message);
            }

            var newMabPlayerDeck = new MabDeck
            {
                Mab_DeckName = "Inicial Deck",
                Mab_AssignedCards = mabPlayerInitialAssignedCards,
                Mab_IsDeckActive = true
            };
            
            newMabCampaign.Mab_PlayerNickname = request!.MabPlayerNickName;
            newMabCampaign.Mab_PlayerLevel = 0;
            newMabCampaign.Mab_Difficulty = request.MabCampaignDifficulty;
            newMabCampaign.Mab_CoinsStash = startingMabCoinsStash;
            newMabCampaign.Mab_PlayerCards = mabPlayerInitialCards;
            newMabCampaign.Mab_Decks.Add(newMabPlayerDeck);            

            this._daoDbContext.MabCampaigns.Add(newMabCampaign);

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabStartCampaignResponse
            {
                MabCampaignId = newMabCampaign.Id,
            }, "New Medieval Auto Battler Campaign started successfully!");
        }
        private static (bool, string) MabStartCampaign_Validation(UsersMabStartCampaignRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabStartCampaign failed! Request is null");
            }

            if (string.IsNullOrWhiteSpace(request.MabPlayerNickName) == true)
            {
                return (false, "Error: MabStartCampaign failed! MabPlayerNickName is null or empty");
            }         

            return (true, string.Empty);
        }
        private async Task<(List<MabPlayerCard>?, string)> MabGetPlayerInicialCards(int mabCampaignId, int maxMabCardLevel, int startingCardsCount)
        {
            var initialValidCardsDB = await _daoDbContext
                .MabCards
                .Where(card =>
                    card.Mab_CardLevel <= maxMabCardLevel &&
                    card.Mab_IsCardDeleted == false &&
                    card.Mab_CardType == MabCardType.Neutral)
                .Select(a => a.Id)
                .ToListAsync();

            if (initialValidCardsDB == null || initialValidCardsDB.Count == 0)
            {
                return (null, "Error: MabGetPlayerInicialCards failed! No mab cards having power + upper hand < 5 were found");
            }

            var randomCardIds = new List<int>();

            while (randomCardIds.Count < startingCardsCount)
            {
                randomCardIds.Add(initialValidCardsDB[random.Next(initialValidCardsDB.Count)]);
            }

            var playerInitialCards = new List<MabPlayerCard>();

            foreach (var cardId in randomCardIds)
            {
                if (cardId == 0)
                {
                    return (null, "Error: MabGetPlayerInicialCards failed! Invalid mab cardId for initial Mab Player Card Copies");
                }

                playerInitialCards.Add(new MabPlayerCard
                {
                    Mab_CampaignId = mabCampaignId,
                    Mab_CardId = cardId,                  
                });
            }

            var pickeAxeCard = await this._daoDbContext
                .MabCards
                .Where(card =>
                    card.Mab_IsCardDeleted == false &&
                    card.Mab_CardType == MabCardType.Infantry &&
                    card.Mab_CardPower == 0 &&
                    card.Mab_CardUpperHand == 0)
                .Select(card => new MabPlayerCard
                {
                    Mab_CampaignId = mabCampaignId,
                    Mab_CardId = card.Id
                })
                .FirstOrDefaultAsync();

            if(pickeAxeCard == null)
            {
                return (null, "Error: MabGetPlayerInicialCards failed! Unable to add the mining tool to the players inventory");
            }

            playerInitialCards.Add(pickeAxeCard);

            return (playerInitialCards, string.Empty);
        }
        private (List<MabAssignedCard>?, string) MabGetPlayerInitialAssignedCards(List<MabPlayerCard> mabPlayerInicialCards)
        {
            var mabPlayerInicialAssignedCardCopies = new List<MabAssignedCard>();

            for (var i = 0; i < Constants.DeckSize; i++)
            {
                mabPlayerInicialAssignedCardCopies.Add(new MabAssignedCard
                {
                    Mab_PlayerCard = mabPlayerInicialCards[i]
                });
            }

            if (mabPlayerInicialAssignedCardCopies.Count == 0)
            {
                return (null, "Error while processing mab player card copies into assigned ones");
            }

            return (mabPlayerInicialAssignedCardCopies, string.Empty);
        }
        private List<int> MabEvaluateCampaignDifficultyLevel(MabCampaignDifficulty mabCampaignDifficulty)
        {
            int startingCoinsStash;

            int startingCardMaxLevel;

            int startingCardsCount;

            int questsBaseGoldBounty;

            int questsBaseXpReward;

            switch (mabCampaignDifficulty)
            {
                case MabCampaignDifficulty.Easy:
                    startingCoinsStash = Constants.BoosterPrice;
                    startingCardMaxLevel = Constants.MinCardLevel + 2;
                    startingCardsCount = Constants.DeckSize * 3;
                    questsBaseGoldBounty = Constants.BoosterPrice * 3;
                    questsBaseXpReward = Constants.QuestsBaseXpReward * 3;
                    break;
                case MabCampaignDifficulty.Medium:
                    startingCoinsStash = Constants.BoosterPrice / 2;
                    startingCardMaxLevel = Constants.MinCardLevel + 1;
                    startingCardsCount = Constants.DeckSize * 2;
                    questsBaseGoldBounty = Constants.BoosterPrice * 2;
                    questsBaseXpReward = Constants.QuestsBaseXpReward * 2;
                    break;
                case MabCampaignDifficulty.Hard:
                    startingCoinsStash = 0;
                    startingCardMaxLevel = Constants.MinCardLevel;
                    startingCardsCount = Constants.DeckSize;
                    questsBaseGoldBounty = Constants.BoosterPrice;
                    questsBaseXpReward = Constants.QuestsBaseXpReward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mabCampaignDifficulty));                 
            }   
     

            return new List<int> { startingCoinsStash, startingCardMaxLevel, startingCardsCount, questsBaseGoldBounty, questsBaseXpReward};
        }


        public async Task<(UsersMabDeleteCampaignResponse?, string)> MabDeleteCampaign(UsersMabDeleteCampaignRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabDeleteCampaign_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var anyUnfinishedMabCampaign = await this._daoDbContext
                .MabCampaigns
                .AsNoTracking()
                .AnyAsync(a => a.UserId == userId && a.Mab_IsCampaignDeleted == false);

            if (anyUnfinishedMabCampaign == false)
            {
                return (null, "Error: MabDeleteCampaign failed! No ongoing mab CAMPAIGN found!");
            }

            var mabUnfinshedMabCampaign = await this._daoDbContext
                .MabCampaigns
                .Include(campaign => campaign.Mab_Battles!)
                    .ThenInclude(battle => battle.Mab_Duels!)
                .Include(campaign => campaign.Mab_PlayerCards!)
                    .ThenInclude(playerCard => playerCard.Mab_AssignedCards!)
                    .ThenInclude(assignedCard => assignedCard.Mab_Deck)
                .FirstOrDefaultAsync(campaign => campaign.UserId == userId && campaign.Mab_IsCampaignDeleted == false);

            var battlesDB = mabUnfinshedMabCampaign!.Mab_Battles;

            var duelsDB = battlesDB!.SelectMany(battle => battle.Mab_Duels!).ToList();

            var playerCardsDB = mabUnfinshedMabCampaign.Mab_PlayerCards;

            var assignedCardsDB = playerCardsDB!.SelectMany(playerCard => playerCard.Mab_AssignedCards).ToList();

            var decksDB = assignedCardsDB.Select(assignedCard => assignedCard.Mab_Deck).ToList();

            if (duelsDB != null && duelsDB.Count > 0)
            {
                this._daoDbContext.MabDuels.RemoveRange(duelsDB!);
            }

            if (battlesDB != null && battlesDB.Count > 0)
            {
                this._daoDbContext.MabBattles.RemoveRange(battlesDB);
            }


            if (playerCardsDB != null && playerCardsDB.Count > 0)
            {
                this._daoDbContext.MabPlayerCards.RemoveRange(playerCardsDB);
            }         

            if (assignedCardsDB != null && assignedCardsDB.Count > 0)
            {
                this._daoDbContext.MabAssignedCards.RemoveRange(assignedCardsDB);
            }

            if (decksDB != null && decksDB.Count > 0)
            {
                this._daoDbContext.MabDecks.RemoveRange(decksDB);
            }



            mabUnfinshedMabCampaign.Mab_IsCampaignDeleted = true;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabDeleteCampaignResponse(), "New Mab Campaign deleted successfully!");
        }
        private static (bool, string) MabDeleteCampaign_Validation(UsersMabDeleteCampaignRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabDeleteCampaign failed! request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabShowQuestDetailsResponse?, string)> MabShowQuestDetails(UsersMabShowQuestDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabShowQuestDetails failed! User is not authenticated");
            }

            var (isValid, message) = MabShowQuestDetails_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabQuestDB = await this._daoDbContext
                .MabQuests
                .AsNoTracking()
                .AsSplitQuery()                
                .Include(quest => quest.Mab_Npcs)
                .Where(quest =>
                    quest.Id == request.Mab_QuestId)
                .Select(quest => new UsersMabShowQuestDetailsResponse
                {
                    Mab_QuestId = quest.Id,
                    Mab_QuestTitle = quest.Mab_QuestTitle,
                    Mab_QuestDescription = quest.Mab_QuestDescription,
                    Mab_Npcs = quest.Mab_Npcs.Select(npc => new UsersMabShowQuestDetailsResponse_npc
                    {
                        Mab_NpcId = npc.Id,
                        Mab_NpcName = npc.Mab_NpcName
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            if(mabQuestDB == null)
            {
                return (null, "Error: MabShowQuestDetails failed! Mab Quest not found!");
            }
            
            return (mabQuestDB, "Mab Player Current Deck fetched successfully!");
        }
        private static (bool, string) MabShowQuestDetails_Validation(UsersMabShowQuestDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabShowQuestDetails_Validation failed! Request is null");
            }

            if (request.Mab_QuestId.HasValue == false)
            {
                return (false, "Error: MabShowQuestDetails_Validation failed! request.Mab_QuestId.HasValue == false");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersMabListQuestsResponse>?, string)> MabListQuests(UsersMabListQuestsRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabListQuests failed! User is not authenticated");
            }

            var (isValid, message) = MabListQuests_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var questsDB = await this._daoDbContext
                .MabQuests
                .AsNoTracking()
                .Select(quest => new UsersMabListQuestsResponse
                {
                    Mab_QuestId = quest.Id,
                    Mab_QuestTitle = quest.Mab_QuestTitle,
                    Mab_QuestDescription = quest.Mab_QuestDescription,
                    Mab_GoldBounty = quest.Mab_GoldBounty,
                    Mab_XpReward = quest.Mab_XpReward,
                    Mab_DefeatedNpcsCount = quest
                        .Mab_Battles
                        .Count(battle => 
                            battle.Mab_QuestId == quest.Id && 
                            battle.Mab_IsBattleFinished == true && 
                            battle.Mab_HasPlayerWon == true),
                    Mab_NpcsCount = quest.Mab_Npcs.Count,                    
                    Mab_IsQuestFulfilled = quest.Mab_Npcs.Count == 
                        quest
                        .Mab_Battles
                        .Count(battle =>
                            battle.Mab_QuestId == quest.Id &&
                            battle.Mab_IsBattleFinished == true &&
                            battle.Mab_HasPlayerWon == true)
                })
                .ToListAsync();

            return (questsDB, "Mab quests listed successfully");
        }
        private static (bool, string) MabListQuests_Validation(UsersMabListQuestsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabListQuests_Validation failed! Request is NOT null, however it MUST be null!");
            }          

            return (true, string.Empty);
        }


        public async Task<(UsersMabStartBattleResponse?, string)> MabStartBattle(UsersMabStartBattleRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabStartBattle is false! User is not authenticated");
            }

            var (isValid, message) = MabStartBattle_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns!
                .AsSplitQuery()               
                .Include(campaign => campaign.Mab_Battles!)
                    .ThenInclude(battle => battle.Mab_Duels!)
                .Include(campaign => campaign.Mab_Battles!)
                    .ThenInclude(battle => battle.Mab_Quest!)
                        .ThenInclude(quest => quest.Mab_Npcs!)
                            .ThenInclude(npc => npc.Mab_NpcCards)
                                .ThenInclude(npcCard => npcCard.Mab_Card)
                .FirstOrDefaultAsync(campaign => 
                    campaign.UserId == userId && 
                    campaign.Mab_IsCampaignDeleted == false);

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabStartBattle is false! Request mab campaign could not be found!");
            }
            
            var anyOngoingBattle = mabCampaignDB.Mab_Battles!.Any(a => a.Mab_IsBattleFinished == false);

            if(anyOngoingBattle == true)
            {
                var onGoingBattles = mabCampaignDB.Mab_Battles!.Where(a => a.Mab_IsBattleFinished == false).ToList();

                var onGoingDuels = onGoingBattles.SelectMany(a => a.Mab_Duels!).ToList();

                this._daoDbContext.MabDuels.RemoveRange(onGoingDuels);

                this._daoDbContext.MabBattles.RemoveRange(onGoingBattles);
            }

            var defeatedNpcsIds = mabCampaignDB
                .Mab_Battles!
                .Where(a => a.Mab_HasPlayerWon == true)
                .Select(a => a.Mab_NpcId)
                .ToList();

            var newMabBattle = new MabBattle();

            var doesPlayerGoFirst = RandomFirstPlayer();

            if (request.Mab_QuestId.HasValue == false && request.Mab_NpcId.HasValue == false)
            {
                var randomMabNpcID = await GetRandomNpcId(mabCampaignDB.Mab_PlayerLevel, defeatedNpcsIds);

                var randomNpc = await this._daoDbContext
                    .MabNpcs
                    .AsNoTracking()            
                    .FirstOrDefaultAsync(a => 
                        a.Mab_IsNpcDeleted == false && 
                        a.Id == randomMabNpcID);

                if (randomNpc == null)
                {
                    return (null, "Error: MabStartBattle is false! No valid mab NPC was found for this battle");
                }

                newMabBattle = new MabBattle
                {
                    Mab_DoesPlayerGoesFirst = doesPlayerGoFirst,
                    Mab_IsPlayerTurn = doesPlayerGoFirst,
                    Mab_IsBattleFinished = false,
                    Mab_CampaignId = mabCampaignDB.Id,
                    Mab_Npc = randomNpc,
                    Mab_Quest = null
                };

                mabCampaignDB.Mab_Battles!.Add(newMabBattle);     
                
                await _daoDbContext.SaveChangesAsync();

                return (
                    new UsersMabStartBattleResponse
                    {
                        Mab_BattleId = newMabBattle.Id,

                        Mab_PlayerNickName = mabCampaignDB.Mab_PlayerNickname!,
                        Mab_PlayerLevel = mabCampaignDB.Mab_PlayerLevel,
                        Mab_DoesPlayerGoFirst = doesPlayerGoFirst,

                        Mab_NpcName = randomNpc.Mab_NpcName,
                        Mab_NpcLevel = randomNpc.Mab_NpcLevel
                    },
                    "A new battle started successfully");
            }

            var questDB = await this._daoDbContext
                .MabQuests
                .Include(quest => quest.Mab_Npcs!
                    .Where(npc => npc.Id == request.Mab_NpcId))
                    .ThenInclude(npc => npc.Mab_NpcCards)
                        .ThenInclude(npcCard => npcCard.Mab_Card)
                .FirstOrDefaultAsync(quest => quest.Id == request.Mab_QuestId);

            var mabNpc = questDB!
                .Mab_Npcs!
                .FirstOrDefault(npc => npc.Id ==  request.Mab_NpcId);

            newMabBattle = new MabBattle
            {
                Mab_DoesPlayerGoesFirst = doesPlayerGoFirst,
                Mab_IsPlayerTurn = doesPlayerGoFirst,
                Mab_IsBattleFinished = false,
                Mab_CampaignId = mabCampaignDB.Id,
                Mab_QuestId = request.Mab_QuestId!.Value,
                Mab_Npc = mabNpc!,
            };

            mabCampaignDB.Mab_Battles!.Add(newMabBattle);

            await _daoDbContext.SaveChangesAsync();

            return (
                    new UsersMabStartBattleResponse
                    {
                        Mab_BattleId = newMabBattle.Id,

                        Mab_PlayerNickName = mabCampaignDB.Mab_PlayerNickname!,
                        Mab_PlayerLevel = mabCampaignDB.Mab_PlayerLevel,
                        Mab_DoesPlayerGoFirst = doesPlayerGoFirst,

                        Mab_NpcName = mabNpc!.Mab_NpcName,
                        Mab_NpcLevel = mabNpc.Mab_NpcLevel
                    },
                    "A new battle started successfully");
        }
        private static (bool, string) MabStartBattle_Validation(UsersMabStartBattleRequest? request)
        {
            if(request == null)
            {
                return ( false, "Error: request is null!");
            }

            if(request.Mab_QuestId.HasValue == true && request.Mab_QuestId < 1)
            {
                return (false, "Error: MabQuestId is invalid!");
            }   

            return (true, string.Empty);    
        }
        private static bool RandomFirstPlayer()
        {
            return random.Next(0, 2) == 0;
        }
        private async Task<int> GetRandomNpcId(int? mabPlayerLevel, List<int> mabDefeatedNpcsIds)
        {
            var lvlThreshold = mabPlayerLevel + Constants.NpcLvl_MaxUpperDifference;

            var mabNpcsIdsDB = await this._daoDbContext
                .MabNpcs
                .AsNoTracking()
                .Where(a =>
                    mabDefeatedNpcsIds!.Contains(a.Id) == false &&
                    (a.Mab_NpcLevel >= mabPlayerLevel - 1 &&
                    a.Mab_NpcLevel <= lvlThreshold) 
                    )
                .Select(a => a.Id)
                .ToListAsync();

            var shuffledMabNpcs = mabNpcsIdsDB.OrderBy(a => random.Next()).ToList();

            return shuffledMabNpcs.FirstOrDefault();
        }


        public async Task<(UsersMabAutoBattleResponse?, string)> MabAutoBattle(UsersMabAutoBattleRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabAutoBattle_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }


            // MAB CAMPAIGN
            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns
                .Include(a => a.Mab_Battles!)
                .ThenInclude(a => a.Mab_Npc)
                .FirstOrDefaultAsync(a => a.Mab_IsCampaignDeleted == false && a.UserId == userId);

            var mabPlayerLevel = mabCampaignDB.Mab_PlayerLevel;

            if (mabCampaignDB == null)
                return (null, "Error: Mab Campaign not found");

            // MAB NPC
            var defeatedNpcIds = mabCampaignDB.Mab_Battles!.Where(a => a.Mab_HasPlayerWon == true).Select(a => a.Mab_NpcId).ToList();

            var mabNpcId = await this.GetRandomNpcId(mabPlayerLevel, defeatedNpcIds);

            var mabNpcDB = await this._daoDbContext
                .MabNpcs
                .Include(a => a.Mab_NpcCards)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => !a.Mab_IsNpcDeleted && a.Id == mabNpcId);

            if (mabNpcDB == null)
                return (null, "Error: no valid mab NPC was found for this battle");

            var mabNpcCards = mabNpcDB.Mab_NpcCards;

            if (mabNpcCards == null || mabNpcCards.Count < 1)
                return (null, "Error: no Mab NPC cards were found for this battle");

            // MAB PLAYER       
            var mabPlayerActiveDeck = await this._daoDbContext
                .MabDecks
                .FirstOrDefaultAsync(a => a.Mab_IsDeckActive == true);

            if (mabPlayerActiveDeck == null)
                return (null, "Error: Mab Player active deck not found");

            var mabAssignedCards = await this._daoDbContext
                .MabAssignedCards
                .Where(a => a.Mab_DeckId == mabPlayerActiveDeck.Id)
                .ToListAsync();

            if (mabAssignedCards == null || mabAssignedCards.Count < 1)
                return (null, "Error: no Mab Player assigned cards were found for this battle");

            var mabPlayerCards = await this._daoDbContext
                .MabPlayerCards
                .Where(a => a.Mab_AssignedCards!.Where(b => b.Mab_DeckId == mabPlayerActiveDeck.Id).Select(b => b.Mab_PlayerCardId).Contains(a.Id))
                .ToListAsync();

            if (mabPlayerCards == null || mabPlayerCards.Count < 1)
                return (null, "Error: no Mab Player cards were found for this battle");

            // MAB CARDS
            var mabBattleCards = await this._daoDbContext
                .MabCards
                .Where(a =>
                    mabPlayerCards.Select(b => b.Mab_CardId).Contains(a.Id) ||
                    mabNpcCards.Select(b => b.Mab_CardId).Contains(a.Id)
                )
                .ToListAsync();

            if (mabBattleCards == null || mabBattleCards.Count < 1)
                return (null, "Error: no Mab Cards were found for this battle");


            // MAB BATTLE
            var doesPlayerGoesFirst = RandomFirstPlayer();

            var mabBattleDB = new MabBattle
            {
                Mab_DoesPlayerGoesFirst = doesPlayerGoesFirst,
                Mab_IsBattleFinished = false,
                Mab_CampaignId = mabCampaignDB.Id,
                Mab_NpcId = mabNpcDB!.Id,
                Mab_Duels = new List<MabDuel>()
            };

            var shuffledPlayerCards = mabPlayerCards.OrderBy(a => random.Next()).ToList();
            var mabPlayerCards_dto = new List<UsersMabAutoBattleResponse_playerCard>();

            var shuffledNpcCards = mabNpcCards.OrderBy(a => random.Next()).ToList();
            var mabNpcCards_dto = new List<UsersMabAutoBattleResponse_npcCard>();

            var response = new UsersMabAutoBattleResponse();

            var winningStreak = 0;

            MabPlayerState? mabPlayerState = MabPlayerState.Normal;

            var earnedXp = 0;

            var bonusXp = 0;

            for (int i = 0; i < Constants.DeckSize; i++)

            {
                var mabCard_player = new MabCard();
                mabCard_player = mabBattleCards
                    .FirstOrDefault(a => a.Id == shuffledPlayerCards[i].Mab_CardId);

                var mabCard_npc = new MabCard();
                mabCard_npc = mabBattleCards
                    .FirstOrDefault(a => a.Id == shuffledNpcCards[i].Mab_CardId);

                var duelPoints = 0;

                var mabCard_player_fullPower = mabCard_player!.Mab_CardPower;
                if (doesPlayerGoesFirst == true && (i + 1) % 2 != 0 || doesPlayerGoesFirst == false && (i + 1) % 2 == 0)
                {
                    (mabCard_player_fullPower, duelPoints) = Helper.MabResolveDuel(
                        mabCard_player!.Mab_CardPower,
                        mabCard_player.Mab_CardUpperHand,
                        mabCard_player.Mab_CardType, 
                        mabCard_npc.Mab_CardPower,
                        mabCard_npc.Mab_CardUpperHand,
                        mabCard_npc!.Mab_CardType,
                        true);
                }

                var mabCard_npc_fullPower = mabCard_npc.Mab_CardPower;
                if (doesPlayerGoesFirst == true && (i + 1) % 2 == 0 || doesPlayerGoesFirst == false && (i + 1) % 2 != 0)
                {
                    (mabCard_npc_fullPower, duelPoints) = Helper.MabResolveDuel(
                        mabCard_npc.Mab_CardPower,
                        mabCard_npc.Mab_CardUpperHand,
                        mabCard_npc.Mab_CardType,
                        mabCard_player!.Mab_CardPower,
                        mabCard_player.Mab_CardUpperHand,
                        mabCard_player.Mab_CardType, 
                        false);
                }

                var isPlayerAttacking =
                    doesPlayerGoesFirst == true && (i + 1) % 2 != 0 ||
                    doesPlayerGoesFirst == false && (i + 1) % 2 == 0;               

                if (duelPoints > 0)
                {
                    winningStreak++;
                }
                else
                {
                    winningStreak = 0;
                }

                mabPlayerState = (MabPlayerState?)winningStreak;

                (earnedXp, bonusXp) = Helper.MabGetEarnedXp(mabPlayerLevel!.Value, mabNpcDB.Mab_NpcLevel, duelPoints, winningStreak);


                mabBattleDB.Mab_Duels.Add(new MabDuel
                {
                    Mab_DuelPoints = duelPoints,
                    Mab_EarnedXp = earnedXp,
                    Mab_BonusXp = bonusXp,

                    IsFinished = true,

                    Mab_IsPlayerAttacking = isPlayerAttacking,
                    Mab_PlayerCardId = shuffledPlayerCards[i].Id,
                    Mab_PlayerCardName = mabCard_player.Mab_CardName,
                    Mab_PlayerCardType = mabCard_player.Mab_CardType,
                    Mab_PlayerCardLevel = mabCard_player.Mab_CardLevel,
                    Mab_PlayerCardPower = mabCard_player.Mab_CardPower,
                    Mab_PlayerCardUpperHand = mabCard_player.Mab_CardUpperHand,
                    Mab_PlayerCardFullPower = mabCard_player_fullPower,

                    Mab_PlayerState = mabPlayerState,

                    Mab_NpcCardId = shuffledNpcCards[i].Id,
                    Mab_NpcCardName = mabCard_npc.Mab_CardName,
                    Mab_NpcCardType = mabCard_npc.Mab_CardType,
                    Mab_NpcCardLevel = mabCard_npc.Mab_CardLevel,
                    Mab_NpcCardPower = mabCard_npc.Mab_CardPower,
                    Mab_NpcCardUpperHand = mabCard_npc.Mab_CardUpperHand,
                    Mab_NpcCardFullPower = mabCard_npc_fullPower,
                });

                var earnedGold = Helper.MabGetEarnedCoins(duelPoints);

                var mabDuels = mabBattleDB.Mab_Duels;

                mabPlayerCards_dto.Add(new UsersMabAutoBattleResponse_playerCard
                {
                    Mab_CardName = mabCard_player.Mab_CardName,
                    Mab_CardPower = mabCard_player.Mab_CardPower,
                    Mab_CardUpperHand = mabCard_player.Mab_CardUpperHand,
                    Mab_CardLevel = mabCard_player.Mab_CardLevel,
                    Mab_CardType = mabCard_player.Mab_CardType,
                    Mab_CardFullPower = mabCard_player_fullPower,

                    Mab_DuelId = mabDuels[i].Id,

                    Mab_DuelPoints = mabDuels[i].Mab_DuelPoints,

                    Mab_DuelEarnedXp = mabDuels[i].Mab_EarnedXp,

                    Mab_DuelBonusXp = mabDuels[i].Mab_BonusXp,

                    Mab_PlayerState = mabDuels[i].Mab_PlayerState
                });

                mabNpcCards_dto.Add(new UsersMabAutoBattleResponse_npcCard
                {
                    Mab_CardName = mabCard_npc.Mab_CardName,
                    Mab_CardCode = mabCard_npc.Mab_CardCode,
                    Mab_CardPower = mabCard_npc.Mab_CardPower,
                    Mab_CardUpperHand = mabCard_npc.Mab_CardUpperHand,
                    Mab_CardLevel = mabCard_npc.Mab_CardLevel,
                    Mab_CardType = mabCard_npc.Mab_CardType,
                    Mab_CardFullPower = mabCard_npc_fullPower
                });
            }

            mabCampaignDB.Mab_Battles!.Add(mabBattleDB);


            mabBattleDB.Mab_DoesPlayerGoesFirst = doesPlayerGoesFirst;

            mabBattleDB.Mab_HasPlayerWon = mabBattleDB.Mab_EarnedGold > 0;

            mabBattleDB.Mab_FinalPlayerState = mabPlayerState;

            mabBattleDB.Mab_IsBattleFinished = false;

            mabCampaignDB.Mab_CoinsStash =
                mabCampaignDB.Mab_CoinsStash == null || mabCampaignDB.Mab_CoinsStash == 0 ?
                mabBattleDB.Mab_EarnedGold :
                mabCampaignDB.Mab_CoinsStash + mabBattleDB.Mab_EarnedGold;

            mabCampaignDB.Mab_PlayerExperience =
                mabCampaignDB.Mab_PlayerExperience == null || mabCampaignDB.Mab_PlayerExperience == 0 ?
                mabBattleDB.Mab_EarnedXp :
                mabCampaignDB.Mab_PlayerExperience + mabBattleDB.Mab_EarnedXp;

            mabCampaignDB.Mab_PlayerLevel = Helper.MabGetPlayerLevel(mabCampaignDB.Mab_PlayerExperience);

            mabCampaignDB.Mab_BattlesCount++;

            if (mabBattleDB.Mab_HasPlayerWon == true)
            {
                mabCampaignDB.Mab_BattleVictoriesCount++;
            }
            else
            {
                mabCampaignDB.Mab_BattleDefeatsCount++;
            }

            await this._daoDbContext.SaveChangesAsync();

            response.Mab_BattleId = mabBattleDB.Id;
            response.Mab_DoesPlayerGoFirst = mabBattleDB.Mab_DoesPlayerGoesFirst;
            response.Mab_PlayerNickname = mabCampaignDB.Mab_PlayerNickname;
            response.Mab_PlayerLevel = mabCampaignDB.Mab_PlayerLevel;
            response.Mab_FinalPlayerState = mabBattleDB.Mab_FinalPlayerState;
            response.Mab_PlayerCards = mabPlayerCards_dto;
            response.Mab_NpcName = mabNpcDB.Mab_NpcName;
            response.Mab_NpcLevel = mabNpcDB.Mab_NpcLevel;
            response.Mab_NpcCards = mabNpcCards_dto;

            return (response, "Mab Battle finished on auto mode successfully!");
        }
        private static (bool, string) MabAutoBattle_Validation(UsersMabAutoBattleRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabAutoBattle failed! Request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabFinishBattleResponse?, string)> MabFinishBattle(UsersMabFinishBattleRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabFinishBattle_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns!
                .Include(a => a.Mab_Battles!)
                .ThenInclude(b => b.Mab_Npc)
                .Include(a => a.Mab_Battles!)
                .ThenInclude(c => c.Mab_Duels)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Mab_IsCampaignDeleted == false);

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabFinishBattle failed ! Requested mab campaign could not be found!");
            }

            var mabBattlesDB = mabCampaignDB.Mab_Battles;
            if (mabBattlesDB == null || mabBattlesDB.Count < 1)
            {
                return (null, "Error: MabFinishBattle failed! MAB BATTLES could be found in this campaign!");
            }

            var currentMabBattle =
                mabBattlesDB
                .OrderByDescending(a => a.Id)
                .FirstOrDefault(a => a.Mab_IsBattleFinished == false);
            if (currentMabBattle == null)
            {
                return (null, "Error: MabFinishBattle failed! No ongoing unfinished MAB BATTLE could be found!");
            }

            var mabDuelsDB = currentMabBattle.Mab_Duels;
            if (mabDuelsDB == null || mabDuelsDB.Count < Constants.DeckSize)
            {
                return (null, "Error: MabFinishBattle failed! Mab Duels not found or not yet finished!");
            }

            var totalPoints = mabDuelsDB.Sum(a => a.Mab_DuelPoints);
            var earnedXp = mabDuelsDB.Sum(a => a.Mab_EarnedXp);
            var bonusXp = mabDuelsDB.Sum(a => a.Mab_BonusXp);
            var finalPlayerState = mabDuelsDB[Constants.DeckSize - 1].Mab_PlayerState;
            var hasPlayerWon = mabDuelsDB.Sum(a => a.Mab_DuelPoints) > 0;

            var earnedGold = Helper.MabGetEarnedCoins(totalPoints);

            currentMabBattle.Mab_EarnedGold = earnedGold;
            currentMabBattle.Mab_EarnedXp = earnedXp;
            currentMabBattle.Mab_BonusXp = bonusXp;
            currentMabBattle.Mab_FinalPlayerState = finalPlayerState;
            currentMabBattle.Mab_HasPlayerWon = hasPlayerWon;
            currentMabBattle.Mab_IsBattleFinished = true;

            mabCampaignDB.Mab_CoinsStash += earnedGold;
            mabCampaignDB.Mab_PlayerExperience += earnedXp;
            mabCampaignDB.Mab_BattlesCount++;

            if (hasPlayerWon == true)
            {
                mabCampaignDB.Mab_BattleVictoriesCount++;
            }
            else
            {
                mabCampaignDB.Mab_BattleDefeatsCount++;
            }

            await _daoDbContext.SaveChangesAsync();

            return (new UsersMabFinishBattleResponse
            {
                Mab_HasPlayerWon = hasPlayerWon,
                Mab_BattlePoints = earnedGold,
                Mab_UpdatedGoldStash = mabCampaignDB.Mab_CoinsStash,
                Mab_BattleEarnedXp = earnedXp,
                Mab_BattleBonusXp = bonusXp,
                Mab_UpdatedPlayerXp = mabCampaignDB.Mab_PlayerExperience,
                Mab_PlayerState = finalPlayerState
            },
            "MAB BATTLE finished successfully! Results were successfully computed for the campaign");
        }
        private static (bool, string) MabFinishBattle_Validation(UsersMabFinishBattleRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersMabListNpcPlayedCardsResponse>?, string)> MabListNpcPlayedCards(UsersMabListNpcPlayedCardsRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabListNpcPlayedCards_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }    
            
            var onGoingMabBattleDB = await this._daoDbContext
                .MabBattles
                .AsNoTracking()
                .AsSplitQuery()
                .Include(a => a.Mab_Npc)
                .ThenInclude(a => a.Mab_NpcCards)
                .ThenInclude(a => a.Mab_Card)
                .Include(a => 
                    a.Mab_Duels!
                    .Where(b => 
                        b.Mab_NpcCardId != null))
                .FirstOrDefaultAsync(b => 
                    b.Mab_Campaign.Mab_IsCampaignDeleted == false &&
                    b.Mab_Campaign.UserId == userId &&
                    b.Mab_IsBattleFinished == false);

            if(onGoingMabBattleDB == null)
            {
                return (null, "Error: MabListNpcPlayedCards failed! NPC PLAYED CARDS were not found!");
            }

            var playedDuelsDB = onGoingMabBattleDB.Mab_Duels!;           
        
            var npcPlayedCardsDB = playedDuelsDB
                .Select(a => a.Mab_NpcCardId)
                .ToList();

            var cardsDB = onGoingMabBattleDB
                .Mab_Npc.Mab_NpcCards
                .Where(a => npcPlayedCardsDB
                    .Any(b => b == a.Id))
                .Select(a => a.Mab_Card)
                .ToList();

            var content = cardsDB
                .Select(a => new UsersMabListNpcPlayedCardsResponse
                {
                    Mab_BattleId = playedDuelsDB
                        .Where(b => a.Mab_NpcCards!.Any(c => c.Id == b.Mab_NpcCardId))
                        .Select(b => b.Id)
                        .FirstOrDefault(),

                    Mab_CardCode = a.Mab_CardCode,

                    Mab_NpcCardId = a.Mab_NpcCards!
                        .Where(b => b.Mab_CardId == a.Id)
                        .Select(b => b.Id)
                        .FirstOrDefault(),

                    Mab_CardName = a.Mab_CardName,
                    Mab_CardLevel = a.Mab_CardLevel,
                    Mab_CardPower = a.Mab_CardPower,
                    Mab_CardUpperHand = a.Mab_CardUpperHand,
                    Mab_CardType = a.Mab_CardType,

                    Mab_CardFullPower = playedDuelsDB
                        .Where(b => a.Mab_NpcCards!.Any(c => c.Id == b.Mab_NpcCardId))
                        .Select(b => b.Mab_NpcCardFullPower)
                        .FirstOrDefault()
                })
                .ToList();

            content = content.OrderBy(a => a.Mab_BattleId).ToList();

            return (content, "NPC played cards fetched successfully!");
        }
        private static (bool, string) MabListNpcPlayedCards_Validation(UsersMabListNpcPlayedCardsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabListNpcPlayedCards failed! Request is NOT null!, however it MUST be null");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersMabListPlayerDuellingCardsResponse>?, string)> MabListPlayerDuellingCards(UsersMabListPlayerDuellingCardsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabListAssignedCards failed! User is not authenticated");
            }

            var (isValid, message) = ListMabListAssignedCards_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }


            var mabActiveCardsDB = await this._daoDbContext
                .MabAssignedCards
                .AsNoTracking()
                .Where(assignedCards => assignedCards.Mab_Deck!.Mab_IsDeckActive == true)
                .Select(assignedCards => new
                {
                    Mab_CardCode = assignedCards.Mab_PlayerCard!.Mab_Card!.Mab_CardCode,
                    Mab_PlayerCardId = assignedCards.Mab_PlayerCardId,
                    Mab_CardName = assignedCards.Mab_PlayerCard!.Mab_Card!.Mab_CardName,
                    Mab_CardType = assignedCards.Mab_PlayerCard.Mab_Card.Mab_CardType,
                    Mab_CardLevel = assignedCards.Mab_PlayerCard.Mab_Card.Mab_CardLevel,
                    Mab_CardPower = assignedCards.Mab_PlayerCard.Mab_Card.Mab_CardPower,
                    Mab_CardUpperHand = assignedCards.Mab_PlayerCard.Mab_Card.Mab_CardUpperHand,
                })
                .ToListAsync();

            if(mabActiveCardsDB == null || mabActiveCardsDB.Count < Constants.DeckSize)
            {
                return (null, "Error: MabListPlayerDuellingCards failed! Mab assigned active cards not found!");
            }        

            var mabDuelsDB = await this._daoDbContext
                .MabDuels
                .AsNoTracking()
                .Where(duel =>
                    duel.Mab_Battle!.Mab_Campaign.Mab_IsCampaignDeleted == false &&
                    duel.Mab_Battle.Mab_Campaign.UserId == userId &&
                    duel.Mab_Battle.Id == request!.Mab_BattleId)
                .ToListAsync();

            var response = mabActiveCardsDB.Select(activeCard => new UsersMabListPlayerDuellingCardsResponse
            {
                Mab_CardCode = activeCard.Mab_CardCode,
                Mab_PlayerCardId = activeCard.Mab_PlayerCardId,
                Mab_CardName = activeCard.Mab_CardName,
                Mab_CardType = activeCard.Mab_CardType,
                Mab_CardLevel = activeCard.Mab_CardLevel,
                Mab_CardPower = activeCard.Mab_CardPower,
                Mab_CardUpperHand = activeCard.Mab_CardUpperHand
            })
            .ToList();
            
            foreach(var card in response)
            {
                if(mabDuelsDB != null && mabDuelsDB.Any(duel => duel.Mab_PlayerCardId == card.Mab_PlayerCardId))
                {
                    var duel = mabDuelsDB.FirstOrDefault(duel => duel.Mab_PlayerCardId == card.Mab_PlayerCardId);

                    card.Mab_CardFullPower = duel!.Mab_PlayerCardFullPower;
                    card.Mab_DuelId = duel.Id;
                    card.Mab_DuelPoints = duel.Mab_DuelPoints;
                    card.Mab_DuelEarnedXp = duel.Mab_EarnedXp;
                    card.Mab_DuelBonusXp = duel.Mab_BonusXp;
                }
            }

            response = response
                .OrderBy(a => a.Mab_DuelId == null)
                .ThenBy(a => a.Mab_DuelId)
                .ToList();

            return (response,
                "Mab Player Assigned Cards fetched successfully!");
        }
        private static (bool, string) ListMabListAssignedCards_Validation(UsersMabListPlayerDuellingCardsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabListAssignedCards failed! Request is null!");
            }  
            
            if(request.Mab_BattleId.HasValue == false || request.Mab_BattleId < 1)
            {
                return (false, "Error: MabListAssignedCards failed! request.Mab_BattleId is null or invalid!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabManageTurnResponse?, string)> MabManageTurn(UsersMabManageTurnRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabManageTurn_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabDuelsDB = await this._daoDbContext
                .MabDuels
                .Where(a =>
                    a.Mab_Battle!.Mab_Campaign.Mab_IsCampaignDeleted == false &&
                    a.Mab_Battle.Mab_Campaign.UserId == userId)
                .ToListAsync();

            if (mabDuelsDB == null || mabDuelsDB.Count < 1)
            {
                return (null, "Error: No valid MAB DUELS found while running duel manager!");
            }

            var currentMabDuelDB = mabDuelsDB.OrderByDescending(a => a.Id).FirstOrDefault();

            if (currentMabDuelDB == null)           
            {
                return (null, "Error: No valid CURRENT MAB DUEL found while running duel manager");
            }

            if(currentMabDuelDB.Mab_PlayerCardId != null && currentMabDuelDB.Mab_NpcCardId != null)
            {
                return (new(), "Mab Turn Manager runned successfully! Nothing was done this duel is over.");
            }

            if (currentMabDuelDB.Mab_PlayerCardId != null) 
            {
                currentMabDuelDB.Mab_IsPlayerAttacking = false;
            }
            else
            {
                currentMabDuelDB.Mab_IsPlayerAttacking = true;
            }

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabManageTurnResponse(), "Mab Turn Manager runned successfully!");
        }
        private static (bool, string) MabManageTurn_Validation(UsersMabManageTurnRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }
            return (true, string.Empty);
        }


        public async Task<(UsersMabCheckDuelStatusResponse?, string)> MabCheckDuelStatus(UsersMabCheckDuelStatusRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabGetWhoGoesNext_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabDuelsDB = await this._daoDbContext
                .MabDuels
                .Where(a =>
                    a.Mab_Battle!.Mab_Campaign.Mab_IsCampaignDeleted == false &&
                    a.Mab_Battle.Mab_Campaign.UserId == userId &&
                    a.Mab_Battle.Mab_Campaign.Mab_Battles.OrderByDescending(b => b.Id).Select(b => b.Id).FirstOrDefault() == a.Mab_BattleId)
                .ToListAsync();                

            if(mabDuelsDB == null || mabDuelsDB.Count < 1)
            {
                return (null, "Error: No valid MAB DUELS found while checking current duel status!");
            }

            var currentMabDuelDB = mabDuelsDB.OrderByDescending(a => a.Id).FirstOrDefault();

            if(currentMabDuelDB == null)
            {
                return (null, "Error: No valid CURRENT MAB DUEL found while checking current duel status");
            }

            var mabDuelsCount = mabDuelsDB.Count;            

            var isMabPlayerTurn = currentMabDuelDB.Mab_IsPlayerAttacking;

            var areMabTurnsFinished =
                currentMabDuelDB.Mab_PlayerCardId != null &&
                currentMabDuelDB.Mab_NpcCardId != null;

            var isMabDuelResolved =
                currentMabDuelDB.Mab_PlayerCardId != null &&
                currentMabDuelDB.Mab_NpcCardId != null &&
                currentMabDuelDB.Mab_PlayerCardFullPower != null &&
                currentMabDuelDB.Mab_NpcCardFullPower != null;


            var isMabBattleFinished =
                mabDuelsCount >= Constants.DeckSize &&
                mabDuelsDB.All(a =>
                    a.Mab_PlayerCardId != null &&
                    a.Mab_NpcCardId != null &&
                    a.Mab_PlayerCardFullPower != null &&
                    a.Mab_NpcCardFullPower != null);

            return (new UsersMabCheckDuelStatusResponse
            {
                Mab_DuelsCount = mabDuelsCount,
                Mab_IsPlayerTurn = isMabPlayerTurn,
                Mab_AreTurnsFinished = areMabTurnsFinished,
                Mab_IsDuelResolved = isMabDuelResolved,
                Mab_IsBattleFinished = isMabBattleFinished,
            }, "Successfully checked who goes next.");
        }
        private static (bool, string) MabGetWhoGoesNext_Validation(UsersMabCheckDuelStatusRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }
            return (true, string.Empty);
        }


        public async Task<(UsersMabStartDuelResponse?, string)> MabStartDuel(UsersMabStartDuelRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabStartDuel failed! User is not authenticated");
            }

            var (isValid, message) = MabStartDuel_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabBattleDB = await this._daoDbContext
                .MabBattles
                .Include(a => a.Mab_Duels)
                .FirstOrDefaultAsync(a =>
                    a.Mab_Campaign.Mab_IsCampaignDeleted == false &&
                    a.Mab_Campaign.UserId == userId &&
                    a.Mab_IsBattleFinished == false
                );

            if(mabBattleDB == null)
            {
                return (null, "Error: MabStartDuel failed! A valid mab Battle could not be found while starting a new mab DUEL!");
            }

            var mabDuelsDB = mabBattleDB.Mab_Duels;

            var anyUnfinishedDuel = mabDuelsDB!.Any(a =>
                a.Mab_DuelPoints == null ||
                a.Mab_EarnedXp == null ||
                a.Mab_BonusXp == null ||
                a.Mab_PlayerCardId == null ||
                a.Mab_PlayerCardFullPower == null ||
                a.Mab_NpcCardId == null ||
                a.Mab_NpcCardFullPower == null ||               
                a.Mab_PlayerState == null);
              
            if(anyUnfinishedDuel == true)
            {
                return (null, "Error: MabStartDuel failed! Any ongoing mab duel must be finished befored starting a new one!");
            }

            var mabDuelsCount = mabDuelsDB!.Count ;

            if(mabDuelsCount >= Constants.DeckSize)
            {
                return (null, "Error: MabStartDuel failed! All due mab duels have been already started!");
            }

            var wasPlayerFirstAttacker = mabBattleDB.Mab_DoesPlayerGoesFirst;

            var isPlayerAttacking = Helper
                .MabIsPlayerAttacking(wasPlayerFirstAttacker!.Value, mabDuelsCount + 1);               

            mabBattleDB.Mab_Duels!.Add(new MabDuel
            {
                Mab_IsPlayerAttacking = isPlayerAttacking,                
            });
           
            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabStartDuelResponse
            {
                Mab_IsPlayerAttacking = isPlayerAttacking,               

            }, "New Mab Duel started successfully");
        }
        public static (bool, string) MabStartDuel_Validation(UsersMabStartDuelRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }           

            return (true, string.Empty);
        }


        public async Task<(UsersMabResolveDuelResponse?, string)> MabResolveDuel(UsersMabResolveDuelRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabResolveDuel failed! User is not authenticated");
            }

            var (isValid, message) = MabMabResolveDuel_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }
          
            var mabCampaignDB = await this._daoDbContext
               .MabCampaigns
               .AsSplitQuery()
               .Include(campaign => campaign.Mab_Battles!
                    .Where(battle => battle.Mab_IsBattleFinished == false))
                    .ThenInclude(battle => battle.Mab_Duels!)
               .Include(campaign => campaign.Mab_PlayerCards!
                    .Where(playerCard =>
                        playerCard.Mab_AssignedCards!.Any() &&
                        playerCard.Mab_AssignedCards!
                            .Any(assignedCard => assignedCard.Mab_PlayerCardId == assignedCard.Id)))
                    .ThenInclude(campaign => campaign.Mab_Card)                   
               .Include(campaign => campaign.Mab_Battles!)
                    .ThenInclude(battle => battle.Mab_Npc)
                        .ThenInclude(npc => npc.Mab_NpcCards)
                            .ThenInclude(npcCard => npcCard.Mab_Card)
               .FirstOrDefaultAsync(campaign =>
                    campaign.Mab_IsCampaignDeleted == false &&
                    campaign.UserId == userId);         

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabResolveDuel failed! A valid mab campaign could not be found!");
            }

            var mabBattleDB = mabCampaignDB
                .Mab_Battles!
                .FirstOrDefault(a => a.Mab_IsBattleFinished == false);
            if (mabBattleDB == null)
            {
                return (null, "Error: MabResolveDuel failed! No ongoing valid mab battle could not be found!");
            }

            var mabDuelsDB = mabBattleDB.Mab_Duels;
            if(mabDuelsDB == null)
            {
                return (null, "Error: MabResolveDuel failed! No valid mab duels found!");
            }

            var finishedMabDuelDB = mabDuelsDB                
                .FirstOrDefault(a => a.Mab_DuelPoints == null);

            if(finishedMabDuelDB == null)
            {
                return (null, "Error: MabResolveDuel failed! No mab duel not yet resolved was found!");
            }

            var mabNpcDB = mabBattleDB.Mab_Npc;
            
            var mabNpcCards = mabNpcDB.Mab_NpcCards;

            var mabCards_ = mabNpcCards.Select(a => a.Mab_Card!).ToList();

            var mabNpcDuellingCard =
                mabCards_.FirstOrDefault(a => 
                    a.Mab_NpcCards != null && 
                    a.Mab_NpcCards!
                    .Any(b => b.Id == finishedMabDuelDB.Mab_NpcCardId));

            if (finishedMabDuelDB.Mab_PlayerCardId == 0)
            {
                finishedMabDuelDB.Mab_DuelPoints = - mabNpcDuellingCard!.Mab_CardPower;

                finishedMabDuelDB.Mab_PlayerCardFullPower = 0;

                finishedMabDuelDB.Mab_NpcCardFullPower = mabNpcDuellingCard.Mab_CardPower;

                finishedMabDuelDB.Mab_PlayerState = MabPlayerState.Normal;

                finishedMabDuelDB.Mab_EarnedXp = 0;

                finishedMabDuelDB.Mab_BonusXp = 0;

                await this._daoDbContext.SaveChangesAsync();

                return (new UsersMabResolveDuelResponse
                {
                    Mab_PlayerCardFullPower = 0,
                    Mab_NpcCardFullPower = mabNpcDuellingCard.Mab_CardPower,
                    Mab_DuelPoints = -mabNpcDuellingCard.Mab_CardPower,
                    Mab_EarnedXp = 0,
                    Mab_BonusXp = 0,
                    Mab_HasPlayerWon = false                  
                }, "PLAYER PASSED! Mab Duel Resolved Successfully");
            }

            var mabPlayerCardsDB = mabCampaignDB.Mab_PlayerCards;

            var mabCardsDB = mabPlayerCardsDB!
                .Select(a => a.Mab_Card!)
                .ToList();            
            
            mabCardsDB.AddRange(mabCards_);
            
            var mabPlayerDuellingCard = mabCardsDB
                    .FirstOrDefault(a => 
                        a.Mab_PlayerCards != null && 
                        a.Mab_PlayerCards!
                            .Any(b => b.Id == finishedMabDuelDB.Mab_PlayerCardId));            

            var isPlayerAttacking = finishedMabDuelDB.Mab_IsPlayerAttacking;

            var (mabAttackerCardFullPower, mabDuelPoints) = (0, 0);

            var mabPlayerCardFullPower = 0;
            var mabNpcCardFullPower = 0;

            var isDuelNumberOdd = mabBattleDB.Mab_Duels.Count == 1 || 
                mabBattleDB.Mab_Duels.Count == 3 || 
                mabBattleDB.Mab_Duels.Count == 5;
           
            for(int i = 1; i <= mabBattleDB.Mab_Duels.Count; i++)
            {
                if(i % 2 == 0)
                {
                    isDuelNumberOdd = false;
                }
                else
                {
                    isDuelNumberOdd = true;
                }
            }

            if (mabBattleDB.Mab_DoesPlayerGoesFirst == true && isDuelNumberOdd == true)
            {
                var attackerCardPower = mabPlayerDuellingCard.Mab_CardPower;
                var attackerCardUpperHand = mabPlayerDuellingCard.Mab_CardUpperHand;
                var attackerCardType = mabPlayerDuellingCard.Mab_CardType;

                var defenderCardPower = mabNpcDuellingCard.Mab_CardPower;
                var defenderCardUpperHand = mabNpcDuellingCard.Mab_CardUpperHand;
                var defenderCardType = mabNpcDuellingCard.Mab_CardType;

                (mabAttackerCardFullPower, mabDuelPoints) = Helper.MabResolveDuel(
                    attackerCardPower,
                    attackerCardUpperHand,
                    attackerCardType,
                    defenderCardPower,
                    defenderCardUpperHand,
                    defenderCardType,
                    true);

                mabPlayerCardFullPower = mabAttackerCardFullPower;
                mabNpcCardFullPower = defenderCardPower;
            }
            else
            {
                var attackerCardPower = mabNpcDuellingCard.Mab_CardPower;
                var attackerCardUpperHand = mabNpcDuellingCard.Mab_CardUpperHand;
                var attackerCardType = mabNpcDuellingCard.Mab_CardType;

                var defenderCardPower = mabPlayerDuellingCard.Mab_CardPower;
                var defenderCardUpperHand = mabPlayerDuellingCard.Mab_CardUpperHand;
                var defenderCardType = mabPlayerDuellingCard.Mab_CardType;

                (mabAttackerCardFullPower, mabDuelPoints) = Helper.MabResolveDuel(
                    attackerCardPower,
                    attackerCardUpperHand,
                    attackerCardType,
                    defenderCardPower,
                    defenderCardUpperHand,
                    defenderCardType,
                    false);

                mabNpcCardFullPower = mabAttackerCardFullPower;
                mabPlayerCardFullPower = defenderCardPower;
            }

            var orderedMabPlayerCardIds = mabDuelsDB.OrderBy(a => a.Id).Select(a => a.Mab_PlayerCardId).ToList();
            var duelsCount = orderedMabPlayerCardIds.Count();

            List <bool> orderedMabDuelResults = mabDuelsDB.OrderBy(a => a.Id).Select(a => a.Mab_DuelPoints > 0).ToList();
            orderedMabDuelResults[duelsCount - 1] = mabDuelPoints > 0;

            var isMabBattleOvercome = mabBattleDB.Mab_HasPlayerWon;

            var mabPlayerState = Helper.MabGetPlayerState(orderedMabPlayerCardIds, orderedMabDuelResults, isMabBattleOvercome);

            var (earnedXp, bonusXp) = Helper.MabGetEarnedXp(
                mabCampaignDB.Mab_PlayerLevel!.Value, mabNpcDB.Mab_NpcLevel, mabDuelPoints, (int)mabPlayerState);

            finishedMabDuelDB.Mab_DuelPoints = mabDuelPoints;

            finishedMabDuelDB.Mab_PlayerCardFullPower = mabPlayerCardFullPower;

            finishedMabDuelDB.Mab_NpcCardFullPower = mabNpcCardFullPower;

            finishedMabDuelDB.Mab_PlayerState = mabPlayerState;

            finishedMabDuelDB.Mab_EarnedXp = earnedXp;

            finishedMabDuelDB.Mab_BonusXp = bonusXp;

            finishedMabDuelDB.Mab_PlayerState = mabPlayerState;

            finishedMabDuelDB.IsFinished = true;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabResolveDuelResponse
            {
                Mab_PlayerCardFullPower = mabPlayerCardFullPower,
                Mab_NpcCardFullPower = mabNpcCardFullPower,
                Mab_DuelPoints = mabDuelPoints,
                Mab_EarnedXp = earnedXp,
                Mab_BonusXp = bonusXp,
                Mab_HasPlayerWon = mabDuelPoints > 0,
                Mab_PlayerState = mabPlayerState
            }, "Mab Duel resolved successfully!");
        }
        public static (bool, string) MabMabResolveDuel_Validation(UsersMabResolveDuelRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabPlayerAttacksResponse?, string)> MabPlayerAttacks(UsersMabPlayerAttacksRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabPlayerAttacks failed! User is not authenticated");
            }

            var (isValid, message) = MabPlayerAttacks_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaingDB = await this._daoDbContext
                .MabCampaigns     
                .AsSplitQuery()
                .Include(campaign => campaign.Mab_Battles!
                    .Where(battle => battle.Mab_IsBattleFinished == false))
                    .ThenInclude(battle => battle.Mab_Duels!)
                .Include(campaign => campaign.Mab_PlayerCards!
                    .Where(playerCard =>
                        playerCard.Id == request!.Mab_PlayerCardId &&
                        playerCard.Mab_Card!.Mab_IsCardDeleted == false))
                    .ThenInclude(playerCard => playerCard.Mab_Card)
                .Include(campaign => campaign.Mab_PlayerCards!
                    .Where(playerCard =>
                        playerCard.Id == request!.Mab_PlayerCardId &&
                        playerCard.Mab_Card!.Mab_IsCardDeleted == false))
                .ThenInclude(playerCard => playerCard.Mab_AssignedCards)
                .FirstOrDefaultAsync(campaign =>
                    campaign.Mab_IsCampaignDeleted == false &&
                    campaign.UserId == userId);

            if(mabCampaingDB == null)
            {
                return (null, "Error: MabPlayerAttacks failed! No valid mab CAMPAIGN was found");
            }

            var mabBattlesDB = mabCampaingDB.Mab_Battles;
            if (mabBattlesDB == null || mabBattlesDB.Count < 1)
            {
                return (null, "Error: MabPlayerAttacks failed! No valid mab BATLES were found");
            }

            var currentMabBattleDB = mabBattlesDB.FirstOrDefault(a => a.Mab_IsBattleFinished == false);
            if (currentMabBattleDB == null)
            {
                return (null, "Error: MabPlayerAttacks failed! No valid current mab BATTLE was found");
            }

            var mabDuelsDB = currentMabBattleDB.Mab_Duels;
            if (mabDuelsDB == null || mabDuelsDB.Count < 1)
            {
                return (null, "Error: MabPlayerAttacks failed! No valid mab Duels were found");
            }          
           
            var onGoingMabDuel = mabDuelsDB.FirstOrDefault(a => a.IsFinished == false);
            if (onGoingMabDuel == null)
            {
                return (null, "Error: MabPlayerAttacks failed! No valid mab DUEL was be found");
            }    
            
            if (onGoingMabDuel.Mab_IsPlayerAttacking == false)
            {
                return (null, "Error: MabPlayerAttacks failed! This is not the PLAYER's turn!");
            }

            var mabPlayerCardsDB = mabCampaingDB.Mab_PlayerCards;
            if (mabPlayerCardsDB == null || mabPlayerCardsDB.Count < 1)
            {
                return (null, "Error: MabPlayerAttacks failed! No valid mab Player Cards were found");
            }

            var mabAssignedCards = mabPlayerCardsDB.Select(a => a.Mab_AssignedCards).FirstOrDefault();
            if (mabAssignedCards == null || mabAssignedCards.Count < 1)
            {
                return (null, "Error: MabPlayerAttacks failed! No valid mab Player Assigned Cards were found");
            }

            var mabCardDB = mabPlayerCardsDB.Where(a => a.Id == request!.Mab_PlayerCardId).Select(a => a.Mab_Card).FirstOrDefault();
            if(mabCardDB == null)
            {
                return (null, "Error: MabPlayerAttacks failed! Requested mab Card is invalid or was deleted!");
            }

            var isMabPlayerCardValid = mabAssignedCards
               .Any(a => a.Mab_PlayerCardId == request!.Mab_PlayerCardId);
            if (isMabPlayerCardValid == false)
            {
                return (null, "Error: MabPlayerAttacks failed! Mab Player Card is invalid or not assigned to the active deck!");
            }

            var isMabPlayerCardAvailable = mabDuelsDB.Any(a => a.Mab_PlayerCardId == request!.Mab_PlayerCardId);
            if (isMabPlayerCardAvailable == true)
            {
                return (null, "Error: MabPlayerAttacks failed! MabPlayerCard has already been used in this battle!");
            }          

            onGoingMabDuel.Mab_PlayerCardId = request!.Mab_PlayerCardId;
            onGoingMabDuel.Mab_PlayerCardName = mabCardDB.Mab_CardName;
            onGoingMabDuel.Mab_PlayerCardType = mabCardDB.Mab_CardType;
            onGoingMabDuel.Mab_PlayerCardLevel = mabCardDB.Mab_CardLevel;
            onGoingMabDuel.Mab_PlayerCardPower = mabCardDB.Mab_CardPower;
            onGoingMabDuel.Mab_PlayerCardUpperHand = mabCardDB.Mab_CardUpperHand;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabPlayerAttacksResponse(), "Mab Player has finished his turn successfully!");
        }
        public static (bool, string) MabPlayerAttacks_Validation(UsersMabPlayerAttacksRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null, however it MUST be null!");
            }

            if (request.Mab_PlayerCardId.HasValue == false || request.Mab_PlayerCardId < 1)
            {
                return (false, "Error: Mab_PlayerCardId.HasValue is missing or invalid!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabPlayerRetreatsResponse?, string)> MabPlayerRetreats(UsersMabPlayerRetreatsRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabPlayerRetreats_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns!
                .Include(a => a.Mab_Battles!)
                .ThenInclude(b => b.Mab_Npc)
                .Include(a => a.Mab_Battles!)
                .ThenInclude(c => c.Mab_Duels)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Mab_IsCampaignDeleted == false);

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabPlayerRetreats failed ! Requested mab campaign could not be found!");
            }

            var mabBattlesDB = mabCampaignDB.Mab_Battles;
            if (mabBattlesDB == null || mabBattlesDB.Count < 1)
            {
                return (null, "Error: MabPlayerRetreats failed! MAB BATTLES could be found in this campaign!");
            }

            var currentMabBattle =
                mabBattlesDB
                .OrderByDescending(a => a.Id)
                .FirstOrDefault(a => a.Mab_IsBattleFinished == false);
            if (currentMabBattle == null)
            {
                return (null, "Error: MabPlayerRetreats failed! No ongoing unfinished MAB BATTLE could be found!");
            }

            var mabDuelsDB = currentMabBattle.Mab_Duels;
            if (mabDuelsDB == null)
            {
                return (null, "Error: MabPlayerRetreats failed! Mab Duels not found!");
            }

            var earnedGold = Constants.RetreatCoinsPenalty;
            var earnedXp = 0;
            var bonusXp = 0;
            var finalPlayerState = MabPlayerState.Panicking;
            var hasPlayerWon = false;

            currentMabBattle.Mab_EarnedGold = earnedGold;
            currentMabBattle.Mab_EarnedXp = earnedXp;
            currentMabBattle.Mab_BonusXp = bonusXp;
            currentMabBattle.Mab_FinalPlayerState = finalPlayerState;
            currentMabBattle.Mab_HasPlayerWon = hasPlayerWon;
            currentMabBattle.Mab_IsBattleFinished = true;

            mabCampaignDB.Mab_CoinsStash += earnedGold;
            mabCampaignDB.Mab_PlayerExperience += earnedXp;
            mabCampaignDB.Mab_BattlesCount++;
            mabCampaignDB.Mab_BattleDefeatsCount++;         

            await _daoDbContext.SaveChangesAsync();

            return (new UsersMabPlayerRetreatsResponse
            {
                Mab_UpdatedGoldStash = mabCampaignDB.Mab_CoinsStash,

                Mab_UpdatedPlayerXp = mabCampaignDB.Mab_PlayerExperience
            },
            "MAB PLAYER retreated from the battle successfully! Results were successfully computed for the campaign");
        }
        private static (bool, string) MabPlayerRetreats_Validation(UsersMabPlayerRetreatsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabNpcAttacksResponse?, string)> MabNpcAttacks(UsersMabNpcAttacksRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabNpcAttacks failed! User is not authenticated");
            }

            var (isValid, message) = MabNpcAttcks_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            MabNpcCard? npcChosenCard = null;        

            var mabBattleDB = await this._daoDbContext
                .MabBattles
                .Include(battle => battle.Mab_Duels!)               
                .Include(battle => battle.Mab_Npc)
                    .ThenInclude(npc => npc.Mab_NpcCards)
                        .ThenInclude(npcCard => npcCard.Mab_Card)                           
                .OrderByDescending(battle => battle.Id)
                .FirstOrDefaultAsync(battle => 
                    battle.Mab_IsBattleFinished == false && 
                    battle.Mab_Campaign!.UserId == userId); 
            
            if (mabBattleDB == null)
            {
                return (null, "Error: MabNpcAttacks failed! No mab battle found for this campaign!");
            }

            var mabDuelDB = mabBattleDB.Mab_Duels!.FirstOrDefault(a => a.Mab_NpcCardId == null);
            if (mabDuelDB == null)
            {
                return (null, "Error: MabNpcAttacks failed! No mab duel found during npc's turn!");
            }
            if (mabDuelDB.Mab_PlayerCardId == null &&
                mabDuelDB.Mab_NpcCardId == null &&
                mabDuelDB.Mab_IsPlayerAttacking == true)
            {
                return (null, "Error: MabNpcAttacks failed! This is not the Npc's turn!");
            }
            var playerCardPower = mabDuelDB.Mab_PlayerCardPower;
            var playerCardUpperHand = mabDuelDB.Mab_PlayerCardUpperHand;
            var playerCardType = mabDuelDB.Mab_PlayerCardType;

            var mabNpcDB = mabBattleDB.Mab_Npc;
            if (mabNpcDB == null)
            {
                return (null, "Error: MabNpcAttacks failed! No mab NPC found for this battle!");
            }

            var mabNpcCardsDB = mabNpcDB.Mab_NpcCards;
            if (mabNpcCardsDB == null || mabNpcCardsDB.Count < Constants.DeckSize)
            {
                return (null, "Error: MabNpcAttacks failed! NPC cards missing or insufficient!");
            }

            // Npc UNAVAILABLE cards
            var usedNpcDeckEntryIds = mabBattleDB
                .Mab_Duels!
                .Where(a => a.Mab_NpcCardId != null)
                .Select(a => a.Mab_NpcCardId!.Value)
                .ToList();

            // Npc AVAILABLE cards
            var availableNpcDeckEntries = mabNpcCardsDB
                .Where(npc => usedNpcDeckEntryIds.Contains(npc.Id) == false)
                .ToList();

            // NPC ATTACKING CASE
            if (mabDuelDB.Mab_PlayerCardId == null)
            {
                npcChosenCard = GetNpcAttackingCard(availableNpcDeckEntries);
            }
            // NPC DEFENDING CASE
            else 
            {
                npcChosenCard = GetNpcDefendingCard(availableNpcDeckEntries, playerCardPower, playerCardUpperHand, playerCardType);
            }         
            
            mabDuelDB.Mab_NpcCardId = npcChosenCard.Id;
            mabDuelDB.Mab_NpcCardName = npcChosenCard.Mab_Card.Mab_CardName;
            mabDuelDB.Mab_NpcCardType = npcChosenCard.Mab_Card.Mab_CardType;
            mabDuelDB.Mab_NpcCardLevel = npcChosenCard.Mab_Card.Mab_CardLevel;
            mabDuelDB.Mab_NpcCardPower = npcChosenCard.Mab_Card.Mab_CardPower;
            mabDuelDB.Mab_NpcCardUpperHand = npcChosenCard.Mab_Card.Mab_CardUpperHand;            

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabNpcAttacksResponse
            {
                Mab_CardId = npcChosenCard.Mab_Card.Id,
                Mab_NpcCardId = npcChosenCard.Id,
                //apagar os 2 acima...
                
                Mab_CardName = npcChosenCard.Mab_Card.Mab_CardName,
                Mab_CardCode = npcChosenCard.Mab_Card.Mab_CardCode,
                Mab_CardLevel = npcChosenCard.Mab_Card.Mab_CardLevel,
                Mab_CardPower = npcChosenCard.Mab_Card.Mab_CardPower,
                Mab_CardUpperHand = npcChosenCard.Mab_Card.Mab_CardUpperHand,
                Mab_CardType = npcChosenCard.Mab_Card.Mab_CardType
            }, "Mab Npc has finished his turn successfully!");
        }
        private static (bool, string) MabNpcAttcks_Validation(UsersMabNpcAttacksRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

           
            return (true, string.Empty);
        }
        private static MabNpcCard? GetNpcAttackingCard(List<MabNpcCard>? npcAvailableCards)
        {       
            var npcUselessCard = npcAvailableCards
                .FirstOrDefault(npcCard =>
                    npcCard.Mab_Card.Mab_CardType != MabCardType.Neutral &&
                    npcCard.Mab_Card.Mab_CardPower == 0 &&
                    npcCard.Mab_Card.Mab_CardUpperHand == 0);

            var npcWeakestCard_Neutral = npcAvailableCards
                .Where(npcCard =>
                        npcCard.Mab_Card.Mab_CardType == MabCardType.Neutral &&
                        (npcCard.Mab_Card.Mab_CardPower != 0 || npcCard.Mab_Card.Mab_CardUpperHand != 0))
                .OrderBy(npcCard => npcCard.Mab_Card.Mab_CardPower)
                .ThenBy(npcCard => npcCard.Mab_Card.Mab_CardUpperHand)
                .FirstOrDefault();
       
            //consertar erro aqui! 01.10.2025
            var weakestNpcCard = npcAvailableCards
                .Where(npcCard =>
                    npcCard.Mab_Card.Mab_CardType != MabCardType.Neutral &&
                    (npcCard.Mab_Card.Mab_CardPower > 0 || npcCard.Mab_Card.Mab_CardUpperHand > 0))
                .OrderBy(npcCard => npcCard.Mab_Card.Mab_CardPower)
                .ThenBy(npcCard => npcCard.Mab_Card.Mab_CardUpperHand)
                .FirstOrDefault();

            var npcTruceCard = npcAvailableCards
                .Where(npcCard =>
                        npcCard.Mab_Card.Mab_CardType == MabCardType.Neutral &&
                        npcCard.Mab_Card.Mab_CardPower == 0 &&
                        npcCard.Mab_Card.Mab_CardUpperHand == 0)
                .OrderBy(npcCard => npcCard.Mab_Card.Mab_CardPower)
                .ThenBy(npcCard => npcCard.Mab_Card.Mab_CardUpperHand)
                .FirstOrDefault();

            if(npcUselessCard != null)
            {
                return npcUselessCard;
            }   
            else if (npcWeakestCard_Neutral != null && npcWeakestCard_Neutral?.Mab_Card.Mab_CardPower <= weakestNpcCard?.Mab_Card.Mab_CardPower)
            {
                return npcWeakestCard_Neutral;
            }
            else if (weakestNpcCard != null)
            {
                return weakestNpcCard;
            }
            else
            {
                return npcTruceCard;
            }
        }
        private static MabNpcCard? GetNpcDefendingCard(List<MabNpcCard>? npcAvailableCards, int? playerCardPower, int? playerCardUpperHand, MabCardType? playerCardType)
        {
            var possibleResults = new List<int?>();            

            for (int i = 0; i < npcAvailableCards.Count; i++)
            {
                var defendingCard = npcAvailableCards[i].Mab_Card;

                var (_, duelPoints) = Helper.MabResolveDuel(
                    playerCardPower!.Value,
                    playerCardUpperHand!.Value,
                    playerCardType,
                    defendingCard.Mab_CardPower,
                    defendingCard.Mab_CardUpperHand,
                    defendingCard.Mab_CardType,
                    true);

                possibleResults.Add(duelPoints);
            }

            var index = possibleResults
                .IndexOf(possibleResults
                    .Where(duelPoints => duelPoints > 0)
                .Max());

            if (index >= 0)
            {
                return npcAvailableCards[index];
            }
            else
            {
                var npcUselessCard = npcAvailableCards
                .FirstOrDefault(npcCard =>
                    npcCard.Mab_Card.Mab_CardType != MabCardType.Neutral &&
                    npcCard.Mab_Card.Mab_CardPower == 0 &&
                    npcCard.Mab_Card.Mab_CardUpperHand == 0);    

                var npcTruceCard = npcAvailableCards
                    .Where(npcCard =>
                        npcCard.Mab_Card.Mab_CardType == MabCardType.Neutral &&
                        npcCard.Mab_Card.Mab_CardPower == 0 &&
                        npcCard.Mab_Card.Mab_CardUpperHand == 0)
                    .OrderBy(npcCard => npcCard.Mab_Card.Mab_CardPower)
                    .ThenBy(npcCard => npcCard.Mab_Card.Mab_CardUpperHand)
                    .FirstOrDefault();

                var weakestNpcCard = npcAvailableCards
                    .Where(npcCard =>
                        npcCard.Mab_Card.Mab_CardPower != 0 || npcCard.Mab_Card.Mab_CardUpperHand != 0)
                    .OrderBy(npcCard => npcCard.Mab_Card.Mab_CardPower)
                    .ThenBy(npcCard => npcCard.Mab_Card.Mab_CardUpperHand)
                    .FirstOrDefault();

                if (npcTruceCard != null)
                {
                    return npcTruceCard;
                }
                else if (npcUselessCard != null)
                {
                    return npcUselessCard;
                }
                else if(weakestNpcCard != null)
                {
                    return weakestNpcCard;
                }
                else
                {
                    return null;
                }
            }

        
        }

        public async Task<(UsersMabGetNpcCardFullPowerResponse?, string)> MabGetNpcCardFullPower(UsersMabGetNpcCardFullPowerRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabGetNpcCardFullPower_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var testTime = Stopwatch.StartNew();

            testTime.Start();

            var mabCampaignDB = await this._daoDbContext
            .MabCampaigns
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Mab_Battles!
                .Where(b =>
                    b.Mab_IsBattleFinished == false &&                   
                    b.Mab_Duels!.Any()))    
            .ThenInclude(a => a.Mab_Duels!
                .Where(b => 
                    b.Mab_PlayerCardId == null &&
                    b.Mab_NpcCardId != null))

            .Include(a => a.Mab_Battles!
                .Where(b =>
                    b.Mab_IsBattleFinished == false &&
                    b.Mab_Duels!.Any()))
            .ThenInclude(a => a.Mab_Npc)
            .ThenInclude(a => a.Mab_NpcCards)
            .ThenInclude(a => a.Mab_Card)

            .Include(a => a.Mab_PlayerCards!
                .Where(b =>                   
                    b.Mab_AssignedCards!.Any() &&                   
                    b.Mab_AssignedCards!
                        .Any(c => c.Mab_PlayerCardId == b.Id)))
            .ThenInclude(a => a.Mab_Card)

            .FirstOrDefaultAsync(a => 
                a.Mab_IsCampaignDeleted == false && 
                a.UserId == userId &&               
                a.Mab_PlayerCards.Any());

            testTime.Stop();

            if(mabCampaignDB == null)
            {
                return (null, "Error: MabGetDefendersFullPower failed! No valid MAB CAMPAIGN was found!");
            }

            var mabBattleDB = mabCampaignDB.Mab_Battles;
            if (mabBattleDB == null || mabBattleDB.Count < 1)
            {
                return (null, "Error: MabGetDefendersFullPower failed! No valid MAB BATTLES were found!");
            }

            var mabOnGoingBattle = mabBattleDB.FirstOrDefault();
            if(mabOnGoingBattle == null)
            {
                return (null, "Error: MabGetDefendersFullPower failed! No valid ongoing MAB BATTLE was found!");
            }

            var mabDuelsDB = mabOnGoingBattle.Mab_Duels;
            if (mabDuelsDB == null || mabDuelsDB.Count < 1)
            {
                return (null, "Error: MabGetDefendersFullPower failed! No valid MAB DUELS were found!");
            }

            var mabOnGoingDuelDB = mabDuelsDB.FirstOrDefault();
            if (mabOnGoingDuelDB == null)
            {
                return (null, "Error: MabGetDefendersFullPower failed! No valid ongoing MAB DUEL were found!");
            }

            var mabPlayerCardsDB = mabCampaignDB.Mab_PlayerCards;
            if (mabPlayerCardsDB == null || mabPlayerCardsDB.Count < 1)
            {
                return (null, "Error: MabGetDefendersFullPower failed! No valid MAB PLAYER CARDS were found!");
            }

            var mabNpcCardsDB = mabBattleDB.Select(a => a.Mab_Npc.Mab_NpcCards).FirstOrDefault();
            if (mabNpcCardsDB == null || mabNpcCardsDB.Count < 1)
            {
                return (null, "Error: MabGetDefendersFullPower failed! No valid MAB NPC CARDS were found!");
            }

            var mabCard_fromPlayer = mabPlayerCardsDB
                .Where(a => a!.Id == request!.Mab_PlayerCardId)
                .Select(a => a.Mab_Card)
                .FirstOrDefault();

            var mabCard_fromNpc = mabNpcCardsDB  
                .Where(a => a.Id == mabOnGoingDuelDB.Mab_NpcCardId)
                .Select(a => a.Mab_Card)
                .FirstOrDefault();

            var npcCardFullPower = Helper.MabGetCardFullPower(
                mabCard_fromNpc.Mab_CardPower,
                mabCard_fromNpc.Mab_CardUpperHand,
                mabCard_fromNpc.Mab_CardType,
                mabCard_fromPlayer.Mab_CardType);           

            return (new UsersMabGetNpcCardFullPowerResponse
            {
                Mab_NpcCardFullPower = npcCardFullPower
            }, string.Empty);
        }
        public static (bool, string) MabGetNpcCardFullPower_Validation(UsersMabGetNpcCardFullPowerRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabGetDefendersFullPower failed! Request is null!");
            }

            if(request.Mab_PlayerCardId.HasValue == false || request.Mab_PlayerCardId < 1)
            {
                return (false, "Error: MabGetDefendersFullPower failed! Invalid or missing request.Mab_PlayerCardId null!");
            }


            return (true, string.Empty);
        }


        public async Task<(UsersMabShowDeckDetailsResponse?, string)> MabShowDeckDetails(UsersMabShowDeckDetailsRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabShowDeckDetails_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }



            if (request!.Mab_DeckId == null)
            {
                var activeDeckIdDB = await this._daoDbContext
                    .MabDecks
                      .AsNoTracking()
                      .FirstOrDefaultAsync(a => a.Mab_Campaign!.UserId == userId && a.Mab_Campaign.Mab_IsCampaignDeleted == false && a.Mab_IsDeckActive == true);

                if (activeDeckIdDB == null)
                {
                    var lastMabDeckAdded = await this._daoDbContext
                        .MabDecks
                        .AsNoTracking()
                        .OrderByDescending(a => a.Id)
                        .FirstOrDefaultAsync(a => a.Mab_Campaign!.UserId == userId && a.Mab_Campaign.Mab_IsCampaignDeleted == false);

                    if (lastMabDeckAdded == null)
                    {
                        return (null, "Error: no mab player decks found for this user!");
                    }

                    lastMabDeckAdded.Mab_IsDeckActive = true;

                    request.Mab_DeckId = lastMabDeckAdded.Id;
                }
                else
                {
                    request.Mab_DeckId = activeDeckIdDB.Id;
                }

            }

            var mabDeckData = await this._daoDbContext
                .MabDecks
                .AsNoTracking()
                .Where(a => a.Mab_Campaign!.UserId == userId &&
                            a.Mab_Campaign.Mab_IsCampaignDeleted!.Value == false &&
                            a.Id == request.Mab_DeckId)
                .Select(a => new
                {
                    DeckId = a.Id,
                    DeckName = a.Mab_DeckName,
                    AssignedCardCopies = a.Mab_AssignedCards
                        .Where(b => b.Mab_PlayerCard != null)
                        .Select(b => new UsersMabShowDeckDetailsResponse_assignedCard
                        {
                            Mab_AssignedCardId = b.Id,
                            Mab_CardName = b.Mab_PlayerCard.Mab_Card.Mab_CardName,
                            Mab_CardLevel = b.Mab_PlayerCard.Mab_Card.Mab_CardLevel,
                            Mab_CardPower = b.Mab_PlayerCard.Mab_Card.Mab_CardPower,
                            Mab_CardUpperHand = b.Mab_PlayerCard.Mab_Card.Mab_CardUpperHand,
                            Mab_CardType = b.Mab_PlayerCard.Mab_Card.Mab_CardType
                        })
                        .OrderByDescending(b => b.Mab_CardLevel)
                        .ThenByDescending(b => b.Mab_CardPower)
                        .ThenByDescending(b => b.Mab_CardUpperHand)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (mabDeckData == null)
            {
                return (null, "Error: failed to fetch mab player active deck");
            }


            var mabDeckLevel = mabDeckData.AssignedCardCopies.Count > 0 ? Helper.MabGetPlayerDeckLevel(mabDeckData.AssignedCardCopies.Select(a => a.Mab_CardLevel).ToList()) : 0;

            var mabDeckDetails = new UsersMabShowDeckDetailsResponse
            {
                Mab_DeckId = mabDeckData.DeckId,
                Mab_DeckName = mabDeckData.DeckName,
                Mab_DeckLevel = mabDeckLevel,
                Mab_AssignedCards = mabDeckData.AssignedCardCopies
            };

            return (mabDeckDetails, "Mab Player Current Deck fetched successfully!");
        }
        private static (bool, string) MabShowDeckDetails_Validation(UsersMabShowDeckDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request != null && request.Mab_DeckId.HasValue == true && request.Mab_DeckId < 1)
            {
                return (false, "Error: invalid or missing Mab_DeckId");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabEditDeckNameResponse?, string)> MabEditDeckName(UsersMabEditDeckNameRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabEditDeckName_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabDeckDB = await this._daoDbContext
                .MabDecks
                .FirstOrDefaultAsync(a =>
                    a.Mab_Campaign!.UserId == userId &&
                    a.Mab_Campaign.Mab_IsCampaignDeleted == false &&
                    a.Id == request!.Mab_DeckId);

            if (mabDeckDB == null)
            {
                return (null, "Error: requested active mab deck not found!");
            }

            mabDeckDB.Mab_DeckName = request!.Mab_DeckNewName;

            await this._daoDbContext.SaveChangesAsync();

            return (new(), "Mab Deck NAME updated successfully!");
        }
        private static (bool, string) MabEditDeckName_Validation(UsersMabEditDeckNameRequest? request)
        {

            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Mab_DeckId.HasValue == false || request.Mab_DeckId < 1)
            {
                return (false, "Error: invalid or missing MabDeckId");
            }

            if (string.IsNullOrWhiteSpace(request.Mab_DeckNewName) == true)
            {
                return (false, "Error: invalid or missing MabDeckName");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabAssignPlayerCardResponse?, string)> MabAssignPlayerCard(UsersMabAssignPlayerCardRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabAssignPlayerCard failed! User is not authenticated");
            }

            var (isValid, message) = MabAssignPlayerCard_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mablayerDeckDB = await this._daoDbContext
              .MabDecks
              .Include(a => a.Mab_AssignedCards)
              .FirstOrDefaultAsync(a =>
                a.Mab_Campaign!.UserId == userId &&
                a.Mab_Campaign.Mab_IsCampaignDeleted == false &&
                a.Id == request!.Mab_DeckId);

            if (mablayerDeckDB == null)
            {
                return (null, "Error: MabAssignPlayerCard failed! requested mab card copy not found!");
            }

            mablayerDeckDB.Mab_AssignedCards!.Add(new MabAssignedCard
            {
                Mab_PlayerCardId = request.Mab_PlayerCardId,
                Mab_DeckId = mablayerDeckDB.Id
            });

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabAssignPlayerCardResponse(), "Mab player card successfully assigned to deck!");
        }
        private static (bool, string) MabAssignPlayerCard_Validation(UsersMabAssignPlayerCardRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Mab_PlayerCardId == null || request.Mab_PlayerCardId < 1)
            {
                return (false, "Error: invalid or missing MabCardCopyId");
            }

            if (request.Mab_DeckId == null || request.Mab_DeckId < 1)
            {
                return (false, "Error: invalid or missing ActiveMabDeckId");
            }


            return (true, string.Empty);
        }


        public async Task<(UsersMabUnassignPlayerCardResponse?, string)> MabUnassignPlayerCard(UsersMabUnassignPlayerCardRequest? request)
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

            await this._daoDbContext
                .MabAssignedCards
                .Where(a => a.Mab_Deck.Mab_Campaign.UserId == userId && a.Id == request.Mab_AssignedCardId)
                .ExecuteDeleteAsync();

            return (new UsersMabUnassignPlayerCardResponse(), "Mab card copy was successfully INACTIVATED!");
        }
        private static (bool, string) UnassignMabCardCopy_Validation(UsersMabUnassignPlayerCardRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Mab_AssignedCardId == null || request.Mab_AssignedCardId < 1)
            {
                return (false, "Error: invalid or missing MabCardCopyId");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersMabListUnassignedPlayerCardsResponse>?, string)> MabListUnassignedPlayerCards(UsersMabListUnassignedPlayerCardsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabListUnassignedCards_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            // Step 1: Get counts per card from DB
            var groupedMabPlayerCardsDB = await _daoDbContext
                .MabPlayerCards
                .AsNoTracking()
                .Where(playerCard => playerCard.Mab_AssignedCards!
                    .Any(assignedCard => assignedCard.Mab_DeckId == request!.Mab_DeckId) == false)
                .GroupBy(playerCard => new
                {
                    playerCard.Mab_CardId,
                })
                .Select(playerCard => new
                {
                    MabPlayerCard = playerCard.Select(playerCard => new
                    {
                        playerCard.Id,

                        playerCard.Mab_Card!.Mab_CardName,
                        playerCard.Mab_Card.Mab_CardType,
                        playerCard.Mab_Card.Mab_CardPower,
                        playerCard.Mab_Card.Mab_CardUpperHand
                    }).FirstOrDefault(),

                    Qty = playerCard.Count()
                })
                .ToListAsync(); // materialize in memory

            // Step 2: Build final MabPlayerTurn_response with string formatting in memory
            var unassignedMabCardsList = groupedMabPlayerCardsDB
                .Select(grouped => new UsersMabListUnassignedPlayerCardsResponse
                {
                    Mab_PlayerCardId = grouped.MabPlayerCard.Id,
                    Mab_CardDescription = 
                        $"{grouped.MabPlayerCard.Mab_CardName} * " +
                        $"{grouped.MabPlayerCard.Mab_CardType} * " +
                        $"{grouped.MabPlayerCard.Mab_CardPower} | {grouped.MabPlayerCard.Mab_CardUpperHand} * " +
                        $"Qty.: {grouped.Qty}"
                })
                .OrderBy(x => x.Mab_CardDescription)
                .ToList();


            return (unassignedMabCardsList, "Mab Player Current Deck fetched successfully!");
        }
        private static (bool, string) MabListUnassignedCards_Validation(UsersMabListUnassignedPlayerCardsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Mab_DeckId < 1)
            {
                return (false, "Error: request is ");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersMabListDecksResponse>?, string)> MabListDecks(UsersMabListDecksRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabListDecks_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabDecks = await this._daoDbContext
                .MabDecks
                .Where(a =>
                    a.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    a.Mab_Campaign!.UserId == userId)
                .Select(a => new UsersMabListDecksResponse
                {
                    Mab_DeckId = a.Id,
                    Mab_DeckName = a.Mab_DeckName
                })
                .ToListAsync();

            if (mabDecks == null || mabDecks.Count < 1)
            {
                return (null, "Error: failed to fetch mab decks");
            }

            return (mabDecks, "Player's mab decks listed successfully");
        }
        private static (bool, string) MabListDecks_Validation(UsersMabListDecksRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it must be null");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabCreatePlayerDeckResponse?, string)> MabCreateDeck(UsersMabCreateDeckRequest? request)
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
                .MabCampaigns
                .AsNoTracking()
                .Where(mabCampaign =>
                    mabCampaign.UserId == userId &&
                    mabCampaign.Mab_IsCampaignDeleted == false)
                .Select(mabCampaign => new
                {
                    MabCampaignID = mabCampaign.Id,
                    MabDeckNames = mabCampaign.Mab_Decks!.Select(a => a.Mab_DeckName!),
                    MabDecksCount = mabCampaign.Mab_Decks!.Count
                })
                .FirstOrDefaultAsync();

            if (mabCampaignDB == null)
            {
                return (null, "Error: mab campaign not found");
            }

            // Base new name
            var baseName = $"New Deck #{mabCampaignDB.MabDecksCount + 1}";
            var newMabDeck_DeckName = baseName;

            // Ensure uniqueness
            int suffix = 1;
            while (mabCampaignDB.MabDeckNames.Contains(newMabDeck_DeckName))
            {
                newMabDeck_DeckName = $"{baseName}({suffix})";
                suffix++;
            }

            var newMabDeck = new MabDeck
            {
                Mab_DeckName = newMabDeck_DeckName,
                Mab_IsDeckActive = false,
                Mab_CampaignId = mabCampaignDB.MabCampaignID
            };

            this._daoDbContext.MabDecks.Add(newMabDeck);

            var isNewMabDeckAddedSuccessfully = await this._daoDbContext.SaveChangesAsync();

            if (isNewMabDeckAddedSuccessfully < 1)
            {
                return (null, "No changes effected while trying to add a new mab deck");
            }

            return (new UsersMabCreatePlayerDeckResponse
            {
                NewMabDeckId = newMabDeck.Id,
                NewMabDeckName = newMabDeck.Mab_DeckName,
            }, "Mab Player card copies listed successfully!");
        }
        private static (bool, string) AddMabPlayerDeck_Validation(UsersMabCreateDeckRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabDeleteDeckResponse?, string)> DeleteMabDeck(UsersMabDeleteDeckRequest? request)
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
                .MabDecks
                .FirstOrDefaultAsync(a =>
                    a.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    a.Mab_Campaign!.UserId == userId &&
                    a.Id == request!.Mab_DeckId);

            if (mabDeckDB == null)
            {
                return (null, "Error: requested mab deck could not be found");
            }

            if (mabDeckDB.Mab_IsDeckActive == true)
            {
                var mostRecentlyCreatedMabDeck = await this._daoDbContext
                .MabDecks
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(a =>
                    a.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    a.Mab_Campaign!.UserId == userId &&
                    a.Id != request!.Mab_DeckId);

                if (mostRecentlyCreatedMabDeck == null)
                {
                    return (null, "Error: failed to delete deck, this is a single deck, create another before deleting this one.");
                }

                mostRecentlyCreatedMabDeck.Mab_IsDeckActive = true;
            }

            var mabAssignedCards = await this._daoDbContext
                .MabAssignedCards
                .Where(a => a.Mab_Deck!.Mab_Campaign!.UserId == userId && a.Mab_DeckId == request.Mab_DeckId)
                .ToListAsync();

            if (mabAssignedCards.Count > 0)
            {
                this._daoDbContext.MabAssignedCards.RemoveRange(mabAssignedCards);
            }

            this._daoDbContext.MabDecks.Remove(mabDeckDB);


            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabDeleteDeckResponse(), "Mab deck deleted successfully");
        }
        private static (bool, string) DeleteMabDeck_Validation(UsersMabDeleteDeckRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Mab_DeckId.HasValue == false || request.Mab_DeckId < 1)
            {
                return (false, "Error: MabDeckId is invalid or missing");
            }

            return (true, string.Empty);

        }


        public async Task<(UsersMabActivateDeckResponse?, string)> MabActivateDeck(UsersMabActivateDeckRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabActivateDeck_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var activeMabDeckDB = await this._daoDbContext
                .MabDecks
                .FirstOrDefaultAsync(a => a.Mab_Campaign!.UserId == userId && a.Mab_IsDeckActive == true);

            if (activeMabDeckDB != null)
            {
                activeMabDeckDB!.Mab_IsDeckActive = false;
            }

            var mabDeckDB = await this._daoDbContext
                .MabDecks
                .FirstOrDefaultAsync(a => a.Mab_Campaign!.UserId == userId && a.Id == request!.Mab_DeckId);

            if (mabDeckDB == null)
            {
                return (null, "Error: requested mab deck could not be found");
            }

            mabDeckDB.Mab_IsDeckActive = true;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabActivateDeckResponse(), mabDeckDB.Mab_DeckName!);
        }
        private static (bool, string) MabActivateDeck_Validation(UsersMabActivateDeckRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (request.Mab_DeckId.HasValue == false || request.Mab_DeckId < 1)
            {
                return (false, "Error: MabDeckId is invalid or missing");
            }

            return (true, string.Empty);

        }


        public async Task<(UsersMabListResourcesResponse?, string)> MabListResources(UsersMabListResourcesRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabListForgeryResources failed! User is not authenticated");
            }

            var (isValid, message) = MabListResources_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabForgeryResourses = await this._daoDbContext
                .MabCampaigns
                .Where(campaign =>
                    campaign.Mab_IsCampaignDeleted == false &&
                    campaign.UserId == userId)
                .Select(campaign => new UsersMabListResourcesResponse
                {
                    Mab_Coins = campaign.Mab_CoinsStash,
                    Mab_Xp = campaign.Mab_PlayerExperience,

                    Mab_Brass = campaign.Mab_BrassStash,
                    Mab_Copper = campaign.Mab_CopperStash,
                    Mab_Iron = campaign.Mab_IronStash,

                    Mab_Steel = campaign.Mab_SteelStash,
                    Mab_Titanium = campaign.Mab_TitaniumStash,
                    Mab_Silver = campaign.Mab_SilverStash,

                    Mab_Gold = campaign.Mab_GoldStash,
                    Mab_Diamond = campaign.Mab_DiamondStash,
                    Mab_Adamantium = campaign.Mab_AdamantiumStash
                })
                .FirstOrDefaultAsync();

            if (mabForgeryResourses == null)
            {
                return (null, "Error: MabListForgeryResources failed! Mab Player Card not found!");
            }

            return (mabForgeryResourses, "Player's resources listed successfully");
        }
        private static (bool, string) MabListResources_Validation(UsersMabListResourcesRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabListForgeryResources failed! Request is NOT null, howerver it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabListRawMaterialsPricesResponse?, string)> MabListRawMaterialsPrices(UsersMabListRawMaterialsPricesRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabListRawMaterialsPrices failed! User is not authenticated");
            }

            var (isValid, message) = MabListRawMaterialsPrices_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns     
                .AsNoTracking()
                .FirstOrDefaultAsync(campaign =>
                    campaign.Mab_IsCampaignDeleted == false &&
                    campaign.UserId == userId);

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabListRawMaterialsPrices failed! Mab Player Card not found!");
            }

            return (new UsersMabListRawMaterialsPricesResponse
            {
                Mab_BrassPrice = Math.Abs(Constants.BasePrice_Brass + mabCampaignDB.Mab_BrassInflation!.Value),
                Mab_CopperPrice = Math.Abs(Constants.BasePrice_Copper + mabCampaignDB.Mab_CopperInflation!.Value),
                Mab_IronPrice = Math.Abs(Constants.BasePrice_Iron + mabCampaignDB.Mab_IronInflation!.Value),


                Mab_SteelPrice = Math.Abs(Constants.BasePrice_Steel + mabCampaignDB.Mab_SteelInflation!.Value),
                Mab_TitaniumPrice = Math.Abs(Constants.BasePrice_Titanium + mabCampaignDB.Mab_TitaniumInflation!.Value),
                Mab_SilverPrice = Math.Abs(Constants.BasePrice_Silver + mabCampaignDB.Mab_SilverInflation!.Value),

                Mab_GoldPrice = Math.Abs(Constants.BasePrice_Gold + mabCampaignDB.Mab_GoldInflation!.Value),
                Mab_DiamondPrice = Math.Abs(Constants.BasePrice_Diamond + mabCampaignDB.Mab_DiamondInflation!.Value),
                Mab_AdamantiumPrice = Math.Abs(Constants.BasePrice_Adamantium + mabCampaignDB.Mab_AdamantiumInflation!.Value)
            }, "Player's resources listed successfully");
        }
        private static (bool, string) MabListRawMaterialsPrices_Validation(UsersMabListRawMaterialsPricesRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabListForgeryResources failed! Request is NOT null, howerver it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabSellRawMaterialResponse?, string)> MabSellRawMaterial(UsersMabSellRawMaterialRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabListRawMaterialsPrices failed! User is not authenticated");
            }

            var (isValid, message) = MabSellRawMaterial_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns               
                .FirstOrDefaultAsync(campaign =>
                    campaign.Mab_IsCampaignDeleted == false &&
                    campaign.UserId == userId);

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabListRawMaterialsPrices failed! Mab Player Card not found!");
            }

            int? rawMaterialPrice = request!.Mab_RawMaterialType switch
            {
                MabRawMaterialType.Brass => Math.Abs(Constants.BasePrice_Brass + mabCampaignDB.Mab_BrassInflation!.Value),
                MabRawMaterialType.Copper => Math.Abs(Constants.BasePrice_Copper + mabCampaignDB.Mab_CopperInflation!.Value),
                MabRawMaterialType.Iron => Math.Abs(Constants.BasePrice_Iron + mabCampaignDB.Mab_IronInflation!.Value),
                MabRawMaterialType.Steel => Math.Abs(Constants.BasePrice_Steel + mabCampaignDB.Mab_SteelInflation!.Value),
                MabRawMaterialType.Titanium => Math.Abs(Constants.BasePrice_Titanium + mabCampaignDB.Mab_TitaniumInflation!.Value),
                MabRawMaterialType.Silver => Math.Abs(Constants.BasePrice_Silver + mabCampaignDB.Mab_SilverInflation!.Value),
                MabRawMaterialType.Gold => Math.Abs(Constants.BasePrice_Gold + mabCampaignDB.Mab_GoldInflation!.Value),
                MabRawMaterialType.Diamond => Math.Abs(Constants.BasePrice_Diamond + mabCampaignDB.Mab_DiamondInflation!.Value),
                MabRawMaterialType.Adamantium => Math.Abs(Constants.BasePrice_Adamantium + mabCampaignDB.Mab_AdamantiumInflation!.Value),
                _ => null
            };        

            if (rawMaterialPrice == null)
            {
                return (null, "Error: MabListRawMaterialsPrices failed! Operation failed!");
            }

            mabCampaignDB.Mab_CoinsStash += rawMaterialPrice;

            if(request.Mab_RawMaterialType == MabRawMaterialType.Brass)
            {
                if(mabCampaignDB.Mab_BrassStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough BRASS to be sold!");
                }

                mabCampaignDB.Mab_BrassStash--;                
            }
            else if(request.Mab_RawMaterialType == MabRawMaterialType.Copper)
            {
                if (mabCampaignDB.Mab_CopperStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough COPPER to be sold!");
                }

                mabCampaignDB.Mab_CopperStash--;               
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Iron)
            {
                if (mabCampaignDB.Mab_IronStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough IRON to be sold!");
                }

                mabCampaignDB.Mab_IronStash--;                
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Steel)
            {
                if (mabCampaignDB.Mab_SteelStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough STEEL to be sold!");
                }

                mabCampaignDB.Mab_SteelStash--;                
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Titanium)
            {
                if (mabCampaignDB.Mab_SteelStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough TITANIUM to be sold!");
                }

                mabCampaignDB.Mab_TitaniumStash--;         
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Silver)
            {
                if (mabCampaignDB.Mab_SilverStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough SILVER to be sold!");
                }

                mabCampaignDB.Mab_SilverStash--;                
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Gold)
            {
                if (mabCampaignDB.Mab_GoldStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough GOLD to be sold!");
                }

                mabCampaignDB.Mab_GoldStash--;               
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Diamond)
            {
                if (mabCampaignDB.Mab_DiamondStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough DIAMOND to be sold!");
                }

                mabCampaignDB.Mab_DiamondStash--;                
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Adamantium)
            {
                if (mabCampaignDB.Mab_AdamantiumStash <= 0)
                {
                    return (null, "Error: MabListRawMaterialsPrices failed! Not enough ADAMANTIUM to be sold!");
                }

                mabCampaignDB.Mab_AdamantiumStash--;               
            }
            else
            {
                return (null, "Error: MabListRawMaterialsPrices failed! Invalid requested material!");
            }

            mabCampaignDB.Mab_BrassInflation = mabCampaignDB.Mab_BrassInflation > 0 ? mabCampaignDB.Mab_BrassInflation - 1 : 0;
            mabCampaignDB.Mab_CopperInflation = mabCampaignDB.Mab_CopperInflation > 0 ? mabCampaignDB.Mab_CopperInflation - 1 : 0;
            mabCampaignDB.Mab_IronInflation = mabCampaignDB.Mab_IronInflation > 0 ? mabCampaignDB.Mab_IronInflation - 1 : 0;
            mabCampaignDB.Mab_SteelInflation = mabCampaignDB.Mab_SteelInflation > 0 ? mabCampaignDB.Mab_SteelInflation - 1 : 0;
            mabCampaignDB.Mab_TitaniumInflation = mabCampaignDB.Mab_TitaniumInflation > 0 ? mabCampaignDB.Mab_TitaniumInflation - 1 : 0;
            mabCampaignDB.Mab_SilverInflation = mabCampaignDB.Mab_SilverInflation > 0 ? mabCampaignDB.Mab_SilverInflation - 1 : 0;
            mabCampaignDB.Mab_GoldInflation = mabCampaignDB.Mab_GoldInflation > 0 ? mabCampaignDB.Mab_GoldInflation - 1 : 0;
            mabCampaignDB.Mab_DiamondInflation = mabCampaignDB.Mab_DiamondInflation > 0 ? mabCampaignDB.Mab_DiamondInflation - 1 : 0;
            mabCampaignDB.Mab_AdamantiumInflation = mabCampaignDB.Mab_AdamantiumInflation > 0 ? mabCampaignDB.Mab_AdamantiumInflation - 1 : 0;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabSellRawMaterialResponse(), "Mab raw material successfully purchased");


        }
        private static (bool, string) MabSellRawMaterial_Validation(UsersMabSellRawMaterialRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabSellRawMaterial_Validation failed! Request is null!");
            }


            if (request.Mab_RawMaterialType == null || request.Mab_RawMaterialType.HasValue == false)
            {
                return (false, "Error: MabSellRawMaterial_Validation failed! request.Mab_RawMaterialType!");
            }

            if (Enum.IsDefined((MabRawMaterialType)request.Mab_RawMaterialType) == false)
            {
                var validTypes = string
                    .Join(", ", Enum.GetValues(typeof(MabRawMaterialType))
                    .Cast<MabRawMaterialType>()
                    .Select(rawMaterialType => $"{rawMaterialType} ({(int)rawMaterialType})"));

                return (false, $"Error: MabSellRawMaterial_Validation failed! Invalid RawMaterialType. It must be one of the following: {validTypes}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabBuyRawMaterialResponse?, string)> MabBuyRawMaterial(UsersMabBuyRawMaterialRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabListRawMaterialsPrices failed! User is not authenticated");
            }

            var (isValid, message) = MabBuyRawMaterial_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns          
                .FirstOrDefaultAsync(campaign =>
                    campaign.Mab_IsCampaignDeleted == false &&
                    campaign.UserId == userId);

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabListRawMaterialsPrices failed! Mab Player Card not found!");
            }

            int? rawMaterialPrice = request.Mab_RawMaterialType switch
            {
                MabRawMaterialType.Brass => Constants.BasePrice_Brass + mabCampaignDB.Mab_BrassInflation,
                MabRawMaterialType.Copper => Constants.BasePrice_Copper + mabCampaignDB.Mab_CopperInflation,
                MabRawMaterialType.Iron => Constants.BasePrice_Iron + mabCampaignDB.Mab_IronInflation,
                MabRawMaterialType.Steel => Constants.BasePrice_Steel + mabCampaignDB.Mab_SteelInflation,
                MabRawMaterialType.Titanium => Constants.BasePrice_Titanium + mabCampaignDB.Mab_TitaniumInflation,
                MabRawMaterialType.Silver => Constants.BasePrice_Silver + mabCampaignDB.Mab_SilverInflation,
                MabRawMaterialType.Gold => Constants.BasePrice_Gold + mabCampaignDB.Mab_GoldInflation,
                MabRawMaterialType.Diamond => Constants.BasePrice_Diamond + mabCampaignDB.Mab_DiamondInflation,
                MabRawMaterialType.Adamantium => Constants.BasePrice_Adamantium + mabCampaignDB.Mab_AdamantiumInflation,
                _ => null
            };

            if (rawMaterialPrice == null)
            {
                return (null, "Error: MabListRawMaterialsPrices failed! Operation failed!");
            }

            if (mabCampaignDB.Mab_CoinsStash < rawMaterialPrice)
            {
                return (null, "Error: MabListRawMaterialsPrices failed! Not enough coins!");
            }

            mabCampaignDB.Mab_CoinsStash -= rawMaterialPrice;

            switch (request.Mab_RawMaterialType)
            {
                case MabRawMaterialType.Brass:
                    mabCampaignDB.Mab_BrassStash++;                    
                    break;
                case MabRawMaterialType.Copper:
                    mabCampaignDB.Mab_CopperStash++;                    
                    break;
                case MabRawMaterialType.Iron:
                    mabCampaignDB.Mab_IronStash++;                    
                    break;
                case MabRawMaterialType.Steel:
                    mabCampaignDB.Mab_SteelStash++;                   
                    break;
                case MabRawMaterialType.Titanium:
                    mabCampaignDB.Mab_TitaniumStash++;                    
                    break;
                case MabRawMaterialType.Silver:
                    mabCampaignDB.Mab_SilverStash++;                   
                    break;
                case MabRawMaterialType.Gold:
                    mabCampaignDB.Mab_GoldStash++ ;                    
                    break;
                case MabRawMaterialType.Diamond:
                    mabCampaignDB.Mab_DiamondStash++;                    
                    break;
                case MabRawMaterialType.Adamantium:
                    mabCampaignDB.Mab_AdamantiumStash++;                    
                    break;
                default:
                    break;
            }


            if (request.Mab_RawMaterialType == MabRawMaterialType.Brass)
            {              
                mabCampaignDB.Mab_BrassStash++;
                mabCampaignDB.Mab_BrassInflation += 2;
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Copper)
            {               
                mabCampaignDB.Mab_CopperStash++;
                mabCampaignDB.Mab_CopperInflation += 3;
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Iron)
            {            
                mabCampaignDB.Mab_IronStash++;
                mabCampaignDB.Mab_IronInflation += 5;
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Steel)
            {              
                mabCampaignDB.Mab_SteelStash++;
                mabCampaignDB.Mab_SteelInflation += 7;
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Titanium)
            {              
                mabCampaignDB.Mab_TitaniumStash++;
                mabCampaignDB.Mab_TitaniumInflation += 9;
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Silver)
            {             
                mabCampaignDB.Mab_SilverStash++;
                mabCampaignDB.Mab_SilverInflation += 11;    
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Gold)
            {             
                mabCampaignDB.Mab_GoldStash++;
                mabCampaignDB.Mab_GoldInflation += 13;                     
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Diamond)
            {      
                mabCampaignDB.Mab_DiamondStash++;               
                mabCampaignDB.Mab_DiamondInflation = mabCampaignDB.Mab_DiamondInflation + 15;
               
            }
            else if (request.Mab_RawMaterialType == MabRawMaterialType.Adamantium)
            {      
                mabCampaignDB.Mab_AdamantiumStash++;
                mabCampaignDB.Mab_AdamantiumInflation += 21;              
            }
            else
            {
                return (null, "Error: MabListRawMaterialsPrices failed! Invalid requested material!");
            }

            mabCampaignDB.Mab_BrassInflation = mabCampaignDB.Mab_BrassInflation > 0 ? mabCampaignDB.Mab_BrassInflation - 1 : 0;
            mabCampaignDB.Mab_CopperInflation = mabCampaignDB.Mab_CopperInflation > 0 ? mabCampaignDB.Mab_CopperInflation - 1 : 0;
            mabCampaignDB.Mab_IronInflation = mabCampaignDB.Mab_IronInflation > 0 ? mabCampaignDB.Mab_IronInflation - 1 : 0;
            mabCampaignDB.Mab_SteelInflation = mabCampaignDB.Mab_SteelInflation > 0 ? mabCampaignDB.Mab_SteelInflation - 1 : 0;
            mabCampaignDB.Mab_TitaniumInflation = mabCampaignDB.Mab_TitaniumInflation > 0 ? mabCampaignDB.Mab_TitaniumInflation - 1 : 0;
            mabCampaignDB.Mab_SilverInflation = mabCampaignDB.Mab_SilverInflation > 0 ? mabCampaignDB.Mab_SilverInflation - 1 : 0;
            mabCampaignDB.Mab_GoldInflation = mabCampaignDB.Mab_GoldInflation > 0 ? mabCampaignDB.Mab_GoldInflation - 1 : 0;
            mabCampaignDB.Mab_DiamondInflation = mabCampaignDB.Mab_DiamondInflation > 0 ? mabCampaignDB.Mab_DiamondInflation - 1 : 0;
            mabCampaignDB.Mab_AdamantiumInflation = mabCampaignDB.Mab_AdamantiumInflation > 0 ? mabCampaignDB.Mab_AdamantiumInflation - 1 : 0;
            
            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabBuyRawMaterialResponse(), "Mab raw material successfully purchased");
        }
        private static (bool, string) MabBuyRawMaterial_Validation(UsersMabBuyRawMaterialRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabBuyRawMaterial_Validation failed! Request is null!");
            }

            if (request.Mab_RawMaterialType == null || request.Mab_RawMaterialType.HasValue == false)
            {
                return (false, "Error: MabSellRawMaterial_Validation failed! request.Mab_RawMaterialType!");
            }

            if (Enum.IsDefined((MabRawMaterialType)request.Mab_RawMaterialType) == false)
            {
                var validTypes = string
                    .Join(", ", Enum.GetValues(typeof(MabRawMaterialType))
                    .Cast<MabRawMaterialType>()
                    .Select(rawMaterialType => $"{rawMaterialType} ({(int)rawMaterialType})"));

                return (false, $"Error: MabBuyRawMaterial_Validation failed! Invalid RawMaterialType. It must be one of the following: {validTypes}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabShowMiningDetailsResponse?, string)> MabShowMiningDetails(UsersMabShowMiningDetailsRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabShowMiningToolDetails failed! User is not authenticated");
            }

            var (isValid, message) = MabShowMiningDetails_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabCardDB = await this._daoDbContext
                .MabPlayerCards
                .Include(playerCards => playerCards.Mab_Campaign)
                .Include(playerCards => playerCards.Mab_Card)
                .Where(playerCard =>
                    playerCard.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    playerCard.Mab_Campaign.UserId == userId &&
                    playerCard.Mab_Card!.Mab_IsCardDeleted! == false &&
                    playerCard.Mab_Card.Mab_CardType == MabCardType.Infantry &&
                    playerCard.Mab_Card.Mab_CardPower == playerCard.Mab_Card.Mab_CardUpperHand)
                .Select(playerCard => new UsersMabShowMiningDetailsResponse
                {
                    Mab_CardName = playerCard.Mab_Card!.Mab_CardName,                   
                    Mab_CardPower = playerCard.Mab_Card.Mab_CardPower,
                    Mab_CardUpperHand = playerCard.Mab_Card.Mab_CardUpperHand,
                    Mab_CardCode = playerCard.Mab_Card.Mab_CardCode,
                })
                .OrderByDescending(card => card.Mab_CardPower)
                .FirstOrDefaultAsync();

            if (mabCardDB == null)
            {
                return (null, "Error: MabShowMiningToolDetails failed! Mab Player Card not found!");
            }

            return (mabCardDB, "Mab player's mining tool details fetched successfully");
        }
        private static (bool, string) MabShowMiningDetails_Validation(UsersMabShowMiningDetailsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabShowMiningToolDetails failed! Request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabExtractRawMaterialResponse?, string)> MabExtractRawMaterial(UsersMabExtractRawMaterialRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabExtractRawMaterial failed! User is not authenticated");
            }

            var (isValid, message) = MabExtractRawMaterial_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabPlayerCardDB = await this._daoDbContext
                .MabPlayerCards
                .Include(playerCards => playerCards.Mab_Campaign)
                .Include(playerCards => playerCards.Mab_Card)
                .Where(playerCard =>
                    playerCard.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    playerCard.Mab_Campaign.UserId == userId &&
                    playerCard.Mab_Card!.Mab_IsCardDeleted! == false &&
                    playerCard.Mab_Card.Mab_CardType == MabCardType.Infantry &&
                    playerCard.Mab_Card.Mab_CardPower == playerCard.Mab_Card.Mab_CardUpperHand)                        
                .OrderByDescending(playerCard => playerCard.Mab_Card!.Mab_CardPower)
                .FirstOrDefaultAsync();

            if (mabPlayerCardDB == null)
            {
                return (null, "Error: MabExtractRawMaterial failed! Mab Player Card not found!");
            }

            var mabCampaignDB = mabPlayerCardDB.Mab_Campaign;

            if(mabCampaignDB == null)
            {
                return (null, "Error: MabExtractRawMaterial failed! Mab Campaign not found!");
            }

            if(mabCampaignDB.Mab_CoinsStash < Constants.BaseMiningPrice)
            {
                return (null, "Error: MabExtractRawMaterial failed! Not enough coins!");
            }
        

            var brassChance = random.Next(0, 2);
            var copperChance = random.Next(0, 2);
            var ironChance = random.Next(0, 5);
            var steelChance = random.Next(0, 10);
            var titaniumChance = random.Next(0, 100);
            var silverChance = random.Next(0, 500);
            var goldChance = random.Next(0, 1_000);
            var diamondChance = random.Next(0, 10_000);
            var adamantiumChance = random.Next(0, 100_000);

            var cardPowerDB = mabPlayerCardDB.Mab_Card!.Mab_CardPower;

            var extractedBrass = cardPowerDB switch
            {
                0 => brassChance == 1 ? 1 : 0,
                1 => 1,
                2 => 2,
                3 => 3,
                4 => 4,
                5 => 5,
                6 => 10,
                7 => 50,
                8 => 100,
                9 => 200,
                _ => 0
            };
            
            var extractedCopper = cardPowerDB switch
            {
                0 => brassChance == 1 ? 1 : 0,
                1 => copperChance == 1 ? 1 : 0,
                2 => 1,
                3 => 2,
                4 => 3,
                5 => 4,
                6 => 5,
                7 => 10,
                8 => 50,
                9 => 100,
                _ => 0
            };
            
            var extractedIron = cardPowerDB switch
            {
                0 => ironChance == 1 ? 1 : 0,
                1 => copperChance == 1 ? 1 : 0,
                2 => brassChance == 1 ? 1 : 0,
                3 => 1,
                4 => 2,
                5 => 3,
                6 => 4,
                7 => 5,
                8 => 25,
                9 => 50,
                _ => 0
            };
            
            var extractedSteel = cardPowerDB switch
            {
                0 => steelChance == 1 ? 1 : 0,
                1 => ironChance == 1 ? 1 : 0,
                2 => copperChance == 1 ? 1 : 0,
                3 => brassChance == 1 ? 1 : 0,
                4 => 1,
                5 => 2,
                6 => 3,
                7 => 4,
                8 => 5,
                9 => 25,
                _ => 0
            };
            
            var extractedTitanium = cardPowerDB switch
            {
                0 => titaniumChance == 1 ? 1 : 0,
                1 => steelChance == 1 ? 1 : 0,
                2 => ironChance == 1 ? 1 : 0,
                3 => copperChance == 1 ? 1 : 0,
                4 => brassChance == 1 ? 1 : 0,
                5 => 1,
                6 => 2,
                7 => 3,
                8 => 4,
                9 => 5,
                _ => 0
            };
            
            var extractedSilver = cardPowerDB switch
            {
                0 => silverChance == 1 ? 1 : 0,
                1 => titaniumChance == 1 ? 1 : 0,
                2 => steelChance == 1 ? 1 : 0,
                3 => ironChance == 1 ? 1 : 0,
                4 => copperChance == 1 ? 1 : 0,
                5 => brassChance == 1 ? 1 : 0,
                6 => 1,
                7 => 2,
                8 => 3,
                9 => 4,
                _ => 0
            };
            
            var extractedGold = cardPowerDB switch
            {
                0 => goldChance == 1 ? 1 : 0,
                1 => silverChance == 1 ? 1 : 0,
                2 => titaniumChance == 1 ? 1 : 0,
                3 => steelChance == 1 ? 1 : 0,
                4 => ironChance == 1 ? 1 : 0,
                5 => copperChance == 1 ? 1 : 0,
                6 => brassChance == 1 ? 1 : 0,
                7 => 1,
                8 => 2,
                9 => 3,
                _ => 0
            };
            
            var extractedDiamond = cardPowerDB switch
            {
                0 => diamondChance == 1 ? 1 : 0,
                1 => goldChance == 1 ? 1 : 0,
                2 => silverChance == 1 ? 1 : 0,
                3 => titaniumChance == 1 ? 1 : 0,
                4 => steelChance == 1 ? 1 : 0,
                5 => ironChance == 1 ? 1 : 0,
                6 => copperChance == 1 ? 1 : 0,
                7 => brassChance == 1 ? 1 : 0,
                8 => 1,
                9 => 2,
                _ => 0
            };

            var extractedAdamantium = cardPowerDB switch
            {
                0 => adamantiumChance == 1 ? 1 : 0,
                1 => diamondChance == 1 ? 1 : 0,
                2 => goldChance == 1 ? 1 : 0,
                3 => silverChance == 1 ? 1 : 0,
                4 => titaniumChance == 1 ? 1 : 0,
                5 => steelChance == 1 ? 1 : 0,
                6 => ironChance == 1 ? 1 : 0,
                7 => copperChance == 1 ? 1 : 0,
                8 => brassChance == 1 ? 1 : 0,
                9 => 1,
                _ => 0
            };

            mabCampaignDB!.Mab_CoinsStash -= Constants.BaseMiningPrice;

            mabCampaignDB.Mab_BrassStash += extractedBrass;
            mabCampaignDB.Mab_CopperStash += extractedCopper;
            mabCampaignDB.Mab_IronStash += extractedIron;

            mabCampaignDB.Mab_SteelStash += extractedSteel;
            mabCampaignDB.Mab_TitaniumStash += extractedTitanium;
            mabCampaignDB.Mab_SilverStash += extractedSilver;

            mabCampaignDB.Mab_GoldStash += extractedGold;
            mabCampaignDB.Mab_DiamondStash += extractedDiamond;
            mabCampaignDB.Mab_AdamantiumStash += extractedAdamantium;

            mabCampaignDB.Mab_MinerTrophy = extractedAdamantium > 0;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabExtractRawMaterialResponse
            {
                Mab_ExtractedBrass = extractedBrass,
                Mab_ExtractedCopper = extractedCopper,
                Mab_ExtractedIron = extractedIron,

                Mab_ExtractedSteel = extractedSteel,
                Mab_ExtractedTitanium = extractedTitanium,
                Mab_ExtractedSilver = extractedSilver,

                Mab_ExtractedGold = extractedGold,
                Mab_ExtractedDiamond = extractedDiamond,
                Mab_ExtractedAdamantium = extractedAdamantium
            }, "Mab player's mining tool details fetched successfully");
        }
        private static (bool, string) MabExtractRawMaterial_Validation(UsersMabExtractRawMaterialRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabExtractRawMaterial_Validation failed! Request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabShowPlayerCardDetailsResponse?, string)> MabShowPlayerCardDetails(UsersMabShowPlayerCardDetailsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabShowPlayerCardDetails failed! User is not authenticated");
            }

            var (isValid, message) = MabShowPlayerCardDetails_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabCardDB = await this._daoDbContext
                .MabPlayerCards
                .Include(playerCards => playerCards.Mab_Campaign)
                .Include(playerCards => playerCards.Mab_Card)                  
                .Where(playerCard =>                    
                    playerCard.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    playerCard.Mab_Campaign.UserId == userId &&
                    playerCard.Id! == request!.Mab_PlayerCardId &&
                    playerCard.Mab_Card!.Mab_IsCardDeleted! == false)
                .Select(playerCard => new UsersMabShowPlayerCardDetailsResponse
                {
                   Mab_CardName = playerCard.Mab_Card!.Mab_CardName,
                   Mab_CardType = playerCard.Mab_Card.Mab_CardType,
                   Mab_CardCode = playerCard.Mab_Card.Mab_CardCode,
                   Mab_CardPower = playerCard.Mab_Card.Mab_CardPower,
                   Mab_CardUpperHand = playerCard.Mab_Card.Mab_CardUpperHand
                })
                .FirstOrDefaultAsync();

            if (mabCardDB == null)
            {
                return (null, "Error: MabShowPlayerCardDetails failed! Mab Player Card not found!");
            }

            return (mabCardDB, "Player's mab card details fetched successfully");
        }
        private static (bool, string) MabShowPlayerCardDetails_Validation(UsersMabShowPlayerCardDetailsRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabShowPlayerCardDetails_Validation failed! Request is null!");
            }

            if (request.Mab_PlayerCardId.HasValue == false || request.Mab_PlayerCardId < 1)
            {
                return (false, "Error: MabShowPlayerCardDetails_Validation failed! Requested Mab_PlayerCardId is missing or invalid!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabForgeCardResponse?, string)> MabForgeCard(UsersMabForgeCardRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabForgeCard failed! User is not authenticated");
            }

            var (isValid, message) = MabForgeCard_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabPlayerCardsDB = await this._daoDbContext
                .MabPlayerCards
                .Include(playerCards => playerCards.Mab_Campaign)
                .Include(playerCards => playerCards.Mab_Card)
                .Include(playerCards => playerCards.Mab_AssignedCards)
                .Where(playerCard =>
                    playerCard.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    playerCard.Mab_Campaign.UserId == userId &&                    
                    playerCard.Mab_Card!.Mab_IsCardDeleted! == false)
                .ToListAsync();             

            if (mabPlayerCardsDB == null || mabPlayerCardsDB.Count < 1)
            {
                return (null, "Error: MabForgeCard failed! Mab Player Cards could not be found!");
            }

            var mabPlayerCardDB = mabPlayerCardsDB.FirstOrDefault(playerCard => playerCard.Id == request.Mab_PlayerCardId);

            if (mabPlayerCardDB == null)
            {
                return (null, "Error: MabForgeCard failed! Mab Player Card not found!");
            }

            var campaignDB = mabPlayerCardDB.Mab_Campaign;

            if (campaignDB == null)
            {
                return (null, "Error: MabForgeCard failed! Mab Campaign not found!");
            }

            var assignedCards = mabPlayerCardDB.Mab_AssignedCards;

            var cardPower = mabPlayerCardDB!.Mab_Card!.Mab_CardPower;

            var cardUpperHand = mabPlayerCardDB.Mab_Card.Mab_CardUpperHand;

            var cardType = mabPlayerCardDB.Mab_Card.Mab_CardType;

            var ownedRawMaterial = cardPower switch
            {
                0 => campaignDB.Mab_BrassStash,
                1 => campaignDB.Mab_CopperStash,
                2 => campaignDB.Mab_IronStash,
                3 => campaignDB.Mab_SteelStash,
                4 => campaignDB.Mab_TitaniumStash,
                5 => campaignDB.Mab_SilverStash,
                6 => campaignDB.Mab_GoldStash,
                7 => campaignDB.Mab_DiamondStash,
                8 => campaignDB.Mab_AdamantiumStash,
                _ => null
            };

            if(ownedRawMaterial == null || ownedRawMaterial.HasValue == false)
            {
                return (new UsersMabForgeCardResponse
                {
                    Mab_CardName = mabPlayerCardDB.Mab_Card.Mab_CardName,
                    Mab_CardPower = cardPower,
                    Mab_CardUpperHand = cardUpperHand,
                    Mab_CardType = cardType,
                    Mab_CardCode = mabPlayerCardDB.Mab_Card.Mab_CardCode

                }, "Error: MabForgeCard failed! Mab Player Card is already at max power, nothing changed!");
            }
         
            List<int?>? evaluateForgingCosts = null;

            (evaluateForgingCosts, message) = Helper.MabEvaluateForgingCosts(campaignDB.Mab_CoinsStash, cardPower, ownedRawMaterial);

            if(evaluateForgingCosts == null || evaluateForgingCosts.Count < 1)
            {
                return (null, message);
            }

            var improvedPower = evaluateForgingCosts[0];

            var coinsCost = evaluateForgingCosts[1];

            var rawMaterialCost = evaluateForgingCosts[2];

            var newMabCardDB = await this._daoDbContext
                .MabCards
                .FirstOrDefaultAsync(card =>
                    card.Mab_IsCardDeleted == false &&
                    card.Mab_CardPower == improvedPower &&
                    card.Mab_CardUpperHand == cardUpperHand &&
                    card.Mab_CardType! == cardType);

            if(newMabCardDB == null)
            {
                return (null, "Error: MabForgeCard failed! No upgrade for the requested Player Card could be found!");
            }

            campaignDB.Mab_CoinsStash = campaignDB.Mab_CoinsStash - coinsCost;

             switch (cardPower)
             {
                case 0:
                    campaignDB.Mab_BrassStash -= rawMaterialCost;
                    break;
                case 1:
                    campaignDB.Mab_CopperStash -= rawMaterialCost;
                    break;
                case 2:
                    campaignDB.Mab_IronStash -= rawMaterialCost;
                    break;
                case 3:
                    campaignDB.Mab_SteelStash -= rawMaterialCost;
                    break;
                case 4:
                    campaignDB.Mab_TitaniumStash -= rawMaterialCost;
                    break;
                case 5:
                    campaignDB.Mab_SilverStash -= rawMaterialCost;
                    break;
                case 6: 
                    campaignDB.Mab_GoldStash -= rawMaterialCost;
                    break;
                case 7:
                    campaignDB.Mab_DiamondStash -= rawMaterialCost;
                    break;
                case 8:
                    campaignDB.Mab_AdamantiumStash -= rawMaterialCost;
                    break;
                default:                   
                    break;
            };

            if(assignedCards != null && assignedCards.Count > 1)
            {
                this._daoDbContext.MabAssignedCards.RemoveRange(assignedCards);
            }

            this._daoDbContext.MabPlayerCards.Remove(mabPlayerCardDB);

            var newPlayerCard = new MabPlayerCard
            {
                Mab_Card = newMabCardDB,
                Mab_Campaign = campaignDB,
            };

            this._daoDbContext.MabPlayerCards.Add(newPlayerCard);

            campaignDB.Mab_ForgingsCount++;

            await this._daoDbContext.SaveChangesAsync();

            return (
                new UsersMabForgeCardResponse
                {
                    Mab_PlayerCardId = newPlayerCard.Id,
                    Mab_CardName = newMabCardDB.Mab_CardName,
                    Mab_CardPower = newMabCardDB.Mab_CardPower,
                    Mab_CardUpperHand = newMabCardDB.Mab_CardUpperHand,
                    Mab_CardType = newMabCardDB.Mab_CardType,
                    Mab_CardCode = newMabCardDB.Mab_CardCode

                }, "Mab player card's power has been improved!");
        }
        private static (bool, string) MabForgeCard_Validation(UsersMabForgeCardRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabForgeCard_Validation failed! Request is null!");
            }

            if (request.Mab_PlayerCardId.HasValue == false || request.Mab_PlayerCardId < 1)
            {
                return (false, "Error: MabForgeCard_Validation failed! Requested Mab_PlayerCardId is missing or invalid!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabSharpenCardResponse?, string)> MabSharpenCard(UsersMabSharpenCardRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabSharpenCard failed! User is not authenticated");
            }

            var (isValid, message) = MabSharpenCard_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabPlayerCardDB = await this._daoDbContext
                .MabPlayerCards
                .Include(playerCards => playerCards.Mab_Campaign)
                .Include(playerCards => playerCards.Mab_Card)
                .Include(playerCards => playerCards.Mab_AssignedCards)
                .FirstOrDefaultAsync(playerCard =>
                    playerCard.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    playerCard.Mab_Campaign.UserId == userId &&
                    playerCard.Id! == request!.Mab_PlayerCardId &&
                    playerCard.Mab_Card!.Mab_IsCardDeleted! == false);

            if (mabPlayerCardDB == null)
            {
                return (null, "Error: MabSharpenCard failed! Mab Player Card not found!");
            }

            var campaignDB = mabPlayerCardDB.Mab_Campaign;

            if (campaignDB == null)
            {
                return (null, "Error: MabSharpenCard failed! Mab Campaign not found!");
            }

            var assignedCards = mabPlayerCardDB.Mab_AssignedCards;

            var cardPower = mabPlayerCardDB!.Mab_Card!.Mab_CardPower;

            var cardUpperHand = mabPlayerCardDB.Mab_Card.Mab_CardUpperHand;

            var cardType = mabPlayerCardDB.Mab_Card.Mab_CardType;


            var playerXp = campaignDB.Mab_PlayerExperience;

            if (playerXp == null || playerXp.HasValue == false || playerXp < 1)
            {
                return (new UsersMabSharpenCardResponse
                {
                    Mab_CardName = mabPlayerCardDB.Mab_Card.Mab_CardName,
                    Mab_CardPower = cardPower,
                    Mab_CardUpperHand = cardUpperHand,
                    Mab_CardType = cardType,
                    Mab_CardCode = mabPlayerCardDB.Mab_Card.Mab_CardCode

                }, "Error: MabSharpenCard failed! Mab Player Card is already at max power, nothing changed!");
            }

            List<int?>? evaluateSharpeningCosts = null;        

            (evaluateSharpeningCosts, message) = Helper.MabEvaluateSharpeningCosts(campaignDB.Mab_CoinsStash, playerXp, cardUpperHand);

            if (evaluateSharpeningCosts == null || evaluateSharpeningCosts.Count < 1)
            {
                return (null, message);
            }

            var improvedUpperHand = evaluateSharpeningCosts[0];

            var coinsCost = evaluateSharpeningCosts[1];

            var xpCost = evaluateSharpeningCosts[2];

            var newMabCardDB = await this._daoDbContext
                .MabCards
                .FirstOrDefaultAsync(card =>
                    card.Mab_IsCardDeleted == false &&
                    card.Mab_CardPower == cardPower &&
                    card.Mab_CardUpperHand == improvedUpperHand &&
                    card.Mab_CardType! == cardType);

            if (newMabCardDB == null)
            {
                return (null, "Error: MabSharpenCard failed! No upgrade for the requested Player Card could be found!");
            }

            campaignDB.Mab_CoinsStash = campaignDB.Mab_CoinsStash - coinsCost;

            campaignDB.Mab_PlayerExperience = campaignDB.Mab_PlayerExperience - xpCost;


            if (assignedCards != null && assignedCards.Count > 1)
            {
                this._daoDbContext.MabAssignedCards.RemoveRange(assignedCards);
            }

            this._daoDbContext.MabPlayerCards.Remove(mabPlayerCardDB);

            var newPlayerCard = new MabPlayerCard
            {
                Mab_Card = newMabCardDB,
                Mab_Campaign = campaignDB,
            };

            this._daoDbContext.MabPlayerCards.Add(newPlayerCard);

            campaignDB.Mab_SharpenCount++;

            await this._daoDbContext.SaveChangesAsync();

            return (
                new UsersMabSharpenCardResponse
                {
                    Mab_PlayerCardId = newPlayerCard.Id,
                    Mab_CardName = newMabCardDB.Mab_CardName,
                    Mab_CardPower = newMabCardDB.Mab_CardPower,
                    Mab_CardUpperHand = newMabCardDB.Mab_CardUpperHand,
                    Mab_CardType = newMabCardDB.Mab_CardType,
                    Mab_CardCode = newMabCardDB.Mab_CardCode

                }, "Player card has been sharpened successfully!");
        }
        private static (bool, string) MabSharpenCard_Validation(UsersMabSharpenCardRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabForgeCard_Validation failed! Request is null!");
            }

            if (request.Mab_PlayerCardId.HasValue == false || request.Mab_PlayerCardId < 1)
            {
                return (false, "Error: MabForgeCard_Validation failed! Requested Mab_PlayerCardId is missing or invalid!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabMeltCardResponse?, string)> MabMeltCard(UsersMabMeltCardRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabMeltCard failed! User is not authenticated");
            }

            var (isValid, message) = MabMeltCard_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabPlayerCardDB = await this._daoDbContext
                .MabPlayerCards
                .Include(playerCards => playerCards.Mab_Campaign)
                .Include(playerCards => playerCards.Mab_Card)
                .Include(playerCards => playerCards.Mab_AssignedCards)
                .FirstOrDefaultAsync(playerCard =>
                    playerCard.Mab_Campaign!.Mab_IsCampaignDeleted == false &&
                    playerCard.Mab_Campaign.UserId == userId &&
                    playerCard.Id! == request!.Mab_PlayerCardId &&
                    playerCard.Mab_Card!.Mab_IsCardDeleted! == false);

            if (mabPlayerCardDB == null)
            {
                return (null, "Error: MabMeltCard failed! Mab Player Card not found!");
            }

            var campaignDB = mabPlayerCardDB.Mab_Campaign;

            if (campaignDB == null)
            {
                return (null, "Error: MabMeltCard failed! Mab Campaign not found!");
            }

            var assignedCards = mabPlayerCardDB.Mab_AssignedCards;

            var cardPower = mabPlayerCardDB!.Mab_Card!.Mab_CardPower;

            var cardUpperHand = mabPlayerCardDB.Mab_Card.Mab_CardUpperHand;

            var cardType = mabPlayerCardDB.Mab_Card.Mab_CardType;        

            List<int?>? evaluateMeltingCost = null;

            (evaluateMeltingCost, message) = Helper.MabEvaluateMeltingCost(campaignDB.Mab_CoinsStash, cardPower, cardUpperHand);

            if (evaluateMeltingCost == null || evaluateMeltingCost.Count < 1)
            {
                return (null, message);
            }

            var extractedRawMaterial = evaluateMeltingCost[0];

            var gainedXp = evaluateMeltingCost[1];

            var coinsCost = evaluateMeltingCost[2];


            switch (cardPower)
            {                
                case 1:
                    campaignDB.Mab_BrassStash += extractedRawMaterial;                    
                    break;
                case 2:
                    campaignDB.Mab_CopperStash += extractedRawMaterial;
                    
                    break;
                case 3:
                    campaignDB.Mab_IronStash += extractedRawMaterial;                    
                    break;
                case 4:
                    campaignDB.Mab_SteelStash += extractedRawMaterial;                  
                    break;
                case 5:
                    campaignDB.Mab_TitaniumStash += extractedRawMaterial;
                    break;
                case 6:
                    campaignDB.Mab_SilverStash += extractedRawMaterial;                   
                    break;
                case 7:
                    campaignDB.Mab_GoldStash += extractedRawMaterial;                    
                    break;
                case 8:
                    campaignDB.Mab_DiamondStash += extractedRawMaterial;
                    
                    break;
                case 9:
                    campaignDB.Mab_AdamantiumStash += extractedRawMaterial;                     
                    break;
                default:
                    break;
            };

            campaignDB.Mab_PlayerExperience += gainedXp;

            campaignDB.Mab_CoinsStash = campaignDB.Mab_CoinsStash - coinsCost;         

            if (assignedCards != null && assignedCards.Count > 1)
            {
                this._daoDbContext.MabAssignedCards.RemoveRange(assignedCards);
            }

            this._daoDbContext.MabPlayerCards.Remove(mabPlayerCardDB);

            campaignDB.Mab_MeltCount++;

            await this._daoDbContext.SaveChangesAsync();

            return (
                new UsersMabMeltCardResponse
                {
                    Mab_ExtractedRawMaterial = extractedRawMaterial,
                    Mab_GainedXp = gainedXp

                }, "Mab player card has been melted successfully!");
        }
        private static (bool, string) MabMeltCard_Validation(UsersMabMeltCardRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: MabForgeCard_Validation failed! Request is null!");
            }

            if (request.Mab_PlayerCardId.HasValue == false || request.Mab_PlayerCardId < 1)
            {
                return (false, "Error: MabForgeCard_Validation failed! Requested Mab_PlayerCardId is missing or invalid!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabShowCampaignStatisticsResponse?, string)> MabShowCampaignStatistics(UsersMabShowCampaignStatisticsRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabShowCampaignStatistics_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var anyMabCampaignStarted = await this._daoDbContext
                .MabCampaigns
                .AsNoTracking()                
                .AnyAsync(campaign => campaign.Mab_IsCampaignDeleted == false && campaign.UserId == userId);

            if(anyMabCampaignStarted == false)
            {
                return (new UsersMabShowCampaignStatisticsResponse(), "No ongoing mab campaign.");
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns
                .AsSplitQuery()
                .Include(campaign => campaign.Mab_PlayerCards)                   
                .Include(campaign => campaign.Mab_Decks)
                .Include(campaign => campaign.Mab_Battles!)
                    .ThenInclude(battle => battle.Mab_Quest)
                        .ThenInclude(quest => quest!.Mab_Npcs)
                .FirstOrDefaultAsync(campaign =>
                    campaign.UserId == userId &&
                    campaign.Mab_IsCampaignDeleted == false);

            var mabStartedQuests = mabCampaignDB
               .Mab_Battles!
               .Where(battle => battle.Mab_Campaign.Mab_IsCampaignDeleted == false &&
                               battle.Mab_Campaign.UserId == userId &&
                               battle.Mab_HasPlayerWon == true)
               .Select(battle => battle.Mab_Quest)
               .Where(quest => quest != null)
               .GroupBy(quest => quest!.Id) // Group by quest ID
               .Select(group => group.First()) // Take one quest from each group
               .ToList();

            var playerCards_CardsIds = mabCampaignDB.Mab_PlayerCards!.Select(playerCard => playerCard.Mab_CardId).ToList();

            mabCampaignDB.Mab_AllCardsCollectedTrophy = await this._daoDbContext
                .MabCards                          
                .AnyAsync(card => 
                    card.Mab_IsCardDeleted == false &&
                    playerCards_CardsIds.Contains(card.Id) == false);

            var defeatedNpcsIds = mabCampaignDB.Mab_Battles.Select(battle => battle.Mab_NpcId).ToList();

            mabCampaignDB.Mab_AllNpcsDefeatedTrophy = await this._daoDbContext
                .MabNpcs
                .AnyAsync(npc =>
                    npc.Mab_IsNpcDeleted == false &&
                    defeatedNpcsIds.Contains(npc.Id) == false);

            if(mabCampaignDB.Mab_BlacksmithTrophy == false)
            {
                mabCampaignDB.Mab_BlacksmithTrophy = 
                    mabCampaignDB.Mab_CountForgings >= 50 && 
                    mabCampaignDB.Mab_SharpenCount >= 50 && 
                    mabCampaignDB.Mab_MeltCount >= 50;
            }

            if(mabCampaignDB.Mab_BourgeoisTrophy == false)
            {
                mabCampaignDB.Mab_BourgeoisTrophy = mabCampaignDB.Mab_CoinsStash >= 1000;
            }

            // Count fulfilled quests (where all NPCs have been defeated)
            var fulfilledQuestsCount = 0;
            foreach (var quest in mabStartedQuests)
            {
                if (quest?.Mab_Npcs != null && quest.Mab_Npcs.Count > 0)
                {
                    // Get all battles for this quest where player won
                    var questBattles = mabCampaignDB
                        .Mab_Battles!
                        .Where(battle => battle.Mab_Quest != null &&
                                       battle.Mab_Quest.Id == quest.Id &&
                                       battle.Mab_HasPlayerWon == true)
                        .ToList();

                    // Get unique NPCs defeated in this quest
                    var defeatedNpcIds = questBattles
                        .Select(battle => battle.Mab_NpcId)
                        .Distinct()
                        .ToHashSet();

                    // Check if all NPCs in the quest have been defeated
                    var allNpcIds = quest
                        .Mab_Npcs
                        .Select(npc => npc.Id)
                        .ToHashSet();

                    if (allNpcIds.All(npcId => defeatedNpcIds.Contains(npcId)))
                    {
                        fulfilledQuestsCount++;
                    }
                }
            }

            var mabQuestsCount = await this._daoDbContext
                .MabQuests
                .AsNoTracking()
                .CountAsync();

            var mabCampaignStatisticsDB = new UsersMabShowCampaignStatisticsResponse
            {
                Mab_StartNewCampaign = false,

                Mab_PlayerNickName = mabCampaignDB.Mab_PlayerNickname!,

                Mab_PlayerLevel = mabCampaignDB.Mab_PlayerLevel!.Value,

                Mab_CurrentPlayerXp = mabCampaignDB.Mab_PlayerExperience,

                Mab_NextPlayerLevelThreshold = Helper.MabGetPlayerNextLevelThreshold(mabCampaignDB.Mab_PlayerLevel!.Value),

                Mab_CoinsStash = mabCampaignDB.Mab_CoinsStash,

                Mab_QuestsCounts = mabQuestsCount,

                Mab_FulfilledQuestsCount = fulfilledQuestsCount,

                Mab_BattlesCount = mabCampaignDB.Mab_BattlesCount!.Value,

                Mab_BattleVictoriesCount = mabCampaignDB.Mab_BattleVictoriesCount!.Value,

                Mab_BattleDefeatsCount = mabCampaignDB.Mab_BattleDefeatsCount!.Value,

                Mab_OpenedBoostersCount = mabCampaignDB.Mab_OpenedBoostersCount!.Value,

                Mab_CreatedDecksCount = mabCampaignDB.Mab_Decks!.Count,

                Mab_CampaignDifficulty = mabCampaignDB.Mab_Difficulty!.Value,

                Mab_AllMabCardsCollectedTrophy = mabCampaignDB.Mab_AllCardsCollectedTrophy!.Value,

                Mab_AllMabNpcsDefeatedTrophy = mabCampaignDB.Mab_AllNpcsDefeatedTrophy!.Value,

                Mab_BourgeoisTrophy = mabCampaignDB.Mab_BourgeoisTrophy,

                Mab_MinerTrophy = mabCampaignDB.Mab_MinerTrophy,

                Mab_BlacksmithTrophy = mabCampaignDB.Mab_BlacksmithTrophy,
            };

            if (mabCampaignStatisticsDB == null)
            {
                return (null, "Error: Mab Campaign not found");
            }

            return (mabCampaignStatisticsDB, "Mab Campaign Statistics fetched successfully!");
        }
        private static (bool, string) MabShowCampaignStatistics_Validation(UsersMabShowCampaignStatisticsRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabEditPlayerNickNameResponse?, string)> MabEditPlayerNickname(UsersMabEditPlayerNicknameRequest? request)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: User is not authenticated");
            }

            var (isValid, message) = MabEditPlayerNickname_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId);

            if (mabCampaignDB == null)
            {
                return (null, "Error: Mab Campaign not found");
            }

            if (mabCampaignDB.Mab_IsCampaignDeleted == true)
            {
                return (null, "Error: requested Mab Campaign is already finished");
            }

            mabCampaignDB.Mab_PlayerNickname = request!.Mab_PlayerNewNickname!;

            await this._daoDbContext.SaveChangesAsync();

            return (new UsersMabEditPlayerNickNameResponse(), "Mab Player Nickname updated successfully!");
        }
        private static (bool, string) MabEditPlayerNickname_Validation(UsersMabEditPlayerNicknameRequest? request)
        {
            if (request == null)
            {
                return (false, "Error: request is null");
            }

            if (string.IsNullOrWhiteSpace(request.Mab_PlayerNewNickname) == true)
            {
                return (false, $"Error: NewMabPlayerNickname request failed: {request.Mab_PlayerNewNickname}");
            }

            return (true, string.Empty);
        }


        public async Task<(UsersMabGetDeckBoosterDealResponse?, string)> MabGetDeckBoosterDeal(UsersMabGetDeckBoosterDealRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabBuyDeckBooster failed! User is not authenticated");
            }

            var (isValid, message) = MabGetDeckBoosterDeal_Validation(request);

            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns                
                .FirstOrDefaultAsync(campaign => 
                    campaign.Mab_IsCampaignDeleted == false && campaign.UserId == userId);

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabBuyDeckBooster failed! No Mab Campaign found!");
            }

            var mabDeckBoosterPrice = $"{Constants.BoosterPrice} pieces of gold";
            var mabDeckBoosterContent = $"{Constants.BoosterSize} cards (from level 0 up to level 5)";


            return (new UsersMabGetDeckBoosterDealResponse 
            {
                Mab_DeckBoosterPrice = mabDeckBoosterPrice,
                Mab_DeckBoosterContent = mabDeckBoosterContent
            }, "Mab Deck Booster acquired succesfully!");
        }
        private static (bool, string) MabGetDeckBoosterDeal_Validation(UsersMabGetDeckBoosterDealRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabBuyDeckBooster failed! Request is NOT null, however it MUST be null!");
            }

            return (true, string.Empty);
        }


        public async Task<(List<UsersMabBuyDeckBoosterResponse>?, string)> MabBuyDeckBooster(UsersMabBuyDeckBoosterRequest? request = null)
        {
            var userId = this._httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (null, "Error: MabBuyDeckBooster failed! User is not authenticated");
            }

            var (isValid, message) = MabBuyDeckBooster_Validation(request);
            if (isValid == false)
            {
                return (null, message);
            }

            var mabCampaignDB = await this._daoDbContext
                .MabCampaigns
                .Include(campaign => campaign.Mab_PlayerCards!)
                    .ThenInclude(playerCard => playerCard.Mab_Card)
                .FirstOrDefaultAsync(campaign => campaign.Mab_IsCampaignDeleted == false && campaign.UserId == userId);

            if (mabCampaignDB == null)
            {
                return (null, "Error: MabBuyDeckBooster failed! No Mab Campaign found!");
            }

            if(mabCampaignDB.Mab_CoinsStash == null || mabCampaignDB.Mab_CoinsStash < Constants.BoosterPrice)
            {
                return (null, "Error: MabBuyDeckBooster failed! Not enough gold!");
            }

            (var booster, message) = await this.GetBoosterCards();
            if(booster == null || booster.Count < Constants.BoosterSize)
            {
                return (null, message);
            }

            mabCampaignDB.Mab_PlayerCards!.AddRange(booster);

            mabCampaignDB.Mab_CoinsStash -= Constants.BoosterPrice;

            mabCampaignDB.Mab_OpenedBoostersCount = mabCampaignDB.Mab_OpenedBoostersCount +1;

            await this._daoDbContext.SaveChangesAsync();

            var response = booster.Select(card => new UsersMabBuyDeckBoosterResponse
            {
                Mab_CardName = card.Mab_Card!.Mab_CardName,
                Mab_CardCode = card.Mab_Card.Mab_CardCode,
                Mab_CardType = card.Mab_Card.Mab_CardType,
                Mab_CardLevel = card.Mab_Card.Mab_CardLevel,
                Mab_CardPower = card.Mab_Card.Mab_CardPower,
                Mab_CardUpperHand = card.Mab_Card.Mab_CardUpperHand
            })
            .ToList();

            return (response, "Mab Deck Booster acquired succesfully!");
        }
        private static (bool, string) MabBuyDeckBooster_Validation(UsersMabBuyDeckBoosterRequest? request)
        {
            if (request != null)
            {
                return (false, "Error: MabBuyDeckBooster failed! Request is NOT null, however it MUST be null!");
            }          

            return (true, string.Empty);
        }
        private async Task<(List<MabPlayerCard>?, string)> GetBoosterCards()
        {
            var mabCardsDB = await this._daoDbContext
                .MabCards             
                .ToListAsync();

            if (mabCardsDB == null || mabCardsDB.Count < 1)
            {
                return (null, "Error: MabBuyDeckBooster failed! Mab Cards not found!");
            }

            var mabCard_BriefMomentOfPeace = mabCardsDB.FirstOrDefault(card => card.Mab_CardLevel == 0 && card.Mab_CardType == MabCardType.Neutral);
            var mabCardsLvlZero = mabCardsDB.Where(card => card.Mab_CardLevel == 0 && card.Mab_CardType != MabCardType.Neutral).ToList();
            var mabCardsLvlOne = mabCardsDB.Where(card => card.Mab_CardLevel == 1).ToList();
            var mabCardsLvlTwo = mabCardsDB.Where(card => card.Mab_CardLevel == 2).ToList();
            var mabCardsLvlThree = mabCardsDB.Where(card => card.Mab_CardLevel == 3).ToList();
            var mabCardsLvlFour = mabCardsDB.Where(card => card.Mab_CardLevel == 4).ToList();
            var mabCardsLvlFive = mabCardsDB.Where(card => card.Mab_CardLevel == 5).ToList();
                    

            var randomNumbers = new List<int>();

            var booster = new List<MabPlayerCard>();

            var count = 0;
            
            while(count < Constants.BoosterSize)
            {
                randomNumbers.Add(random.Next(1, 401));
                count++;
            }

            foreach(int randomNumber in randomNumbers)
            {
                switch (randomNumber)
                {
                    case int n when (n >= 1 && n <= 2): // 0,05%, 2 em 400
                        booster.Add(new MabPlayerCard
                        {
                            Mab_Card = mabCard_BriefMomentOfPeace
                        });
                        break;                    
                    case int n when (n >= 3 && n <= 280): // 69,5%, 278 em 400
                        booster.Add(new MabPlayerCard
                        {
                            Mab_Card = mabCardsLvlZero.OrderBy(card => random.Next()).FirstOrDefault()
                        });
                        break;
                    case int n when (n >= 281 && n <= 320): // 10%, 40 em 400
                        booster.Add(new MabPlayerCard
                        {
                            Mab_Card = mabCardsLvlOne.OrderBy(card => random.Next()).FirstOrDefault()
                        });
                        break;
                    case int n when (n >= 321 && n <= 352): // 8%, 32 em 400
                        booster.Add(new MabPlayerCard
                        {
                            Mab_Card = mabCardsLvlTwo.OrderBy(card => random.Next()).FirstOrDefault()
                        });
                        break;
                    case int n when (n >= 353 && n <= 376): // 6%, 24 em 400
                        booster.Add(new MabPlayerCard
                        {
                            Mab_Card = mabCardsLvlThree.OrderBy(card => random.Next()).FirstOrDefault()
                        });
                        break;
                    case int n when (n >= 377 && n <= 392): // 4%, 16 em 400
                        booster.Add(new MabPlayerCard
                        {
                            Mab_Card = mabCardsLvlFour.OrderBy(card => random.Next()).FirstOrDefault()
                        });
                        break;
                    case int n when (n >= 393 && n <= 400): // 2%, 8 em 400
                        booster.Add(new MabPlayerCard
                        {
                            Mab_Card = mabCardsLvlFive.OrderBy(card => random.Next()).FirstOrDefault()
                        });
                        break;           
                }

            }

            return (booster, string.Empty);
        }
        #endregion
    }
}