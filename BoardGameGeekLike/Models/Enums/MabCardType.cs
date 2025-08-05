namespace BoardGameGeekLike.Models.Enums
{
    public enum MabCardType
    {
        Neutral = 0,
        Ranged = 1,   // > Infantry
        Cavalry = 2,  // > Ranged
        Infantry = 3, // > Cavalry
    }
}
