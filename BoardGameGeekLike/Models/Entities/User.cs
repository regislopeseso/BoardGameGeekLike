using BoardGameGeekLike.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("users")]
    public class User : IdentityUser
    {
        public string? Name { get; set; }

        public DateOnly SignUpDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public DateOnly BirthDate { get; set; }

        public Gender Gender { get; set; }

        public bool IsDeleted { get; set; } = false;

        public bool IsDummy { get; set; } = false;

        [InverseProperty("User")]
        public List<Session>? Sessions { get; set; }


        [InverseProperty("User")]
        public List<Rating>? Ratings { get; set; }

      
        [InverseProperty(nameof(MabCampaign.User))]        
        public List<MabCampaign>? MabCampaigns { get; set; }
       

        [InverseProperty("User")]
        public List<LifeCounterTemplate>? LifeCounterTemplates { get; set; }


        [InverseProperty("User")]
        public List<LifeCounterManager>? LifeCounterManagers { get; set; }
    }
}