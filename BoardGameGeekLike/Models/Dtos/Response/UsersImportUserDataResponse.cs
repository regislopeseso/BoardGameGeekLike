namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersImportUserDataResponse
    {
        public string FileName { get; set; } = "user-data.csv";
        public string Base64Data { get; set; } = string.Empty; 
        public string ContentType { get; set; } = "text/csv"; 
    }
}
