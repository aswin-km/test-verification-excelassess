namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class ScoreCalculator
    {
        public bool IsCorrectAnswer { get; set; } = false;
        public double Score { get; set; } = 0;
        public double MaxScore { get; set; } = 0;
        public long QuestionId { get; set; }
    }
}
