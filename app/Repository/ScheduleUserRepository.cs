using Dapper;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Models;
using Npgsql;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Repository
{


    public interface IScheduleUserRepository
    {
        Task<CommonScheduleUserAttemptDetails?> GetScheduleUserAttemptDetail(long scheduleUserAttemptId);
        public Task<StudentResultResponse> GetStudentResultsAsync(Guid scheduleUserGuid);
        Task<int> GetMaxScore(long scheduleId);
    }

    public class ScheduleUserRepository(DBConnectionString dBConnectionString) : IScheduleUserRepository
    {
        private readonly string _dbConnectionString = dBConnectionString.ConnectionString;
        public async Task<StudentResultResponse> GetStudentResultsAsync(Guid scheduleUserGuid)
        {
            await using var dbConnection = new NpgsqlConnection(_dbConnectionString);
            StudentResultResponse result = new();
            IEnumerable<StudentResult>? queryData = null;
            try
            {
                await dbConnection.OpenAsync();


                queryData = await dbConnection.QueryAsync<StudentResult>(
                        @"SELECT 
                            sua.marksscored, 
                            sua.percentagescored, 
                            ass.gradeid, 
                            gsl.name AS grade, 
                    gsl.scorefrom,
                    gsl.scoreto,
                            af.totalmarks,
                            sua.status, 
                            sua.isresultprocessed
                        FROM 
                            scheduleuserattempts sua
                            INNER JOIN scheduleuser su ON su.scheduleuserid = sua.scheduleuserid
                            INNER JOIN scheduledetails sd ON sd.scheduledetailid = su.scheduledetailid
                            INNER JOIN assessmentforms af ON af.assessmentformid = sd.assessmentformid
                            INNER JOIN assessment ass ON ass.assessmentid = af.assessmentid
                            INNER JOIN gradeschemelevel gsl ON gsl.gradeschemeid = ass.gradeid
                        WHERE 
                            su.scheduleuserguid = @ScheduleUserGuid",
                    new { ScheduleUserGuid = scheduleUserGuid }
                );

                StudentResult? studentResult = queryData.Where(x => x.ScoreFrom <= (x.PercentageScored ?? 0m) && x.ScoreTo >= (x.PercentageScored ?? 0m)).FirstOrDefault();
                if (studentResult != null)
                {
                    result = new StudentResultResponse
                    {
                        TotalMarks = studentResult.TotalMarks,
                        PercentageScored = studentResult.PercentageScored ?? 0,
                        MarksScored = studentResult.MarksScored ?? 0,
                        IsResultProcessed = studentResult.IsResultProcessed,
                        Grade = studentResult.Status != 2 ? "No Result" : studentResult.Grade,
                        GradeSchemas = await GradeLevels(studentResult.GradeId)
                    };
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Student Results Retrieval Failed", scheduleUserGuid);
                throw new Exception(errorMessage, ex);
            }
            finally { await dbConnection.CloseAsync(); }
            return result;
        }

        public async Task<List<GradeSchema>> GradeLevels(long gradeId)
        {
            List<GradeSchema> gradeSchemas = [];
            await using var dbConnection = new NpgsqlConnection(_dbConnectionString);
            IEnumerable<GradeSchema>? queryData = null;
            try
            {
                await dbConnection.OpenAsync();
                queryData = await dbConnection.QueryAsync<GradeSchema>("select name as Grade,scorefrom || ' - ' || scoreto as GradeLevel from gradeschemelevel where gradeschemeid=@GradeId  order by scoreto desc",
                 new { GradeId = gradeId });

                gradeSchemas = queryData.ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Grade Level Retrieval Failed", gradeId);
                throw new Exception(errorMessage, ex);
            }
            finally { await dbConnection.CloseAsync(); }
            return gradeSchemas;
        }
        public async Task<CommonScheduleUserAttemptDetails?> GetScheduleUserAttemptDetail(long scheduleUserAttemptId)
        {
            using var db = new NpgsqlConnection(_dbConnectionString);
            try
            {
                await db.OpenAsync();
                string query = @"
   SELECT 
					ScheduleId,
					ScheduleDetailGuid,
					RepositoryId,
					AssessmentFormPath,
					ScheduleStartDateTime,
					ScheduleEndDateTime,
					TestStartDateTime,
					TestEndDateTime,
					AdditionalTimeForAssessment,
					ScheduleUserId,
					ScheduleUserGuid,
					ScheduledJsonPath,
					UserSymmetricKey, 
					IsAdditionalTimeEnabled,
					AssessmentDuration,
					AdditionalTime,
					ProductId,
					OrganizationId,
					UserGuid,
					ScheduleUserAttemptGuid,
					ScheduleUserAttemptId,
					Assessmentpath,
					RepositoryPath,
                    ScheduleGuid,
                    ProductGuid,
                    OrganizationGuid,
                    BrowserType
FROM 
    scheduleuserattemptdetails   
 WHERE 
	ScheduleUserAttemptId = @ScheduleUserAttemptId;";

                var result = await db.QueryFirstOrDefaultAsync<CommonScheduleUserAttemptDetails>(query, new { ScheduleUserAttemptId = scheduleUserAttemptId });
                return result;
            }
            finally
            {
                await db.CloseAsync();
            }
        }

        public async Task<int> GetMaxScore(long scheduleId)
        {
            await using var connection = new NpgsqlConnection(_dbConnectionString);
            try
            {
                await connection.OpenAsync();
                string sqlQuery = @"
                    SELECT 
                        af.totalmarks AS TotalMarks
                    FROM assessmentforms af
                    INNER JOIN scheduledetails sd ON sd.assessmentformid = af.assessmentformid
                    WHERE sd.scheduleid = @Scheduleid";

                // Parameters
                var parameters = new { Scheduleid = scheduleId };
                return await connection.ExecuteScalarAsync<int>(sqlQuery, parameters);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating into scheduleUserAttempts table for scheduleId : {scheduleId}", ex);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
