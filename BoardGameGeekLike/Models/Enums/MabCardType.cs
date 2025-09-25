namespace BoardGameGeekLike.Models.Enums
{
    public enum MabCardType
    {
        Neutral = 0,  // > Neutral
        
        Ranged = 1,   // > Infantry, Neutral
        
        Cavalry = 2,  // > Ranged, Neutral
        
        Infantry = 3, // > Cavalry, Neutral
    }
}
