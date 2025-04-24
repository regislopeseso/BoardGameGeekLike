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
        //public required string UserName { get; set; }
        public required string Email { get; set; }
        public DateOnly BirthDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsDummy { get; set; } = false;
    }
}