using BoardGameGeekLike.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
  public class UsersMabAutoBattleResponse
  {
    public string? PlayerNickname { get; set; }

    public int? PlayerLevel { get; set; }

    public string? NpcName { get; set; }

    public int? NpcLevel { get; set; }

    public bool? Mab_HasPlayerWon {  get; set; }

    public int? Mab_BattleResults { get; set;}

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MabPlayerState? Mab_FinalPlayerState { get; set; }

    public List<UsersMabAutoBattleResponse_playerCard>? Mab_PlayerCards { get; set; }

    public List<UsersMabAutoBattleResponse_npcCard>? Mab_NpcCards { get; set; }

    public List<UsersMabAutoBattleResponse_duelResult>? Mab_DuelResults { get; set; }
  }
}