namespace BoardGameGeekLike.Utilities
{
    public static class Constants
    {

        public const string User_ProfileDetails_TableTitle = ";TABLE #1: USER DETAILS";
        public const string User_ProfileDetails_TableHeaders = ";Name;Email;BirthDate;Gender;Sign Up Date";

        public const string User_LifeCounter_Templates_TableTitle = ";TABLE #2: LIFE COUNTER TEMPLATES";
        public const string User_LifeCounter_Templates_TableHeader = ";LIFE COUNTER TEMPLATE NAME;PLAYERS STARTING LIFE POINTS;PLAYERS COUNT;FIXED MAX LIFE POINTS MODE;PLAYERS MAX LIFE POINTS;AUTO DEFEAT MODE;AUTO END MODE;LIFE COUNTER MANAGER COUNT";

        public const string User_LifeCounter_Managers_TableTitle = ";TABLE #3: LIFE COUNTER MANAGERS";
        public const string User_LifeCounter_Managers_TableHeader = ";LIFE COUNTER TEMPLATE NAME;LIFE COUNTER MANAGER NAME;PLAYERS STARTING LIFE POINTS;PLAYERS COUNT;FIRST PLAYER INDEX;FIXED MAX LIFE POINTS MODE;PLAYERS MAX LIFE POINTS;AUTO DEFEAT MODE;AUTO END MODE;STARTING TIME MARK;ENDING TIME MARK;DURATION (MINUTES);IS FINISHED";

        public const string User_LifeCounter_Players_TableTitle = ";TABLE #4: LIFE COUNTER PLAYERS";
        public const string User_LifeCounter_Players_TableHeader = ";LIFE COUNTER MANAGER NAME;PLAYER NAME;STARTING LIFE POINTS;CURRENT LIFE POINTS;FIXED MAX LIFE POINTS MODE;MAX LIFE POINTS;AUTO DEFEAT MODE;IS DEFEATED";

        public const string User_BoardGame_Sessions_TableTitle = ";TABLE #5: BOARD GAME SESSIONS";
        public const string User_BoardGame_Sessions_TableHeader = ";BOARD GAME NAME;DATE;PLAYER COUNT;DURATION (MINUTES);IS DELETED";

        public const string User_BoardGame_Ratings_TableTitle = ";TABLE #6: BOARD GAME RATINGS";
        public const string User_BoardGame_Ratings_TableHeader = ";BOARD GAME NAME;RATE";



        public const int MaxPlayerLevel = 9;
        public const int MinPlayerLevel = 0;

        public const int MaxNpcLevel = 9;
        public const int MinNpcLevel = 0;

        public const int MaxCardLevel = 9;
        public const int MinCardLevel = 0;

        public const int MaxCardPower = 9;
        public const int MinCardPower = 0;

        public const int MaxCardUpperHand = 9;
        public const int MinCardUpperHand = 0;

        public const int DeckSize = 5;

        public const int BoosterPrice = 5;
        public const int BoosterSize = 3;
    }
}
