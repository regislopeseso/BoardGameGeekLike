using BoardGameGeekLike.Models.Entities;
using BoardGameGeekLike.Models.Enums;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

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

        public static int MabGetPlayerNextLevelThreshold(int? playerLevel)
        {
            return (playerLevel) switch
            {
                0 => Constants.LevelOneExpThreshold,
                1 => Constants.LevelTwoExpThreshold,
                2 => Constants.LevelThreeExpThreshold,
                3 => Constants.LevelFourExpThreshold,
                4 => Constants.LevelFiveExpThreshold,
                5 => Constants.LevelSixExpThreshold,
                6 => Constants.LevelSevenExpThreshold,
                7 => Constants.LevelEightExpThreshold,
                8 => Constants.LevelNineExpThreshold,
                9 => Constants.LevelTenExpThreshold,
                _ => 999999,
            };
        }

        public static MabPlayerState MabGetPlayerState 
        (
            List<int?> orderedMabPlayerCardIds, 
            List<bool> orderedMabDuelResults, 
            bool? isMabBattleOvercome
        )
        {
            if(orderedMabPlayerCardIds == null || 
                orderedMabPlayerCardIds.Count == 0 ||
                orderedMabDuelResults == null ||
                orderedMabDuelResults.Count(a => a == true) < 1)
            {
                return MabPlayerState.Normal;
            }

            var winningStreak = 0;

            foreach (var duelResult in orderedMabDuelResults)
            {
                if(duelResult == null)
                {
                    continue;
                }
                    
                winningStreak =
                    duelResult == true ?
                    winningStreak + 1 : 0;
            }

            MabPlayerState mabPlayerState =
                winningStreak switch
                {
                    1 => MabPlayerState.Flawless,
                    2 => MabPlayerState.Matchless,
                    3 => MabPlayerState.Impredictable,
                    4 => MabPlayerState.Unstoppable,
                    5 => MabPlayerState.Triumphant,
                    _ => MabPlayerState.Normal,
                };

            if (isMabBattleOvercome == true &&
                 mabPlayerState == MabPlayerState.Triumphant &&
                 orderedMabPlayerCardIds.Skip(1).All(a => a == 0))
            {
                mabPlayerState = MabPlayerState.Glorious;
            }

            return mabPlayerState;
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


        public static (int, int) MabResolveDuel(int attackerCardPower, int attackerCardUpperHand, MabCardType? attackerCardType, int defenderCardPower, int defenderCardUpperHand, MabCardType? defenderCardType, bool? isPlayerAttacking)
        {
            if((attackerCardType == MabCardType.Neutral && attackerCardPower == 0 && attackerCardUpperHand == 0) || (
                defenderCardType == MabCardType.Neutral && defenderCardPower == 0 && defenderCardUpperHand == 0))
            {
                return (0, 0);
            }

            var attackerCardFullPower = MabGetCardFullPower(attackerCardPower, attackerCardUpperHand, attackerCardType, defenderCardType);

            var duelPoints = MabGetDuelPoints(attackerCardFullPower, defenderCardPower, isPlayerAttacking);

            return (attackerCardFullPower, duelPoints);

        }
        public static int MabGetDuelPoints(int attackerCardFullPower, int defenderCardPower,  bool? isPlayerAttacking)
        {
            var mabDuelPoints = attackerCardFullPower - defenderCardPower;

            mabDuelPoints = isPlayerAttacking == true ?
                mabDuelPoints : -mabDuelPoints;

            return mabDuelPoints;
        }
        public static int MabGetCardFullPower(int cardPower, int cardUpperHand, MabCardType? attackerCardType, MabCardType? defenderCardType)
        {
            return (attackerCardType, defenderCardType) switch
            {
                (MabCardType.Neutral, MabCardType.Neutral) => cardPower + cardUpperHand,
                (MabCardType.Neutral, MabCardType.Ranged) => cardPower,
                (MabCardType.Neutral, MabCardType.Cavalry) => cardPower,
                (MabCardType.Neutral, MabCardType.Infantry) => cardPower,

                (MabCardType.Ranged, MabCardType.Neutral) => cardPower + cardUpperHand,
                (MabCardType.Ranged, MabCardType.Ranged) => cardPower,
                (MabCardType.Ranged, MabCardType.Cavalry) => cardPower,
                (MabCardType.Ranged, MabCardType.Infantry) => cardPower + cardUpperHand,

                (MabCardType.Cavalry, MabCardType.Neutral) => cardPower + cardUpperHand,
                (MabCardType.Cavalry, MabCardType.Ranged) => cardPower + cardUpperHand,
                (MabCardType.Cavalry, MabCardType.Cavalry) => cardPower,
                (MabCardType.Cavalry, MabCardType.Infantry) => cardPower,

                (MabCardType.Infantry, MabCardType.Neutral) => cardPower + cardUpperHand,
                (MabCardType.Infantry, MabCardType.Ranged) => cardPower,
                (MabCardType.Infantry, MabCardType.Cavalry) => cardPower + cardUpperHand,
                (MabCardType.Infantry, MabCardType.Infantry) => cardPower,
                _ => cardPower + 3 * cardUpperHand, 
            };
        }     

        public static int? MabGetEarnedCoins(int? duelPoints)
        {
            if (duelPoints >= 0 && duelPoints < 5)
            {
                return duelPoints * 4;
            }
            else if (duelPoints >= 5 && duelPoints < 10)
            {
                return duelPoints * 3;
            }
            else if (duelPoints >= 10 && duelPoints < 15)
            {
                return duelPoints * 2;
            }
            else if (duelPoints >= 15 && duelPoints < 20)
            {
                return duelPoints;
            }
            else if (duelPoints >= 20)
            {
                return 2;
            }
            else
            {
                return -1;
            }
        }

        public static (int, int) MabGetEarnedXp(int playerLevel, int npcLevel, int duelPoints, int playerState)
        {
            if (duelPoints <= 0)
            {
                return (0, 0);
            }

            var lvlDif = npcLevel - playerLevel;
            var xpPenalty = 0;

            if(lvlDif < 0)
            {
                xpPenalty = Math.Abs(lvlDif * 10); //Min. 5, Max. 90


                lvlDif = 0;
            }

            int basisXp = 1;

            int earnedXp = basisXp + duelPoints - xpPenalty;

            earnedXp = earnedXp < 0 ? 0 : earnedXp;

            double bonusXpFormula = earnedXp * (1 / (playerState + lvlDif));

            int bonusXp = (int)Math.Ceiling(bonusXpFormula);        

            return (earnedXp, bonusXp);            
        }
    
        
        public static bool MabIsPlayerAttacking(bool wasPlayerFirstAttacker, int currentDuelNumber)
        {
            currentDuelNumber = currentDuelNumber == 0 ? 1 : currentDuelNumber;

            var isCurrrentRoundNumberEven = currentDuelNumber % 2 == 0;

            var isPlayerAttacking = (wasPlayerFirstAttacker, isCurrrentRoundNumberEven) switch
            {
                (true, true) => false,
                (true, false) => true,
                (false, true) => true,
                (false, false) => false,
            };           

            return isPlayerAttacking;
        }


        public static (List<int?>?, string) MabEvaluateForgingCosts(int? ownedCoins, int? cardPower, int? ownedMaterial)
        {
            if (ownedCoins < 1)
            {
                return (null, "MabEvaluateForgingCosts failed! No coins available");
            }

            List<int?>? results = new();

            var improvedPower = cardPower + 1;         
         
            var coinsCost = cardPower switch
            {
                0 => 1,
                1 => 2,
                2 => 4,
                3 => 8,
                4 => 16,
                5 => 32,
                6 => 64,
                7 => 128,
                8 => 256,
                9 => 512,
                _ => 999,
            };

            if (ownedCoins < coinsCost)
            {
                return (null, "MabEvaluateForgingCosts failed! Not enough coins available");
            }

            var materialCost = cardPower switch
            {
                0 => 1,
                1 => 2,
                2 => 5,
                3 => 10,
                4 => 15,
                5 => 30,
                6 => 60,
                7 => 100,
                8 => 150,
                9 => 200,
                _ => 999,
            };

            if (ownedMaterial < materialCost)
            {
                return (null, "MabEvaluateForgingCosts failed! Not enough raw material!");
            }

            results.Add(improvedPower);

            results.Add(materialCost);

            results.Add(materialCost);           

            return (results, string.Empty);
        }

        public static (List<int?>?, string) MabEvaluateSharpeningCosts(int? ownedCoins, int? xp,  int? cardUpperHand)
        {
            if (ownedCoins < 1)
            {
                return (null, "MabEvaluateSharpeningCosts failed! No coins available");
            }

            var improvedUpperHand = cardUpperHand + 1;

            List<int?>? results = new();

            var coinsCost = cardUpperHand switch
            {
                0 => 1,
                1 => 2,
                2 => 4,
                3 => 8,
                4 => 16,
                5 => 32,
                6 => 64,
                7 => 128,
                8 => 256,
                9 => 512,
                _ => 999,
            };

            var xpCost = cardUpperHand switch
            {
                0 => 2,
                1 => 10,
                2 => 25,
                3 => 50,
                4 => 75,
                5 => 125,
                6 => 150,
                7 => 175,
                8 => 200,
                9 => 250,
                _ => 999,
            };

            
            if (ownedCoins < coinsCost)
            {
                return (null, "MabEvaluateSharpeningCosts failed! Not enough coins available");
            }

            if (xp < xpCost)
            {
                return (null, "MabEvaluateSharpeningCosts failed! Not enough XP!");
            }

            results.Add(improvedUpperHand);

            results.Add(coinsCost);

            results.Add(xpCost);

            return (results, string.Empty);       
        }

        public static (List<int?>?, string) MabEvaluateMeltingCost(int? ownedCoins, int? cardPower, int? cardUpperHand)
        {
            if (ownedCoins < 1)
            {
                return (null, "MabEvaluateMeltingCost failed! No coins available");
            }    

            List<int?>? results = new();

            var coinsCost = (cardPower + cardUpperHand) switch
            {
                0 => 1,
                1 => 2,
                2 => 3,
                3 => 4,
                4 => 5,
                5 => 6,
                6 => 7,
                7 => 8,
                8 => 9,
                9 => 10,
                10 => 11,
                11 => 12,
                12 => 13,
                13 => 14,
                14 => 15,
                15 => 16,
                16 => 17,
                17 => 18,
                18 => 19,
                _ => 0,
            };

            var extractedRawMaterial = cardPower switch
            {           
                1 => 1,
                2 => 2,
                3 => 3,
                4 => 4,
                5 => 5,
                6 => 6,
                7 => 7,
                8 => 8,
                9 => 9,
                _ => 0,
            };

            var gainedXp = cardUpperHand switch
            {
                0 => 5,
                1 => 10,
                2 => 25,
                3 => 50,
                4 => 75,
                5 => 125,
                6 => 150,
                7 => 175,
                8 => 200,
                9 => 250,
                _ => 999,
            };
   
            results.Add(extractedRawMaterial);

            results.Add(gainedXp);

            results.Add(coinsCost);

            return (results, string.Empty);
        }

    }
}

