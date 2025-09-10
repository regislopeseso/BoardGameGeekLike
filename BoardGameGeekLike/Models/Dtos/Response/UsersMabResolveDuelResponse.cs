namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabResolveDuelResponse
    {
        public int? Mab_PlayerCardFullPower { get; set; }

        public int? Mab_NpcCardFullPower { get; set; }

        public int? Mab_DuelPoints { get; set; }

        public int? Mab_EarnedXp { get; set; }

        public int? Mab_BonusXp { get; set; }

        public bool? Mab_HasPlayerWon {  get; set; }
    }
}
