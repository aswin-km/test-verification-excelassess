
using ExcelAssess.Redis;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Helper;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Models;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Repository
{
    public class ScheduleUserFromCacheRepository(IRedisClient redisClient) : IScheduleUserFromCacheRepository
    {
        private static string UserResponseCacheKey(long scheduleUserAttemptId) => $"{CachePrefix.UserResponseCache}{scheduleUserAttemptId}";
        private static string DataKey(string scheduleUserAttemptId, string questionId) => $"{CachePrefix.QuesionResponseCache}{scheduleUserAttemptId}:{questionId}";
        private static string MapKey(string scheduleUserAttemptId) => $"{CachePrefix.QuesionResponseMapCache}{scheduleUserAttemptId}";

        private readonly IRedisClient _redisClient = redisClient;
        public async Task<string?> GetUserFullResponseFromCache(Int64 scheduleUserAttemptId)
        {
            var keyValueId = UserResponseCacheKey(scheduleUserAttemptId);
            try
            {
                return await _redisClient.GetValueAsync(keyValueId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error While Retrieving User Response From Cache");
            }
        }
        public async Task<List<UserResponseMap>?> GetResponseMap(long scheduleUserAttemptId)
        {
            string mapKey = MapKey(scheduleUserAttemptId.ToString());
            var hashEntries = await _redisClient.GetDatabase().HashGetAllAsync(mapKey);
            if (hashEntries == null)
                return null;
            return hashEntries.Select(entry => new UserResponseMap
            {
                QuestionGuid = Guid.Parse(entry.Name.ToString()),
                AttemptId = Convert.ToInt32(entry.Value),
                ScheduleUserAttemptId = scheduleUserAttemptId
            }).ToList();
        }

        public async Task<string?> GetDataByMap(UserResponseMap userResponseMap)
        {
            var response = await _redisClient.GetDatabase().StringGetAsync(DataKey(userResponseMap.ScheduleUserAttemptId.ToString(), userResponseMap.QuestionGuid.ToString()));
            if (response.HasValue)
            {
                return response.ToString();
            }
            return null;
        }

    }
    public interface IScheduleUserFromCacheRepository
    {
        Task<string?> GetDataByMap(UserResponseMap userResponseMap);
        Task<List<UserResponseMap>?> GetResponseMap(long scheduleUserAttemptId);
        Task<string?> GetUserFullResponseFromCache(long scheduleUserAttemptId);
    }
}
