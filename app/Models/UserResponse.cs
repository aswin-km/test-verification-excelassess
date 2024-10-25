using ExcelAssess.TestPlayer.ResponseVerification.Console.Helper;
using Newtonsoft.Json;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class UserResponse
    {
        public required UserBasicInfo UserBasicInfo { get; set; }
        public required TestResponse TestResponse { get; set; }
    }
    public class UserBasicInfo
    {
        [JsonProperty("scheduleuserid")]
        public Int64 ScheduleUserid { get; set; }
        [JsonProperty("organizationguid")]
        public Guid OrganizationGuid { get; set; }
        [JsonProperty("productid")]
        public int ProductId { get; set; }
        [JsonProperty("issubmitted")]
        public bool IsSubmitted { get; set; }
        [JsonProperty("relogintime")]
        public DateTime? ReloginTime { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "currentsection")]
        public int CurrentSection { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "ipaddress")]
        public required string IpAddress { get; set; }
        [JsonProperty("scheduleuserattemptid")]
        public Int64 ScheduleUserAttemptId { get; set; }
        [JsonProperty("tenantid")]
        public Int64 TenantId { get; set; }
        [JsonProperty("submittype")]
        public SubmitType SubmitType { get; set; }
    }
}
