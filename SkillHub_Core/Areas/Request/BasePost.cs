using LMSCore.Models;
using LMSCore.Areas.Request;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.TranscriptService;
using System.ComponentModel.DataAnnotations.Schema;
using LMSCore.DTO.HomeworkFileDTO;
using LMSCore.Enum;
using LMSCore.DTO.StaffDTO;
using LMSCore.DTO.StudentDTO;

namespace LMSCore.Areas.Request
{
    public class BasePost
    {
        [JsonIgnore]
        public bool Enable { get; set; } = true;
        [JsonIgnore]
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        [JsonIgnore]
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
    }
    public class StudentDetailPost : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin người dùng")]
        public StudentInformationPost Information { get; set; }
        public StudentParentsPost Parents { get; set; }
        public StudentAddressPost Address { get; set; }
        public StudentAccount Account { get; set; }
        public StudentDetailPost()
        {
            Information = new StudentInformationPost();
            Parents = new StudentParentsPost();
            Address = new StudentAddressPost();
            Account = new StudentAccount();
        }
    }

    public class StudentInformationPost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên học viên")]
        public string FullName { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? DOB { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Trung tâm - Học viên chỉ thuộc 1 chi nhánh duy nhất
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int? BranchId { get; set; }
        /// <summary>
        /// 1 - Chờ kiểm tra
        /// 2 - Đã kiểm tra
        /// 3 - Không học
        /// 4 - Chờ xếp lớp
        /// 5 - Đang học
        /// 6 - Học xong
        /// </summary>
        [JsonIgnore]
        public int LearningStatus { get { return 4; } }
        [JsonIgnore]
        public string LearningStatusName { get { return tbl_UserInformation.GetLearningStatusName(LearningStatus); } }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int? SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public string Extension { get; set; }
        /// <summary>
        /// nghề nghiệp
        /// </summary>
        public int? JobId { get; set; }
    }
    public class StudentParentsPost
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// Giới thiệu thêm
        /// </summary>
        public string Extension { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
    }
    public class StudentAddressPost
    {
        public string Address { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
    }
    public class StudentAccount
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class CheckWritingRequestPost : BasePost
    {
        /// <summary>
        /// lịch sử chấm bài
        /// </summary>
        [Required(ErrorMessage ="Vui lòng chọn thông tin lịch sử chấm bài")]
        public int HistoryCheckWritingId { get; set; }
        /// <summary>
        /// tiêu chí đánh giá
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn tiêu chí đánh giá")]
        public int BandDescriptorId { get; set; }
    }
    public class HistoryCheckWritingPost : BasePost
    {
        /// <summary>
        /// nội dung đề kiểm tra
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        public string Question { get; set; }
        /// <summary>
        /// nội dung trả lời của học viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập nội dung câu trả lời")]
        public string Answer { get; set; }
        /// <summary>
        /// 1 - IELTS Writing task 1
        /// 2 - IELTS Writing task 2
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn task")]
        public int TaskOrder { get; set; }
    }
    
    public class StaffDetailPost : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin người dùng")]
        public StaffInformationPost Information { get; set; }
        public StaffAddressPost Address { get; set; }
        public StaffBankPost Bank { get; set; }
        public StaffAccountPost Account { get; set; }
        public AssginToProgramPost AssginToProgram { get; set; }
        public StaffDetailPost()
        {
            Information = new StaffInformationPost();
            Address = new StaffAddressPost();
            Bank = new StaffBankPost();
            Account = new StaffAccountPost();
            AssginToProgram = new AssginToProgramPost();
        }
    }
    public class StaffInformationPost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string FullName { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? DOB { get; set; }
        /// <summary>
        /// Giới tính
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập chức vụ")]
        public int? RoleId { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Giới thiệu thêm
        /// </summary>
        public string Extension { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public string BranchIds { get; set; }
    }
    public class StaffAddressPost
    {
        public string Address { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
    }
    public class StaffBankPost
    {
        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        public string BankAccountNumber { get; set; }
        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        public string BankAccountName { get; set; }
        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// Tên chi nhánh
        /// </summary>
        public string BankBranch { get; set; }
    }
    public class StaffAccountPost
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class AssginToProgramPost
    {
        public List<AssginToProgramItem> Items { get; set; }
        public AssginToProgramPost()
        {
            Items = new List<AssginToProgramItem>();
        }
    }
    public class AssginToProgramItem
    {
        public int ProgramId { get; set; }
        public double TeachingFee { get; set; }
    }
    public class ClassTranscriptDetailPost : BasePost
    {
        /// <summary>
        /// Id bảng điểm lớp
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn bảng điểm")]
        public int ClassTranscriptId { get; set; }
        /// <summary>
        /// Tên cột điểm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tên cột")]
        public string Name { get; set; }
        /// <summary>
        /// Loại cột
        /// SampleTranscriptDetailEnum Type
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại cột")]
        public SampleTranscriptDetailEnum.Type Type { get; set; }
        /// <summary>
        /// Giá trị tối đa
        /// </summary>
        public string MaxValue { get; set; }
    }
    public class ClassTranscriptPost : BasePost
    {
        /// <summary>
        /// Lớp
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn bảng điểm")]
        public int ClassId { get; set; }
        /// <summary>
        /// Dùng bảng điểm mẫu
        /// </summary>
        public int? SampleTranscriptId { get; set; }
        /// <summary>
        /// Tên đợt thi
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ngày thi
        /// </summary>
        public DateTime? Date { get; set; }
    }

    public class SaveGradesInClassPost : BasePost
    {
        /// <summary>
        /// bảng điểm của lớp
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn bảng điểm")]
        public int ClassTranscriptId { get; set; }
        /// <summary>
        /// học viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int StudentId { get; set; }
        /// <summary>
        /// danh sách điểm
        /// </summary>
        public List<SaveGradesInClassDetail> Details { get; set; }
    }
    public class SaveGradesInClassDetail : BasePost
    {
        /// <summary>
        /// cột điểm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn cột điểm")]
        public int ClassTranscriptDetailId { get; set; }
        /// <summary>
        /// giá trị của cột điểm
        /// </summary>
        public string Value { get; set; }
    }
    public class SampleTranscriptDetailPost : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn bảng điểm")]
        public int SampleTranscriptId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên cột")]
        public string Name { get; set; }
        /// <summary>
        /// SampleTranscriptDetailEnum Type
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại cột")]
        public SampleTranscriptDetailEnum.Type Type { get; set; }
    }
    public class SampleTranscriptPost : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bảng điểm")]
        public string Name { get; set; }
    }
    public class ScheduleRecordCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bản ghi")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lịch học")]
        public int ScheduleId { get; set; }
        [Required(ErrorMessage = "Vui lòng upload bản ghi")]
        public string UrlLink { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn nguồn")]
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return tbl_ScheduleRecord.GetTypeName(Type);
            }
        }
    }
    public class AttendancesForm
    {
        public int ScheduleId { get; set; }
        public int Status { get; set; }
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                return tbl_RollUp.GetStatusName(Status);
            }
        }
    }
    public class AttendanceForm
    {
        public int ScheduleId { get; set; }
        public int StudentId { get; set; }
        public int? Status { get; set; }
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                return tbl_RollUp.GetStatusName(Status ?? 0);
            }
        }
        public string Note { get; set; }

    }
    public class GenerateExamCreate : BasePost
    {
        public GenIeltsExamCreate Exam { get; set; }
    }
    public class GenIeltsExamCreate : BasePost
    {
        [StringLength(100, ErrorMessage = "Tên đề thi không vượt quá 100 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập tên đề")]
        public string Name { get; set; }
        [StringLength(100, ErrorMessage = "Mã đề thi không vượt quá 100 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập mã đề")]
        public string Code { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public string Thumbnail { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        [JsonIgnore]
        public bool Active { get { return false; } }
        public List<GenIeltsSkillCreate> IeltsSkill { get; set; }
    }
    public class GenIeltsSkillCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn đề")]
        public int? IeltsExamId { get; set; }
        [StringLength(100, ErrorMessage = "Vui lòng nhập tên kỹ năng")]
        public string Name { get; set; }
        /// <summary>
        /// Tổng thời gian làm của kỹ năng phải bằng thời gian làm đề
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        public List<GenIeltsSectionCreate> IeltsSection { get; set; }
    }
    public class GenIeltsSectionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn kỹ năng")]
        public int? IeltsSkillId { get; set; }
        [StringLength(100, ErrorMessage = "Tên không được phép dài quá 100 ký tự")]
        public string Name { get; set; }
        /// <summary>
        /// Giải thích bài làm (hiện khi học viên hoàn thành bài thi)
        /// </summary>
        public string Explain { get; set; }
        /// <summary>
        /// Đoạn văn
        /// </summary>
        public string ReadingPassage { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        public List<GenQuestionModel> Questions { get; set; }
    }
    public class GenerateQuestionCreate
    {
        [Required]
        public int IeltsSectionId { get; set; }
        public List<GenQuestionModel> Questions { get; set; }
    }
    public class GenQuestionModel
    {
        public int? Type { get; set; }
        public int? Level { get; set; }
        public int? Number { get; set; }
    }
    public class RegisterTuitionPackageModel : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn gói học phí")]
        public int? TuitionPackageId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int? PaymentMethodId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public string Note { get; set; }
        public int Type { get { return 5; } }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return "Học phí hằng tháng";
            }
        }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double Paid { get; set; }
        /// <summary>
        /// Thanh toán bằng số tiền bảo lưu
        /// </summary>
        public int ClassReserveId { get; set; }
        /// <summary>
        /// Lớp đăng ký
        /// </summary>
        public int? ClassId { get; set; }
        /// <summary>
        /// Ngày hoàn thành
        /// </summary>
        [JsonIgnore]
        public DateTime? CompleteDate { get { return DateTime.Now; } }
        /// <summary>
        /// khi đăng ký học cho học viên nếu bật field này = true thì tự động tạo hợp đồng cho học viên
        /// </summary>
        public bool? IsCreateContract { get; set; } = false;
        /// <summary>
        /// nội dung hợp đồng
        /// </summary>
        public string Content { get; set; }
        public DateTime? PaymentDate { get; set; }

    }
    public class TuitionPackageCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập mã gói")]
        public string Code { get; set; }
        /// <summary>
        /// Số tháng
        /// </summary>
        public int Months { get; set; }
        /// <summary>
        /// 1 - Giảm theo số tiền
        /// 2 - Giảm theo phần trăm 
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại khuyến mãi")]
        public int? DiscountType { get; set; }
        [JsonIgnore]
        public string DiscountTypeName
        {
            get
            {
                return DiscountType == 1 ? "Giảm theo số tiền" : DiscountType == 2 ? "Giảm theo phần trăm" : "";
            }
        }
        public double Discount { get; set; }
        /// <summary>
        /// Giảm tối đa
        /// </summary>
        public double? MaxDiscount { get; set; }
        public string Description { get; set; }
    }
    public class HomeworkCreate : BasePost
    {
        /// <summary>
        /// id lớp học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int? ClassId { get; set; }
        /// <summary>
        /// tên btvn
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// id bộ đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        /// <summary>
        /// thời gian bắt đầu làm bài
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu làm bài")]
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// thời gian kết thúc làm bài
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc làm bài")]
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Giáo viên chấm bài
        /// </summary>
        public int? TeacherId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn loại bài tập")]
        public HomeworkType? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return HomeworkTypeName(Type ?? HomeworkType.Exam);
            }
        }
        /// <summary>
        /// Điểm sàn
        /// </summary>
        public double? CutoffScore { get; set; }
        /// <summary>
        /// Số buổi
        /// </summary>
        public int? SessionNumber { get; set; }
        /// <summary>
        /// Vị trí của bài tập
        /// </summary>
        public int? Index { get; set; }
        public string HomeworkContent { get; set; }
        public List<HomeworkFileCreate> AddFiles { get; set; } = new List<HomeworkFileCreate>();
    }
    public class HomeworkResultCreate : BasePost
    {
        /// <summary>
        /// id lớp học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int? ClassId { get; set; }
        /// <summary>
        /// id bài tập
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn bài tập")]
        public int? HomeworkId { get; set; }
        public string Content { get; set; }
        public List<HomeworkFileCreate> AddFiles { get; set; } = new List<HomeworkFileCreate>();
    }
    public class IeltsExamResultCreate : BasePost
    {
        public int DoingTestId { get; set; }
    }
    public class IeltsSkillResultCreate : BasePost
    {
        public int DoingTestId { get; set; }
        public string TimeSpentOfSkill { get; set; }
        public int IeltsExamResultId { get; set; }
        public int IeltsSkillId { get; set; }
    }
    public class IeltsSectionResultCreate : BasePost
    {
        public int DoingTestId { get; set; }
        public int IeltsSkillResultId { get; set; }
        public int IeltsSectionId { get; set; }
    }
    public class IeltsQuestionGroupResultCreate : BasePost
    {
        public int DoingTestId { get; set; }
        public int IeltsSectionResultId { get; set; }
        public int IeltsQuestionGroupId { get; set; }
    }
    public class IeltsQuestionResultCreate : BasePost
    {
        public int DoingTestId { get; set; }
        public int IeltsQuestionGroupResultId { get; set; }
        public int IeltsExamId { get; set; }
        public int IeltsSkillId { get; set; }
        public int IeltsSectionId { get; set; }
        public int IeltsQuestionGroupId { get; set; }
        public int IeltsQuestionId { get; set; }
    }
    public class IeltsAnswerResultCreate : BasePost
    {
        public int IeltsQuestionResultId { get; set; }
        public int IeltsQuestionId { get; set; }
        public int IeltsAnswerId { get; set; }
        public string IeltsAnswerContent { get; set; }
        /// <summary>
        /// true - Đáp án đúng
        /// </summary>
        public bool Correct { get; set; }
        /// <summary>
        /// Áp dụng cho câu Sắp xếp câu , các dạng khác truyền 0
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Lựa chọn của học viên
        /// </summary>
        public bool MyChoice { get; set; }
        /// <summary>
        /// Đáp án học viên viên
        /// </summary>
        public int MyIeltsAnswerId { get; set; }
        /// <summary>
        /// Nội dung làm bài của học viên
        /// </summary>
        public string MyIeltsAnswerContent { get; set; }
        /// <summary>
        /// Đáp án Sắp xếp câu  của học viên
        /// </summary>
        public int MyIndex { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public int Type { get; set; }
    }
    public class DoingTestCurrentSection
    {
        public int DoingTestId { get; set; }
        public int IeltsSectionId { get; set; }
    }
    public class DoingTestCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn đề thi")]
        public int? IeltsExamId { get; set; }
        /// <summary>
        /// Loại 1 thì truyền valueId = 0
        /// </summary>
        public int ValueId { get; set; }
        /// <summary>
        /// 1 - Làm bài thử
        /// 2 - Làm bài hẹn test 
        /// 3 - Bài tập về nhà
        /// 4 - Bộ đề
        /// 5 - Luyện tập
        /// 6 - Bài thi
        /// </summary>
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Làm bài thử"
                    : Type == 2 ? "Làm bài hẹn test"
                    : Type == 3 ? "Bài tập về nhà"
                    : Type == 4 ? "Bộ đề"
                    : Type == 5 ? "Luyện tập"
                    : Type == 6 ? "Làm bài thi" : "";
            }
        }
        /// <summary>
        /// 1 - Đang làm 
        /// 2 - Đã nộp
        /// 3 - Đã hủy
        /// </summary>
        [JsonIgnore]
        public int Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Đang làm"; } }
    }
    public class AddQuestionsGroupToExam
    {
        [Required]
        public int IeltsSectionId { get; set; }
        [Required]
        public List<int> IeltsQuestionGroupIds { get; set; }
    }
    public class IeltsQuestionGroupCreate : BasePost
    {
        public int IeltsSectionId { get; set; }
        [StringLength(100, ErrorMessage = "Tên nhóm câu không được quá 100 ký tự")]
        public string Name { get; set; }
        /// <summary>
        /// Nội dung câu
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
        /// <summary>
        /// 1 - Dễ 
        /// 2 - Trung bình
        /// 3 - Khó
        /// </summary>
        public int Level { get; set; }
        [JsonIgnore]
        public string LevelName
        {
            get
            {
                return Level == 1 ? "Dễ"
                    : Level == 2 ? "Trung bình"
                    : Level == 3 ? "Khó" : "";
            }
        }
        /// <summary>
        /// Từ khóa
        /// </summary>
        public string TagIds { get; set; }
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
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Trắc nghiệm"
                    : Type == 2 ? "Chọn từ vào ô trống"
                    : Type == 3 ? "Điền vào ô trống"
                    : Type == 4 ? "Mindmap"
                    : Type == 5 ? "True/False/Not Given"
                    : Type == 6 ? "Sắp xếp câu "
                    : Type == 7 ? "Viết"
                    : Type == 8 ? "Nói" : "";
            }
        }
        /// <summary>
        /// Câu gốc
        /// </summary>
        [JsonIgnore]
        public int SourceId { get; set; }
        /// <summary>
        /// Tạo thủ công => tạo thêm 1 câu tương tự vào ngân hàng câu hỏi
        /// </summary>
        [JsonIgnore]
        public bool IsHandmade { get; set; } = true;
        public List<IeltsQuestionInsertOrUpdate> IeltsQuestions { get; set; }
        public IeltsQuestionGroupCreate()
        {
            IeltsQuestions = new List<IeltsQuestionInsertOrUpdate>();
        }

    }
    public class IeltsSectionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn kỹ năng")]
        public int? IeltsSkillId { get; set; }
        [StringLength(100, ErrorMessage = "Tên không được phép dài quá 100 ký tự")]
        public string Name { get; set; }
        /// <summary>
        /// Giải thích bài làm (hiện khi học viên hoàn thành bài thi)
        /// </summary>
        public string Explain { get; set; }
        /// <summary>
        /// Đoạn văn
        /// </summary>
        public string ReadingPassage { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
    }
    public class IeltsSkillCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn đề")]
        public int? IeltsExamId { get; set; }
        [StringLength(100, ErrorMessage = "Vui lòng nhập tên kỹ năng")]
        public string Name { get; set; }
        /// <summary>
        /// Tổng thời gian làm của kỹ năng phải bằng thời gian làm đề
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
    }
    public class IeltsExamCreate : BasePost
    {
        [StringLength(100, ErrorMessage = "Tên đề thi không vượt quá 100 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập tên đề")]
        public string Name { get; set; }
        [StringLength(100, ErrorMessage = "Mã đề thi không vượt quá 100 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập mã đề")]
        public string Code { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public string Thumbnail { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        [JsonIgnore]
        public bool Active { get { return false; } }
        [Required(ErrorMessage = "Vui lòng chọn cấp độ")]
        public int LevelExam { get; set; }
        [JsonIgnore]
        public string LevelExamName
        {
            get { return LevelExam == 1 ? "Dễ" : LevelExam == 2 ? "Trung Bình" : LevelExam == 3 ? "Khó" : ""; }
        }
        public int Time { get; set; }
    }
    public class SalaryCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn năm")]
        public int Year { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn tháng")]
        public int Month { get; set; }
        /// <summary>
        /// Lương cơ bản
        /// </summary>
        public double BasicSalary { get; set; }
        /// <summary>
        /// Lương dạy học
        /// </summary>
        public double TeachingSalary { get; set; }
        /// <summary>
        /// Khấu trừ
        /// </summary>
        public double Deduction { get; set; }
        /// <summary>
        /// Thưởng thêm
        /// </summary>
        public double Bonus { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        /// <summary>
        /// Tổng lương
        /// </summary>
        [JsonIgnore]
        public double TotalSalary
        {
            get
            {
                return (BasicSalary + TeachingSalary + Bonus) - Deduction;
            }
        }
        /// <summary>
        /// 1 - Chưa chốt
        /// 2 - Đã chốt
        /// 3 - Đã thanh toán
        /// </summary>
        [JsonIgnore]
        public int Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Chưa chốt"; } }
    }
    public class CurriculumDetailInClassCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn tài liệu trong lớp")]
        public int? CurriculumIdInClass { get; set; }
        [JsonIgnore]
        public int CurriculumDetailId { get { return 0; } }
        public string Name { get; set; }
        [JsonIgnore]
        public bool? IsComplete { get { return false; } }
        [JsonIgnore]
        public double? CompletePercent { get { return 0; } }
    }
    public class TutoringFeeCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn giáo viên")]
        public int? TeacherId { get; set; }
        /// <summary>
        /// phí dạy kèm
        /// </summary>
        public double? Fee { get; set; }
        public string Note { get; set; }
    }
    public class TutorSalaryCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn giáo viên")]
        public int? TutorId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn cấu hình lương")]
        public int? TutorSalaryConfigId { get; set; }
        public string Note { get; set; }
    }
    public class StudentAssessmentCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn buổi học")]
        public int ScheduleId { get; set; }
        public string Listening { get; set; }
        public string Speaking { get; set; }
        public string Reading { get; set; }
        public string Writing { get; set; }
        public string Note { get; set; }
    }
    public class CurriculumInClassCreate : BasePost
    {
        public int? CurriculumId { get; set; }
        public int? ClassId { get; set; }
    }
    public class DocumentLibraryCreate : BasePost
    {
        /// <summary>
        /// Id chủ đề
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn chủ đề tài liệu")]
        public int? DirectoryId { get; set; }
        public string Background { get; set; }
        /// <summary>
        /// Link file
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn file")]
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string FileUrlRead { get; set; }
        public string Description { get; set; }
    }
    public class DocumentLibraryDirectoryCreate : BasePost
    {
        /// <summary>
        /// Tên chủ đề
        /// </summary>
        [Required(ErrorMessage = ("Vui lòng nhập tên chủ đề"))]
        public string Name { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
    }
    public class FeedbackReplyCreate : BasePost
    {
        [Required(ErrorMessage = ("Vui lòng chọn phản hồi"))]
        public int? FeedbackId { get; set; }
        /// <summary>
        /// Nội dung
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
    }
    public class FeedbackCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Title { get; set; }
        /// <summary>
        /// Nội dung phản hồi
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
        /// <summary>
        /// Cờ ưu tiên
        /// </summary>
        public bool? IsPriority { get; set; }
        /// <summary>
        /// Cờ ẩn danh
        /// </summary>
        public bool? IsIncognito { get; set; }
    }
    public class ContractCreate : BasePost
    {
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung hợp đồng")]
        public string Content { get; set; }
    }
    public class NewsFeedReplyCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn bình luận")]
        public int? NewsFeedCommentId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
    }
    public class NewsFeedCommentCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn bản tin")]
        public int? NewsFeedId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
    }
    public class NewsFeedLikeCreate : BasePost
    {
        /// <summary>
        /// id bản tin
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn bản tin")]
        public int? NewsFeedId { get; set; }
    }
    public class ScheduleAvailableCreate : BasePost
    {
        public int? TeacherId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime? StartTime { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        public DateTime? EndTime { get; set; }
        public string Note { get; set; }
    }
    public class VideoCourseStudentCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        public int? VideoCourseId { get; set; }
    }
    public class PackageStudentCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn bộ đề")]
        public int? PackageId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
    }
    public class MarkSalaryCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn giáo viên")]
        public int? TeacherId { get; set; }
        /// <summary>
        /// Lương chấm bài
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập lương chấm bài")]
        public double? Salary { get; set; }
    }
    public class PaymentAllowCreates
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public List<int> UserIds { get; set; }
    }
    public class PackageSkillCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn phần")]
        public int PackageSectionId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên kỹ năng")]
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// đề thi
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn đề thi")]
        public int IeltsExamId { get; set; }
    }
    public class PackageSectionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn bộ đề")]
        public int PackageId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Số lượng đề
        /// </summary>
        [JsonIgnore]
        public int ExamQuatity { get { return 0; } }
    }


    public class NewsFeedCreate : BasePost
    {
        /// <summary>
        /// Nội dung bản tin
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Màu bản tin
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// Nền bản tin
        /// </summary>
        public string BackGroundUrl { get; set; }
        /// <summary>
        /// Nhóm
        /// </summary>
        public int? NewsFeedGroupId { get; set; }
        /// <summary>
        /// Danh sách chi nhánh
        /// </summary>
        public List<int> ListBranchId { get; set; }

        /// <summary>
        /// File => tbl_NewsFeedFile
        /// </summary>
        public List<NewsFeedFile> FileListCreate { get; set; }
    }

    public class NewsFeedFile
    {
        /// <summary>
        /// Đường dẫn file
        /// </summary>
        public string FileUrl { get; set; }
    }

    public class UserInNFGroupCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn nhóm")]
        public int? NewsFeedGroupId { get; set; }
        public List<Members> Members { get; set; }
    }

    public class Members
    {
        /// <summary>
        /// id user
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn người dùng")]
        public int? UserId { get; set; }

        /// <summary>
        /// Kiểu role
        /// 1 - Quản trị viên
        /// 2 - Thàn viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn chức vụ")]
        public int? Type { get; set; }

        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Quản trị viên" : Type == 2 ? "Thành viên" : "";
            }
        }
    }




    public class NewsFeedGroupCreate : BasePost
    {
        /// <summary>
        /// Tên nhóm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tên nhóm")]
        public string Name { get; set; }
        /// <summary>
        /// Hình nền
        /// </summary>
        public string BackGround { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
    }
    public class CartCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public int Quantity { get; set; }
    }
    public class TagCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int? TagCategoryId { get; set; }
    }
    public class ReasonOutCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class TagCategoryCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        /// <summary>
        /// 1 - Khóa Video
        /// 2 - Câu hỏi
        /// 3 - Bộ đề
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập loại")]
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Khóa video"
                    : Type == 2 ? "Câu hỏi"
                    : Type == 3 ? "Bộ đề" : "";
            }
        }
    }
    public class SalaryConfigCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lương")]
        public double? Value { get; set; }
        public string Note { get; set; }
    }
    public class RefundCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int BranchId { get; set; }
        public double Price { get; set; } = 0;
        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
        public int? PaymentMethodId { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        /// <summary>
        /// 1 - Hoàn tiền thủ công
        /// 2 - Hoàn tiền bảo lưu
        /// 3 - Hoàn tiền chờ xếp lớp
        /// 4 - Hoàn tiền thanh toán dư
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại hoàn tiền")]
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Hoàn tiền thủ công"
                        : Type == 2 ? "Hoàn tiền bảo lưu"
                        : Type == 3 ? "Hoàn tiền chờ xếp lớp"
                        : Type == 4 ? "Hoàn tiền thanh toán dư" : "";
            }
        }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Đã duyệt
        /// 3 - Hủy
        /// </summary>
        [JsonIgnore]
        public int? Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Chờ duyệt"; } }
        /// <summary>
        /// truyền khi type 1
        /// </summary>
        public int StudentId { get; set; } = 0;
        /// <summary>
        /// truyền khi type 3
        /// </summary>
        public int ClassRegistrationId { get; set; } = 0;
        /// <summary>
        /// truyền khi type 2
        /// </summary>
        public int ClassReserveId { get; set; } = 0;
        /// <summary>
        /// truyền khi type 4
        /// </summary>
        public int BillId { get; set; } = 0;
    }

    public class ClassReserveCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên trong lớp")]
        public int? StudentInClassId { get; set; }
        /// <summary>
        /// Ngày hết hạn bảo lưu
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn hạn bảo lưu")]
        public DateTime? Expires { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Đang bảo lưu
        /// 2 - Đã học lại
        /// 3 - Đã hoàn tiền
        /// 4 - Hết hạn bảo lưu
        /// </summary>
        [JsonIgnore]
        public int? Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Đang bảo lưu"; } }
        [Currency]
        public double Price { get; set; }
    }
    public class ClassChangeCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên trong lớp")]
        public int? StudentInClassId { get; set; }
        public int BranchId { get; set; }
        public int? PaymentMethodId { get; set; }
        public DateTime? PaymentAppointmentDate { get; set; }
        /// <summary>
        /// Lớp mới
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn lớp mới")]
        public int? NewClassId { get; set; }
        public string Note { get; set; }
        [Currency]
        public double Paid { get; set; }
        [Currency]
        public double Price { get; set; }
    }
    public class NotificationInClassCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int? ClassId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsSendMail { get; set; }
    }
    public class TimeLineCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int? ClassId { get; set; }
        public string Note { get; set; }
    }
    public class RollUpCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn buổi học")]
        public int? ScheduleId { get; set; }
        /// <summary>
        ///    co = 1,
        ///    vang_co_phep = 2,
        ///    vang_khong_phep = 3,
        ///    di_muon = 4,
        ///    ve_som = 5,
        ///    nghi_le = 6
        /// </summary>
        public int? Status { get; set; }
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                string result = "";
                switch (Status)
                {
                    case 1: result = "Có mặt"; break;
                    case 2: result = "Vắng có phép"; break;
                    case 3: result = "Vắng không phép"; break;
                    case 4: result = "Đi muộn"; break;
                    case 5: result = "Về sớm"; break;
                    case 6: result = "Nghỉ lễ"; break;
                }
                return result;
            }
        }
        /// <summary>
        ///    gioi = 1,
        ///    kha = 2,
        ///    trung_binh = 3,
        ///    kem = 4,
        ///    theo_doi_dac_biet = 5,
        ///    co_co_gang = 6,
        ///    khong_co_gang = 7,
        ///    khong_nhan_xet = 8
        /// </summary>
        public int? LearningStatus { get; set; }
        [JsonIgnore]
        public string LearningStatusName
        {
            get
            {
                string result = "";
                switch (LearningStatus)
                {
                    case 1: result = "Giỏi"; break;
                    case 2: result = "Khá"; break;
                    case 3: result = "Trung bình"; break;
                    case 4: result = "Kém"; break;
                    case 5: result = "Theo dỏi đặc biệt"; break;
                    case 6: result = "Có cố gắng"; break;
                    case 7: result = "Không cố gắng"; break;
                    case 8: result = "Không nhận xét"; break;
                }
                return result;
            }
        }
        public string Note { get; set; }
    }
    public class StudentInClassCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [JsonIgnore]
        public bool? Warning { get { return false; } }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Chính thức
        /// 2 - Học thử
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public int? Type { get; set; }
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Chính thức"
                        : Type == 2 ? "Học thử"
                        : null;
            }
        }
    }

    public class StudentInClassAppend : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
        public List<int> StudentIds { get; set; }
        [JsonIgnore]
        public bool? Warning { get { return false; } }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Chính thức
        /// 2 - Học thử
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public int? Type { get; set; }
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Chính thức"
                        : Type == 2 ? "Học thử"
                        : null;
            }
        }
    }

    public class PaymentSessionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        public double Value { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int? PaymentMethodId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lý do tạo phiếu")]
        [StringLength(1000)]
        public string Reason { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        /// <summary>
        /// 1 - Thu
        /// 2 - Chi
        /// </summary>
        public int? Type { get; set; }
        [StringLength(20)]
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Thu" : Type == 2 ? "Chi" : "";
            }
        }
    }

    public class PaymentSessionCreateV2 : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        public double Value { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int? PaymentMethodId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lý do tạo phiếu")]
        [StringLength(1000)]
        public string Reason { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        /// <summary>
        /// 1 - Thu
        /// 2 - Chi
        /// </summary>
        public int? Type { get; set; }
        [StringLength(20)]
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Thu" : Type == 2 ? "Chi" : "";
            }
        }
        [Required(ErrorMessage = "Vui lòng chọn ngày thanh toán")]
        public DateTime? PaymentDate { get; set; }
        public int? SpendingConfigId { get; set; }
    }

    public class BillCreateV3 : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        public int? DiscountId { get; set; }
        public int? PaymentMethodId { get; set; }
        /// <summary>
        /// Ngày hẹn thanh toán
        /// </summary>
        public DateTime? PaymentAppointmentDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Đăng ký học 
        /// 2 - Mua dịch vụ
        /// 3 - Đăng ký lớp dạy kèm
        /// 4 - Tạo thủ công
        /// 5 - Học phí hằng tháng
        /// 6 - Phí chuyển lớp
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Đăng ký học"
                        : Type == 2 ? "Mua dịch vụ"
                        : Type == 3 ? "Đăng ký lớp dạy kèm"
                        : Type == 4 ? "Tạo thủ công"
                        : Type == 5 ? "Học phí hằng tháng"
                        : Type == 6 ? "Phí chuyển lớp" : "";
            }
        }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double Paid { get; set; }
        /// <summary>
        /// Giá - truyền vào khi tạo thanh toán thủ công
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Thanh toán bằng số tiền bảo lưu
        /// </summary>
        public int? ClassReserveId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày thanh toán")]
        public DateTime? PaymentDate { get; set; }
        public List<BillDetailCreateV2> Details { get; set; }
    }

    public class BillCreateV2 : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        public int? DiscountId { get; set; }
        public int? PaymentMethodId { get; set; }
        /// <summary>
        /// Ngày hẹn thanh toán
        /// </summary>
        public DateTime? PaymentAppointmentDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Đăng ký học 
        /// 2 - Mua dịch vụ
        /// 3 - Đăng ký lớp dạy kèm
        /// 4 - Tạo thủ công
        /// 5 - Học phí hằng tháng
        /// 6 - Phí chuyển lớp
        /// 7 - Mua gói combo
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return BillTypeName(Type);
            }
        }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double Paid { get; set; }
        /// <summary>
        /// Giá - truyền vào khi tạo thanh toán thủ công
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Thanh toán bằng số tiền bảo lưu
        /// </summary>
        public int? ClassReserveId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public List<BillDetailCreateV2> Details { get; set; }
        /// <summary>
        /// khi đăng ký học cho học viên nếu bật field này = true thì tự động tạo hợp đồng cho học viên
        /// </summary>
        public bool? IsCreateContract { get; set; } = false;
        /// <summary>
        /// nội dung hợp đồng
        /// </summary>
        public string Content { get; set; }
    }
    public class BillByComboCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        public int? DiscountId { get; set; }
        public int? PaymentMethodId { get; set; }
        /// <summary>
        /// Ngày hẹn thanh toán
        /// </summary>
        public DateTime? PaymentAppointmentDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 7 - Mua gói combo
        /// </summary>
        [JsonIgnore]
        public int Type { get; set; } = (int)BillType.BuyComboPack;
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return BillTypeName(Type);
            }
        }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double Paid { get; set; }
        /// <summary>
        /// Giá - truyền vào khi tạo thanh toán thủ công
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Thanh toán bằng số tiền bảo lưu
        /// </summary>
        public int? ClassReserveId { get; set; }
        public DateTime? PaymentDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn gói combo")]
        public int ComboId { get; set; }
        /// <summary>
        /// khi đăng ký học cho học viên nếu bật field này = true thì tự động tạo hợp đồng cho học viên
        /// </summary>
        public bool? IsCreateContract { get; set; } = false;
        /// <summary>
        /// nội dung hợp đồng
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// THông tin chờ xếp lớp
        /// </summary>
        public List<ComnboPrograms> Programs { get; set; }
    }
    public class ComnboPrograms
    {
        public int Id { get; set; }
        public List<StudentExpectationCreate> Expectations { get; set; } = new List<StudentExpectationCreate>();
    }
    public class BillCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        public int? DiscountId { get; set; }
        public int? PaymentMethodId { get; set; }
        /// <summary>
        /// Ngày hẹn thanh toán
        /// </summary>
        public DateTime? PaymentAppointmentDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Đăng ký học 
        /// 2 - Mua dịch vụ
        /// 3 - Đăng ký lớp dạy kèm
        /// 4 - Tạo thủ công
        /// 5 - Học phí hằng tháng
        /// 6 - Phí chuyển lớp
        /// 7 - Phí chuyển lớp khi bảo lưu
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return BillTypeName(Type);
            }
        }
        /// <summary>
        /// Đã trả
        /// </summary>
        public double Paid { get; set; }
        /// <summary>
        /// Giá - truyền vào khi tạo thanh toán thủ công
        /// </summary>
        public double Price { get; set; }
        public double? Reduce { get; set; }
        public List<BillDetailCreate> Details { get; set; }

        public MonthlyDebtCreate Classes { get; set; }

    }
    public class MonthlyDebtCreate
    {
        public int? ClassId { get; set; }
        public int Quantity { get; set; }
    }

    public class MonthlyBillCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [JsonIgnore]
        public int Type = 5;
        [JsonIgnore]
        public string TypeName = "Học phí hằng tháng";
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int? PaymentMethodId { get; set; }
        /// <summary>
        /// Ngày hẹn thanh toán
        /// </summary>
        public DateTime? PaymentAppointmentDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Số tháng thanh toán. Mặc định 1
        /// </summary>
        public double Quantity { get; set; } = 1;
        public double Paid { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn tháng cần thanh toán")]
        public int? Month { get; set; }
    }

    public class ChangeClassBillCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [JsonIgnore]
        public int Type = 6;
        [JsonIgnore]
        public string TypeName = "Phí chuyển lớp";
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int? PaymentMethodId { get; set; }
        /// <summary>
        /// Ngày hẹn thanh toán
        /// </summary>
        public DateTime? PaymentAppointmentDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
        public string Note { get; set; }
        public double Paid { get; set; }
    }
    public class BillDetailCreateV2 : BasePost
    {
        /// <summary>
        /// Lớp
        /// </summary>
        public int? ClassId { get; set; }
        /// <summary>
        /// Chương trình học
        /// </summary>
        public int? ProgramId { get; set; }
        /// <summary>
        /// Giáo trình - chọn đối với lớp dạy kèm
        /// </summary>
        public int? CurriculumId { get; set; }
        /// <summary>
        /// Giỏ hàng
        /// </summary>
        public int? CartId { get; set; }
        public int Quantity { get; set; } = 1;
        [JsonIgnore]
        public int ClassChangeId { get; set; }
        /// <summary>
        /// Khảo sát nhu cầu ngày học, ca học mong muốn của học viên
        /// </summary>
        public List<StudentExpectationCreate> Expectations { get; set; }
    }
    public class BillDetailCreate : BasePost
    {
        /// <summary>
        /// Lớp
        /// </summary>
        public int? ClassId { get; set; }
        /// <summary>
        /// Chương trình học
        /// </summary>
        public int? ProgramId { get; set; }
        /// <summary>
        /// Giáo trình - chọn đối với lớp dạy kèm
        /// </summary>
        public int? CurriculumId { get; set; }
        /// <summary>
        /// Giỏ hàng
        /// </summary>
        public int? CartId { get; set; }
        /// <summary>
        /// Số lượng tháng trả trước với lớp theo tháng
        /// </summary>
        public int NumberOfMonths { get; set; } = 1;
        public int Quantity { get; set; } = 1;
        /// <summary>
        /// Khảo sát nhu cầu ngày học, ca học mong muốn của học viên
        /// </summary>
        public List<StudentExpectationCreate> Expectations { get; set; }
    }
    public class ScheduleCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
        public int RoomId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime? StartTime { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        public DateTime? EndTime { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn giáo viên")]
        public int? TeacherId { get; set; }
        public string Note { get; set; }
        public double? TeachingFee { get; set; } = 0;
    }
    public class GenerateScheduleCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime? StartDay { get; set; }
        /// <summary>
        /// Phòng học
        /// </summary>
        public int RoomId { get; set; } = 0;
        public List<GenerateScheduleModel> TimeModels { get; set; }
    }
    public class GenerateScheduleModel
    {
        /// <summary>
        /// Ngày trong tuần
        /// </summary>
        [Required(ErrorMessage = "Vui lòng thứ")]
        public int DayOfWeek { get; set; }
        /// <summary>
        /// Ca học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn ca học")]
        public int StudyTimeId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn giáo viên")]
        public int? TeacherId { get; set; }
    }
    public class MultipleScheduleCreate
    {
        public List<ScheduleCreate> schedules { get; set; }
    }
    public class TeacherOffCreate : BasePost
    {
        /// <summary>
        /// thời gian bắt đầu
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// thời gian kết thúc
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// lý do nghỉ
        /// </summary>
        [StringLength(1000)]
        [Required(ErrorMessage = "Vui lòng nhập lý do nghỉ")]
        public string Reason { get; set; }
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Duyệt
        /// 3 - Không duyệt
        /// </summary>
        [JsonIgnore]
        public int? Status { get { return 1; } }
        [StringLength(20)]
        [JsonIgnore]
        public string StatusName { get { return "Chờ duyệt"; } }
        /// <summary>
        /// ghi chú
        /// </summary>
        [StringLength(1000)]
        [JsonIgnore]
        public string Note { get { return ""; } }
    }
    public class ClassCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên lớp học")]
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public int? GradeId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn chương trình")]
        public int? ProgramId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn giáo trình")]
        public int? CurriculumId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian dự kiến mở lớp")]
        public DateTime? EstimatedDay { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giá lớp học")]
        public double? Price { get; set; }
        /// <summary>
        /// 1 - Sắp diễn ra
        /// 2 - Đang diễn ra
        /// 3 - Kết thúc
        /// </summary>
        [JsonIgnore]
        public int? Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Sắp diễn ra"; } }
        /// <summary>
        /// Hoc vụ
        /// </summary>
        public int? AcademicId { get; set; }
        //[Required(ErrorMessage = "Vui lòng chọn giáo viên")]
        //public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Offline
        /// 2 - Online
        /// 3 - Dạy kèm
        /// </summary>
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Offline"
                        : Type == 2 ? "Online"
                        : Type == 3 ? "Dạy kèm" : "";
            }
        }
        /// <summary>
        /// Lương trên buổi dạy
        /// </summary>
        public double TeachingFee { get; set; } = 0;
        public double TutorFee { get; set; } = 0;
        /// <summary>
        /// Số lượng tối đa
        /// </summary>
        public int MaxQuantity { get; set; } = 20;
        //public int? CertificateTemplateId { get; set; }
        //chọn mẫu bảng điểm
        //public int ScoreboardTemplateId { get; set; }
        /// <summary>
        /// 1 - Thanh toán một lần
        /// 2 - Thanh toán theo tháng
        /// </summary>
        public int PaymentType { get; set; }
        public List<int> ClassRegistrationIds { get; set; } = new List<int>();
    }

    public class ScheduleCreates
    {
        public int? ClassId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomCode { get; set; }
        public int? TeacherId { get; set; }
        public string TutorIds { get; set; }
        public List<string> TutorNames { get; set; }
        public string TeacherName { get; set; }
        public string TeacherCode { get; set; }
        public string Note { get; set; }
    }


    public class ScheduleCreatesV2
    {
        public int? ClassId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomCode { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string TeacherCode { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - không trùng thông tin
        /// 2 - trùng phòng học
        /// 3 - trùng giáo viên
        /// </summary>
        public string Types { get; set; }
    }
    public class GenerateScheduleResponse
    {
        public int TotalCurriculumLesson { get; set; }
        public int TotalClassSchedule { get; set; }
        public List<ScheduleCreatesV2> Datas { get; set; }
    }

    public class StudyTimeCreate : BasePost
    {
        /// <summary>
        /// định dạng HH:mm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public string StartTime { get; set; }
        /// <summary>
        /// định dạng HH:mm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        public string EndTime { get; set; }
    }
    public class FileInCurriculumDetailCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int? CurriculumDetailId { get; set; }
        [JsonIgnore]
        public string FileName { get; set; }
        [JsonIgnore]
        public string FileUrl { get; set; }
        [JsonIgnore]
        public string FileUrlRead { get; set; }

    }
    public class CurriculumDetailCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn giáo trình")]
        public int? CurriculumId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
    }
    public class CurriculumCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn chương trình học")]
        public int? ProgramId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên giáo trình")]
        public string Name { get; set; }
        /// <summary>
        /// Số buổi
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số buổi học")]
        public int Lesson { get; set; } = 0;
        /// <summary>
        /// Thời gian
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập thời gian học")]
        public int Time { get; set; } = 0;
    }
    public class ProgramCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn chuyên môn")]
        public int? GradeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
    }
    public class GradeCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập mã")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
    }
    public class StudentNoteCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn sinh viên")]
        public int? StudentId { get; set; }
        public string Note { get; set; }
    }
    public class StaffNoteCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        public int? StaffId { get; set; }
        public string Note { get; set; }
    }
    public class TestAppointmentCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        public DateTime? Time { get; set; }
        /// <summary>
        /// 1 - Chưa kiểm tra
        /// 2 - Đã kiểm tra 
        /// </summary>
        [JsonIgnore]
        public int? Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Chưa kiểm tra"; } }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Tại trung tâm
        /// 2 - Làm bài trực tuyến
        /// </summary>
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Tại trung tâm"
                    : Type == 2 ? "Làm bài trực tuyến" : "";
            }
        }
        /// <summary>
        /// Đề
        /// </summary>
        public int? IeltsExamId { get; set; }
    }
    public class CustomerCreateByWebsite : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        public string FullName { get; set; }
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// Trung tâm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
    }
    public class CustomerCreate : BasePost
    {
        public int? LearningNeedId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        public string FullName { get; set; }
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int? SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// Trung tâm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        public int? JobId { get; set; }
        /// <summary>
        /// Tên phụ huynh
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// Email phụ huynh
        /// </summary>
        public string ParentEmail { get; set; }
        /// <summary>
        /// Số điện thoại phụ huynh
        /// </summary>
        /// 
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string ParentMobile { get; set; }
        /// <summary>
        /// Điểm đầu vào
        /// </summary>
        public double? EntryPoint { get; set; }
        /// <summary>
        /// Điểm đầu ra mong muốn
        /// </summary>
        public double? DesiredOutputScore { get; set; }
        /// <summary>
        /// Chương trình muốn học
        /// </summary>
        public int? DesiredProgram { get; set; }
    }
    public class CustomerStatusCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        public string ColorCode { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập vị trí của trạng thái")]
        public int? Index { get; set; }
    }
    public class GeneralNotificationCreate : BasePost
    {
        public string Title { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Danh sách người nhận thông báo
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn người nhận")]
        public string UserIds { get; set; }
        public bool IsSendMail { get; set; }
    }
    public class FrequentlyQuestionCreate : BasePost
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string RoleIds { get; set; }
    }
    public class IdiomCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Content { get; set; }
    }
    public class PurposeCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
    }
    public class JobCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
    }
    public class DayOffCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng điền tên ngày nghỉ")]
        public string Name { get; set; }
        public DateTime? sDate { get; set; }
        public DateTime? eDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public string BranchIds { get; set; }
    }
    public class SourceCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
    }
    public class LearningNeedCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
    }
    public class TutorSalaryConfigCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập mã")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        public double? Salary { get; set; }
    }
    public class DiscountCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập mã")]
        public string Code { get; set; }
        /// <summary>
        /// 1 - Giảm tiền 
        /// 2 - Giảm phần trăm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Giảm tiền"
                    : Type == 2 ? "Giảm phần trăm" : "";
            }
        }
        /// <summary>
        /// 1 - Gói lẻ
        /// 2 - Gói combo
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn gói")]
        public int? PackageType { get; set; }
        [JsonIgnore]
        public string PackageTypeName
        {
            get
            {
                return PackageType == 1 ? "Gói lẻ"
                    : PackageType == 2 ? "Gói combo" : "";
            }
        }
        [Required(ErrorMessage = "Vui lòng nhập giá trị khuyến mãi")]
        public double Value { get; set; }
        /// <summary>
        /// 1 - Đang khả dụng
        /// 2 - Đã kết thúc
        /// </summary>
        [JsonIgnore]
        public int? Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Đang diễn ra"; } }
        public string Note { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày hết hạn")]
        public DateTime? Expired { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn số lượng")]
        public int? Quantity { get; set; }
        /// <summary>
        /// Số lượng đã dùng
        /// </summary>
        [JsonIgnore]
        public int? UsedQuantity { get { return 0; } }
        /// <summary>
        /// Khuyến mãi tối đa
        /// </summary>
        public double MaxDiscount { get; set; } = 0;
        /// <summary>
        /// trung tâm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public string BranchIds { get; set; }
    }
    public class RoomCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int? BranchId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
    }
    public class CustomerNoteCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int? CustomerId { get; set; }
        public string Note { get; set; }
    }
    public class BranchCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập mã")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Mobile { get; set; }
        public string Email { get; set; }
    }
    public class HomeworkFileCreate : BasePost
    {
        public string File { get; set; }
        [JsonIgnore]
        public HomeworkFileType Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return HomeworkFileTypeName(Type);
            }
        }
        [JsonIgnore]
        public int HomeworkId { get; set; }
    }
    public class SeminarRecordCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn hội thảo")]
        public int? SeminarId { get; set; }
        public string Name { get; set; }
        public string VideoUrl { get; set; }
    }
    public class NotificationInVideoCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn khoá học")]
        public int? VideoCourseId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
        [JsonIgnore]
        public bool IsSend { get { return false; } }
    }
    public class ExerciseGroupInExamCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn phần trong đề thi")]
        public int? ExamSectionId { get; set; }
        public string Content { get; set; }
        public string Paragraph { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn cấp độ")]
        public ExerciseLevel? Level { get; set; }
        [JsonIgnore]
        public string LevelName
        {
            get { return Level == ExerciseLevel.Easy ? "Dễ" : Level == ExerciseLevel.Normal ? "Trung bình" : Level == ExerciseLevel.Difficult ? "Khó" : ""; }
        }
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public ExerciseType? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == ExerciseType.MultipleChoice ? "Trắc nghiệp"
                    : Type == ExerciseType.DragDrop ? "Chọn từ vào ô trống"
                    : Type == ExerciseType.FillInTheBlank ? "Điền vào ô trống" : "";
                //: Type == ExerciseType.Essay ? "Tự luận" : "";
            }
        }
        public List<ExerciseInExamCreate> ExerciseInExamCreates { get; set; }
    }
    public class ExerciseInExamCreate : BasePost
    {
        public string Content { get; set; }
        /// <summary>
        /// Áp dụng cho câu Chọn từ vào ô trống và điền từ
        /// </summary>
        public string InputId { get; set; }
        public List<AnswerInExamCreate> AnswerInExamCreates { get; set; }
        public string DescribeAnswer { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập điểm")]
        public double? Point { get; set; }
    }
    public class AnswerInExamCreate : BasePost
    {
        public string AnswerContent { get; set; }
        public bool? IsTrue { get; set; }
    }
    public class ExamSectionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn đề")]
        public int? ExamId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Nội dung chú thích đáp án đúng của đoạn văn
        /// </summary>
        public string Explanations { get; set; }
        public string Audio { get; set; }
    }
    public class ExerciseGroupCreate : BasePost
    {

        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phần trong đề thi")]
        public int? ExamSectionId { get; set; }
        public string Content { get; set; }
        public string Paragraph { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn cấp độ")]
        public ExerciseLevel? Level { get; set; }
        [JsonIgnore]
        public string LevelName
        {
            get { return Level == ExerciseLevel.Easy ? "Dễ" : Level == ExerciseLevel.Normal ? "Trung bình" : Level == ExerciseLevel.Difficult ? "Khó" : ""; }
        }
        [Required(ErrorMessage = "Vui lòng chọn loại")]
        public ExerciseType? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == ExerciseType.MultipleChoice ? "Trắc nghiệm"
                    : Type == ExerciseType.DragDrop ? "Chọn từ vào ô trống"
                    : Type == ExerciseType.FillInTheBlank ? "Điền vào ô trống"
                    : Type == ExerciseType.Mindmap ? "Mindmap"
                    : Type == ExerciseType.Sort ? "Sắp xếp câu  câu"
                    : Type == ExerciseType.TrueOrFalse ? "Chọn đúng/sai"
                    : Type == ExerciseType.Write ? "Viết"
                    : Type == ExerciseType.Speak ? "Nói" : "";
            }
        }
        public string Tags { get; set; }
        public List<ExerciseCreate> ExerciseCreates { get; set; }
    }
    public class ExerciseCreate : BasePost
    {
        public string Content { get; set; }
        /// <summary>
        /// Áp dụng cho câu Chọn từ vào ô trống và điền từ
        /// </summary>
        public string InputId { get; set; }
        public List<AnswerCreate> AnswerCreates { get; set; }
        public string DescribeAnswer { get; set; }
        public double? Point { get; set; }
    }
    public class SwapExpectationCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng học viên muốn thay đổi")]
        public List<int> ClassRegistrationIds { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian học")]
        public List<StudentExpectationCreate> Expectations { get; set; }

    }
    public class AnswerCreate : BasePost
    {
        public string AnswerContent { get; set; }
        public bool? IsTrue { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public AnswerType Type { get; set; }
        public int Index { get; set; }
    }

    public class ExamCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đề")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã đề")]
        public string Code { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public int NumberExercise { get { return 0; } }
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập thời gian làm bài")]
        public int Time { get; set; }
        [JsonIgnore]
        public int? DifficultExercise { get { return 0; } }
        [JsonIgnore]
        public int? NormalExercise { get { return 0; } }
        [JsonIgnore]
        public int? EasyExercise { get { return 0; } }
        /// <summary>
        /// Số điểm đạt
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số điểm đạt")]
        public double PassPoint { get; set; }
        public string Audio { get; set; }
    }
    public class ZoomConfigCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập AccountId")]
        public string AccountId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ClientId")]
        public string ClientId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ClientSecret")]
        public string ClientSecret { get; set; }
        [JsonIgnore]
        public bool Active { get { return false; } }
    }
    public class SeminarCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên buổi hội thảo")]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime? StartTime { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        public DateTime? EndTime { get; set; }
        public int VideoCourseId { get; set; } = 0;
        public int LeaderId { get; set; }
        [JsonIgnore]
        public int Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Chưa diễn ra"; } }
        /// <summary>
        /// Số lượng người tham gia, bỏ trống là không giới hạn
        /// </summary>
        public int Member { get; set; } = 0;
        public string Thumbnail { get; set; }
    }
    public class CertificateProgramCreate : BasePost
    {
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? UserId { get; set; }
        public string Content { get; set; }
    }
    public class CertificateCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn khoá học")]
        public int? VideoCourseId { get; set; }
        public string Content { get; set; }
    }
    public class AnswerInVideoCreate : BasePost
    {
        [Required(ErrorMessage = "Không tìm thấy câu hỏi")]
        public int? QuestionInVideoId { get; set; }
        public string Content { get; set; }
    }
    public class QuestionInVideoCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn khoá học")]
        public int? VideoCourseId { get; set; }
        public string Content { get; set; }
    }
    public class FileInVideoCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn bài học")]
        public int? LessonVideoId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }
    public class LessonVideoCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn phần")]
        public int? SectionId { get; set; }
        /// <summary>
        /// 1 - Video
        /// 2 - Bài tập
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại bài học")]
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get { return Type == ((int)LessonType.Video) ? "Video" : Type == ((int)LessonType.BaiTap) ? "Bài tập" : ""; }
        }
        public string Name { get; set; }
        /// <summary>
        /// File video
        /// </summary>
        public string VideoUrl { get; set; }
        /// <summary>
        /// Bài tập
        /// </summary>
        public int? ExamId { get; set; }
        public LessonFileType? FileType { get; set; }
    }
    public class SectionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn khoá học")]
        public int? VideoCourseId { get; set; }
        public string Name { get; set; }
    }
    public class ProductCreate : BasePost
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// Mẫu 1,2,3
        /// </summary>
        public string Tags { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Điều kiện đã học khoá này
        /// </summary>
        public int BeforeCourseId { get; set; } = 0;
        /// <summary>
        /// Giá bán
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        public double? Price { get; set; }
        /// <summary>
        /// 1 - Khóa video
        /// 2 - Bộ đề
        /// 3 - Lượt chấm bài
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        public int? Type { get; set; }
        /// <summary>
        /// Dùng cho lượt chấm
        /// </summary>
        public int MarkQuantity { get; set; }
    }
    public class RegisterModel : BasePost
    {
        [Required(ErrorMessage = "Fullname is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [JsonIgnore]
        public int? StatusId
        {
            get { return ((int)AccountStatus.active); }
        }
        [JsonIgnore]
        public int RoleId { get { return ((int)RoleEnum.student); } }
        [JsonIgnore]
        public string RoleName { get { return "Học viên"; } }
        public string Password { get; set; }
        [JsonIgnore]
        public int LearningStatus { get { return 5; } }
        [JsonIgnore]
        public string LearningStatusName { get { return tbl_UserInformation.GetLearningStatusName(5); } }

        /// <summary>
        /// Guiwr 
        /// </summary>
        public bool SendOPT { get; set; } = false;
    }
    public class UserCreate : BasePost
    {
        [Required(ErrorMessage = "Fullname is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        public DateTime? DOB { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int Gender { get; set; } = ((int)GenderEnum.khac);
        /// <summary>
        /// Số điện thoại
        /// </summary>
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        public string Address { get; set; }
        [JsonIgnore]
        public int? StatusId
        {
            get { return ((int)AccountStatus.active); }
        }
        [Required(ErrorMessage = "Vui lòng chọn quyền")]
        public int RoleId { get; set; }
        [JsonIgnore]
        public string RoleName
        {
            get
            {
                return RoleId == ((int)RoleEnum.admin) ? "Admin"
                  : RoleId == ((int)RoleEnum.teacher) ? "Giáo viên"
                  : RoleId == ((int)RoleEnum.student) ? "Học viên"
                  : RoleId == ((int)RoleEnum.manager) ? "Quản lý"
                  : RoleId == ((int)RoleEnum.sale) ? "Tư vấn viên"
                  : RoleId == ((int)RoleEnum.accountant) ? "Kế toán"
                  : RoleId == ((int)RoleEnum.academic) ? "Học vụ"
                  : RoleId == ((int)RoleEnum.parents) ? "Phụ huynh"
                  : RoleId == ((int)RoleEnum.tutor) ? "Trợ giảng"
                  : "";
            }
        }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập khẩu")]
        public string Password { get; set; }
        /// <summary>
        /// Trung tâm
        /// Mẫu 1,2
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public string BranchIds { get; set; }
        /// <summary>
        /// 1 - Chờ kiểm tra
        /// 2 - Đã kiểm tra
        /// 3 - Không học
        /// 4 - Chờ xếp lớp
        /// 5 - Đang học
        /// 6 - Học xong
        /// </summary>
        [JsonIgnore]
        public int LearningStatus { get { return 5; } }
        [JsonIgnore]
        public string LearningStatusName { get { return "Đang học"; } }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int? SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public int? CustomerId { get; set; }
        public int? ParentId { get; set; }
        /// <summary>
        /// Giới thiệu thêm
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// Chương trình dạy
        /// </summary>
        public string ProgramIds { get; set; }
        public bool isReceiveMailNotification { get; set; } = false;
        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        public string BankAccountNumber { get; set; }
        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        public string BankAccountName { get; set; }
        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// Tên chi nhánh
        /// </summary>
        public string BankBranch { get; set; }
        /// <summary>
        /// Phụ huynh của
        /// </summary>
        public int ParentOff { get; set; }

        public int studentId { get; set; }
    }
    public class StudentRollUpQrCodeCreate : BasePost
    {
        /// <summary>
        /// học viên
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        /// <summary>
        /// buổi học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn buổi học")]
        public int? ScheduleId { get; set; }
    }
    public class StudyRouteCreate : BasePost
    {
        /// <summary>
        /// học viên
        /// </summary>
        [Required(ErrorMessage = "Họp viên không được bỏ trống")]
        public int? StudentId { get; set; }
        /// <summary>
        /// chương trình
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn chương trình")]
        public string ProgramIds { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        public string Note { get; set; }
    }
    public class CertificateTemplateCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên chứng chỉ")]
        public string Name { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Hình nền
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập hình nền")]
        public string Background { get; set; }
        /// <summary>
        /// Mặt sau
        /// </summary>
        public string Backside { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập chiều ngang")]
        public int? Width { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập chiều cao")]
        public int? Height { get; set; }
    }
    public class ComboCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên combo")]
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Chương trình học trong combo
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn chương trình học")]
        public string ProgramIds { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lại giảm giá")]
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get { return lmsEnum.ComboTypeName(Type); }
        }
        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm")]
        public double? Value { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngày kết thúc")]
        public DateTime? EndDate { get; set; }
    }
    public class LearningHistoryCreate : BasePost
    {
        [Required(ErrorMessage = "Học viên không được bỏ trống")]
        public int? StudentId { get; set; }
        [Required(ErrorMessage = "Nội dung không được bỏ trống")]
        public string Content { get; set; }
        public int? ClassId { get; set; }
    }

    public class WarningHistoryCreate : BasePost
    {
        [Required(ErrorMessage = "Học viên không được bỏ trống")]
        public int StudentId { get; set; }
        [Required(ErrorMessage = "Nội dung không được bỏ trống")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Lớp không được bỏ trống")]
        public int ClassId { get; set; }
        [Required(ErrorMessage = "Loại cảnh báo không được bỏ trống")]
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Bị cảnh báo"
                    : Type == 2 ? "Gỡ cảnh báo" : "";
            }
        }
    }

    public class StudentPointRecordCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn năm")]
        public int Year { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn tháng")]
        public int Month { get; set; }
    }
    public class PointRecordForOneStudentCreate : StudentPointRecordCreate
    {
        public int? StudentId { get; set; }
        public int Attend { get; set; }
        public int TotalLessons { get; set; }
        public int Unexcused { get; set; }
        public List<PointModel> Transcript { get; set; }// {StudentId: 197, Reading: '', Listening: '2', Writing: '', Grammar: null, …} theo transcriptId
    }
    public class CommissionConfigCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn tư vấn viên")]
        public int SalesId { get; set; }
        public string Name { get; set; }
        public string Descriptions { get; set; }
        public List<CommissionNormCreate> CommissionNormCreate { get; set; }
    }

    public class CommissionNormCreate : BasePost
    {
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mức bắt đầu hoa hồng")]
        public double MinNorm { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mức kết thúc hoa hồng")]
        public double MaxNorm { get; set; }
        [Required(ErrorMessage = "Phần trăm hoa hồng không được bỏ trống")]
        public double PercentNew { get; set; }
        public double PercentRenewals { get; set; }
    }
    public class CommissionCampaignCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn tư vấn viên")]
        public int SalesId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public double RevenueTargets { get; set; }
    }
    public class CommissionCreate : BasePost
    {
        //public int StudentId { get; set; }
        public int SaleId { get; set; }
        // bảng hoa hồng cho 1 tháng duy nhất
        public int Year { get; set; }
        public int Month { get; set; }
        // mã chiến dịch, không có thì khỏi tính hoa hồng
        public int CampaignId { get; set; }
    }
    public class SaleRevenueCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        public int SaleId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn Bill")]
        public int BillId { get; set; }
        // đợt đó thanh toán bao nhiêu
        [Required(ErrorMessage = "Vui lòng chọn nhập số tiền vừa thanh toán")]
        public double Value { get; set; }
    }
    public class RatingSheetCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int ClassId { get; set; }
        public string Note { get; set; }
    }
    public class RatingQuestionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phiếu đánh giá")]
        public int RatingSheetId { get; set; }
        /// <summary>
        /// 1: Trách nghiệm
        /// 2: Tự luận
        /// </summary>
        public int Type { get; set; } = 1;

        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Trách nghiệm"
                        : Type == 2 ? "Tự luận" : "";
            }
        }
    }
    public class RatingOptionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung lựa chọn")]
        public string Name { get; set; }
        /// <summary>
        /// 0: False
        /// 1: True
        /// </summary>
        public bool TrueOrFalse { get; set; } = false;
        [Required(ErrorMessage = "Vui lòng chọn câu hỏi")]
        public int RatingQuestionId { get; set; }
        public string Essay { get; set; }
    }
    public class StudentRatingFormCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn sinh viên")]
        public int StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phiếu khảo sát")]
        public int RatingSheetId { get; set; }
    }
    public class StudentRatingChoiceCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn bảng khảo sát ")]
        public int StudentRatingFormId { get; set; }
        /// <summary>
        /// FE tự lấy câu hỏi + các (lựa chọn + đúng hoặc sai)
        /// </summary>
        public List<RatingChoice> ListRatingChoice { get; set; }
    }
    public class LessionVideoTestCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn chương")]
        public int SectionId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Name { get; set; }
    }
    public class LessionVideoQuestionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn bài test")]
        public int TestId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        public string Name { get; set; }
        /// <summary>
        /// 1:Dễ
        /// 2:Tb
        /// 3:Khó
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập mức độ")]
        public int Level { get; set; } = 1;
        [JsonIgnore]
        public string LevelName
        {
            get
            {
                return Level == 1 ? "Dễ" :
                       Level == 2 ? "Trung bình" :
                       Level == 3 ? "Khó" : null;
            }
        }
    }
    public class LessionVideoOptionCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn câu hỏi")]
        public int QuestionId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
        public bool TrueFalse { get; set; } = false;
    }
    public class LessionVideoAssignmentCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn sinh viên ")]
        public int StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn bài kiểm tra ")]
        public int TestId { get; set; }
    }

    public class StudentExpectationCreate : BasePost
    {
        /// <summary>
        /// Ngày trong tuần: 1-Sunday, 2-Monday...
        /// </summary>
        public int? ExectedDay { get; set; } = 1;
        /// <summary>
        /// Ca học
        /// </summary>
        public int? StudyTimeId { get; set; }
        public int? ClassExpectationId { get; set; }
        public string Note { get; set; }
    }

    public class StudentMonthlyDebtCreate : BasePost
    {
        public int? StudentId { get; set; }
        public int? ClassId { get; set; }
        public int? BillId { get; set; }
        public double? Price { get; set; }
        public DateTime? Month { get; set; }
        public bool IsPaymentDone { get; set; } = false;
    }

    //sử dụng cho insertUserAndTestAppointment
    public class NewUserCreate : BasePost
    {
        [Required(ErrorMessage = "Fullname is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        public DateTime? DOB { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int Gender { get; set; } = ((int)GenderEnum.khac);
        /// <summary>
        /// Số điện thoại
        /// </summary>
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        public string Address { get; set; }
        [JsonIgnore]
        public int? StatusId
        {
            get { return ((int)AccountStatus.active); }
        }
        [JsonIgnore]
        public int RoleId { get { return 3; } }
        [JsonIgnore]
        public string RoleName
        {
            get
            {
                return "Học viên";
            }
        }
        public string Avatar { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập khẩu")]
        public string Password { get; set; }
        /// <summary>
        /// Trung tâm
        /// Mẫu 1,2
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn trung tâm")]
        public int BranchId { get; set; }
        [JsonIgnore]
        public int LearningStatus { get { return 1; } }
        [JsonIgnore]
        public string LearningStatusName { get { return tbl_UserInformation.GetLearningStatusName(1); } }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int? SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public int? CustomerId { get; set; }
        [JsonIgnore]
        public bool isReceiveMailNotification { get; set; } = false;
        /// <summary>
        /// Tên phụ huynh
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// Email phụ huynh
        /// </summary>
        public string ParentEmail { get; set; }
        /// <summary>
        /// Số điện thoại phụ huynh
        /// </summary>
        /// 
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string ParentMobile { get; set; }
        /// <summary>
        /// Điểm đầu vào
        /// </summary>
        public double? EntryPoint { get; set; }
        /// <summary>
        /// Điểm đầu ra mong muốn
        /// </summary>
        public double? DesiredOutputScore { get; set; }
        /// <summary>
        /// Chương trình muốn học
        /// </summary>
        public int? DesiredProgram { get; set; }
        /// <summary>
        /// Đề thi
        /// </summary>
        public int? IeltsExamId { get; set; }
    }
    public class NewTestAppointmentCreate : BasePost
    {
        public DateTime? Time { get; set; }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Tại trung tâm
        /// 2 - Làm bài trực tuyến
        /// </summary>
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Tại trung tâm"
                    : Type == 2 ? "Làm bài trực tuyến" : "";
            }
        }
    }

    public class UserAndTestAppointmentCreate
    {
        public NewUserCreate UserModel { get; set; }
        public NewTestAppointmentCreate TestAppointmentModel { get; set; }
    }

    public class CurriculumVitaeCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên ứng viên")]
        [RegularExpression(@"^[\p{L}\s]*$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
        public string FullName { get; set; }
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng gửi CV ứng tuyển")]
        public string LinkCV { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn trung tâm muốn ứng tuyển")]
        public int BranchId { get; set; }
        /// <summary>
        /// 1 - Giáo viên Ielts
        /// 2 - Giáo viên Toeic
        /// 3 - Quản lý
        /// 4 - Tư vấn viên
        /// 5 - Kế toán
        /// 6 - Học vụ
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn vị trí ứng tuyển")]
        public int JobPositionId { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// 1 - chờ lịch phỏng vấn
        /// 2 - đã hẹn phỏng vấn
        /// 3 - đã phỏng vấn
        /// </summary>
        [JsonIgnore]
        public int Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "Chờ lịch phỏng vấn"; } }
    }
    public class InterviewAppointmentCreate : BasePost
    {
        //ứng viên
        [Required(ErrorMessage = "Vui lòng chọn hồ sơ ứng viên")]
        public int CurriculumVitaeId { get; set; }
        //người tổ chức
        [Required(ErrorMessage = "Vui lòng chọn người phỏng vấn")]
        public int OrganizerId { get; set; }
        //trung tâm
        [JsonIgnore]
        public int BranchId { get; set; }
        /// <summary>
        /// 1 - Giáo viên Ielts
        /// 2 - Giáo viên Toeic
        /// 3 - Quản lý
        /// 4 - Tư vấn viên
        /// 5 - Kế toán
        /// 6 - Học vụ
        /// </summary>
        [JsonIgnore]
        public int JobPositionId { get; set; }
        //ngày phỏng vấn
        public DateTime InterviewDate { get; set; }

        //nếu sau buổi pv mà pass thì update các thông tin sau
        //ngày ứng viên có thể bắt đầu đi làm
        [JsonIgnore]
        public DateTime? WorkStartDate { get; set; } = null;
        //mức lương 
        [JsonIgnore]
        public double? Offer { get; set; } = null;
        /// <summary>
        /// 1 - chưa phỏng vấn
        /// 2 - đạt yêu cầu
        /// 3 - không đạt yêu cầu
        /// </summary>
        [JsonIgnore]
        public int Status { get { return 1; } }
        [JsonIgnore]
        public string StatusName { get { return "chưa phỏng vấn"; } }
    }

    public class InputTestResultCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn buổi phỏng vấn")]
        public int InterviewAppointmentId { get; set; }
        //ứng viên
        [JsonIgnore]
        public int CurriculumVitaeId { get; set; }
        //người tổ chức
        [JsonIgnore]
        public int OrganizerId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điểm phần nghe")]
        [Range(0, double.MaxValue, ErrorMessage = "Điểm phần nghe phải là một số không âm")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Điểm phần nghe chỉ được chứa các chữ số")]
        public double? ListeningScore { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điểm phần đọc")]
        [Range(0, double.MaxValue, ErrorMessage = "Điểm phần đọc phải là một số không âm")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Điểm phần đọc chỉ được chứa các chữ số")]
        public double? ReadingScore { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điểm phần viết")]
        [Range(0, double.MaxValue, ErrorMessage = "Điểm phần viết phải là một số không âm")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Điểm phần viết chỉ được chứa các chữ số")]
        public double? WritingScore { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điểm phần nói")]
        [Range(0, double.MaxValue, ErrorMessage = "Điểm phần nói phải là một số không âm")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Điểm phần nói chỉ được chứa các chữ số")]
        public double? SpeakingScore { get; set; }
        //nếu làm kiểm tra online
        /// <summary>
        /// Đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        public string AttachFile { get; set; }
        /// <summary>
        /// 1 - tại trung tâm
        /// 2 - làm online
        /// </summary>
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Tại trung tâm"
                    : Type == 2 ? "Làm online" : "";
            }

        }
    }
    public class SendMailInviteInterViewCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn ứng viên")]
        public int CurriculumVitaeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phường xã")]
        public int WardId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn quận huyện")]
        public int DistrictId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn tỉnh thành phố")]
        public int AreaId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập người liên hệ")]
        [RegularExpression(@"^[\p{L}\s]*$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng")]
        public string ContractName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập điện thoại liên hệ")]
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string ContractPhone { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn vị trí ứng tuyển")]
        public int JobPositionId { get; set; }
    }
    public class SendMailInterviewResultCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn ứng viên")]
        public int CurriculumVitaeId { get; set; }
        /// <summary>
        /// thời gian bắt đầu làm việc
        /// chỉ cần nhập giờ và phút
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// thời gian kết thúc làm việc
        /// chỉ cần nhập giờ và phút
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        public DateTime EndTime { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập người liên hệ")]
        [RegularExpression(@"^[\p{L}\s]*$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng")]
        public string ContractName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email liên hệ")]
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ")]
        public string ContractEmail { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập điện thoại liên hệ")]
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string ContractPhone { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn vị trí ứng tuyển")]
        public int JobPositionId { get; set; }
    }
    public class ElsaSpeakRequest : BasePost
    {
        public int? studentId { get; set; }
        public string filePath { get; set; }
        public string sentence { get; set; }
    }

    public class ScoreboardTemplateCreate : BasePost
    {
        [Required(ErrorMessage = ("Vui lòng nhập mã"))]
        public string Code { get; set; }
        [Required(ErrorMessage = ("Vui lòng nhập tên mẫu"))]
        public string Name { get; set; }
    }
    public class ScoreColumnTemplateCreate : BasePost
    {
        //bảng điểm mẫu
        [Required(ErrorMessage = "Vui lòng chọn bảng điểm mẫu")]
        public int ScoreBoardTemplateId { get; set; }
        //tên cột 
        [Required(ErrorMessage = "Vui lòng nhập tên cột")]
        public string Name { get; set; }
        //hệ số
        //public int? Factor { get; set; }
        //vị trí
        [JsonIgnore]
        public int Index { get; set; }
        /// <summary>
        /// loại cột
        /// 1 - cột điểm 
        /// 2 - cột điểm trung binh
        /// 3 - cột ghi chú (cột này không tính vào điểm trung bình)
        /// </summary>
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Điểm"
                    : Type == 2 ? "Điểm trung bình"
                    : Type == 3 ? "Ghi chú" : "";
            }
        }
    }
    public class ScoreColumnCreate : BasePost
    {
        //lớp
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int ClassId { get; set; }
        //tên cột 
        [Required(ErrorMessage = "Vui lòng nhập tên cột")]
        public string Name { get; set; }
        //hệ số
        [Required(ErrorMessage = "Vui lòng nhập hệ số")]
        public int? Factor { get; set; }
        //vị trí
        [JsonIgnore]
        public int Index { get; set; }
        /// <summary>
        /// loại cột
        /// 1 - cột điểm 
        /// 2 - cột điểm trung binh
        /// 3 - cột ghi chú (cột này không tính vào điểm trung bình)
        /// </summary>
        public int Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Điểm"
                    : Type == 2 ? "Điểm trung bình"
                    : Type == 3 ? "Ghi chú" : "";
            }
        }
    }

    public class ScoreCreate : BasePost
    {
        //học viên
        [Required(ErrorMessage = "Vui lòng chọn sinh viên")]
        public int StudentId { get; set; }
        //lớp
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int ClassId { get; set; }
        //đợt thi
        [Required(ErrorMessage = "Vui lòng chọn đợt thi")]
        public int TranscriptId { get; set; }
        //cột
        [Required(ErrorMessage = "Vui lòng chọn cột")]
        public int ScoreColumnId { get; set; }
        //giá trị cột ( điểm số hoặc ghi chú )
        public string Value { get; set; }
    }

    public class TrainingRouteCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập trình độ hiện tại")]
        public string CurrentLevel { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập trình độ mong muốn")]
        public string TargetLevel { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tuổi")]
        public int? Age { get; set; }
        public string Name { get; set; }
    }
    public class TrainingRouteDetailCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập kỹ năng")]
        public string Skill { get; set; }
        /// <summary>
        /// 0 - Basic
        /// 1 - Advance
        /// 2 - Master
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn cấp độ")]
        public int? Level { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lộ trình")]
        public int? TrainingRouteId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int? TrainingRouteFormId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn bài kiểm tra")]
        public int? IeltsExamId { get; set; }
    }
    public class TrainingRouteFormCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lộ trình")]
        public int? TrainingRouteId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
    }
    public class StudentInTrainingCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lộ trình")]
        public int? TrainingRouteId { get; set; }
    }

    public class AddToClassFromReserveModel : BasePost
    {
        public int ClassReserveId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        public int? ClassId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int? BranchId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int? PaymentMethodId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày thanh toán tiếp theo")]
        public DateTime? PaymentAppointmentDate { get; set; }
        public string Note { get; set; }
        [Currency]
        public double Paid { get; set; }
    }
    public class JobPositionCreate : BasePost
    {
        [Required(ErrorMessage = ("Vui lòng nhập tên vị trí"))]
        public string Name { get; set; }
    }

    public class CalculatorMediumScoreCreate
    {
        public int ClassId { get; set; }
        public int TranscriptId { get; set; }
    }

    public class CustomerAfterImport
    {
        /// <summary>
        /// Trung tâm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập trung tâm")]
        public int? BranchId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public string LearningNeedName { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public string PurposeName { get; set; }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>

        public string SourceName { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public string SaleName { get; set; }
        /// <summary>
        /// Tên phụ huynh
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// Email phụ huynh
        /// </summary>
        public string ParentEmail { get; set; }
        /// <summary>
        /// Số điện thoại phụ huynh
        /// </summary>
        public string ParentMobile { get; set; }
        /// <summary>
        /// Điểm đầu vào
        /// </summary>
        public double? EntryPoint { get; set; }
        /// <summary>
        /// Điểm đầu ra mong muốn
        /// </summary>
        public double? DesiredOutputScore { get; set; }
        /// <summary>
        /// Chương trình muốn học
        /// </summary>
        public string DesiredProgram { get; set; }
    }

    public class InsertStudentExcel : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên học viên")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn giới tính của học viên")]
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^[0-9]{9,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public string LearningNeedName { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public string PurposeName { get; set; }
        /// <summary>
        /// Nguồn khách hàng
        /// </summary>

        public string SourceName { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public string SaleName { get; set; }
    }

    public class InsertEmployeeExcel : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên nhân viên")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn giới tính của nhân viên")]
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập chức vụ")]
        public string RoleName { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^[0-9]{9,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
    }

    public class ParentCreate : BasePost
    {
        [Required(ErrorMessage = "Fullname is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        public DateTime? DOB { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int Gender { get; set; } = ((int)GenderEnum.khac);
        /// <summary>
        /// Số điện thoại
        /// </summary>
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        public string Address { get; set; }
        [JsonIgnore]
        public int? StatusId
        {
            get { return ((int)AccountStatus.active); }
        }
        [JsonIgnore]
        public int RoleId { get; set; } = 8;
        [JsonIgnore]
        public string RoleName
        {
            get
            {
                return RoleId == ((int)RoleEnum.admin) ? "Admin"
                  : RoleId == ((int)RoleEnum.teacher) ? "Giáo viên"
                  : RoleId == ((int)RoleEnum.student) ? "Học viên"
                  : RoleId == ((int)RoleEnum.manager) ? "Quản lý"
                  : RoleId == ((int)RoleEnum.sale) ? "Tư vấn viên"
                  : RoleId == ((int)RoleEnum.accountant) ? "Kế toán"
                  : RoleId == ((int)RoleEnum.academic) ? "Học vụ"
                  : RoleId == ((int)RoleEnum.parents) ? "Phụ huynh"
                  : RoleId == ((int)RoleEnum.tutor) ? "Trợ giảng"
                  : "";
            }
        }
        public string Avatar { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập khẩu")]
        public string Password { get; set; }
        /// <summary>
        /// Trung tâm
        /// Mẫu 1,2
        /// </summary>
        [JsonIgnore]
        public string BranchIds { get; set; }
        /// <summary>
        /// 1 - Chờ kiểm tra
        /// 2 - Đã kiểm tra
        /// 3 - Không học
        /// 4 - Chờ xếp lớp
        /// 5 - Đang học
        /// 6 - Học xong
        /// </summary>
        [JsonIgnore]
        public int LearningStatus { get { return 5; } }
        [JsonIgnore]
        public string LearningStatusName { get { return "Đang học"; } }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Nhu cầu học
        /// </summary>
        public int? LearningNeedId { get; set; }
        /// <summary>
        /// Tư vấn viên
        /// </summary>
        public int? SaleId { get; set; }
        /// <summary>
        /// Mục đích học
        /// </summary>
        public int? PurposeId { get; set; }
        public int? CustomerId { get; set; }
        public int? ParentId { get; set; }
        /// <summary>
        /// Giới thiệu thêm
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// Chương trình dạy
        /// </summary>
        public string ProgramIds { get; set; }
        public bool isReceiveMailNotification { get; set; } = false;
        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        public string BankAccountNumber { get; set; }
        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        public string BankAccountName { get; set; }
        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// Tên chi nhánh
        /// </summary>
        public string BankBranch { get; set; }
        /// <summary>
        /// Phụ huynh của
        /// </summary>
        public int ParentOff { get; set; }

        public int studentId { get; set; }
    }
    public class AttendaceConfigCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên cấu hình")]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn loại điểm danh")]
        public List<AttendaceTypeCreate> AttendanceTypes { get; set; }
        [JsonIgnore]
        public bool Active { get; set; } = false;
    }
    public class AttendaceTypeCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn loại cấu hình")]
        public int TypeId { get; set; }
        [JsonIgnore]
        public string TypeName { get { return lmsEnum.AttendaceTypeName(TypeId); } }
        [Required(ErrorMessage = "Vui lòng chọn thời gian điểm danh")]
        public string TimeAttendace { get; set; }
    }
    public class StudentCertificateCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        public int? StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int? ClassId { get; set; }
    }
    public class CertificateConfigCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        public int? ClassId { get; set; }
        /// <summary>
        /// Tên chứng chỉ
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tên chứng chỉ")]
        public string CertificateName { get; set; }
        /// <summary>
        /// Tên khóa học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tên khóa học")]
        public string CertificateCourse { get; set; }
        /// <summary>
        /// ex: Dear: anh/chị
        /// </summary>
        public string SubTitle { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Loại chứng chỉ 
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại chứng chỉ")]
        public string Type { get; set; }
    }

    public class CustomerAudioCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int? CustomerId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn file")]
        public string File { get; set; }
        public string Note { get; set; }
    }
    public class SpendingConfigCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khoản chi")]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public int BranchId { get; set; }
    }
    public class PopupConfigCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        public DateTime STime { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        public DateTime ETime { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn độ trễ")]
        public double Durating { get; set; }
        public string Url { get; set; }
        public bool IsShow { get; set; } = false;
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh")]
        public string BranchIds { get; set; }
    }

    public class StudentDeviceCreate : BasePost
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin học viên")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập thông tin tên đăng nhập của học viên")]
        public string UserName { get; set; }
        public string DeviceName { get; set; }
        /// <summary>
        /// Hệ điều hành
        /// </summary>
        public string OS { get; set; }
        public string Browser { get; set; }
        public int? RoleId { get; set; }
    }

    public class HomeworkInCurriculumCreate : BasePost
    {
        /// <summary>
        /// id lớp học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn giáo trình")]
        public int? CurriculumId { get; set; }
        /// <summary>
        /// tên btvn
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// id bộ đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        /// <summary>
        /// thời gian bắt đầu làm bài
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// thời gian kết thúc làm bài
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        public string Note { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn loại bài tập")]
        public HomeworkType? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return HomeworkTypeName(Type ?? HomeworkType.Exam);
            }
        }
        public string HomeworkContent { get; set; }
        /// <summary>
        /// Điểm sàn
        /// </summary>
        public double? CutoffScore { get; set; }
        /// <summary>
        /// Số buổi
        /// </summary>
        public int? SessionNumber { get; set; }
        /// <summary>
        /// Vị trí của bài tập
        /// </summary>
        public int? Index { get; set; }
        public List<HomeworkFileInCurriculumCreate> AddFiles { get; set; } = new List<HomeworkFileInCurriculumCreate>();
    }

    public class HomeworkFileInCurriculumCreate : BasePost
    {
        public string File { get; set; }
        [JsonIgnore]
        public HomeworkFileType Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return HomeworkFileTypeName(Type);
            }
        }
        [JsonIgnore]
        public int HomeworkInCurriculumId { get; set; }
    }

}