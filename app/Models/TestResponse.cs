using Newtonsoft.Json;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class TestResponseData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "choiceids")]
        public List<Int64>? ChoiceIds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "choiceguid")]
        public object? ChoiceGuid { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "choiceid")]
        public Int64? ChoiceId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "choicetext")]
        public string? ChoiceText { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "blanks")]
        public List<TestResponseBlank>? Blanks { get; set; }
    }

    public class TestResponseBlank
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "blankid")]
        public int? BlankId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "blankguid")]
        public Guid? BlankGuid { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "text")]
        public string? Text { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "optionid")]
        public int? OptionId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "optionguid")]
        public Guid? OptionGuid { get; set; }
    }
    public class TestResponse
    {
        [JsonProperty("questionid")]
        public Int64 QuestionId { get; set; }

        [JsonProperty("sectionid")]
        public Int64 SectionId { get; set; }
        [JsonProperty("questiontype")]
        public required string QuestionType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "data")]
        public TestResponseData? Data { get; set; }
        [JsonProperty("isbookmarked")]
        public bool IsBookmarked { get; set; }
        [JsonProperty("noofhintsused")]
        public Int16 NoOfHintsUsed { get; set; }
        [JsonProperty("questionguid")]
        public Guid QuestionGuid { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "timestamp")]
        public DateTime? TimeStamp { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "timespent")]
        public int TimeSpent { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isanswered")]
        public bool IsAnswered { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "servertime")]
        public DateTime? ServerTime { get; set; }
        [JsonProperty("attemptid")]
        public int AttemptId { get; set; }
    }
}
