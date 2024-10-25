using Amazon.S3.Model;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Helper;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Models;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using static ExcelAssess.TestPlayer.ResponseVerification.Console.Services.ResponseVerificationService;


namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Services
{
    class ResponseVerificationService(IScheduleUserFromS3Repository scheduleUserFromS3Repository,
        IScheduleUserFromCacheRepository scheduleUserFromCacheRepository,
        IScheduleUserRepository scheduleUserRepository, ILogger<ResponseVerificationService> logger,
        IResultCalculationService resultCalculationService, IConfiguration config)
    {
        private readonly IScheduleUserFromS3Repository _scheduleUserFromS3Repository = scheduleUserFromS3Repository;
        private readonly IScheduleUserFromCacheRepository _scheduleUserFromCacheRepository = scheduleUserFromCacheRepository;
        private readonly IScheduleUserRepository _scheduleUserRepository = scheduleUserRepository;
        private readonly ILogger _logger = logger;
        private readonly IResultCalculationService _resultCalculationService = resultCalculationService;
        private readonly IConfiguration _config = config;

        private static string UserResponseCacheKey(long scheduleUserAttemptId) => $"{CachePrefix.UserResponseCache}{scheduleUserAttemptId}";
        private static string DataKey(string scheduleUserAttemptId, string questionId) => $"{CachePrefix.QuesionResponseCache}{scheduleUserAttemptId}:{questionId}";
        private static string MapKey(string scheduleUserAttemptId) => $"{CachePrefix.QuesionResponseMapCache}{scheduleUserAttemptId}";

        public async Task<List<TestResponse>?> GetUserFullResponseFromS3(CommonScheduleUserAttemptDetails scheduleUserAttemptDetail)
        {
            string userResponseS3FilePath = UserResponseS3FilePath(scheduleUserAttemptDetail);
            bool isUserFullResponseExistsInS3 = await _scheduleUserFromS3Repository.S3FileExists("S3BucketName", userResponseS3FilePath);
            if (isUserFullResponseExistsInS3)
            {
                var jsonUserResponseContent = await GetUserFullResponseStringFromS3(scheduleUserAttemptDetail, userResponseS3FilePath);
                return DecryptedData(jsonUserResponseContent, scheduleUserAttemptDetail.UserSymmetricKey);
            }
            return null;
        }

        public async Task<string> GetUserFullResponseStringFromS3(CommonScheduleUserAttemptDetails scheduleUserAttemptDetail, string userResponseS3FilePath)
        {
            Stream jsonFile = await _scheduleUserFromS3Repository.S3DownloadFile("S3BucketName", userResponseS3FilePath);
            StreamReader reader = new(jsonFile);
            return await reader.ReadToEndAsync();
        }

        private static string UserResponseS3FilePath(CommonScheduleUserAttemptDetails scheduleUserAttemptDetail)
        {
            return string.Format(FilePath.UserAllTestResponsePath, scheduleUserAttemptDetail.OrganizationId, scheduleUserAttemptDetail.ProductId, scheduleUserAttemptDetail.ScheduleId, scheduleUserAttemptDetail.ScheduleUserAttemptGuid);
        }

        public async Task<List<TestResponse>?> GetUserFullResponseFromCache(CommonScheduleUserAttemptDetails scheduleUserAttemptDetail)
        {
            string? cachedUserResponseResponse = await GetUserFullResponseFromCache(scheduleUserAttemptDetail.ScheduleUserAttemptId);
            if (!string.IsNullOrEmpty(cachedUserResponseResponse))
            {
                return DecryptedData(cachedUserResponseResponse, scheduleUserAttemptDetail.UserSymmetricKey);
            }
            return null;
        }
        public async Task<string?> GetUserFullResponseFromCache(Int64 scheduleUserAttemptId)
        {
            return await _scheduleUserFromCacheRepository.GetUserFullResponseFromCache(scheduleUserAttemptId);
        }
        public List<TestResponse>? DecryptedData(string encryptedData, string userSymmetricKey)
        {
            return CryptographyHelper.Decrypt<List<TestResponse>>(encryptedData, userSymmetricKey);
        }

        public Task<StudentResultResponse> GetStudentResultsAsync(Guid scheduleUserGuid)
        {
            return _scheduleUserRepository.GetStudentResultsAsync(scheduleUserGuid);
        }

        public class ScheduleUserAttempt
        {
            public long ScheduleUserAttemptId { get; set; }
        }

        public List<long> GetScheduleUserAttemptFromCsv()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "input.csv");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Making header matching case-insensitive by converting to lowercase
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                HeaderValidated = null
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<ScheduleUserAttempt>();
                List<long> scheduleUserAttemptIds = [];

                foreach (var record in records)
                {
                    scheduleUserAttemptIds.Add(record.ScheduleUserAttemptId);
                }
                return scheduleUserAttemptIds;
            }
        }

        private async Task<VerificationResult> VerifyUser(long scheduleUserAttemptId)
        {
            var verificationResult = new VerificationResult();
            verificationResult.ScheduleUserAttemptId = scheduleUserAttemptId;
            //Get common scheuld user attempt details
            var scheduleUserAttemptDetail = await GetScheduleUserAttemptDetail(scheduleUserAttemptId);

            //Download cache values
            CacheValuesResult cacheValuesResult = await DownloadCacheValues(scheduleUserAttemptId, verificationResult);
            //Get latest response from s3
            //List<TestResponse>? latestUserResponse = await GetLatestUserResponseFromS3(scheduleUserAttemptDetail);
            //if(latestUserResponse != null)
            //{
            //    verificationResult = await ValidateResult(latestUserResponse, scheduleUserAttemptDetail, verificationResult);
            //    bool response = ValidateUserResponseData(cacheValuesResult, latestUserResponse, scheduleUserAttemptDetail.UserSymmetricKey);
            //}

            return verificationResult;
        }

        private async Task<VerificationResult> ValidateResult(List<TestResponse> latestUserResponse, CommonScheduleUserAttemptDetails scheduleUserAttemptDetail, VerificationResult verificationResult)
        {
            decimal percentageScored = 0;
            var calculateResult = await _resultCalculationService.ValidateAndReturnScores(latestUserResponse, scheduleUserAttemptDetail);
            if(calculateResult == null)
            {
                _logger.LogInformation($"Calculating score is null for scheduleUserAttemptId : {scheduleUserAttemptDetail.ScheduleUserAttemptId}");
                return verificationResult;
            }
            var dbResult = await GetStudentResultsAsync(scheduleUserAttemptDetail.ScheduleUserGuid);
            if (dbResult == null)
            {
                _logger.LogInformation($"DB score is null for scheduleUserAttemptId : {scheduleUserAttemptDetail.ScheduleUserAttemptId}");
                return verificationResult;
            }
            var obtainScore = calculateResult?.Select(x => x.Score).Sum();
            var maxScore = await _scheduleUserRepository.GetMaxScore(scheduleUserAttemptDetail.ScheduleId);
            if (maxScore != 0)
            {
                percentageScored = (decimal)(obtainScore / maxScore) * 100;
            }
            if(dbResult.PercentageScored == percentageScored)
            {
                verificationResult.ScoreMatch = "Yes";
            }
            else
            {
                _logger.LogInformation($"PercentageScored not matched for scheduleUserAttemptId : {scheduleUserAttemptDetail.ScheduleUserAttemptId}, db PercentageScored : {dbResult.PercentageScored}, calculated PercentageScored : {percentageScored}");
            }
            if (dbResult.PercentageScored == percentageScored)
            {
                verificationResult.ScoreMatch = "Yes";
            }
            else
            {
                _logger.LogInformation($"PercentageScored not matched for scheduleUserAttemptId : {scheduleUserAttemptDetail.ScheduleUserAttemptId}, db PercentageScored : {dbResult.PercentageScored}, calculated PercentageScored : {percentageScored}");
            }
            return verificationResult;
        }

        private bool ValidateUserResponseData(CacheValuesResult cacheValuesResult, List<TestResponse>? latestUserResponse, string userSymmetricKey)
        {
            if (!string.IsNullOrEmpty(cacheValuesResult.UserFullResponse))
            {
                var cachedTestResponse = CryptographyHelper.Decrypt<List<TestResponse>>(cacheValuesResult.UserFullResponse, userSymmetricKey);
                return AreTestResponseListsEqual(cachedTestResponse, latestUserResponse);
            }
            else if(cacheValuesResult.QuestionResponses != null && cacheValuesResult.QuestionResponses.Count > 0)
            {
                List<TestResponse> cachedTestResponse = new List<TestResponse>();
                foreach(var response in cacheValuesResult.QuestionResponses)
                {
                    //var cachedTestResponse = CryptographyHelper.Decrypt<List<TestResponse>>(cacheValuesResult.UserFullResponse, userSymmetricKey);

                }
            }
            return false;
        }
        public static bool AreTestResponseListsEqual(List<TestResponse> cachedTestResponse, List<TestResponse> latestTestResponse)
        {
            if (cachedTestResponse == null || latestTestResponse == null)
                return cachedTestResponse == latestTestResponse;  // Both null or one is null

            // Ensure both lists contain the same QuestionIds
            return cachedTestResponse.Select(x => x.QuestionId).OrderBy(id => id).SequenceEqual(
                   latestTestResponse.Select(x => x.QuestionId).OrderBy(id => id));
        }


        private async Task<CommonScheduleUserAttemptDetails> GetScheduleUserAttemptDetail(long scheduleUserAttemptId)
        {
            return await _scheduleUserRepository.GetScheduleUserAttemptDetail(scheduleUserAttemptId) ??
                throw new Exception($"ScheduleUserAttemptDetail not found for scheduleUserAttemptId : {scheduleUserAttemptId}");
        }
        private async Task<List<TestResponse>?> GetLatestUserResponseFromS3(CommonScheduleUserAttemptDetails scheduleUserAttemptDetail)
        {
            List<TestResponse> testResponse = new();
            string sourceFolder = string.Format(FilePath.ScheduleUserAttemptFilePath, scheduleUserAttemptDetail.OrganizationId, scheduleUserAttemptDetail.ProductId, scheduleUserAttemptDetail.ScheduleId, scheduleUserAttemptDetail.ScheduleUserAttemptGuid);
            var questionResponseFilePaths = (await GetAllFilesByFolderPath(sourceFolder))?.S3Objects.Select(obj => obj.Key);
            if (questionResponseFilePaths == null || !questionResponseFilePaths.Any()) return null;
            List<string>? latestQuestionResponseFiles = GetLatestAttemptFilePaths(questionResponseFilePaths.ToList());
            foreach (var file in latestQuestionResponseFiles)
            {
                string response = await _scheduleUserFromS3Repository.S3DownloadFileAsync(_config["S3BucketName"], file);
                var decryptedTestResponse = CryptographyHelper.Decrypt<UserResponse>(response, scheduleUserAttemptDetail.UserSymmetricKey);
                if (decryptedTestResponse != null)
                    testResponse.Add(decryptedTestResponse.TestResponse);
            }
            return testResponse;
        }
        public static List<string> GetLatestAttemptFilePaths(List<string> filePaths)
        {
            // Parse file paths into a list of (QuestionGuid, AttemptId, FilePath) tuples
            var parsedFiles = filePaths
                .Select(filePath =>
                {
                    var fileName = filePath.Split('/').Last().Replace(".json", "");
                    var parts = fileName.Split('_');
                    return (
                        QuestionGuid: parts[0],
                        AttemptId: int.Parse(parts[1]),
                        FilePath: filePath
                    );
                })
                .ToList();

            // Group by QuestionGuid and select the file with the highest AttemptId
            return parsedFiles
                .GroupBy(file => file.QuestionGuid)
                .Select(g => g.OrderByDescending(file => file.AttemptId).First().FilePath)
                .ToList();
        }
        private async Task<ListObjectsV2Response?> GetAllFilesByFolderPath(string folderPath)
        {
            return await _scheduleUserFromS3Repository.GetS3Objects(_config["S3BucketName"], folderPath);
        }
        private async Task<CacheValuesResult> DownloadCacheValues(long scheduleUserAttemptId, VerificationResult verificationResult)
        {
            string folderName = "output";
            CacheValuesResult cacheValuesResult = new CacheValuesResult();
            //full Response
            string? userFullResponse = await DownloadUserFullResponseCacheValues(scheduleUserAttemptId);
            if (!string.IsNullOrEmpty(userFullResponse))
            {
                cacheValuesResult.UserFullResponse = userFullResponse;

                //Download the full cached response values to the current directory
                await SaveTextToFileAsync(userFullResponse, UserResponseCacheKey(scheduleUserAttemptId), folderName, scheduleUserAttemptId);
            }
            else
            {
                _logger.LogInformation($"Full response Cache is null for scheduleUserAttemptId : {scheduleUserAttemptId}");
            }

            //question-response-map
            List<UserResponseMap>? userResponseMaps = await DownloadResponseMapCacheValues(scheduleUserAttemptId);
            if (userResponseMaps != null && userResponseMaps.Count > 0)
            {
                cacheValuesResult.UserResponseMaps = userResponseMaps;
                string userResponseMapsContent = JsonSerializer.Serialize(userResponseMaps, new JsonSerializerOptions { WriteIndented = true });

                //Download the cached user response map values to the current directory
                await SaveTextToFileAsync(userResponseMapsContent, MapKey(scheduleUserAttemptId.ToString()), folderName, scheduleUserAttemptId);

                cacheValuesResult.QuestionResponses = await DownloadQuestionResponsesCacheValues(userResponseMaps, folderName, scheduleUserAttemptId);
                if(cacheValuesResult.QuestionResponses == null || cacheValuesResult.QuestionResponses.Count <= 0)
                {
                    _logger.LogInformation($"QuestionResponses Cache is null for scheduleUserAttemptId : {scheduleUserAttemptId}");
                }
                if (cacheValuesResult.QuestionResponses?.Count == userResponseMaps.Count)
                {
                    verificationResult.DownloadCache = "Verified";
                }
                else
                {
                    verificationResult.DownloadCache = "Failed";
                }
            }
            else
            {
                _logger.LogInformation($"Question-response-map Cache is null for scheduleUserAttemptId : {scheduleUserAttemptId}");
                verificationResult.DownloadCache = "NoData";
            }
            return cacheValuesResult;
        }

        private async Task<Dictionary<string, string>> DownloadQuestionResponsesCacheValues(List<UserResponseMap> userResponseMaps, string folderName, long scheduleUserAttemptId)
        {
            Dictionary<string, string> questionResponses = new();
            foreach (var userResponseMap in userResponseMaps)
            {
                string? questionResponse = await _scheduleUserFromCacheRepository.GetDataByMap(userResponseMap);
                if (!string.IsNullOrEmpty(questionResponse))
                {
                    string key = DataKey(userResponseMap.ScheduleUserAttemptId.ToString(), userResponseMap.QuestionGuid.ToString());
                    await SaveTextToFileAsync(questionResponse, key, folderName, scheduleUserAttemptId);
                    questionResponses.Add(key, questionResponse);
                }
            }
            return questionResponses;
        }

        private async Task<List<UserResponseMap>?> DownloadResponseMapCacheValues(long scheduleUserAttemptId)
        {
            return await _scheduleUserFromCacheRepository.GetResponseMap(scheduleUserAttemptId);
        }

        private async Task<string?> DownloadUserFullResponseCacheValues(long scheduleUserAttemptId)
        {
            return await _scheduleUserFromCacheRepository.GetUserFullResponseFromCache(scheduleUserAttemptId);
        }

        public void SaveVerificationResults<T>(List<T> results, string fileName, string folderName)
        {
            // Get the path to the application's root directory
            string projectRootPath = AppContext.BaseDirectory; // Base directory of the application

            // Define a specific folder for storing CSV files (e.g., 'Data' folder)
            string dataDirectory = Path.Combine(projectRootPath, folderName); // Folder for your data
            Directory.CreateDirectory(dataDirectory); // Ensure the directory exists

            // Specify the CSV file path
            string filePath = Path.Combine(dataDirectory, fileName);

            // Writing to the CSV file
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write the list of results to the CSV file
                csv.WriteRecords(results);
            }
        }

        public static async Task SaveTextToFileAsync(string content, string fileName, string folderName, long scheduleUserAttemptId)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            } 
            // Get the path to the application's root directory
            string projectRootPath = AppContext.BaseDirectory; // Base directory of the application

            // Define a specific folder for storing CSV files (e.g., 'Data' folder)
            string dataDirectory = Path.Combine(projectRootPath, folderName, scheduleUserAttemptId.ToString()); // Folder for your data
            Directory.CreateDirectory(dataDirectory); // Ensure the directory exists

            // Specify the CSV file path
            string filePath = Path.Combine(dataDirectory, fileName);
            // Save the string content as a text file asynchronously
            await File.WriteAllTextAsync(filePath, content);
        }

        public async Task Start()
        {            
            var scheduleUserAttemptIds = GetScheduleUserAttemptFromCsv();
            var verificationResults = new List<VerificationResult>();
            foreach (var id in scheduleUserAttemptIds)
            {
                var result = await VerifyUser(id);
                verificationResults.Add(result);
            }
            SaveVerificationResults(verificationResults, $"report{DateTimeOffset.Now.ToUnixTimeSeconds()}.csv", "report");
            //  await GetUserFullResponseFromCache(5);

        }
    }

}
