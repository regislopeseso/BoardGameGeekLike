﻿using BoardGameGeekLike.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class DataBackUp_userObj_lifeCounterTemplate
    {      
        public string? LifeCounterTemplateName { get; set; }

        public int? PlayersStartingLifePoints { get; set; }

        public int? PlayersCount { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }

        public int? LifeCounterManagersCount { get; set; } = 0;

        public List<int>? LifeCounterManagerIds { get; set; }
    }
}
