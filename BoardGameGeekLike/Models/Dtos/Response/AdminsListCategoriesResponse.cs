namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsListCategoriesResponse
    {
        public int? CategoryId { get; set; }
        public string? Name { get; set; }    
        public bool? IsDeleted { get; set; }
    }
}
