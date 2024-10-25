using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Models
{
    public class TestDetails
    {
        public Preferences? Preferences { get; set; }
        public List<Form>? Forms { get; set; }
    }

    public class Preferences
    {
        public AssessmentSettings? AssessmentSettings { get; set; }
        public Playerpreferences? PlayerPreferences { get; set; }
        public PostAssessmentPreferences? PostAssessmentPreferences { get; set; }
        public string Ishorizontalalignment { get; set; }
        public string Shuffle { get; set; }
        public string Casesensitive { get; set; }
        public string Ignorewhitespace { get; set; }
        public string Numeric { get; set; }
    }
    public class AssessmentSettings
    {
        public bool RandomizeTest { get; set; }
        public int NoOfSequences { get; set; }
    }
    public class Playerpreferences
    {
        public bool AllowToSendFeedback { get; set; }
        public bool ShowHints { get; set; }
        public bool SendMailAfterTest { get; set; }
        public bool AllowNegativeMarking { get; set; }
        public bool EnableGroupingPassageQuestions { get; set; }
        public bool ShowNotepad { get; set; }
        public bool ShowCalculator { get; set; }
        public int EnableBuzzer { get; set; }
        public int NoOfQuestionsPerPage { get; set; }
        public int SubmitButtonAtPercentage { get; set; }
        public bool AllowSkipQuestions { get; set; }
        public bool AllowBookmarkTest { get; set; }
        public bool SubmitButtonAtLast { get; set; }
        public bool SecuredTest { get; set; }
        public bool AllowBackwardNavigation { get; set; }
        public bool ShowInstructionPage { get; set; }
        public bool PickUnseenQuestions { get; set; }
        public bool Remindersubmit { get; set; }
        public bool InstructionAgreementNeeded { get; set; }
        public bool EnableQuestionNavigationLink { get; set; }
        public bool ReviewQuestions { get; set; }
        public bool ResetAnswers { get; set; }
        public bool TestMonitoringImages { get; set; }
        public int FrequencyOfImageCapturing { get; set; }
        public bool ShowTimer { get; set; }
        public bool EnableOnlineProctoring { get; set; }
        public int NoOfAttempts { get; set; }
        public int AssessmentDuration { get; set; }
    }
    public class PostAssessmentPreferences
    {
        public bool ViewSummaryReport { get; set; }
        public bool DisplayAverage { get; set; }
        public bool DisplayResponse { get; set; }
        public bool ShowGrade { get; set; }
        public bool ShowTestSummary { get; set; }
        public bool ShowSectionSummary { get; set; }
        public bool ShowQuestionSummary { get; set; }
        public bool ShowAllocationOfMarks { get; set; }
        public bool ShowScoringRate { get; set; }
        public bool ShowNoOfCorrectAnswers { get; set; }
        public bool ShowNoOfQuestions { get; set; }
        public bool ShowAnswerTime { get; set; }
        public bool ViewSolution { get; set; }
        public bool ShowCorrectAnswer { get; set; }
        public bool ShowPercentageOfCorrectAnswers { get; set; }
        public bool ShowDetailedReport { get; set; }
        public bool ShowScore { get; set; }
        public bool PrintRequired { get; set; }
        public bool GenerateParticipationCertificate { get; set; }
        public bool GenerateReportCard { get; set; }
    }
    public class Form
    {
        public long FormId { get; set; }
        public string FormGuid { get; set; }
        public string FormName { get; set; }
        public long CreatedBy { get; set; }
        public long ModifiedBy { get; set; }
        public List<Section>? Sections { get; set; }
        public List<Passage>? Passages { get; set; }
    }
    public class Section
    {
        public long SectionId { get; set; }
        public string SectionGuid { get; set; }
        public string SectionName { get; set; }
        public int SectionOrder { get; set; }
        public string Instructions { get; set; }
        public long CreatedBy { get; set; }
        public long ModifiedBy { get; set; }
        public List<Question>? Questions { get; set; }
    }

    public class Passage
    {
        public int? PassageId { get; set; }
        public string PassageTitle { get; set; }
        public string PassageAlignment { get; set; }
        public List<Passagetext> PassageText { get; set; }
    }

    public class Passagetext
    {
        public string Text { get; set; }
    }
}
