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

    }
}
