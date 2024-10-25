namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Helper
{
    public static class FilePath
    {
        public static readonly string CompleteUserResponsesPathTemplate = "content/organizations/{0}/products/{1}/schedules/{2}/{3}/completeresponses/{4}_{5}.json";
        public static readonly string UserResponsesPathTemplate = "content/organizations/{0}/products/{1}/schedules/{2}/{3}/responses/{4}.json";
        public static readonly string TestJsonPath = "{0}/{1}";
        public static readonly string UserAllTestResponsePath = "content/organizations/{0}/products/{1}/schedules/{2}/{3}/testresponse.json";
        public static readonly string QuestionListS3Path = "content/organizations/{0}/products/{1}/schedules/{2}/{3}/questionlist.json";
        public static readonly string ScheduleUserAttemptStatusPath = "content/organizations/{0}/products/{1}/schedules/{2}/{3}/scheduleuserattemptstatus.json";
        public static readonly string ScheduleUserAttemptFilePath = "content/organizations/{0}/products/{1}/schedules/{2}/{3}/completeresponses";
    }
}
