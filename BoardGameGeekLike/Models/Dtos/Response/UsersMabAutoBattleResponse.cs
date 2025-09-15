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
    public bool? Mab_DoesPlayerGoFirst { get; set; }



    public string? Mab_PlayerNickname { get; set; }

    public int? Mab_PlayerLevel { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MabPlayerState? Mab_FinalPlayerState { get; set; }

    public List<UsersMabAutoBattleResponse_playerCard>? Mab_PlayerCards { get; set; }



    public string? Mab_NpcName { get; set; }

    public int? Mab_NpcLevel { get; set; }    

    public List<UsersMabAutoBattleResponse_npcCard>? Mab_NpcCards { get; set; }
  }
}