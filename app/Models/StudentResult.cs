namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class StudentResult
    {
        public decimal TotalMarks { get; set; }
        public decimal? MarksScored { get; set; }
        public decimal? PercentageScored { get; set; }
        public string Grade { get; set; }
        public long GradeId { get; set; }
        public int Status { get; set; }
        public Boolean IsResultProcessed { get; set; }
        public decimal ScoreFrom { get; set; }
        public decimal ScoreTo { get; set; }

    }
}
