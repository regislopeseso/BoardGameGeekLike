namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsShowCategoryDetailsResponse
    {
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }  
        public bool IsDeleted { get; set; }
    }
}
