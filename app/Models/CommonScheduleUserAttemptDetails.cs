namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public enum BrowserType
    {
        Normal = 0,
        ESSB = 1,
        Respondus = 2
    }
    public class CommonScheduleUserAttemptDetails
    {
        public Int64 ScheduleId { get; set; }
        public Guid ScheduleDetailGuid { get; set; }
        public Int64 RepositoryId { get; set; }
        public required string AssessmentFormPath { get; set; }
        public DateTime ScheduleStartDateTime { get; set; }
        public DateTime ScheduleEndDateTime { get; set; }
        public DateTime? TestStartDateTime { get; set; }
        public DateTime? TestEndDateTime { get; set; }
        public Int16? AdditionalTimeForAssessment { get; set; }
        public Int64 ScheduleUserId { get; set; }
        public Guid ScheduleUserGuid { get; set; }
        public required string ScheduledJsonPath { get; set; }
        public required string UserSymmetricKey { get; set; }
        public Boolean IsAdditionalTimeEnabled { get; set; }
        public int AssessmentDuration { get; set; }
        public int AdditionalTime { get; set; }
        public int ProductId { get; set; }
        public int OrganizationId { get; set; }
        public Guid UserGuid { get; set; }
        public Guid ScheduleUserAttemptGuid { get; set; }
        public long ScheduleUserAttemptId { get; set; }
        public required string AssessmentPath { get; set; }
        public required string RepositoryPath { get; set; }
        public Guid ScheduleGuid { get; set; }
        public Guid ProductGuid { get; set; }
        public Guid OrganizationGuid { get; set; }
        public BrowserType BrowserType { get; set; }
    }
}