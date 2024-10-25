using ExcelAssess.Aws.SecretsManager.Cache;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Helper;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Models;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Services
{
    public interface IResultCalculationService
    {
        public Task<List<ScoreCalculator>?> ValidateAndReturnScores(List<TestResponse> testResponses, CommonScheduleUserAttemptDetails scheduleUserAttemptDetail);
    }
    public class ResultCalculationService(IScheduleUserFromS3Repository scheduleUserFromS3Repository,
        IAwsSecretsManagerCache awsSecretsManagerCache,
        IAssessmentRepository assessmentRepository,
        IConfiguration config) : IResultCalculationService
    {
        private readonly IScheduleUserFromS3Repository _scheduleUserFromS3Repository = scheduleUserFromS3Repository;
        private readonly IAwsSecretsManagerCache _awsSecretsManagerCache = awsSecretsManagerCache;
        private readonly IAssessmentRepository _assessmentRepository = assessmentRepository;
        private readonly IConfiguration _config = config;
        private readonly IDictionary<string, Question> questionsCache = new Dictionary<string, Question>();

        public async Task<List<ScoreCalculator>?> ValidateAndReturnScores(List<TestResponse> testResponses, CommonScheduleUserAttemptDetails scheduleUserAttemptDetail)
        {
            List<ScoreCalculator> scoreCalculators = new();
            foreach (var data in testResponses)
            {
                scoreCalculators.Add(await ValidateAndReturnScore(data, scheduleUserAttemptDetail));
            }
            return scoreCalculators;
        }

        private async Task<Question> LookUpQuestions(TestResponse testResponse, CommonScheduleUserAttemptDetails scheduleUserAttemptDetail)
        {
            Question questions;
            if (!questionsCache.TryGetValue(testResponse.QuestionGuid.ToString(), out questions))
            {
                questions = await GetQuestionAnswerDetailsFromS3(scheduleUserAttemptDetail, testResponse.QuestionGuid);
            }

            return questions;

        }
        private async Task<ScoreCalculator> ValidateAndReturnScore(TestResponse testResponse, CommonScheduleUserAttemptDetails scheduleUserAttemptDetail)
        {
            ScoreCalculator scoreCalculator = new();
            if (testResponse == null)
                return null;


            Question? s3QuestionAnswer = await LookUpQuestions(testResponse, scheduleUserAttemptDetail);

            bool isNegativeMarking = s3QuestionAnswer.AllowNegativeMarking;

            if (Enum.TryParse(testResponse.QuestionType, out QuestionType questionType))
            {
                List<Answer> correctAnswers = s3QuestionAnswer?.Answers
                    ?? throw new Exception("Correct Answers Not Found");
                switch (questionType)
                {
                    case QuestionType.MRQ:
                        scoreCalculator = ValidateMrqAnswer(testResponse, correctAnswers, s3QuestionAnswer, isNegativeMarking);
                        break;
                    case QuestionType.MCQ:
                        scoreCalculator = ValidateMcqAnswer(testResponse, correctAnswers, isNegativeMarking);
                        break;
                    case QuestionType.FIBDnD:
                    case QuestionType.FIBDD:
                    case QuestionType.MTF:
                        scoreCalculator = ValidateAnswer(testResponse, correctAnswers, isNegativeMarking);
                        break;
                    case QuestionType.TF:
                        if (testResponse?.Data?.ChoiceId > 0)
                            scoreCalculator = ValidateTfAnswer(testResponse, correctAnswers, isNegativeMarking);
                        break;
                    case QuestionType.FIBT:
                        scoreCalculator = ValidateFibTextAnswer(testResponse, correctAnswers, isNegativeMarking);
                        break;
                    default:
                        return null;
                }
                scoreCalculator.MaxScore = s3QuestionAnswer.MaxScore;
                scoreCalculator.QuestionId = testResponse.QuestionId;
            }
            return scoreCalculator;
        }
        private async Task<Question?> GetQuestionAnswerDetailsFromS3(CommonScheduleUserAttemptDetails scheduleUserAttemptDetail, Guid questionGuid)
        {
            string? assessmentPath = scheduleUserAttemptDetail.AssessmentPath;
            if (string.IsNullOrWhiteSpace(assessmentPath)) { assessmentPath = await _assessmentRepository.GetAssessmentGuid(scheduleUserAttemptDetail.ScheduleUserId); }
            string? repositoryPath = scheduleUserAttemptDetail.RepositoryPath;
            if (string.IsNullOrEmpty(repositoryPath)) { repositoryPath = await _assessmentRepository.GetTestJsonPath(scheduleUserAttemptDetail.ScheduleUserAttemptId); }
            string testJsonFilePath = string.Format(FilePath.TestJsonPath, repositoryPath, assessmentPath);
            if (string.IsNullOrEmpty(repositoryPath))
            {
                return null;
            }
            Stream questionAnswerJsonObj = await _scheduleUserFromS3Repository.S3DownloadFile(_config["S3BucketName"], testJsonFilePath);
            StreamReader questionAnswerJson = new(questionAnswerJsonObj);
            var jsonContent = await questionAnswerJson.ReadToEndAsync();
            var testDetails = await DecryptTestDetails(jsonContent);
            if (testDetails.Forms == null) return null;
            foreach (var sections in testDetails.Forms.Select(x => x.Sections))
            {
                if (sections == null) continue;

                foreach (var questions in sections.Select(x => x.Questions))
                {
                    if (questions == null) continue;

                    foreach (var question in questions)
                    {
                        if (question.QuestionGuid == questionGuid.ToString())
                        {
                            question.AllowNegativeMarking = testDetails.Preferences?.PlayerPreferences?.AllowNegativeMarking ?? false;
                            questionsCache?.Add(question.QuestionGuid, question);
                            return question;
                        }
                    }
                }
            }
            return null;
        }

        private async Task<TestDetails> DecryptTestDetails(string jsonContent)
        {
            string? encryptionKey = await _awsSecretsManagerCache.GetSecretAsync(_config["EncryptionKey"]) ?? throw new Exception($"encryptionKey is not found for the keyname");
            TestDetails? testDetails = CryptographyHelper.Decrypt<TestDetails>(jsonContent, encryptionKey) ?? throw new Exception("testDetails is null");
            return testDetails;
        }

        private static ScoreCalculator ValidateMrqAnswer(TestResponse testResponse, List<Answer> correctAnswers, Question cachedQuestionAnswer, bool isNegativeMarking)
        {
            ScoreCalculator scoreCalculator = new();
            double score = 0;
            List<long> correctChoiceIds = correctAnswers.Where(x => x.IsCorrect).Select(x => x.ChoiceId).ToList();
            if (testResponse?.Data?.ChoiceIds != null && testResponse?.Data?.ChoiceIds.Count > 0)
            {
                int correctAnswerCount = correctChoiceIds.Count(choiceId =>
                            testResponse?.Data?.ChoiceIds?.Exists(dynamicChoiceId =>
                                dynamicChoiceId == choiceId) == true);
                var maxChoices = cachedQuestionAnswer?.Data?.maxchoices?.Value;
                var minChoices = cachedQuestionAnswer?.Data?.minchoices?.Value;
                if (correctAnswerCount <= maxChoices && correctAnswerCount >= minChoices)
                {
                    foreach (var userChoiceId in testResponse.Data.ChoiceIds)
                    {
                        Answer validateChoiceResponse = correctAnswers.First(x => x.ChoiceId == userChoiceId);
                        if (isNegativeMarking)
                        {
                            score += validateChoiceResponse.Score ?? default;
                        }
                        else
                        {
                            if (validateChoiceResponse.IsCorrect)
                                score += validateChoiceResponse.Score ?? default;
                            else
                                score += 0;
                        }
                    }
                    scoreCalculator.Score = score;
                }
                scoreCalculator.IsCorrectAnswer = correctAnswerCount == maxChoices;
            }
            return scoreCalculator;
        }
        private static ScoreCalculator ValidateMcqAnswer(TestResponse testResponse, List<Answer> correctAnswers, bool isNegativeMarking)
        {
            ScoreCalculator scoreCalculator = new();
            if (testResponse?.Data?.ChoiceId > 0)
            {
                var correctOption = correctAnswers.FirstOrDefault(x => x.IsCorrect);
                scoreCalculator.IsCorrectAnswer = testResponse?.Data?.ChoiceId == correctOption?.ChoiceId;

                if (isNegativeMarking)
                {
                    var userResponseAnsData = correctAnswers.FirstOrDefault(x => x.ChoiceId == testResponse?.Data?.ChoiceId);
                    scoreCalculator.Score = userResponseAnsData?.Score ?? default;
                }
                else
                    scoreCalculator.Score = scoreCalculator.IsCorrectAnswer ? correctOption?.Score ?? default : default;
            }
            return scoreCalculator;
        }
        private ScoreCalculator ValidateAnswer(TestResponse testResponse, List<Answer> correctAnswers, bool isNegativeMarking)
        {
            ScoreCalculator scoreCalculator = new();
            double score = 0;
            if (testResponse?.Data?.Blanks == null || testResponse.Data?.Blanks.Count == 0 || correctAnswers == null || correctAnswers.Count == 0)
            {
                return scoreCalculator;
            }
            List<bool> anyCorrectAnswerForBlank = [];

            foreach (var responseBlank in testResponse.Data.Blanks)
            {
                var correctAnswerForBlank = correctAnswers.FirstOrDefault(x => x.BlankId == responseBlank.BlankId);
                if (correctAnswerForBlank == null)
                {
                    // No correct answer found for this blank
                    anyCorrectAnswerForBlank.Add(false);
                }
                else
                {
                    // Check if the selected option for this blank matches the correct one
                    bool isCorrect = correctAnswerForBlank.Options.Exists(o => o.OptionId == responseBlank.OptionId && o.IsCorrect);
                    var correctOptionForBlank = correctAnswerForBlank.Options.FirstOrDefault(x => x.IsCorrect);
                    var userOptionForBlank = correctAnswerForBlank.Options.FirstOrDefault(x => x.OptionId == responseBlank.OptionId);
                    anyCorrectAnswerForBlank.Add(isCorrect);
                    if (isNegativeMarking)
                    {
                        score += userOptionForBlank?.Score ?? default;
                    }
                    else
                        score += isCorrect ? correctOptionForBlank?.Score ?? default : default;
                }
            }
            scoreCalculator.Score = score;
            if (correctAnswers.Select(x => x.BlankId).Count() == testResponse?.Data?.Blanks.Count(x => x.OptionId != 0) && anyCorrectAnswerForBlank.Count > 0)
                scoreCalculator.IsCorrectAnswer = anyCorrectAnswerForBlank.All(x => x);
            else
                scoreCalculator.IsCorrectAnswer = false;
            return scoreCalculator;
        }

        private static ScoreCalculator ValidateFibTextAnswer(TestResponse testResponse, List<Answer> correctAnswers, bool isNegativeMarking)
        {
            double score = 0;
            bool isCorrect = false;
            List<bool> anyCorrectAnswerForBlank = [];
            ScoreCalculator scoreCalculator = new();
            if (testResponse == null || correctAnswers == null || testResponse.Data == null || testResponse.Data.Blanks == null)
            {
                return scoreCalculator; // Invalid input
            }

            var responses = testResponse.Data.Blanks;
            foreach (var response in responses)
            {
                var matchingCorrectAnswer = correctAnswers.Find(answer => answer.BlankId == response.BlankId);

                if (matchingCorrectAnswer != null)
                {
                    if (string.Equals(matchingCorrectAnswer.AnswerText, response?.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        isCorrect = true;
                        anyCorrectAnswerForBlank.Add(true);
                        score += matchingCorrectAnswer.Score ?? default;
                    }
                    if (matchingCorrectAnswer.AlternateAnswers != null)
                    {
                        foreach (var altAnswer in matchingCorrectAnswer.AlternateAnswers)
                        {
                            if (string.Equals(altAnswer, response?.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                // Alternate match found
                                isCorrect = true;
                                anyCorrectAnswerForBlank.Add(true);
                                score += matchingCorrectAnswer.Score ?? default;
                            }
                        }
                    }
                    if (!isCorrect && isNegativeMarking && matchingCorrectAnswer?.NegativeScore != null)
                    {
                        score += (double)matchingCorrectAnswer.NegativeScore;
                    }
                }
            }
            scoreCalculator.Score = score;
            if (correctAnswers.Select(x => x.BlankId).Count() == responses.Count(x => !string.IsNullOrEmpty(x.Text)) && anyCorrectAnswerForBlank.Count > 0)
                scoreCalculator.IsCorrectAnswer = anyCorrectAnswerForBlank.All(x => x);
            else
                scoreCalculator.IsCorrectAnswer = false;
            return scoreCalculator;
        }

        private static ScoreCalculator ValidateTfAnswer(TestResponse testResponse, List<Answer> correctAnswers, bool isNegativeMarking)
        {
            ScoreCalculator scoreCalculator = new();
            var correctChoice = correctAnswers.FirstOrDefault(x => x.IsCorrect);
            scoreCalculator.IsCorrectAnswer = testResponse.Data?.ChoiceId == correctChoice?.ChoiceId;
            if (isNegativeMarking)
            {
                var userResponseAnsData = correctAnswers.FirstOrDefault(x => x.ChoiceId == testResponse.Data?.ChoiceId);
                scoreCalculator.Score = userResponseAnsData?.Score ?? default;
            }
            else
                scoreCalculator.Score = scoreCalculator.IsCorrectAnswer ? correctChoice?.Score ?? default : 0;
            return scoreCalculator;
        }
    }
}
