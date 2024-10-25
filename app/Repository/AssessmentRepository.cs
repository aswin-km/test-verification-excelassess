using Dapper;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Models;
using Npgsql;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Repository
{
    public interface IAssessmentRepository
    {
        public Task<string?> GetAssessmentGuid(Int64 scheduleUserId);
        public Task<string?> GetTestJsonPath(Int64 scheduleUserAttemptId);
    }
    public class AssessmentRepository(DBConnectionString dbConfiguration) : IAssessmentRepository
    {
        private readonly string _dbConnectionService = dbConfiguration.ConnectionString;

        public async Task<string?> GetAssessmentGuid(Int64 scheduleUserId)
        {
            await using var _connection = new NpgsqlConnection(_dbConnectionService);
            try
            {
                await _connection.OpenAsync();

                string sqlQuery = @"SELECT a.assessmentpath
                                    FROM assessment a
                                    JOIN assessmentforms af ON a.assessmentid = af.assessmentid
                                    JOIN scheduledetails sd ON af.assessmentformid = sd.assessmentformid
                                    JOIN scheduleuser su ON sd.scheduledetailid = su.scheduledetailid
                                    WHERE su.scheduleuserid =  @scheduleUserId";

                return await _connection.QueryFirstOrDefaultAsync<string>(sqlQuery, new { scheduleUserId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching repositorypath from repository table for scheduleUserId : {scheduleUserId}", ex);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<string?> GetTestJsonPath(Int64 scheduleUserAttemptId)
        {
            await using var _connection = new NpgsqlConnection(_dbConnectionService);
            try
            {
                await _connection.OpenAsync();

                string sqlQuery = @"SELECT r.repositorypath FROM repository r INNER JOIN scheduleuserattempts sua ON r.repositoryid=sua.repositoryid WHERE sua.scheduleuserattemptid = @scheduleUserAttemptId";

                return await _connection.QueryFirstOrDefaultAsync<string>(sqlQuery, new { scheduleUserAttemptId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching repositorypath from repository table for scheduleUserAttemptId : {scheduleUserAttemptId}", ex);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
}
