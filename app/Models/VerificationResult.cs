namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class VerificationResult
    {
        public long ScheduleUserAttemptId { get; set; }
        public string? DownloadCache { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ScoreMatch { get; set; }
    }
}
