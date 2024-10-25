using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class Question
    {
        public long QuestionId { get; set; }
        public string QuestionGuid { get; set; }
        public string QuestionText { get; set; }
        public string? Qtiversion { get; set; }
        public string QuestionType { get; set; }
        public short? QuestionLevelId { get; set; }
        public int QuestionOrder { get; set; }
        public string Version { get; set; }
        public bool Ispublished { get; set; }
        public short? AnsweringTime { get; set; }
        public double MaxScore { get; set; }
        public bool ResponseLevelScoring { get; set; }
        public long CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }
        public dynamic? Data { get; set; }
        public List<Answer>? Answers { get; set; }
        public dynamic? Hints { get; set; }
        public int? PassageId { get; set; }
        public bool AllowNegativeMarking { get; set; }
    }
    public class Answer
    {
        public long ChoiceId { get; set; }
        public double? Score { get; set; }
        public double? NegativeScore { get; set; }
        public bool IsCorrect { get; set; }
        public int? BlankId { get; set; }

        public List<Option>? Options { get; set; }

        public string? AnswerText { get; set; }

        public List<dynamic>? AlternateAnswers { get; set; }
    }

    public class Option
    {
        public long OptionId { get; set; }
        public string OptionGuid { get; set; }
        public string OptionText { get; set; }
        public double? Score { get; set; }
        public double NegativeScore { get; set; }
        public bool IsCorrect { get; set; }
    }
}
