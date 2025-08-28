namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabResolveDuelResponse
    {
        public int MabPlayerCardFullPower { get; set; }

        public int MabNpcCardFullPower { get; set; }

        public int MabBattleDuelPoints { get; set; }

        public int NextDuelNumber { get; set; }
    }
}
