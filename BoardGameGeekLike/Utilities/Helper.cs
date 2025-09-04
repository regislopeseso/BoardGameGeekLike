namespace BoardGameGeekLike.Utilities
{
    public static class Helper
    {
        public static string BoolToEnabledDisabled(bool? value)
        {
            return value == true ? "ENABLED" : "DISABLED";
        }

        public static bool? ParseEnabledDisabledToBool(string? value)
        {
            var normalized = value?.Trim().ToLowerInvariant();

            return normalized switch
            {
                "enabled" => true,
                "disabled" => false,
                _ => throw new FormatException($"Invalid value '{value}' — expected 'Enabled' or 'Disabled'")
            };
        }

        public static int MabGetCardLevel(int power, int upperHand)
        {
            return (int)Math.Ceiling((double)(power + upperHand) / 2d);
        }

        public static int MabGetNpcLevel(List<int> cardLevels)
        {
            return (int)Math.Ceiling((cardLevels.Sum() / (double)cardLevels.Count));
        }

        public static int MabGetPlayerLevel(int? playerXp)
        {
            if(playerXp >= Constants.LevelOneExpThreshold &&
                playerXp < Constants.LevelTwoExpThreshold)
            {
                return 1;
            }
            else if (playerXp >= Constants.LevelTwoExpThreshold &&
                           playerXp < Constants.LevelThreeExpThreshold)
            {
                return 2;
            }
            else if (playerXp >= Constants.LevelThreeExpThreshold &&
                           playerXp < Constants.LevelFourExpThreshold)
            {
                return 3;
            }
            else if (playerXp >= Constants.LevelFourExpThreshold &&
                           playerXp < Constants.LevelFiveExpThreshold)
            {
                return 4;
            }
            else if (playerXp >= Constants.LevelFiveExpThreshold &&
                           playerXp < Constants.LevelSixExpThreshold)
            {
                return 5;
            }
            else if (playerXp >= Constants.LevelSixExpThreshold &&
                           playerXp < Constants.LevelSevenExpThreshold)
            {
                return 6;
            }
            else if (playerXp >= Constants.LevelSevenExpThreshold &&
                           playerXp < Constants.LevelEightExpThreshold)
            {
                return 7;
            }
            else if (playerXp >= Constants.LevelEightExpThreshold &&
                           playerXp < Constants.LevelNineExpThreshold)
            {
                return 8;
            }
            else if (playerXp >= Constants.LevelNineExpThreshold &&
                                       playerXp < Constants.LevelTenExpThreshold)
            {
                return 9;
            }
            else if (playerXp >= Constants.LevelTenExpThreshold)
            {
                return 10;
            }
            else
            {
                return 0;
            }
        }

        public static int MabGetPlayerDeckLevel(List<int> cardLevels)
        {
            return (int)Math.Ceiling((cardLevels.Sum() / (double)cardLevels.Count));
        }

        public static List<int> MabGetPowerSequence(int level, int pos)
        {
            var validCardLvlSequence = new List<int>();
            switch (pos)
            {
                case 1:
                    //Ex. level == 6
                    //(4, 4, 6, 8, 8 )
                    //level-2, level-2, level,   level+2, level+2
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level + 2);
                    validCardLvlSequence.Add(level + 2);
                    break;
                case 2:
                    //  Ex. level == 6
                    //  (4, 4, 7, 7, 8 )
                    //  level-2, level-2, level+1, level+1, level+2
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 2);
                    break;
                case 3:
                    //  Ex. level == 6
                    //  (4, 5, 5, 8, 8 )
                    //  level-2, level-1, level-1, level+2, level+2
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 2);
                    validCardLvlSequence.Add(level + 2);
                    break;
                case 4:
                    //  Ex. level == 6
                    //  4, 5, 6, 7, 8
                    //  level-2, level-1, level, level+1, level+2
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 2);
                    break;
                case 5:
                    //  Ex. level == 6
                    //  4, 5, 7, 7, 7
                    //  level-2, level-1, level+1, level+1, level+1
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 1);
                    break;
                case 6:
                    //  Ex. level == 6
                    //  4, 6, 6, 6, 8
                    //  level-2, level, level, level, level+2
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level + 2);
                    break;
                case 7:
                    //  Ex. level == 6
                    //  4, 6, 6, 7, 7
                    //  level-2, level, level, level+1, level+1
                    validCardLvlSequence.Add(level - 2);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 1);
                    break;
                case 8:
                    //  Ex. level == 6
                    //  5, 5, 5, 7, 8
                    //  level-1, level-1, level-1, level+1, level+2
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 2);
                    break;
                case 9:
                    //  Ex. level == 6
                    //  5, 5, 6, 6, 8
                    //  level-1, level-1, level, level, level+2
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level + 2);
                    break;
                case 10:
                    //  Ex. level == 6
                    //  5, 5, 6, 7, 7
                    //  level-1, level-1, level, level+1, level+1
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level + 1);
                    validCardLvlSequence.Add(level + 1);
                    break;
                case 11:
                    //  //  Ex. level == 6
                    //  5, 6, 6, 6, 7
                    //  level-1, level, level, level, level+1 
                    validCardLvlSequence.Add(level - 1);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level + 1);
                    break;
                default:
                    //  //  Ex. level == 6
                    //  6, 6, 6, 6, 6
                    //  level, level, level, level, level
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level);
                    validCardLvlSequence.Add(level); ;
                    break;
            }
            return validCardLvlSequence;
        }


        public static int MabGetCardFullPower(int power, int upperhand, int firstType, int secondType)
        {
            return (firstType, secondType) switch
            {
                (0, 0) => power,
                (0, 1) => power,
                (0, 2) => power,
                (0, 3) => power,
                (1, 0) => power + 2*upperhand,
                (1, 1) => power,
                (1, 2) => power + upperhand,
                (1, 3) => power,
                (2, 0) => power + 2*upperhand,
                (2, 1) => power,
                (2, 2) => power,
                (2, 3) => power + upperhand,
                (3, 0) => power + 2*upperhand,
                (3, 1) => power + upperhand,
                (3, 2) => power,
                (3, 3) => power,
                _ => power
            };
        }

        public static int MabGetDuelPoints(int playerFullPower, int npcFullPower)
        {
            return playerFullPower - npcFullPower;
        }

        public static int MabGetEarnedGold(int duelPoints)
        {
            if (duelPoints > 0 && duelPoints < 10)
            {
                return 1;
            }
            else if (duelPoints > 10 && duelPoints < 15)
            {
                return 2;
            }
            else if (duelPoints > 15)
            {
                return 3;
            }
            else
            {
                return -1;
            }
        }

        public static (int, int) MabGetEarnedXp(int playerLevel, int npcLevel, int duelPoints, int playerState)
        {
            if (duelPoints < 0) return (0, 0);
            {
                // --- Tunables ---
                const double Alpha = 2.0;   // baseline XP
                const double Beta = 1.0;    // scales with sqrt(duelPoints)
                const double Gamma = 0.25;  // bonus per NPC level >= player
                const double Eta = 0.20;    // penalty per NPC level < player
                const double MMin = 0.25;   // minimum level multiplier
                const double MMax = 1.50;   // maximum level multiplier

                // State multipliers (index by playerState enum int value)
                double[] StateMultiplier =  
                {
                    1.00, // None
                    1.10, // Flawless
                    1.20, // Matchless
                    1.30, // Impredictable
                    1.40, // Unstoppable
                    1.55, // Triumphant
                    1.75  // Glorious
                };

                int delta = npcLevel - playerLevel;

                // Base XP from duel performance
                double baseXp = Alpha + Beta * Math.Sqrt(Math.Max(0, duelPoints));

                // Level difference multiplier
                double mlvl = (delta >= 0)
                    ? (1.0 + Gamma * delta)
                    : (1.0 / (1.0 + Eta * Math.Abs(delta)));
                mlvl = Math.Clamp(mlvl, MMin, MMax);

                // Calculate XP without state multiplier first
                double xpWithoutState = baseXp * mlvl;

                // State multiplier
                double mstate = (playerState >= 0 && playerState < StateMultiplier.Length)
                    ? StateMultiplier[playerState]
                    : 1.0;

                // Calculate final XP with state multiplier
                double finalXp = xpWithoutState * mstate;

                // Calculate gained and bonus XP
                int gainedXp = (int)Math.Floor(Math.Max(0.0, finalXp));
                int bonusXp = (int)Math.Floor(Math.Max(0.0, finalXp - xpWithoutState));

                return (gainedXp, bonusXp);
            }
        }
        }

    }

