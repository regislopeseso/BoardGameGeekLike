namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersExportUserDataResponse
    {
        public string FileName { get; set; } = "user-data.csv";
        public string Base64Data { get; set; } = string.Empty; // Base64 CSV
        public string ContentType { get; set; } = "text/csv";  // Optional: for frontend
    }
}
