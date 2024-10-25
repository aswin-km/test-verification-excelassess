using Newtonsoft.Json;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class StudentResultResponse
    {
        [JsonProperty("marksscored")]
        public decimal MarksScored { get; set; }
        [JsonProperty("percentagescored")]
        public decimal PercentageScored { get; set; }

        [JsonProperty("grade")]
        public string Grade { get; set; }
        [JsonProperty("totalmarks")]
        public decimal TotalMarks { get; set; }
        [JsonProperty("isresultprocessed")]
        public Boolean IsResultProcessed { get; set; }

        [JsonProperty("gradeschemas")]
        public List<GradeSchema> GradeSchemas { get; set; }
    }

    public class GradeSchema
    {
        [JsonProperty("grade")]
        public string Grade { get; set; }
        [JsonProperty("gradelevel")]
        public string GradeLevel { get; set; }
    }
}
