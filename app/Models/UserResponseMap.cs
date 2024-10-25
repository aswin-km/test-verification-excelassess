namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class UserResponseMap
    {
        public int AttemptId { get; set; }
        public Guid QuestionGuid { get; set; }
        public long ScheduleUserAttemptId { get; set; }
    }
}
