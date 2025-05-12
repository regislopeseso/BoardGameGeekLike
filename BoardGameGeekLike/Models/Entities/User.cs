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
        public required string? Email { get; set; }
        public DateOnly SignUpDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly BirthDate { get; set; }
        public Gender Gender { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsDummy { get; set; } = false;
    }
}