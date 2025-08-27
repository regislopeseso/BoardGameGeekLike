namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersResolveMabBattleDuelResponse
    {
        public int MabPlayerDuellingCardCopyFullPower { get; set; }

        public int MabNpcDuellingDeckEntryFullPower { get; set; }

        public int MabBattleRoundPoints { get; set; }

        public int NextRoundNumber { get; set; }
    }
}
