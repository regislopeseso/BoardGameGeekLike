using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabCheckDuelStatusResponse
    {
        public int? Mab_DuelsCount { get; set; }

        public bool? Mab_IsPlayerTurn {  get; set; }  

        public bool? Mab_AreTurnsFinished { get; set; }

        public bool? Mab_IsDuelResolved { get; set; }
        
        public bool? Mab_IsBattleFinished { get; set; }
    }
}
