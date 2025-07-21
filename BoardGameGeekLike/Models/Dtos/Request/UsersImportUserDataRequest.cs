namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersImportUserDataRequest
    {
        public string Base64CsvData { get; set; } = string.Empty;
    }
}
