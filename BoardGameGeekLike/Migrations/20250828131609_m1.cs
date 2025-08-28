using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    /// <inheritdoc />
    public partial class m1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SignUpDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDummy = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDummy = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Power = table.Column<int>(type: "int", nullable: false),
                    UpperHand = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDummy = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabCards", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabNpcs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDummy = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabNpcs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mechanics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDummy = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mechanics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LifeCounterTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LifeCounterTemplateName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayersStartingLifePoints = table.Column<int>(type: "int", nullable: true),
                    PlayersCount = table.Column<int>(type: "int", nullable: true),
                    FixedMaxLifePointsMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    PlayersMaxLifePoints = table.Column<int>(type: "int", nullable: true),
                    AutoDefeatMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    AutoEndMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    LifeCounterManagersCount = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeCounterTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LifeCounterTemplates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerNickName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayerLevel = table.Column<int>(type: "int", nullable: true),
                    Difficulty = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    GoldStash = table.Column<int>(type: "int", nullable: true),
                    GoldValue = table.Column<double>(type: "double", nullable: true),
                    CountMatches = table.Column<int>(type: "int", nullable: true),
                    CountVictories = table.Column<int>(type: "int", nullable: true),
                    CountDefeats = table.Column<int>(type: "int", nullable: true),
                    CountBoosters = table.Column<int>(type: "int", nullable: true),
                    AllCardsCollectedTrophy = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    AllNpcsDefeatedTrophy = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabCampaigns_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "boardgames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinPlayersCount = table.Column<int>(type: "int", nullable: false),
                    MaxPlayersCount = table.Column<int>(type: "int", nullable: false),
                    MinAge = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    AverageRating = table.Column<decimal>(type: "decimal(2,1)", nullable: false),
                    RatingsCount = table.Column<int>(type: "int", nullable: false),
                    AvgDuration_minutes = table.Column<int>(type: "int", nullable: false),
                    SessionsCount = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDummy = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boardgames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_boardgames_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabNpcCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Mab_CardId = table.Column<int>(type: "int", nullable: false),
                    Mab_NpcId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabNpcCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabNpcCards_MabCards_Mab_CardId",
                        column: x => x.Mab_CardId,
                        principalTable: "MabCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MabNpcCards_MabNpcs_Mab_NpcId",
                        column: x => x.Mab_NpcId,
                        principalTable: "MabNpcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LifeCounterManagers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LifeCounterManagerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayersStartingLifePoints = table.Column<int>(type: "int", nullable: true),
                    PlayersCount = table.Column<int>(type: "int", nullable: true),
                    FirstPlayerIndex = table.Column<int>(type: "int", nullable: true),
                    FixedMaxLifePointsMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    PlayersMaxLifePoints = table.Column<int>(type: "int", nullable: true),
                    AutoDefeatMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    AutoEndMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    StartingTime = table.Column<long>(type: "bigint", nullable: true),
                    EndingTime = table.Column<long>(type: "bigint", nullable: true),
                    Duration_minutes = table.Column<double>(type: "double", nullable: true),
                    IsFinished = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LifeCounterTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeCounterManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LifeCounterManagers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LifeCounterManagers_LifeCounterTemplates_LifeCounterTemplate~",
                        column: x => x.LifeCounterTemplateId,
                        principalTable: "LifeCounterTemplates",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabBattles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsPlayerTurn = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    BattlePoints = table.Column<int>(type: "int", nullable: true),
                    HasPlayerWon = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    IsFinished = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Results = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mab_CampaignId = table.Column<int>(type: "int", nullable: false),
                    Mab_NpcId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabBattles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabBattles_MabCampaigns_Mab_CampaignId",
                        column: x => x.Mab_CampaignId,
                        principalTable: "MabCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MabBattles_MabNpcs_Mab_NpcId",
                        column: x => x.Mab_NpcId,
                        principalTable: "MabNpcs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabPlayerCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Mab_CardId = table.Column<int>(type: "int", nullable: true),
                    Mab_CampaignId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabPlayerCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabPlayerCards_MabCampaigns_Mab_CampaignId",
                        column: x => x.Mab_CampaignId,
                        principalTable: "MabCampaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MabPlayerCards_MabCards_Mab_CardId",
                        column: x => x.Mab_CardId,
                        principalTable: "MabCards",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabPlayerDecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Mab_CampaignId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabPlayerDecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabPlayerDecks_MabCampaigns_Mab_CampaignId",
                        column: x => x.Mab_CampaignId,
                        principalTable: "MabCampaigns",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BoardGameMechanic",
                columns: table => new
                {
                    BoardGamesId = table.Column<int>(type: "int", nullable: false),
                    MechanicsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardGameMechanic", x => new { x.BoardGamesId, x.MechanicsId });
                    table.ForeignKey(
                        name: "FK_BoardGameMechanic_boardgames_BoardGamesId",
                        column: x => x.BoardGamesId,
                        principalTable: "boardgames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardGameMechanic_mechanics_MechanicsId",
                        column: x => x.MechanicsId,
                        principalTable: "mechanics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Rate = table.Column<decimal>(type: "decimal(2,1)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BoardGameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ratings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ratings_boardgames_BoardGameId",
                        column: x => x.BoardGameId,
                        principalTable: "boardgames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BoardGameId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    PlayersCount = table.Column<int>(type: "int", nullable: false),
                    Duration_minutes = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sessions_boardgames_BoardGameId",
                        column: x => x.BoardGameId,
                        principalTable: "boardgames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LifeCounterPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartingLifePoints = table.Column<int>(type: "int", nullable: true),
                    CurrentLifePoints = table.Column<int>(type: "int", nullable: true),
                    FixedMaxLifePointsMode = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxLifePoints = table.Column<int>(type: "int", nullable: true),
                    AutoDefeatMode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    IsDefeated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LifeCounterManagerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeCounterPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LifeCounterPlayers_LifeCounterManagers_LifeCounterManagerId",
                        column: x => x.LifeCounterManagerId,
                        principalTable: "LifeCounterManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabDuels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlayerCardId = table.Column<int>(type: "int", nullable: true),
                    NpcCardId = table.Column<int>(type: "int", nullable: true),
                    HasPlayerWon = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    DuelPoints = table.Column<int>(type: "int", nullable: true),
                    Mab_BattleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabDuels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabDuels_MabBattles_Mab_BattleId",
                        column: x => x.Mab_BattleId,
                        principalTable: "MabBattles",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MabPlayerAssignedCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Mab_PlayerCardId = table.Column<int>(type: "int", nullable: true),
                    Mab_PlayerDeckId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MabPlayerAssignedCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MabPlayerAssignedCards_MabPlayerCards_Mab_PlayerCardId",
                        column: x => x.Mab_PlayerCardId,
                        principalTable: "MabPlayerCards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MabPlayerAssignedCards_MabPlayerDecks_Mab_PlayerDeckId",
                        column: x => x.Mab_PlayerDeckId,
                        principalTable: "MabPlayerDecks",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardGameMechanic_MechanicsId",
                table: "BoardGameMechanic",
                column: "MechanicsId");

            migrationBuilder.CreateIndex(
                name: "IX_boardgames_CategoryId",
                table: "boardgames",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LifeCounterManagers_LifeCounterTemplateId",
                table: "LifeCounterManagers",
                column: "LifeCounterTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LifeCounterManagers_UserId",
                table: "LifeCounterManagers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LifeCounterPlayers_LifeCounterManagerId",
                table: "LifeCounterPlayers",
                column: "LifeCounterManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_LifeCounterTemplates_UserId",
                table: "LifeCounterTemplates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MabBattles_Mab_CampaignId",
                table: "MabBattles",
                column: "Mab_CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_MabBattles_Mab_NpcId",
                table: "MabBattles",
                column: "Mab_NpcId");

            migrationBuilder.CreateIndex(
                name: "IX_MabCampaigns_UserId",
                table: "MabCampaigns",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MabDuels_Mab_BattleId",
                table: "MabDuels",
                column: "Mab_BattleId");

            migrationBuilder.CreateIndex(
                name: "IX_MabNpcCards_Mab_CardId",
                table: "MabNpcCards",
                column: "Mab_CardId");

            migrationBuilder.CreateIndex(
                name: "IX_MabNpcCards_Mab_NpcId",
                table: "MabNpcCards",
                column: "Mab_NpcId");

            migrationBuilder.CreateIndex(
                name: "IX_MabPlayerAssignedCards_Mab_PlayerCardId",
                table: "MabPlayerAssignedCards",
                column: "Mab_PlayerCardId");

            migrationBuilder.CreateIndex(
                name: "IX_MabPlayerAssignedCards_Mab_PlayerDeckId",
                table: "MabPlayerAssignedCards",
                column: "Mab_PlayerDeckId");

            migrationBuilder.CreateIndex(
                name: "IX_MabPlayerCards_Mab_CampaignId",
                table: "MabPlayerCards",
                column: "Mab_CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_MabPlayerCards_Mab_CardId",
                table: "MabPlayerCards",
                column: "Mab_CardId");

            migrationBuilder.CreateIndex(
                name: "IX_MabPlayerDecks_Mab_CampaignId",
                table: "MabPlayerDecks",
                column: "Mab_CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ratings_BoardGameId",
                table: "ratings",
                column: "BoardGameId");

            migrationBuilder.CreateIndex(
                name: "IX_ratings_UserId",
                table: "ratings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_BoardGameId",
                table: "sessions",
                column: "BoardGameId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId",
                table: "sessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BoardGameMechanic");

            migrationBuilder.DropTable(
                name: "LifeCounterPlayers");

            migrationBuilder.DropTable(
                name: "MabDuels");

            migrationBuilder.DropTable(
                name: "MabNpcCards");

            migrationBuilder.DropTable(
                name: "MabPlayerAssignedCards");

            migrationBuilder.DropTable(
                name: "ratings");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "mechanics");

            migrationBuilder.DropTable(
                name: "LifeCounterManagers");

            migrationBuilder.DropTable(
                name: "MabBattles");

            migrationBuilder.DropTable(
                name: "MabPlayerCards");

            migrationBuilder.DropTable(
                name: "MabPlayerDecks");

            migrationBuilder.DropTable(
                name: "boardgames");

            migrationBuilder.DropTable(
                name: "LifeCounterTemplates");

            migrationBuilder.DropTable(
                name: "MabNpcs");

            migrationBuilder.DropTable(
                name: "MabCards");

            migrationBuilder.DropTable(
                name: "MabCampaigns");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
