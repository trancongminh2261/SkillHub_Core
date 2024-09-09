using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using static LMSCore.Areas.Models.ClassRegistrationSearch;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Areas.Models
{
    public class SaveGradesInClassSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn bảng điểm")]
        public int ClassTranscriptId { get; set; }
    }
    public class ClassTranscriptSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int ClassId { get; set; }
    }
    public class ScheduleRecordSearch
    {
        public int? ScheduleId { get; set; }
    }
    public class TotalQuestionUncompletedSkillSearch
    {
        public int DoingTestId { get; set; }
        public int IeltsSkillId { get; set; }
    }
    public class MonthlyTuitionSearch : SearchOptions
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
    public class IeltsSectionSearch : SearchOptions
    {
        [Required]
        public int IeltsSkillId { get; set; }
    }
    public class StudentHomeworkSearch : SearchOptions
    {
        /// <summary>
        /// id btvn
        /// mẫu: 1
        /// </summary>
        public int? HomeworkId { get; set; }
        /// <summary>
        /// id lớp
        /// mẫu: 1
        /// </summary>
        public int? ClassId { get; set; }
        /// <summary>
        /// id học viên
        /// mẫu: 1,2
        /// </summary>
        public string StudentIds { get; set; }
        /// <summary>
        /// trạng thái
        /// mẫu: 1,2
        /// 1 - chưa làm
        /// 2 - đang làm
        /// 3 - đã nộp
        /// 4 - không nộp
        /// 5 - nộp trễ
        /// </summary>
        public string Statuses { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
    public class HomeworkSearch : SearchOptions
    {
        public int? ClassId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int? StudentId { get; set; }
    }
    public class HomeworkResultSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn bài tập")]
        public int HomeworkId { get; set; }
        public int? StudentId { get; set; }
        public HomeworkResultType? Type { get; set; }
    }
    public class IeltsExamResultSearch : SearchOptions
    {
        public int IeltsExamId { get; set; }
        public int StudentId { get; set; }
        public int ValueId { get; set; }
        /// <summary>
        /// 1 - Làm bài thử
        /// 2 - Làm bài hẹn test 
        /// 3 - Bài tập về nhà
        /// 4 - Bộ đề
        /// 5 - Luyện tập
        /// </summary>
        public int Type { get; set; }
        public int TeacherId { get; set; }
        /// <summary>
        /// 1 - Đang chấm bài
        /// 2 - Đã chấm xong
        /// </summary>
        public int Status { get; set; }
    }
    public class CustomerStatusSearch : SearchOptions
    {
        public string Name { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
    }
    public class CustomerStatusV2Search : SearchOptions
    {
        public string Name { get; set; }
        public string BranchIds { get; set; }
        public int? SaleId { get; set; }
    }
    public class IeltsSkillSearch : SearchOptions
    {
        [Required]
        public int IeltsExamId { get; set; }
    }
    public class ScheduleTeacherOffSearch : SearchOptions
    {
        public int TeacherOffId { get; set; }
    }
    public class IeltsQuestionGroupSearch : SearchOptions
    {
        /// <summary>
        /// 1 - Dễ 
        /// 2 - Trung bình
        /// 3 - Khó
        /// </summary>
        public string Levels { get; set; }
        /// <summary>
        /// 1 - Trắc nghiệm
        /// 2 - Chọn từ vào ô trống
        /// 3 - Điền vào ô trống
        /// 4 - Mindmap
        /// 5 - True/False/Not Given
        /// 6 - Sắp xếp câu  
        /// 7 - Viết 
        /// 8 - Nói
        /// </summary>
        public string Types { get; set; }
        /// <summary>
        /// Từ khóa
        /// </summary>
        public string TagIds { get; set; }
        /// <summary>
        /// true - Câu gốc
        /// </summary>
        public bool? IsSource { get; set; }
        /// <summary>
        /// Kiểm tra tồn tại trong đề hay không
        /// </summary>
        public int HasInIeltsExamId { get; set; }
    }
    public class CertificateTemplateSearch : SearchOptions
    {
        public string Name { get; set; }
    }
    public class ChartSearch
    {
        public string BranchIds { get; set; }
        public int? Year { get; set; }
    }

    public class NewChartSearch
    {
        [Required(ErrorMessage = "Bạn chưa chọn ngày")]
        public DateTime FromDate { get; set; }
        [Required(ErrorMessage = "Bạn chưa chọn ngày")]
        public DateTime ToDate { get; set; }
    }

    public class OverviewFilter
    {
        public string BranchIds { get; set; }
        public int? UserId { get; set; }
    }
    public class StudyRouteSearch : SearchOptions
    {
        public int? StudentId { get; set; }
    }
    public class PointSearch : SearchOptions
    {
        public string ParentIds { get; set; }
        public string StudentIds { get; set; }
        public int ClassId { get; set; } = 0;
    }
    public class CurriculumDetailInClassSearch
    {
        public int? CurriculumIdInClassId { get; set; }
    }
    public class StudentAssessmentSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
    }
    public class FilesCurriculumInClassSearch
    {
        public int? CurriculumDetailInClassId { get; set; }
    }
    public class DocumentLibrarySearch : SearchOptions
    {
        public int? DirectoryId { get; set; }
        /// <summary>
        /// 0 - Ngày tạo
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
    }
    public class DocumentLibraryDirectorySearch
    {
        public string Name { get; set; }
        /// <summary>
        /// 0 - Ngày tạo
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
    }
    public class FeedbackReplySearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn phản hồi")]
        public int? FeedbackId { get; set; }
        /// <summary>
        /// 0 - Ngày tạo
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;

    }
    public class FeedbackSearch : SearchOptions
    {
        /// <summary>
        /// Lọc theo trạng thái
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 0 - Ngày tạo
        /// 1 - Số sao đánh giá 
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
        public string UserIds { get; set; }
    }
    public class FeedbackV2Search : SearchOptions
    {
        /// <summary>
        /// Lọc theo trạng thái
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 0 - Ngày tạo
        /// 1 - Số sao đánh giá 
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
        public string UserIds { get; set; }
        public string BranchIds { get; set; }
    }
    public class ContractSearch : SearchOptions
    {
        public int StudentId { get; set; } = 0;
    }
    public class NewsFeedReplySearch : SearchOptions
    {
        [Required(ErrorMessage = ("Vui lòng chọn bình luận"))]
        public int? NewsFeedCommentId { get; set; }
    }
    public class NewsFeedCommentSearch : SearchOptions
    {
        /// <summary>
        /// Lọc theo id bản tin
        /// </summary>
        [Required(ErrorMessage = ("Vui lòng chọn bản tin"))]
        public int? NewsFeedId { get; set; }
        /// <summary>
        /// 0 : Ngày tạo
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
    }
    public class ScheduleAvailableSearch : SearchOptions
    {
        public override int PageSize { get; set; } = 999999;
        public int TeacherId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime? From { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        public DateTime? To { get; set; }
    }
    public class HistoryDonateSearch : SearchOptions
    {
        public int Type { get; set; } = 0;
    }
    public class MarkQuantitySearch : SearchOptions
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
    }
    public class MarkSalarySearch : SearchOptions
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
    }
    public class PaymentApproveSearch : SearchOptions
    {
        public int Status { get; set; } = 0;
        public int UserId { get; set; } = 0;
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
    public class PaymentApproveV2Search : SearchOptions
    {
        public int Status { get; set; } = 0;
        public int UserId { get; set; } = 0;
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string BranchIds { get; set; }
    }
    public class PackageStudentSearch : SearchOptions
    {
        /// <summary>
        /// mẫu : 1,2,3
        /// </summary>
        public string Tags { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// 2 - Giá
        /// </summary>
        public int Sort { get; set; } = 0;
        public int StudentId { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
    }
    public class PackageSkillSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn phần")]
        public int PackageSectionId { get; set; }
    }
    public class PackageSectionSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn bộ đề")]
        public int? PackageId { get; set; }
    }

    public class NewsFeedSearch : SearchOptions
    {

        /// <summary>
        /// Lọc theo nhóm
        /// </summary>
        public int? NewsFeedGroupId { get; set; }


        /// <summary>
        /// Lọc theo
        /// </summary>
        public int? BranchIds { get; set; }

    }
    public class UserInNFGroupSearch : SearchOptions
    {
        [Required]
        public int? NewsFeedGroupId { get; set; }
        /// <summary>
        /// Lọc theo tên thành viên
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Lọc theo role gốc của thành viên
        /// Giáo viên
        /// Học vụ
        /// Học viên
        /// </summary>
        public string RoleId { get; set; }
        /// <summary>
        /// Lọc theo kiểu thành viên nhóm
        /// Quản trị viên
        /// Thành viên
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// 0 - Ngày tạo
        /// 1 - Tên thành viên
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
    }

    public class NewsFeedGroupSearch : SearchOptions
    {
        public int? ClassId { get; set; }
        public int? BranchId { get; set; }
        /// <summary>
        /// 0 - Ngày tạo
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
    }
    public class VideoActiveCodeSearch : SearchOptions
    {
        public int BillDetailId { get; set; }
        public int StudentId { get; set; }
    }
    public class TagSearch : SearchOptions
    {
        //[Required(ErrorMessage = "Vui lòng chọn loại từ khóa")]
        public int? Type { get; set; } = 0;
        public int? TagCategoryId { get; set; }
    }
    public class TagCategorySearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public int? Type { get; set; }
    }
    public class TeachingDetailSearch : SearchOptions
    {
        public int SalaryId { get; set; }
    }
    public class SalarySearch : SearchOptions
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string Roles { get; set; }
        public int? Status { get; set; }
    }
    public class SalaryV2Search : SearchOptions
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string Roles { get; set; }
        public int? Status { get; set; }
        public string BranchIds { get; set; }
    }
    public class SalaryExportSearch
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string Search { get; set; }
    }
    public class SalaryConfigSearch : SearchOptions
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
    }
    public class SalaryConfigV2Search : SearchOptions
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string BranchIds { get; set; }
    }
    public class RefundSearch : SearchOptions
    {
        public string BranchIds { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
    public class RefundStatusSearch
    {
        public string Search { get; set; }
        public string BranchIds { get; set; }
        public string Type { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
    public class ClassReserveSearch : SearchOptions
    {
        public string BranchIds { get; set; }
        public string Status { get; set; }
    }

    public class ClassRegistrationSearch : SearchOptions
    {
        public string BranchIds { get; set; }
        public string ProgramIds { get; set; }
        public string Status { get; set; }
    }
    public class StudentRegistrationSearch : SearchOptions
    {
        public string BranchIds { get; set; }
        public int ProgramId { get; set; }
        public string ExectedDays { get; set; }
        public string StudyTimeIds { get; set; }
        public bool IsUndetermined { get; set; } = false;
        // Loại bỏ 
        /// <summary>
        /// Bỏ thứ trong tuần
        /// </summary>
        public string RejectExectedDays { get; set; }
        /// <summary>
        /// Bỏ ca học
        /// </summary>
        public string RejectStudyTimeIds { get; set; }
        /// <summary>
        /// Bỏ học viên chưa xác định thời gian
        /// true: bỏ học viên chưa xác định
        /// </summary>
        public bool? RejectUndetermined { get; set; } = false;
    }
    public class ProgramRegistrationSearch
    {
        public string BranchIds { get; set; }
    }
    public class ScheduleRegistrationSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn chương trình học")]
        public int ProgramId { get; set; }

        public string BranchIds { get; set; }
    }
    public class ClassChangeSearch : SearchOptions
    {
        public string BranchIds { get; set; }
    }
    public class NotificationInClassSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
    }
    public class TimeLineSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
    }
    public class RollUpTeacherSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
    }
    public class RollUpSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn buổi")]
        public int? ScheduleId { get; set; }
        public string StudentIds { get; set; }
        public string ParentIds { get; set; }
    }
    public class StudentInClassSearch : SearchOptions
    {
        public int ClassId { get; set; } = 0;
        /// <summary>
        /// Lấy danh sách học viên cảnh báo
        /// </summary>
	    public bool? Warning { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
        public string ParentIds { get; set; }
        public string StudentIds { get; set; }
        public bool disable { get; set; }
    }
    public class StudentInClassV2Search : SearchOptions
    {
        public int ClassId { get; set; } = 0;
        /// <summary>
        /// Lấy danh sách học viên cảnh báo
        /// </summary>
	    public bool? Warning { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
        public string ParentIds { get; set; }
        public string StudentIds { get; set; }
        public bool disable { get; set; }
        public string BranchIds { get; set; }
    }

    public class StudentInRegisSearch : SearchOptions
    {
        public int ClassId { get; set; } = 0;

        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }

        public string ParentIds { get; set; }

        /// <summary>
        /// 0 - Số buổi còn lại
        /// 1 - Ngày kết thúc
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Với type = 0
        /// </summary>
        public int LessonRemaining { get; set; } = 5;

        /// <summary>
        /// Với type = 1
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    public class StudentInRegisV2Search : SearchOptions
    {
        public int ClassId { get; set; } = 0;

        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }

        public string ParentIds { get; set; }

        /// <summary>
        /// 0 - Số buổi còn lại
        /// 1 - Ngày kết thúc
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Với type = 0
        /// </summary>
        public int LessonRemaining { get; set; } = 5;

        /// <summary>
        /// Với type = 1
        /// </summary>
        public DateTime? EndDate { get; set; }
        public string BranchIds { get; set; }
    }

    public class AvailableStudentSearch
    {
        /// <summary>
        /// Lọc theo lớp nào
        /// </summary>
        public int classId { get; set; }
    }

    public class PaymentSessionSearch : SearchOptions
    {
        public string BranchIds { get; set; }
        /// <summary>
        /// 1 - Thu 
        /// 2 - Chi
        /// </summary>
        public int? Type { get; set; }
        public int? BillId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
    public class PaymentSessionPdf
    {
        [Required]
        public string Content { get; set; }
    }
    public class PaymentSessionOfStudentSearch
    {
        [Required]
        public int BillId { get; set; }
        [Required]
        public int BranchId { get; set; }
    }
    public class ClassReserveOptionSearch
    {
        public int StudentId { get; set; }
        public ClassReserveOptionSearch()
        {

        }
    }
    public class BillSearch : SearchOptions
    {
        public int? Type { get; set; }
        public string StudentIds { get; set; }
        public string BranchIds { get; set; }
        public string ParentIds { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int Id { get; set; }
        /// <summary>
        /// 1 - Chưa thanh toán hết
        /// 2 - Đã thanh toán hết
        /// </summary>
        public int Status { get; set; }
    }

    public class AppointmentDueSoonSearch : SearchOptions
    {
        public int? Type { get; set; }
        [JsonIgnore]
        public string StudentIds { get; set; }
        [JsonIgnore]
        public string BranchIds { get; set; }
        public string ToDate { get; set; }
    }
    public class ScheduleSearch : SearchOptions
    {
        public override int PageSize { get; set; } = 999999;
        public int ClassId { get; set; } = 0;
        public int StudentId { get; set; } = 0;
        /// <summary>
        /// mẫu: 1,2,3
        /// </summary>
	    public string BranchIds { get; set; }
        /// <summary>
        /// mẫu: 1,2,3
        /// </summary>
	    public string TeacherIds { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int ParentId { get; set; } = 0;
    }
    public class ClassSearch : SearchOptions
    {
        public string Name { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// Mẫu
        /// </summary>
        public string ClassIds { get; set; }
        public string ProgramIds { get; set; }
        public string GradeIds { get; set; }
        public string Types { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// 2 - Status
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
        public int StudentId { get; set; }
    }
    public class TeacherOffSearch : SearchOptions
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Duyệt
        /// 3 - Không duyệt
        /// mẫu 1,2
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// id nghỉ phép
        /// </summary>
        public int? teacherOffId { get; set; }
    }
    public class TeacherOffV2Search : SearchOptions
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Duyệt
        /// 3 - Không duyệt
        /// mẫu 1,2
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// id nghỉ phép
        /// </summary>
        public int? teacherOffId { get; set; }
        public string BranchIds { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
    public class TeacherInProgramSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn chương trình")]
        public int? ProgramId { get; set; }
        public string BranchIds { get; set; }
    }
    public class CurriculumDetailSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn giáo trình")]
        public int? CurriculumId { get; set; }
    }
    public class CurriculumSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn chương trình")]
        public int? ProgramId { get; set; }
    }
    public class ProgramSearch : SearchOptions
    {

        public string Code { get; set; }
        public string Name { get; set; }
        public int? GradeId { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
    }
    public class GradeSearch : SearchOptions
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class StudentNoteSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
    }
    public class StaffNoteSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        public int? StaffId { get; set; }
    }
    public class TestAppointmentSearch : SearchOptions
    {

        public string FullName { get; set; }
        public string UserCode { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string LearningStatus { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// 0 - Id,1 - Name,2 - Ngày hẹn
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }
    }
    public class CustomerNoteSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int? CustomerId { get; set; }
    }
    public class CustomerSearch : SearchOptions
    {
        public string FullName { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string CustomerStatusIds { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class CustomerV2Search : SearchOptions
    {
        public string FullName { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string CustomerStatusIds { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? SaleId { get; set; }
    }

    public class DiscountSearch : SearchOptions
    {
        public string Code { get; set; }
        public int Status { get; set; }
        public string BranchIds { get; set; }
        public int? PackageType { get; set; }
    }
    public class RoomSearch : SearchOptions
    {
        public int BranchId { get; set; } = 0;
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class BranchSearch : SearchOptions
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class OverviewSearch : SearchOptions
    {
        public string Search { get; set; }
    }
    public class NotificationInVideoSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn khoá học")]
        public int? VideoCourseId { get; set; }
    }
    public class CertificateSearch : SearchOptions
    {
        public int UserId { get; set; } = 0;
    }
    public class ExamResultSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn đề")]
        public int? ExamId { get; set; }
        public int StudentId { get; set; } = 0;
        public int VideoCourseId { get; set; } = 0;
    }
    public class ExerciseGroupSearch : SearchOptions
    {
        public int Id { get; set; } = 0;
        /// <summary>
        /// Kiểm tra tồn tại trong đề, truyền vào ExamId
        /// </summary>
        public int NotExistInExam { get; set; }
        public string Tags { get; set; }
        public ExerciseLevel? Level { get; set; }
        public ExerciseType? Type { get; set; }
    }
    public class ExamSearch : SearchOptions
    {
    }
    public class QuestionInVideoSearch : SearchOptions
    {
        [Required]
        public int VideoCourseId { get; set; }
    }
    public class SeminarSearch : SearchOptions
    {
        public string Name { get; set; }
        public SeminarStatus? Status { get; set; }
    }
    public class VideoCourseStudentSearch : SearchOptions
    {
        /// <summary>
        /// mẫu : code,be
        /// </summary>
        public string Stags { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
    }
    public class StudentInVideoCourseSearch : SearchOptions
    {
        [Required]
        public int VideoCourseId { get; set; }
        public string FullName { get; set; }
        /// <summary>
        /// yyyy/MM/dd
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// yyyy/MM/dd
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
    public class ProductSearch : SearchOptions
    {
        /// <summary>
        /// 1 - Khóa video
        /// 2 - Bộ đề
        /// 3 - Lượt chấm bài
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        public int? Type { get; set; }
        /// <summary>
        /// mẫu : 1,2,3
        /// </summary>
        public string Tags { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// 2 - Giá
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
    }
    public class ChangeInfo : SearchOptions
    {
        public string UserCode { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string Statuss { get; set; }
    }
    public class UserSearch : SearchOptions
    {
        public override int PageSize { get; set; } = 999999;
        /// <summary>
        /// 0 - Ngày tạo
        /// 1 - Tên 
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
        public string FullName { get; set; }
        /// <summary>
        /// Có thể nhập Id của bất kì user nào cũng được
        /// </summary>
        public string UserInformationIds { get; set; }
        public string UserCode { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string RoleIds { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string Genders { get; set; }
        /// <summary>
        /// id phụ huynh
        /// </summary>
        public string ParentIds { get; set; }
        /// <summary>
        /// id học sinh
        /// </summary>
        public string StudentIds { get; set; }
        /// <summary>
        /// id của giáo viên
        /// </summary>
        public string TeacherIds { get; set; }
    }
    public class UserExportSearch
    {
        /// <summary>
        /// đối tượng export
        /// 1 - Học viên
        /// 2 - Nhân viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn đối tượng cần xuất thông tin")]
        public int Type { get; set; } = 1;
        /// <summary>
        /// học viên - 3
        /// nhân viên - 2,4,5,6,7,9
        /// </summary>
        public string RoleIds { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
    }
    public class WardSearch : SearchOptions
    {
        public int DistrictId { get; set; } = 0;
        public string Name { get; set; }
    }
    public class DistrictSearch : SearchOptions
    {
        public int AreaId { get; set; } = 0;
        public string Name { get; set; }
    }
    public class AreaSearch : SearchOptions
    {
        public string Name { get; set; }
    }
    public class LearningHistorySearch : SearchOptions
    {
        [Required(ErrorMessage = "Họp viên không được bỏ trống")]
        public int? StudentId { get; set; }
    }
    public class WarningHistorySearch
    {
        [Required(ErrorMessage = "Họp viên không được bỏ trống")]
        public int StudentId { get; set; }
        public int? ClassId { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageIndex { get; set; } = 1;
    }
    public class StudentPointRecordSearch : SearchOptions
    {
        public int ClassId { get; set; } = 0;
        public int Year { get; set; } = 2023;
        public int Month { get; set; } = 1;
    }
    public class AttendanceSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int? ClassId { get; set; }
    }
    public class HistoryCheckWritingSearch : SearchOptions
    {
        public int? UserId { get; set; }
    }

    public class SearchOptions
    {
        public virtual int PageSize { get; set; } = 20;
        public virtual int PageIndex { get; set; } = 1;
        public virtual string Search { get; set; }
    }
    public class ComboSearch : SearchOptions
    {
        public ComboStatus? Status { get; set; }
        public ComboType? Type { get; set; }
        public string ProgramIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
    public class HeaderOptions : SearchOptions
    {
        public string BranchIds { get; set; }
    }
    public class MenuNumberOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public string BranchIds { get; set; }
    }
    public class FrequentlyQuestionSearch : SearchOptions
    {
        public int? RoleId { get; set; }
    }
    public class TutorSalarySearch : SearchOptions
    {
        public int? TutorSalaryConfig { get; set; }
    }
    public class CommissionNormSearch : SearchOptions
    {
        public string Name { get; set; }
    }

    public class CommissionConfigSearch : SearchOptions
    {
        public string FullName { get; set; }
        public string UserCode { get; set; }
    }
    public class CommissionCampaignSearch : SearchOptions
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
    }
    public class CommissionSearch : SearchOptions
    {
        public int Year { get; set; } = 2023;
        public int Month { get; set; } = 1;
    }
    public class SaleRevenueSearch : SearchOptions
    {
        public int? SaleId { get; set; }
    }
    public class AllSaleSearch : SearchOptions
    {
        public int? Year { get; set; } = DateTime.Now.Year;
        public int? Month { get; set; } = DateTime.Now.Month;
    }
    public class StudentInClassBySaleSearch : SearchOptions
    {
        public int? SaleId { get; set; }
        public int? Year { get; set; } = DateTime.Now.Year;
        public int? Month { get; set; } = DateTime.Now.Month;
    }


    public class ConsultantRevenueSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn tư vấn viên")]
        public int? SaleId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class LeadSwitchTestSearch : SearchOptions
    {
        public int? SaleId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class RatingSheetSearch : SearchOptions
    {
        public int ClassId { get; set; }
    }
    public class RatingQuestionSearch : SearchOptions
    {
        public int RatingSheetId { get; set; }
    }
    public class RatingOptionSearch : SearchOptions
    {
        public string Name { get; set; }
        public int? RatingQuestionId { get; set; }
    }
    public class StudentRatingFormSearch : SearchOptions
    {
        public int? StudentId { get; set; }
        public int? RatingSheetId { get; set; }
    }
    public class StudentRatingChoiceSearch : SearchOptions
    {
        public int StudentRatingFormId { get; set; }
    }
    public class TeacherScheduleEmptySearch
    {
        [Required]
        public int ProgramId { get; set; }
        [Required]
        public int BranchId { get; set; }
        public string TeacherId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        /// <summary>
        /// 1,2,3
        /// </summary>
        public string StudyTime { get; set; } = null;
    }
    public class RoomScheduleEmptySearch
    {
        public string RooomId { get; set; } = null;
        [Required]
        public int BranchId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string StudyTime { get; set; } = null;
    }
    public class TeacherScheduleStatistic : SearchOptions
    {
        public int BranchId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
    public class LessionVideoTestSearch : SearchOptions
    {
        public int SectionId { get; set; }

    }
    public class LessionVideoQuestionSearch : SearchOptions
    {
        public int TestId { get; set; }
        //public string Name { get; set; }
        public string Level { get; set; }
    }
    public class LessionVideoOptionSearch : SearchOptions
    {
        public int QuestionId { get; set; }
        //public string Content { get; set; } 

    }
    public class LessionVideoAssignmentSearch : SearchOptions
    {
        public int StudentId { get; set; }
        public int TestId { get; set; }
    }
    public class StudentMonthlyDebtSearch : SearchOptions
    {
        public int? StudentId { get; set; }
        public int? ClassId { get; set; }
        /// <summary>
        /// By student name
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
        public int? Month { get; set; }
    }

    public class CurriculumVitaeSearch : SearchOptions
    {
        public int? BranchId { get; set; }
        public int? JobPosition { get; set; }
        /// <summary>
        /// By full name
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
    }

    public class InterviewAppointmentSearch : SearchOptions
    {
        public string CurriculumVitaeName { get; set; }
        public int? BranchId { get; set; }
        /// <summary>
        /// By Interview Appointment Date
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
    }

    public class InputTestResultSearch : SearchOptions
    {
        public string FullName { get; set; }
        /// <summary>
        /// By Created On
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; } = false;
    }
    public class ScoreBoardTemplateSearch : SearchOptions
    {
    }
    public class ScoreColumnTemplateSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn bảng điểm mẫu")]
        public int ScoreBoardTemplateId { get; set; }
    }
    public class ScoreColumnSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int ClassId { get; set; }
    }
    public class ScoreSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn đợt thi")]
        public int TranscriptId { get; set; }

    }
    public class OldClassSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên trong lớp")]
        public int? StudentInClassId { get; set; }
    }
    public class TrainingRouteFormSearch : SearchOptions
    {
        public int? TrainingRouteId { get; set; }
    }
    public class TrainingRouteDetailSearch : SearchOptions
    {
        public int? TrainingRouteId { get; set; }
        public int? TrainingRouteFormId { get; set; }
        public int? StudentId { get; set; }
    }
    public class StudentInTrainingSearch : SearchOptions
    {
        public int? TrainingRouteId { get; set; }
        public int? StudentId { get; set; }
    }
    public class TrainingDoingTestSearch : TrainingRouteDetailSearch
    {
        public int? Level { get; set; }
        public int? Type { get; set; }
    }
    public class CommissionOfSaleSearch : SearchOptions
    {
        public string BranchIds { get; set; }
        public int? Month { get; set; } = DateTime.Now.Month;
        public int? Year { get; set; } = DateTime.Now.Year;
    }
    public class DayOffSearch : SearchOptions
    {
        public string BranchIds { get; set; }
    }
    #region báo cáo thống kê
    public class StatisticalSearch : SearchOptions
    {
        public string BranchIds { get; set; } = "";
        public int? Month { get; set; } = DateTime.Now.Month;
        public int? Year { get; set; } = DateTime.Now.Year;
    }
    public class ReportDetailSearch : StatisticalSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn thông tin cần xem chi tiết")]
        public int Id { get; set; }
    }
    #endregion

    #region Export báo cáo thống kê
    public class ExportStatisticalSearch
    {
        public string BranchIds { get; set; } = "";
        public int? Month { get; set; } = DateTime.Now.Month;
        public int? Year { get; set; } = DateTime.Now.Year;
    }
    #endregion
    public class CustomerExportSearch
    {
        public string FullName { get; set; }
        public string Search { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string CustomerStatusIds { get; set; }
        /// <summary>
        /// mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// 0 - Id
        /// 1 - Name
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// true: tăng dần, false: giảm dần
        /// </summary>
        public bool SortType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? SaleId { get; set; }
    }
    public class AttendaceConfigSearch : SearchOptions
    {
        public string AttendanceTypeIds { get; set; }
        public bool? Active { get; set; }
    }
    #region Lọc giáo viên và trợ giảng phù hợp lớp học
    public class AppropriateSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int BranchId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn chương trình")]
        public int ProgramId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn giáo trình")]
        public int CurriculumId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        public DateTime StartDay { get; set; }
        public List<LessonTime> LessonTime { get; set; }
    }
    public class LessonTime
    {
        /// <summary>
        /// Ngày trong tuần
        /// </summary>
        public int DayOfWeek { get; set; }
        /// <summary>
        /// Ca học
        /// </summary>
        public int StudyTimeId { get; set; }
    }
    public class CountTime
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class AppropriateTeacherModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public List<SameTime> SameTimes { get; set; }

        public bool AppropriateTeacher;
    }
    public class SameTime
    {
        /// <summary>
        /// 1-Trùng ngày dạy, 2-Trùng ngày nghỉ
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
    #endregion

    public class CustomerForSalerSearch : SearchOptions
    {
        /// <summary>
        /// Trung tâm / Có thể chọn nhiều
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// Khách hàng / Có thể chọn nhiều
        /// </summary>
        public string CustomerIds { get; set; }
        /// <summary>
        /// Tư vấn viên / Có thể chọn nhiều
        /// </summary>
        public string SaleIds { get; set; }
        /// <summary>
        /// Nhu cầu học / Có thể chọn nhiều
        /// </summary>
        public string LearningNeedIds { get; set; }
        /// <summary>
        /// Mục tiêu học / Có thể chọn nhiều
        /// </summary>
        public string PurposeIds { get; set; }
        /// <summary>
        /// Nguồn khách hàng / Có thể chọn nhiều
        /// </summary>
        public string SourceIds { get; set; }
        /// <summary>
        /// true - Sẽ lấy khách hàng chưa được assign hoặc có tư vấn viên nhưng tư vấn viên không tồn tại trong hệ thống /
        /// false - Sẽ lấy tất cả khách hàng
        /// </summary>
        public bool? IsAssign { get; set; }
        /// <summary>
        /// Từ ngày / Check CreateOn
        /// </summary>
        public string FromDate { get; set; }
        /// <summary>
        /// Đến ngày / Check CreateOn
        /// </summary>
        public string ToDate { get; set; }
    }

    public class SalerSearch : SearchOptions
    {
        /// <summary>
        /// Trung tâm / Có thể chọn nhiều
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// Tư vấn viên / Có thể chọn nhiều
        /// </summary>
        public string SaleIds { get; set; }
    }

    public class StudentForSalerSearch : SearchOptions
    {
        /// <summary>
        /// Trung tâm / Có thể chọn nhiều
        /// </summary>
        public string BranchIds { get; set; }
        /// <summary>
        /// Học sinh / Có thể chọn nhiều
        /// </summary>
        public string StudentIds { get; set; }
        /// <summary>
        /// Tư vấn viên / Có thể chọn nhiều
        /// </summary>
        public string SaleIds { get; set; }
        /// <summary>
        /// Nhu cầu học / Có thể chọn nhiều
        /// </summary>
        public string LearningNeedIds { get; set; }
        /// <summary>
        /// Mục tiêu học / Có thể chọn nhiều
        /// </summary>
        public string PurposeIds { get; set; }
        /// <summary>
        /// Nguồn khách hàng / Có thể chọn nhiều
        /// </summary>
        public string SourceIds { get; set; }
        /// <summary>
        /// true - Sẽ lấy học viên chưa được assign hoặc có tư vấn viên nhưng tư vấn viên không tồn tại trong hệ thống /
        /// false - Sẽ lấy tất cả học viên
        /// </summary>
        public bool? IsAssign { get; set; }
        /// <summary>
        /// Từ ngày / Check CreateOn
        /// </summary>
        public string FromDate { get; set; }
        /// <summary>
        /// Đến ngày / Check CreateOn
        /// </summary>
        public string ToDate { get; set; }
    }
    public class CustomerAudioSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int CustomerId { get; set; }
    }
    public class SpendingConfigSearch : SearchOptions
    {
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int BranchId { get; set; }
        public bool? Active { get; set; }
    }
    public class DropdownSpendingConfig
    {
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int BranchId { get; set; }
    }
    public class HomeworkFileSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn bài tập")]
        public int HomeworkId { get; set; }
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng loại bài tập")]
        public HomeworkFileType Type { get; set; }

    }
    public class StudentCertificateSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
    }
    public class ExpectedSalarySearch : SearchOptions
    {
        /// <summary>
        /// Từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// Đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }
        public string BranchIds { get; set; }
    }

    public class ExpectedSheduleSearch : SearchOptions
    {
        /// <summary>
        /// Từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// Đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thông tin giáo viên")]
        public string TeacherId { get; set; }
        /// <summary>
        /// True: Lấy lịch giáo viên đã điểm danh
        /// False: Lấy tất cả lịch của giáo viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn lấy thông tin lịch học")]
        public bool IsOnlyAtendence { get; set; }
    }

    public class GetZoomSearch : SearchOptions
    {
        public int? BranchId { get; set; }
        public int? ClassId { get; set; }
        public int? TeacherId { get; set; }
    }
    public class PopupConfigSearch : SearchOptions
    {
        public int? BranchId { get; set; }
    }
    public class StatisticalStudentInClassSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int ClassId { get; set; }
    }

    public class StatisticalHomeworkKeywordsSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int ClassId { get; set; }
    }

    public class TeacherAvailableScheduleSearch
    {
        /// <summary>
        /// Từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// Đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// List ID giáo viên
        /// Mẫu 1,2
        /// </summary>
        public string TeacherIds { get; set; }
        /// <summary>
        /// Id chi nhánh
        /// </summary>
        public int BranchId { get; set; }
    }

    public class DetailTeacherAvailableScheduleSearch : SearchOptions
    {
        /// <summary>
        /// Ngày trong tuần
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Id của ca học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn ca học")]
        public int StudyTimeId { get; set; }
        /// <summary>
        /// Id của chi nhánh
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int BranchId { get; set; }
        /// <summary>
        /// List ID giáo viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn giáo viên")]
        public string TeacherIds { get; set; }
    }

    public class RoomAvailableScheduleSearch
    {
        /// <summary>
        /// Từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// Đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// List ID phòng học
        /// Mẫu 1,2
        /// </summary>
        public string RoomIds { get; set; }
        /// <summary>
        /// Id chi nhánh
        /// </summary>
        public int BranchId { get; set; }
    }

    public class DetailRoomAvailableScheduleSearch : SearchOptions
    {
        /// <summary>
        /// Ngày trong tuần
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Id của ca học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn ca học")]
        public int StudyTimeId { get; set; }
        /// <summary>
        /// Id của chi nhánh
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int BranchId { get; set; }
        /// <summary>
        /// List ID giáo viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn phòng học")]
        public string RoomIds { get; set; }
    }

    public class GetStudentDeviceLimitSearch : SearchOptions
    {
        /// <summary>
        /// Mẫu 1,2,3...
        /// </summary>
        public string BranchIds { get; set; }
        public int? StudentId { get; set; }
    }
    public class HomeworkSearchInCurriculum : SearchOptions
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int? CurriculumId { get; set; }
    }
    public class HomeworkFileInCurriculumSearch
    {
        [Required(ErrorMessage = "Vui lòng chọn bài tập")]
        public int HomeworkInCurriculumId { get; set; }
        [Required(ErrorMessage = "Vui lòng loại bài tập")]
        public HomeworkFileType Type { get; set; }
    }

    public class HomeworkSequenceConfigInClassSearch
    {
        [Required(ErrorMessage = "Vui lòng điền thông tin lớp")]
        public int? ClassId { get; set; }
    }

    public class HomeworkSequenceConfigInCurriculumSearch
    {
        [Required(ErrorMessage = "Vui lòng điền thông giáo trình")]
        public int? CurriculumId { get; set; }
    }
}
