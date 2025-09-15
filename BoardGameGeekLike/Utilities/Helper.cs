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


        public static (int, int) MabResolveDuel(int attackerCardPower, int attackerCardUpperHand, MabCardType? attackerCardType, int defenderCardPower, MabCardType? defenderCardType, bool? isPlayerAttacking)
        {
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
                (MabCardType.Neutral, MabCardType.Neutral) => cardPower,
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
            if (duelPoints <= 0)
            {
                return (0, 0);
            }

            var lvlDif = npcLevel - playerLevel;
            if(lvlDif < 0)
            {
                lvlDif = 0;
            }

            int basisXp = 1;

            int earnedXp = basisXp + duelPoints;

            int bonusXp = playerState + lvlDif;        

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
    
    }        
}

