﻿namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterManagerDetailsResponse
    {
        public string? LifeCounterManagerName { get; set; }
        public int? PlayersCount { get; set; }
        public List<UsersGetLifeCounterManagerDetailsResponse_players>? LifeCounterManagerPlayers { get; set; }
        
        public bool? FixedMaxLifePointsMode { get; set; }
        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoEndMode { get; set; }
    }
}
