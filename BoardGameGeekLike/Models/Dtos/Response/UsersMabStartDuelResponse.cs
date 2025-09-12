using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabStartDuelResponse
    {
        public bool? Mab_IsPlayerAttacking { get; set; }      
    }
}
