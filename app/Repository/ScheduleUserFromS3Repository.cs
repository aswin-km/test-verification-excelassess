
using Amazon.S3.Model;
using ExcelAssess.Aws.S3Storage;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Repository
{
    public interface IScheduleUserFromS3Repository
    {
        Task<bool> S3FileExists(string v, string userResponseS3FilePath);
        public Task<Stream> S3DownloadFile(string S3BucketName, string filePath);
        Task<ListObjectsV2Response?> GetS3Objects(string S3BucketName, string folderPath);
        Task<string> S3DownloadFileAsync(string S3BucketName, string filePath);
    }
    public class ScheduleUserFromS3Repository(IAwsS3Storage awsS3Storage) : IScheduleUserFromS3Repository
    {
        private readonly IAwsS3Storage _awsS3Storage = awsS3Storage;
        public async Task<bool> S3FileExists(string S3BucketName, string filePath)
        {
            try
            {
                return await _awsS3Storage.S3FileExists(S3BucketName, filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"S3 Exception {S3BucketName} {filePath}", ex);
            }
        }

        public async Task<Stream> S3DownloadFile(string S3BucketName, string filePath)
        {
            try
            {
                return await _awsS3Storage.DownloadFileStreamAsync(S3BucketName, filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"S3 Exception {S3BucketName} {filePath}", ex);
            }
        }
        public async Task<string> S3DownloadFileAsync(string S3BucketName, string filePath)
        {
            try
            {
                return await _awsS3Storage.DownloadFileAsync(S3BucketName, filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"S3 Exception {S3BucketName} {filePath}", ex);
            }
        }
        public async Task<ListObjectsV2Response?> GetS3Objects(string S3BucketName, string folderPath)
        {
            try
            {
                return await _awsS3Storage.ListObjectsAsync(S3BucketName, folderPath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in fetching S3Objects : " + folderPath + "   " + ex.Message);
            }
            return null;
        }
    }
}
