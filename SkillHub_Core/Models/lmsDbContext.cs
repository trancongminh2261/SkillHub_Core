namespace LMSCore.Models
{
    using LMS_Project.Models;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using SkillHub_Core.Models;
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
        }

        public virtual DbSet<tbl_VideoConfigOption> tbl_VideoConfigOption { get; set; }
        public virtual DbSet<tbl_VideoConfigQuestion> tbl_VideoConfigQuestion { get; set; }
        public virtual DbSet<tbl_VideoConfig> tbl_VideoConfig { get; set; }
        public virtual DbSet<tbl_AntiDownVideo> tbl_AntiDownVideo { get; set; }
        public virtual DbSet<tbl_VideoCourseAllow> tbl_VideoCourseAllow { get; set; }
        public virtual DbSet<tbl_WriteLog> tbl_WriteLog { get; set; }
        public virtual DbSet<tbl_StudyRouteDetail> tbl_StudyRouteDetail { get; set; }
        public virtual DbSet<tbl_StudyRoute> tbl_StudyRoute { get; set; }
        public virtual DbSet<tbl_Department> tbl_Department { get; set; }
        public virtual DbSet<tbl_TimeWatchingVideo> tbl_TimeWatchingVideo { get; set; }
        public virtual DbSet<tbl_GradingEssay> tbl_GradingEssay { get; set; }
        public virtual DbSet<tbl_Standard> tbl_Standard { get; set; }
        public virtual DbSet<tbl_Permission> tbl_Permission { get; set; }
        public virtual DbSet<tbl_Tag> tbl_Tag { get; set; }
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
        public virtual DbSet<tbl_CertificateConfig> tbl_CertificateConfig { get; set; }
        public virtual DbSet<tbl_SectionCompleted> tbl_SectionCompleted { get; set; }
        public virtual DbSet<tbl_LessonCompleted> tbl_LessonCompleted { get; set; }
        public virtual DbSet<tbl_AnswerInVideo> tbl_AnswerInVideo { get; set; }
        public virtual DbSet<tbl_QuestionInVideo> tbl_QuestionInVideo { get; set; }
        public virtual DbSet<tbl_VideoCourseStudent> tbl_VideoCourseStudent { get; set; }
        public virtual DbSet<tbl_FileInVideo> tbl_FileInVideo { get; set; }
        public virtual DbSet<tbl_LessonVideo> tbl_LessonVideo { get; set; }
        public virtual DbSet<tbl_Section> tbl_Section { get; set; }
        public virtual DbSet<tbl_VideoCourse> tbl_VideoCourse { get; set; }
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
        public virtual DbSet<tbl_Topic> tbl_Topic { get; set; }
        public virtual DbSet<tbl_Document> tbl_Document { get; set; } 

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
