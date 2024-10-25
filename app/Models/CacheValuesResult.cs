namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class CacheValuesResult
    {
        public string? UserFullResponse { get; set; }
        public List<UserResponseMap>? UserResponseMaps { get; set; }
        public Dictionary<string, string> QuestionResponses { get; set; }
    }
}
