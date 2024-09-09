namespace LMSCore.Models
{
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class lmsDbContext : DbContext
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        private static readonly string connectionStrings = configuration.GetSection("ConnectionStrings:DbContext").Value.ToString();
        private static DbContextOptions<lmsDbContext> options = new DbContextOptionsBuilder<lmsDbContext>()
                        .UseSqlServer(connectionStrings)
                        .Options;

        public lmsDbContext() : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserExport>().HasNoKey();
        }
        public virtual DbSet<tbl_HistoryCheckWriting> tbl_HistoryCheckWriting { get; set; }
        public virtual DbSet<tbl_BandDescriptor> tbl_BandDescriptor { get; set; }
        public virtual DbSet<tbl_CheckWritingResponse> tbl_CheckWritingResponse { get; set; }
        public virtual DbSet<tbl_SaveTableInitDetail> tbl_SaveTableInitDetail { get; set; }
        public virtual DbSet<tbl_SaveTableInit> tbl_SaveTableInit { get; set; }
        public virtual DbSet<tbl_SaveGradesInClass> tbl_SaveGradesInClass { get; set; }
        public virtual DbSet<tbl_ClassTranscriptDetail> tbl_ClassTranscriptDetail { get; set; }
        public virtual DbSet<tbl_ClassTranscript> tbl_ClassTranscript { get; set; }
        public virtual DbSet<tbl_SampleTranscriptDetail> tbl_SampleTranscriptDetail { get; set; }
        public virtual DbSet<tbl_SampleTranscript> tbl_SampleTranscript { get; set; }
        public virtual DbSet<tbl_WarningHistory> tbl_WarningHistory { get; set; }
        public virtual DbSet<tbl_ScheduleRecord> tbl_ScheduleRecord { get; set; }
        public virtual DbSet<tbl_LeadSwitchTest> tbl_LeadSwitchTest { get; set; }
        public virtual DbSet<tbl_ConsultantRevenue> tbl_ConsultantRevenue { get; set; }
        public virtual DbSet<tbl_MonthlyTuition> tbl_MonthlyTuition { get; set; }
        public virtual DbSet<tbl_TuitionPackage> tbl_TuitionPackage { get; set; }
        public virtual DbSet<tbl_JobPosition> tbl_JobPosition { get; set; }
        public virtual DbSet<tbl_StaffNote> tbl_StaffNote { get; set; }
        public virtual DbSet<tbl_TrainingRouteForm> tbl_TrainingRouteForm { get; set; }
        public virtual DbSet<tbl_TrainingRouteDetail> tbl_TrainingRouteDetail { get; set; }
        public virtual DbSet<tbl_TrainingRoute> tbl_TrainingRoute { get; set; }
        public virtual DbSet<tbl_StudentInTraining> tbl_StudentInTraining { get; set; }
        public virtual DbSet<tbl_TutorSalary> tbl_TutorSalary { get; set; }
        public virtual DbSet<tbl_TutorSalaryConfig> tbl_TutorSalaryConfig { get; set; }
        public virtual DbSet<tbl_Score> tbl_Score { get; set; }
        public virtual DbSet<tbl_ScoreColumn> tbl_ScoreColumn { get; set; }
        public virtual DbSet<tbl_ScoreColumnTemplate> tbl_ScoreColumnTemplate { get; set; }
        public virtual DbSet<tbl_ScoreBoardTemplate> tbl_ScoreboardTemplate { get; set; }
        public virtual DbSet<tbl_WordInfo> tbl_WordInfo { get; set; }
        public virtual DbSet<tbl_UtteranceInfo> tbl_UtteranceInfo { get; set; }
        public virtual DbSet<tbl_MailTemplate> tbl_MailTemplate { get; set; }
        public virtual DbSet<tbl_InputTestResult> tbl_InputTestResult { get; set; }
        public virtual DbSet<tbl_InterviewAppointment> tbl_InterviewAppointment { get; set; }
        public virtual DbSet<tbl_CurriculumVitae> tbl_CurriculumVitae { get; set; }
        public virtual DbSet<tbl_StudentMonthlyDebt> tbl_StudentMonthlyDebt { get; set; }
        public virtual DbSet<tbl_StudentExpectation> tbl_StudentExpectation { get; set; }
        public virtual DbSet<tbl_WriteLog> tbl_WriteLog { get; set; }
        public virtual DbSet<tbl_CommissionConfig> tbl_CommissionConfig { get; set; }
        public virtual DbSet<tbl_OTPHistory> tbl_OTPHistory { get; set; }
        public virtual DbSet<tbl_IeltsAnswerResultComment> tbl_IeltsAnswerResultComment { get; set; }
        public virtual DbSet<tbl_IeltsAnswerResult> tbl_IeltsAnswerResult { get; set; }
        public virtual DbSet<tbl_IeltsQuestionResult> tbl_IeltsQuestionResult { get; set; }
        public virtual DbSet<tbl_IeltsQuestionGroupResult> tbl_IeltsQuestionGroupResult { get; set; }
        public virtual DbSet<tbl_IeltsSectionResult> tbl_IeltsSectionResult { get; set; }
        public virtual DbSet<tbl_IeltsSkillResult> tbl_IeltsSkillResult { get; set; }
        public virtual DbSet<tbl_IeltsExamResult> tbl_IeltsExamResult { get; set; }
        public virtual DbSet<tbl_IeltsAnswer> tbl_IeltsAnswer { get; set; }
        public virtual DbSet<tbl_IeltsQuestion> tbl_IeltsQuestion { get; set; }
        public virtual DbSet<tbl_IeltsQuestionGroup> tbl_IeltsQuestionGroup { get; set; }
        public virtual DbSet<tbl_IeltsSection> tbl_IeltsSection { get; set; }
        public virtual DbSet<tbl_IeltsSkill> tbl_IeltsSkill { get; set; }
        public virtual DbSet<tbl_IeltsExam> tbl_IeltsExam { get; set; }
        public virtual DbSet<tbl_DoingTestDetail> tbl_DoingTestDetail { get; set; }
        public virtual DbSet<tbl_DoingTest> tbl_DoingTest { get; set; }
        public virtual DbSet<tbl_TutoringFee> tbl_TutoringFee { get; set; }
        public virtual DbSet<tbl_FileCurriculumInClass> tbl_FileCurriculumInClass { get; set; }
        public virtual DbSet<tbl_CurriculumDetailInClass> tbl_CurriculumDetailInClass { get; set; }
        public virtual DbSet<tbl_CurriculumInClass> tbl_CurriculumInClass { get; set; }
        public virtual DbSet<tbl_DocumentLibraryDirectory> tbl_DocumentLibraryDirectory { get; set; }
        public virtual DbSet<tbl_DocumentLibrary> tbl_DocumentLibrary { get; set; }
        public virtual DbSet<tbl_Contract> tbl_Contract { get; set; }
        public virtual DbSet<tbl_FeedbackReply> tbl_FeedbackReply { get; set; }
        public virtual DbSet<tbl_Feedback> tbl_Feedback { get; set; }
        public virtual DbSet<tbl_NewsFeedReply> tbl_NewsFeedReply { get; set; }
        public virtual DbSet<tbl_NewsFeedComment> tbl_NewsFeedComment { get; set; }
        public virtual DbSet<tbl_NewsFeedLike> tbl_NewsFeedLike { get; set; }
        public virtual DbSet<tbl_StudentAssessment> tbl_StudentAssessment { get; set; }
        public virtual DbSet<tbl_ScheduleAvailable> tbl_ScheduleAvailable { get; set; }
        public virtual DbSet<tbl_HistoryDonate> tbl_HistoryDonate { get; set; }
        public virtual DbSet<tbl_MarkSalary> tbl_MarkSalary { get; set; }
        public virtual DbSet<tbl_MarkQuantity> tbl_MarkQuantity { get; set; }
        public virtual DbSet<tbl_PaymentApprove> tbl_PaymentApprove { get; set; }
        public virtual DbSet<tbl_PaymentAllow> tbl_PaymentAllow { get; set; }
        public virtual DbSet<tbl_PackageStudent> tbl_PackageStudent { get; set; }
        public virtual DbSet<tbl_PackageSkill> tbl_PackageSkill { get; set; }
        public virtual DbSet<tbl_PackageSection> tbl_PackageSection { get; set; }
        public virtual DbSet<tbl_Like> tbl_Like { get; set; }
        public virtual DbSet<tbl_FileInNewsFeed> tbl_FileInNewsFeed { get; set; }
        public virtual DbSet<tbl_Comment> tbl_Comment { get; set; }
        public virtual DbSet<tbl_UserInNFGroup> tbl_UserInNFGroup { get; set; }
        public virtual DbSet<tbl_NewsFeed> tbl_NewsFeed { get; set; }
        public virtual DbSet<tbl_Background> tbl_Background { get; set; }
        public virtual DbSet<tbl_NewsFeedGroup> tbl_NewsFeedGroup { get; set; }
        public virtual DbSet<tbl_VideoActiveCode> tbl_VideoActiveCode { get; set; }
        public virtual DbSet<tbl_Cart> tbl_Cart { get; set; }
        public virtual DbSet<tbl_Tag> tbl_Tag { get; set; }
        public virtual DbSet<tbl_TagCategory> tbl_TagCategory { get; set; }
        public virtual DbSet<tbl_Salary> tbl_Salary { get; set; }
        public virtual DbSet<tbl_SalaryConfig> tbl_SalaryConfig { get; set; }
        public virtual DbSet<tbl_Refund> tbl_Refund { get; set; }
        public virtual DbSet<tbl_ClassReserve> tbl_ClassReserve { get; set; }
        public virtual DbSet<tbl_ClassRegistration> tbl_ClassRegistration { get; set; }
        public virtual DbSet<tbl_ClassChange> tbl_ClassChange { get; set; }
        public virtual DbSet<tbl_NotificationInClass> tbl_NotificationInClass { get; set; }
        public virtual DbSet<tbl_Point> tbl_Point { get; set; }
        public virtual DbSet<tbl_Transcript> tbl_Transcript { get; set; }
        public virtual DbSet<tbl_TimeLine> tbl_TimeLine { get; set; }
        public virtual DbSet<tbl_RollUp> tbl_RollUp { get; set; }
        public virtual DbSet<tbl_Bill> tbl_Bill { get; set; }
        public virtual DbSet<tbl_PaymentSession> tbl_PaymentSession { get; set; }
        public virtual DbSet<tbl_BillDetail> tbl_BillDetail { get; set; }
        public virtual DbSet<tbl_PaymentMethod> tbl_PaymentMethod { get; set; }
        public virtual DbSet<tbl_TeacherOff> tbl_TeacherOff { get; set; }
        public virtual DbSet<tbl_TutorInProgram> tbl_TutorInProgram { get; set; }
        public virtual DbSet<tbl_TeacherInProgram> tbl_TeacherInProgram { get; set; }
        public virtual DbSet<tbl_Schedule> tbl_Schedule { get; set; }
        public virtual DbSet<tbl_StudentInClass> tbl_StudentInClass { get; set; }
        public virtual DbSet<tbl_Class> tbl_Class { get; set; }
        public virtual DbSet<tbl_FileInCurriculumDetail> tbl_FileInCurriculumDetail { get; set; }
        public virtual DbSet<tbl_CurriculumDetail> tbl_CurriculumDetail { get; set; }
        public virtual DbSet<tbl_Curriculum> tbl_Curriculum { get; set; }
        public virtual DbSet<tbl_StudyTime> tbl_StudyTime { get; set; }
        public virtual DbSet<tbl_Program> tbl_Program { get; set; }
        public virtual DbSet<tbl_Grade> tbl_Grade { get; set; }
        public virtual DbSet<tbl_StudentNote> tbl_StudentNote { get; set; }
        public virtual DbSet<tbl_TestAppointment> tbl_TestAppointment { get; set; }
        public virtual DbSet<tbl_CustomerNote> tbl_CustomerNote { get; set; }
        public virtual DbSet<tbl_CustomerStatus> tbl_CustomerStatus { get; set; }
        public virtual DbSet<tbl_Customer> tbl_Customer { get; set; }
        public virtual DbSet<tbl_Permission> tbl_Permission { get; set; }
        public virtual DbSet<tbl_FrequentlyQuestion> tbl_FrequentlyQuestion { get; set; }
        /// <summary>
        /// Các mẫu
        /// </summary>
        public virtual DbSet<tbl_Template> tbl_Template { get; set; }
        public virtual DbSet<tbl_Idiom> tbl_Idiom { get; set; }
        /// <summary>
        /// Thông báo chung
        /// </summary>
        public virtual DbSet<tbl_GeneralNotification> tbl_GeneralNotification { get; set; }
        public virtual DbSet<tbl_Purpose> tbl_Purpose { get; set; }
        public virtual DbSet<tbl_Job> tbl_Job { get; set; }
        public virtual DbSet<tbl_DayOff> tbl_DayOff { get; set; }
        public virtual DbSet<tbl_Source> tbl_Source { get; set; }
        public virtual DbSet<tbl_LearningNeed> tbl_LearningNeed { get; set; }
        public virtual DbSet<tbl_Discount> tbl_Discount { get; set; }
        public virtual DbSet<tbl_Room> tbl_Room { get; set; }
        public virtual DbSet<tbl_Branch> tbl_Branch { get; set; }
        public virtual DbSet<tbl_SeminarRecord> tbl_SeminarRecord { get; set; }
        public virtual DbSet<tbl_AnswerResult> tbl_AnswerResult { get; set; }
        public virtual DbSet<tbl_ExerciseResult> tbl_ExerciseResult { get; set; }
        public virtual DbSet<tbl_ExerciseGroupResult> tbl_ExerciseGroupResult { get; set; }
        public virtual DbSet<tbl_ExamSectionResult> tbl_ExamSectionResult { get; set; }
        public virtual DbSet<tbl_ExamResult> tbl_ExamResult { get; set; }
        public virtual DbSet<tbl_ZoomRoom> tbl_ZoomRoom { get; set; }
        public virtual DbSet<tbl_ZoomConfig> tbl_ZoomConfig { get; set; }
        public virtual DbSet<tbl_Seminar> tbl_Seminar { get; set; }
        public virtual DbSet<tbl_Certificate> tbl_Certificate { get; set; }
        public virtual DbSet<tbl_SectionCompleted> tbl_SectionCompleted { get; set; }
        public virtual DbSet<tbl_LessonCompleted> tbl_LessonCompleted { get; set; }
        public virtual DbSet<tbl_AnswerInVideo> tbl_AnswerInVideo { get; set; }
        public virtual DbSet<tbl_QuestionInVideo> tbl_QuestionInVideo { get; set; }
        public virtual DbSet<tbl_VideoCourseStudent> tbl_VideoCourseStudent { get; set; }
        public virtual DbSet<tbl_FileInVideo> tbl_FileInVideo { get; set; }
        public virtual DbSet<tbl_LessonVideo> tbl_LessonVideo { get; set; }
        public virtual DbSet<tbl_Section> tbl_Section { get; set; }
        public virtual DbSet<tbl_Product> tbl_Product { get; set; }
        public virtual DbSet<tbl_ChangeInfo> tbl_ChangeInfo { get; set; }
        public virtual DbSet<tbl_Config> tbl_Config { get; set; }
        public virtual DbSet<tbl_Area> tbl_Area { get; set; }
        public virtual DbSet<tbl_District> tbl_District { get; set; }
        public virtual DbSet<tbl_Notification> tbl_Notification { get; set; }
        public virtual DbSet<tbl_NotificationInVideo> tbl_NotificationInVideo { get; set; }
        public virtual DbSet<tbl_UserInformation> tbl_UserInformation { get; set; }
        public virtual DbSet<tbl_Ward> tbl_Ward { get; set; }
        public virtual DbSet<tbl_ExerciseGroup> tbl_ExerciseGroup { get; set; }
        public virtual DbSet<tbl_Exercise> tbl_Exercise { get; set; }
        public virtual DbSet<tbl_Answer> tbl_Answer { get; set; }
        public virtual DbSet<tbl_Exam> tbl_Exam { get; set; }
        public virtual DbSet<tbl_ExamSection> tbl_ExamSection { get; set; }
        public virtual DbSet<tbl_StudentRollUpQrCode> tbl_StudentRollUpQrCode { get; set; }
        public virtual DbSet<tbl_StudyRoute> tbl_StudyRoute { get; set; }
        public virtual DbSet<tbl_CertificateTemplate> tbl_CertificateTemplate { get; set; }
        public virtual DbSet<tbl_LearningHistory> tbl_LearningHistory { get; set; }
        public virtual DbSet<tbl_StudentPointRecord> tbl_StudentPointRecord { get; set; }
        public virtual DbSet<tbl_CommissionNorm> tbl_CommissionNorm { get; set; }
        public virtual DbSet<tbl_CommissionCampaign> tbl_CommissionCampaign { get; set; }
        public virtual DbSet<tbl_SaleRevenue> tbl_SaleRevenue { get; set; }
        public virtual DbSet<tbl_Commission> tbl_Commission { get; set; }
        public virtual DbSet<tbl_RatingSheet> tbl_RatingSheet { get; set; }
        public virtual DbSet<tbl_RatingQuestion> tbl_RatingQuestion { get; set; }
        public virtual DbSet<tbl_RatingOption> tbl_RatingOption { get; set; }
        public virtual DbSet<tbl_StudentRatingForm> tbl_StudentRatingForm { get; set; }
        public virtual DbSet<tbl_StudentRatingChoice> tbl_StudentRatingChoice { get; set; }
        public virtual DbSet<tbl_PaymentDetail> tbl_PaymentDetail { get; set; }
        public virtual DbSet<tbl_LessionVideoTest> tbl_LessionVideoTest { get; set; }
        public virtual DbSet<tbl_LessionVideoQuestion> tbl_LessionVideoQuestion { get; set; }
        public virtual DbSet<tbl_LessionVideoOption> tbl_LessionVideoOption { get; set; }
        public virtual DbSet<tbl_LessionVideoAssignment> tbl_LessionVideoAssignment { get; set; }
        public virtual DbSet<tbl_Homework> tbl_Homework { get; set; }
        public virtual DbSet<tbl_StudentHomework> tbl_StudentHomework { get; set; }
        public virtual DbSet<tbl_CustomerHistory> tbl_CustomerHistory { get; set; }
        public virtual DbSet<tbl_AttendaceConfig> tbl_AttendaceConfig { get; set; }
        public virtual DbSet<tbl_AttendaceType> tbl_AttendaceType { get; set; }
        public virtual DbSet<tbl_StudentCertificate> tbl_StudentCertificate { get; set; }
        public virtual DbSet<tbl_CustomerAudio> tbl_CustomerAudio { get; set; }
        public virtual DbSet<tbl_SpendingConfig> tbl_SpendingConfig { get; set; }
        public virtual DbSet<tbl_HomeworkFile> tbl_HomeworkFile { get; set; }
        public virtual DbSet<tbl_HomeworkResult> tbl_HomeworkResult { get; set; }
        public virtual DbSet<tbl_CertificateConfig> tbl_CertificateConfig { get; set; }
        public virtual DbSet<tbl_PopupConfig> tbl_PopupConfig { get; set; }
        public virtual DbSet<tbl_ReasonOut> tbl_ReasonOut { get; set; }
        public virtual DbSet<tbl_Combo> tbl_Combo { get; set; }
        public virtual DbSet<tbl_StudentDevice> tbl_StudentDevice { get; set; }
        public virtual DbSet<tbl_HomeworkInCurriculum> tbl_HomeworkInCurriculum { get; set; }
        public virtual DbSet<tbl_HomeworkFileInCurriculum> tbl_HomeworkFileInCurriculum { get; set; }
        public virtual DbSet<tbl_HomeworkSequenceConfigInClass> tbl_HomeworkSequenceConfigInClass { get; set; }
        public virtual DbSet<tbl_HomeworkSequenceConfigInCurriculum> tbl_HomeworkSequenceConfigInCurriculum { get; set; }

        //dịch vụ zns
        public virtual DbSet<tbl_ZnsConfig> tbl_ZnsConfig { get; set; }
        public virtual DbSet<tbl_ZnsTemplate> tbl_ZnsTemplate { get; set; }

        public async Task<List<T>> SqlQuery<T>(string sql)
        {
            List<T> data = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionStrings))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (reader.Read())
                    {
                        T item = MapObjectFromData<T>(reader);
                        data.Add(item);
                    }
                }
                await connection.CloseAsync();
            }

            return data;
        }

        private T MapObjectFromData<T>(SqlDataReader reader)
        {
            T obj = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                try
                {
                    var columnIndex = reader.GetOrdinal(property.Name);
                    if (!reader.IsDBNull(columnIndex))
                    {
                        var propertyType = property.PropertyType;
                        if (Nullable.GetUnderlyingType(propertyType) != null && reader[property.Name] is DBNull)
                        {
                            // Handle nullable types
                            property.SetValue(obj, null);
                        }
                        else
                        {
                            property.SetValue(obj, Convert.ChangeType(reader[property.Name], Nullable.GetUnderlyingType(propertyType) ?? propertyType));
                        }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // Column not found, skip
                    continue;
                }
            }

            return obj;
        }
    }
}
