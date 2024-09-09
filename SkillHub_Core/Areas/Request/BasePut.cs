using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System.Text.Json.Serialization;
using static LMSCore.Models.lmsEnum;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using LMSCore.Enum;

namespace LMSCore.Areas.Request
{
    public class BasePut
    {
        [Required(ErrorMessage = "Id is required")]
        public int? Id { get; set; }
        [JsonIgnore]
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
    }
    public class StaffDetailMePut
    {
        public StaffInformationMePut Information { get; set; }
        public StaffAddressPut Address { get; set; }
        public StaffAcountPut Account { get; set; }
        public StaffDetailMePut()
        {
            Information = new StaffInformationMePut();
            Address = new StaffAddressPut();
            Account = new StaffAcountPut();
        }
    }
    public class StaffDetailPut : BasePut
    {
        public StaffInformationPut Information { get; set; }
        public StaffAddressPut Address { get; set; }
        public StaffBankPut Bank { get; set; }
        public StaffAcountPut Account { get; set; }
        public StaffDetailPut()
        {
            Information = new StaffInformationPut();
            Address = new StaffAddressPut();
            Bank = new StaffBankPut();
            Account = new StaffAcountPut();
        }
    }

    public class StaffInformationMePut
    {
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
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Giới thiệu thêm
        /// </summary>
        public string Extension { get; set; }
    }
    public class StaffInformationPut
    {
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
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Giới thiệu thêm
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// 0 - Hoạt động 
        /// 1 - Khóa
        /// </summary>
        public int? StatusId { get; set; }
    }
    public class StaffAddressPut
    {
        public string Address { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
    }
    public class StaffBankPut
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
    public class StaffAcountPut
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class ChangeIndexRequest
    {
        [Required(ErrorMessage = "Không tìm thấy dữ liệu cần thay đổi")]
        public List<ChangeIndexRequestItem> Items { get; set; }
    }
    public class ChangeIndexRequestItem
    {
        public int Id { get; set; }
        public int Index { get; set; }
    }
    public class ClassTranscriptDetailPut : BasePut
    {
        /// <summary>
        /// Tên cột điểm
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Giá trị tối đa - áp dụng cho cột điểm ( Type = Grades )
        /// </summary>
        public string MaxValue { get; set; }
    }
    public class ClassTranscriptPut : BasePut
    {
        /// <summary>
        /// Tên bảng điểm
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ngày thi
        /// </summary>
        public DateTime? Date { get; set; }
    }
    public class SampleTranscriptDetailPut : BasePut
    {
        public string Name { get; set; }
        public SampleTranscriptDetailEnum.Type? Type { get; set; }
    }
    public class SampleTranscriptPut : BasePut
    {
        public string Name { get; set; }
    }
    public class ScheduleRecordUpdate : BasePut
    {
        public string Name { get; set; }
        public string UrlLink { get; set; }
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return tbl_ScheduleRecord.GetTypeName(Type);
            }
        }
    }
    public class TuitionPackageUpdate : BasePut
    {
        public string Code { get; set; }
        /// <summary>
        /// Số tháng
        /// </summary>
        public int? Months { get; set; }
        /// <summary>
        /// 1 - Giảm theo số tiền
        /// 2 - Giảm theo phần trăm 
        /// </summary>
        public int? DiscountType { get; set; }
        [JsonIgnore]
        public string DiscountTypeName
        {
            get
            {
                return DiscountType == 1 ? "Giảm theo số tiền" : DiscountType == 2 ? "Giảm theo phần trăm" : null;
            }
        }
        public double? Discount { get; set; }
        /// <summary>
        /// Giảm tối đa
        /// </summary>
        public double? MaxDiscount { get; set; }
        public string Description { get; set; }
    }
    public class ZnsConfigUpdate : BasePut
    {
        /// <summary>
        /// id ứng dụng
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// khóa bí mật của ứng dụng
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// RefreshToken
        /// </summary>
        public string RefreshToken { get; set; }
    }
    public class ZnsTemplateUpdate : BasePut
    {
        /// <summary>
        /// id template
        /// </summary>
        public string TemplateId { get; set; }
    }
    public class HomeworkUpdate : BasePut
    {
        /// <summary>
        /// tên btvn
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// thời gian bắt đầu làm bài
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// thời gian kết thúc làm bài
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// id bộ đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Giáo viên chấm bài
        /// </summary>
        public int? TeacherId { get; set; }
        public HomeworkType Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return HomeworkTypeName(Type);
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
        public List<HomeworkFileUpdate> Files { get; set; } = new List<HomeworkFileUpdate>();
    }
    public class StudentHomeworkResultUpdate : BasePut
    {
        public string Content { get; set; }
        public List<HomeworkFileUpdate> Files { get; set; } = new List<HomeworkFileUpdate>();
    }
    public class TeacherHomeworkResultUpdate : BasePut
    {
        public double? Point { get; set; }
        public string TeacherNote { get; set; }
    }
    public class HomeworkFileUpdate : BasePut
    {
        public string File { get; set; }
        [JsonIgnore]
        public HomeworkFileType Type { get; set; }
    }
    public class IeltsQuestionGroupUpdate : BasePut
    {
        [StringLength(100)]
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
        /// Từ khóa
        /// </summary>
        public string TagIds { get; set; }
        public int? Level { get; set; }
        [JsonIgnore]
        public string LevelName { get { return Level == 1 ? "Dễ" : Level == 2 ? "Trung bình" : Level == 3 ? "Khó" : null; } }
        public List<IeltsQuestionInsertOrUpdate> IeltsQuestions { get; set; }
        public IeltsQuestionGroupUpdate()
        {
            IeltsQuestions = new List<IeltsQuestionInsertOrUpdate>();
        }
    }
    public class IeltsQuestionInsertOrUpdate : BasePut
    {
        [JsonIgnore]
        public int IeltsQuestionGroupId { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// đáp án mẫu
        /// </summary>
        public string SampleAnswer { get; set; }
        public string InputId { get; set; }
        /// <summary>
        /// Giải thích bài làm (hiện khi học viên hoàn thành bài thi)
        /// </summary>
        public string Explain { get; set; }
        /// <summary>
        /// Vị trí của câu trong nhóm
        /// </summary>
        public int? Index { get; set; }
        public double? Point { get; set; }
        public bool? Enable { get; set; }
        public string Audio { get; set; }
        public List<IeltsAnswerInsertOrUpdate> IeltsAnswers { get; set; }
        public IeltsQuestionInsertOrUpdate()
        {
            IeltsAnswers = new List<IeltsAnswerInsertOrUpdate>();
        }
    }
    public class IeltsAnswerInsertOrUpdate : BasePut
    {
        [JsonIgnore]
        public int IeltsQuestionId { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// true - Đáp án đúng
        /// </summary>
        public bool? Correct { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// Áp dụng cho câu sắp xếp
        /// </summary>
        public int? Index { get; set; }
        public bool? Enable { get; set; }
    }
    public class IeltsSectionUpdate : BasePut
    {
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
    public class ChangeIndexModel
    {
        public List<ChangeIndexItem> Items { get; set; }
    }
    public class ChangeIndexItem : BasePut
    {
        [Required(ErrorMessage = "Vui lòng chọn vị trí mới")]
        public int Index { get; set; }
    }
    //public class StudyRouteIndex : BasePut
    //{
    //    [Required(ErrorMessage = "Vui lòng chọn vị trí mới")]
    //    public int Index { get; set; }
    //}
    public class StudyRouteIndex
    {
        [Required(ErrorMessage = "Vui lòng chọn vị trí lên")]
        public int IdUp { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn vị trí xuống")]
        public int IdDown { get; set; }
    }
    public class IeltsSkillUpdate : BasePut
    {
        [StringLength(100, ErrorMessage = "Tên đề thi không vượt quá 100 ký tự")]
        public string Name { get; set; }
        /// <summary>
        /// Tổng thời gian làm của kỹ năng phải bằng thời gian làm đề
        /// </summary>
        public int? Time { get; set; }
        /// <summary>
        /// Âm thanh
        /// </summary>
        public string Audio { get; set; }
    }
    public class IeltsExamUpdate : BasePut
    {
        [StringLength(100, ErrorMessage = "Tên đề thi không vượt quá 100 ký tự")]
        public string Name { get; set; }
        [StringLength(100, ErrorMessage = "Mã đề thi không vượt quá 100 ký tự")]
        public string Code { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// Tổng thời gian làm bài từng kỹ năng
        /// </summary>
        public int? Time { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// true - hiện
        /// </summary>
        public bool? Active { get; set; }
        public int? LevelExam { get; set; }
    }
    public class PaymentSessionUpdate : BasePut
    {
        public string PrintContent { get; set; }
    }
    public class PaymentSessionUpdateV2 : BasePut
    {
        public string PrintContent { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    public class FileCurriculumInClassUpdate : BasePut
    {
        public int? Index { get; set; }
    }
    public class CurriculumDetailInClassUpdate : BasePut
    {
        public string Name { get; set; }
        public int? Index { get; set; }
    }
    public class CurriculumInClassUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class FileInCurriculumDetailUpdate : BasePut
    {
        public int? Index { get; set; }
    }

    public class DocumentLibraryUpdate : BasePut
    {
        public string Background { get; set; }
        public string FileUrl { get; set; }
        public string FileUrlRead { get; set; }
        public string Description { get; set; }
    }
    public class DocumentLibraryDirectoryUpdate : BasePut
    {
        /// <summary>
        /// Tên chủ đề
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
    }
    public class FeedbackReplyUpdate : BasePut
    {
        /// <summary>
        /// Nội dung
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
    }
    public class FeedbackUpdate : BasePut
    {
        public string Title { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Cờ ưu tiên
        /// </summary>
        public bool? IsPriority { get; set; }
        /// <summary>
        /// Cờ ẩn danh
        /// </summary>
        public bool? IsIncognito { get; set; }
        public int? Status { get; set; }
    }

    public class ContractUpdate : BasePut
    {
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
    }
    public class NewsFeedReplyUpdate : BasePut
    {
        /// <summary>
        /// Nội dung bình luận
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
    }
    public class NewsFeedCommentUpdate : BasePut
    {
        /// <summary>
        /// Nội dung bình luận
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }
    }
    public class ScheduleAvailableUpdate : BasePut
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Note { get; set; }
    }
    public class SalaryUpdate : BasePut
    { /// <summary>
      /// Khấu trừ
      /// </summary>
        public double? Deduction { get; set; }
        /// <summary>
        /// Thưởng thêm
        /// </summary>
        public double? Bonus { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        /// <summary>
        /// 1 - Chưa chốt
        /// 2 - Đã chốt
        /// 3 - Đã thanh toán
        /// </summary>
        public int? Status { get; set; }
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Chưa chốt"
                        : Status == 2 ? "Đã chốt"
                        : Status == 3 ? "Đã thanh toán" : null;
            }
        }
    }

    public class SalaryMultipleUpdate
    {
        [JsonIgnore]
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
        public List<int> Ids { get; set; }
        /// <summary>
        /// 1 - Chưa chốt
        /// 2 - Đã chốt
        /// 3 - Đã thanh toán
        /// </summary>
        public int? Status { get; set; }
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Chưa chốt"
                        : Status == 2 ? "Đã chốt"
                        : Status == 3 ? "Đã thanh toán" : null;
            }
        }
    }
    public class PackageSkillUpdate : BasePut
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
    }
    public class PackageSectionUpdate : BasePut
    {
        public string Name { get; set; }
    }


    public class NewsFeedUpdate : BasePut
    {
        /// <summary>
        /// Nội dung bản tin
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Màu Bản tin
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// Nền Bản tin
        /// </summary>
        public string BackGroundUrl { get; set; }
        /// <summary>
        /// Nhóm
        /// </summary>
        public int? NewsFeedGroupId { get; set; }
        /// <summary>
        /// Danh sách chi nhánh
        /// </summary>
        public List<int?> ListBranchId { get; set; }

        /// <summary>
        /// File => tbl_NewsFeedFile
        /// </summary>
        public List<NewsFeedFile> FileListUpdate { get; set; }
    }

    public class UserInNFGroupUpdate : BasePut
    {
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
    public class NewsFeedGroupUpdate : BasePut
    {
        /// <summary>
        /// Tên nhóm
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Hình nền
        /// </summary>
        public string BackGround { get; set; }
    }
    public class CartUpdate : BasePut
    {
        public int? Quantity { get; set; }
    }
    public class RefundUpdate : BasePut
    {
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Đã duyệt
        /// 3 - Hủy
        /// </summary>
        public int? Status { get; set; }
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Chờ duyệt"
                        : Status == 2 ? "Đã duyệt"
                        : Status == 3 ? "Hủy" : null;
            }
        }
    }
    public class ClassReserveUpdate : BasePut
    {
        public string Note { get; set; }
        /// <summary>
        /// Ngày hết hạn bảo lưu
        /// </summary>
        public DateTime? Expires { get; set; }
    }
    public class ClassRegistrationUpdate : BasePut
    {
        public string Note { get; set; }
    }
    public class StudentInClassUpdate : BasePut
    {
        public bool? Warning { get; set; }
        public string WarningContent { get; set; }
        public string Note { get; set; }
    }
    public class PaymentMethodUpdate : BasePut
    {
        public bool? Active { get; set; }
        public string Thumbnail { get; set; }
        public string Description { get; set; }
        //thông tin key 
        public string Version { get; set; }//Phiên bản api mà merchant kết nối.
        public string PartnerCode { get; set; }//PartnerCode--TmnCode
        public string Secretkey { get; set; }
        public string OrderType { get; set; } = "orther";//topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến

        // vnpay
        /// <summary>
        /// Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// Đơn vị tiền tệ sử dụng thanh toán. VND
        /// </summary>
        public string CurrCode { get; set; }
        /// <summary>
        /// ngôn ngữ
        /// </summary>
        public string Locale { get; set; }
        /// <summary>
        /// publickey dùng cho momo
        /// </summary>
        public string PublicKey { get; set; }
    }
    public class ScheduleUpdate : BasePut
    {
        public int? RoomId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? TeacherId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Dùng cho dạy kèm
        /// 1 - Mới đặt 
        /// 2 - Hủy
        /// 3 - Đã học 
        /// 4 - Giáo viên vắng mặt
        /// 5 - Sự cố kỹ thuật
        /// 6 - Giáo viên vào trễ
        /// 7 - Học viên vắng mặt
        /// </summary>
        public int? StatusTutoring { get; set; }
        /// <summary>
        /// Dùng cho dạy kèm
        /// </summary>
        [JsonIgnore]
        public string StatusTutoringName
        {
            get
            {
                return StatusTutoring == 1 ? "Mới đặt"
                        : StatusTutoring == 2 ? "Hủy"
                        : StatusTutoring == 3 ? "Đã học"
                        : StatusTutoring == 4 ? "Giáo viên vắng mặt"
                        : StatusTutoring == 5 ? "Sự cố kỹ thuật"
                        : StatusTutoring == 6 ? "Giáo viên vào trễ"
                        : StatusTutoring == 7 ? "Học viên vắng mặt" : null;
            }
        }
        public double? TeachingFee { get; set; }
        public double? TutorFee { get; set; }
    }
    public class TeacherInProgramUpdate
    {
        [Required(ErrorMessage = "Vui lòng nhập Id giáo viên")]
        public int TeacherId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Id chương trình")]
        public int ProgramId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lương giáo viên/buổi")]
        public double TeachingFee { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập thông tin trạng thái")]
        public bool IsActive { get; set; }
    }
    public class ScheduleUpdateV2 : BasePut
    {
        public int? RoomId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? TeacherId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Dùng cho dạy kèm
        /// 1 - Mới đặt 
        /// 2 - Hủy
        /// 3 - Đã học 
        /// 4 - Giáo viên vắng mặt
        /// 5 - Sự cố kỹ thuật
        /// 6 - Giáo viên vào trễ
        /// 7 - Học viên vắng mặt
        /// </summary>
        public int? StatusTutoring { get; set; }
        /// <summary>
        /// Dùng cho dạy kèm
        /// </summary>
        [JsonIgnore]
        public string StatusTutoringName
        {
            get
            {
                return StatusTutoring == 1 ? "Mới đặt"
                        : StatusTutoring == 2 ? "Hủy"
                        : StatusTutoring == 3 ? "Đã học"
                        : StatusTutoring == 4 ? "Giáo viên vắng mặt"
                        : StatusTutoring == 5 ? "Sự cố kỹ thuật"
                        : StatusTutoring == 6 ? "Giáo viên vào trễ"
                        : StatusTutoring == 7 ? "Học viên vắng mặt" : null;
            }
        }
        // Logic mới sẽ không cập nhật lương/buổi ở lịch dạy
        //public double? TeachingFee { get; set; }
        public double? TutorFee { get; set; }
    }
    public class ClassUpdate : BasePut
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public double? Price { get; set; }
        /// <summary>
        /// 1 - Sắp diễn ra
        /// 2 - Đang diễn ra
        /// 3 - Kết thúc
        /// </summary>
        public int? Status { get; set; }
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Sắp diễn ra"
                         : Status == 2 ? "Đang diễn ra"
                         : Status == 3 ? "Kết thúc" : null;
            }
        }
        /// <summary>
        /// Học vụ
        /// </summary>
        public int? AcademicId { get; set; }
        /// <summary>
        /// Giáo viên
        /// </summary>
        public int? TeacherId { get; set; }
        /// <summary>
        /// Số lượng tối đa
        /// </summary>
        public int? MaxQuantity { get; set; }
        /// <summary>
        /// Mẫu chứng chỉ
        /// </summary>
        public int? CertificateTemplateId { get; set; }
        /// <summary>
        /// Ngày bắt đầu dự kiến
        /// </summary>
        public DateTime? EstimatedDay { get; set; }
    }
    public class TeacherOffUpdate : BasePut
    {
        /// <summary>
        /// 1 - Chờ duyệt
        /// 2 - Duyệt
        /// 3 - Không duyệt
        /// </summary>
        public int? Status { get; set; }
        [StringLength(20)]
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Chờ duyệt"
                    : Status == 2 ? "Duyệt"
                    : Status == 3 ? "Không duyệt" : null;
            }
        }
        /// <summary>
        /// ghi chú
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; }
    }
    public class StudyTimeUpdate : BasePut
    {
        /// <summary>
        /// định dạng HH:mm
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// định dạng HH:mm
        /// </summary>
        public string EndTime { get; set; }
    }
    public class CurriculumDetailUpdate : BasePut
    {
        public string Name { get; set; }
        public int? Index { get; set; }
    }

    public class CurriculumUpdate : BasePut
    {
        public string Name { get; set; }
        /// <summary>
        /// Số buổi
        /// </summary>
        public int? Lesson { get; set; }
        /// <summary>
        /// Thời gian
        /// </summary>
        public int? Time { get; set; }
    }
    public class ProgramUpdate : BasePut
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public double? Price { get; set; }
        public string Description { get; set; }
    }
    public class GradeUpdate : BasePut
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class TestAppointmentUpdate : BasePut
    {
        public DateTime? Time { get; set; }
        /// <summary>
        /// 1 - Chờ kiểm tra
        /// 2 - Đã kiểm tra
        /// 3 - Không học
        /// 4 - Chờ xếp lớp
        /// </summary>
        public int? LearningStatus { get; set; }
        public string LearningStatusName { get { return tbl_UserInformation.GetLearningStatusName(LearningStatus ?? 0); } }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 1 - Tại trung tâm
        /// 2 - Làm bài trực tuyến
        /// </summary>
        public int? Type { get; set; }
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Tại trung tâm"
                    : Type == 2 ? "Làm bài trực tuyến" : null;
            }
        }
        public string ListeningPoint { get; set; }
        public string SpeakingPoint { get; set; }
        public string ReadingPoint { get; set; }
        public string WritingPoint { get; set; }
        public string Vocab { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Học phí tối đa, dùng để tư vấn khóa học
        /// </summary>
        public string Tuitionfee { get; set; }
        /// <summary>
        /// Đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        public string AttachFile { get; set; }
    }
    public class CustomerUpdate : BasePut
    {
        public int? LearningNeedId { get; set; }
        public int? CustomerStatusId { get; set; }
        public int? ReasonOutId { get; set; }
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
        /// <summary>
        /// Ngày lên lịch lại (Khách hàng hẹn lại hôm khác)
        /// </summary>
        public DateTime? RescheduledDate { get; set; }
    }
    public class CustomerStatusUpdate : BasePut
    {
        public string Name { get; set; }
        public string ColorCode { get; set; }
    }
    public class FrequentlyQuestionUpdate : BasePut
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string RoleIds { get; set; }
    }
    public class TemplateUpdate
    {
        /// <summary>
        /// 1 - Hợp đồng
        /// 2 - Điều khoản
        /// 3 - Mẫu phiếu thu
        /// 4 - Mẫu phiếu chi
        /// </summary>
        public int? Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return Type == 1 ? "Hợp đồng"
                    : Type == 2 ? "Điều khoản"
                    : Type == 3 ? "Phiếu thu"
                    : Type == 4 ? "Phiếu chi" : "";
            }
        }
        public string Content { get; set; }
    }
    public class IdiomUpdate : BasePut
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Content { get; set; }
    }
    public class PurposeUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class JobUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class DayOffUpdate : BasePut
    {
        public string Name { get; set; }
        public DateTime? sDate { get; set; }
        public DateTime? eDate { get; set; }
        public string BranchIds { get; set; }
    }

    public class SourceUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class LearningNeedUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class TutorSalaryConfigUpdate : BasePut
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public double? Salary { get; set; }
    }
    public class DiscountUpdate : BasePut
    {
        public double? Value { get; set; }
        /// <summary>
        /// 1 - Đang diễn ra
        /// 2 - Đã kết thúc
        /// </summary>
        public int? Status { get; set; }
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Đang diễn ra"
                    : Status == 2 ? "Đã kết thúc" : null;
            }
        }
        public string Note { get; set; }
        public DateTime? Expired { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int? Quantity { get; set; }
        /// <summary>
        /// Khuyến mãi tối đa
        /// </summary>
        public double? MaxDiscount { get; set; }
        /// <summary>
        /// trung tâm
        /// </summary>
        public string BranchIds { get; set; }
    }
    public class RoomUpdate : BasePut
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class BranchUpdate : BasePut
    {

        public string Code { get; set; }
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
    public class SeminarRecordUpdate : BasePut
    {
        public string Name { get; set; }
        public string VideoUrl { get; set; }
    }
    public class ExamSectionUpdate : BasePut
    {
        public string Name { get; set; }
        /// <summary>
        /// Nội dung chú thích đáp án đúng của đoạn văn
        /// </summary>
        public string Explanations { get; set; }
        public string Audio { get; set; }
    }
    public class CertificateUpdate : BasePut
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }
    public class ExerciseGroupUpdate : BasePut
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string Paragraph { get; set; }
        public ExerciseLevel? Level { get; set; }
        [JsonIgnore]
        public string LevelName
        {
            get { return Level == ExerciseLevel.Easy ? "Dễ" : Level == ExerciseLevel.Normal ? "Trung bình" : Level == ExerciseLevel.Difficult ? "Khó" : ""; }
        }
        public string Tags { get; set; }
        public List<ExerciseUpdate> ExerciseUpdates { get; set; }
    }
    public class ExerciseUpdate : BasePut
    {
        public string Content { get; set; }
        /// <summary>
        /// Áp dụng cho câu Chọn từ vào ô trống và điền từ
        /// </summary>
        public string InputId { get; set; }
        public List<AnswerUpdate> AnswerUpdates { get; set; }
        public string DescribeAnswer { get; set; }
        public int? Index { get; set; }
        public bool? Enable { get; set; }
        public double? Point { get; set; }
    }
    public class AnswerUpdate : BasePut
    {
        public string AnswerContent { get; set; }
        public bool? IsTrue { get; set; }
        public bool? Enable { get; set; }
        public int? Index { get; set; }
    }
    public class ExamUpdate : BasePut
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        public int? Time { get; set; }
        /// <summary>
        /// Số điểm đạt
        /// </summary>
        public double? PassPoint { get; set; }
        public string Audio { get; set; }
    }
    public class SeminarUpdate : BasePut
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? VideoCourseId { get; set; }
        public int? LeaderId { get; set; }
        public int? Member { get; set; }
        public string Thumbnail { get; set; }
    }
    public class LessonVideoUpdate : BasePut
    {
        public string Name { get; set; }
        /// <summary>
        /// File video
        /// </summary>
        public string VideoUrl { get; set; }
        /// <summary>
        /// Bài tập
        /// </summary>
        public int? ExamId { get; set; }
    }
    public class SectionUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class ProductUpdate : BasePut
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public int? BeforeCourseId { get; set; }
        public double? Price { get; set; }
    }
    public class ResetPasswordModel
    {
        public string Key { get; set; }
        /// <summary>
        /// Mật khẩu mới
        /// </summary>
        [StringLength(128, ErrorMessage = "Mật khẩu phải có ít nhất 6 kí tự và tối đa 128 ký tự", MinimumLength = 6)]
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc nhập")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Xác nhận mật khẩu mới
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khẩu mới")]
        [StringLength(128, ErrorMessage = "Mật khẩu xác nhận phải có ít nhất 6 kí tự và tối đa 128 ký tự", MinimumLength = 6)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không đúng")]
        public string ConfirmNewPassword { get; set; }
    }
    public class ChangePasswordModel
    {
        /// <summary>
        /// Mật khẩu cũ
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// Mật khẩu mới
        /// </summary>
        [StringLength(128, ErrorMessage = "Mật khẩu phải có ít nhất 6 kí tự và tối đa 128 ký tự", MinimumLength = 6)]
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc nhập")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Xác nhận mật khẩu mới
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khẩu mới")]
        [StringLength(128, ErrorMessage = "Mật khẩu xác nhận phải có ít nhất 6 kí tự và tối đa 128 ký tự", MinimumLength = 6)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không đúng")]
        public string ConfirmNewPassword { get; set; }
    }
    public class UserUpdate
    {
        [Key]
        public int UserInformationId { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ!")]
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public DateTime? DOB { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// 0 - Hoạt động
        /// 1 - Khoá
        /// </summary>
        public int? StatusId { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public int? AreaId { get; set; } // tình/tp
        public int? DistrictId { get; set; }//quận/huyện
        public int? WardId { get; set; } // phường/xã 
        public string Password { get; set; }
        [JsonIgnore]
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
        /// <summary>
        /// Trung tâm
        /// Mẫu 1,2
        /// </summary>
        public string BranchIds { get; set; }
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
        public int? ParentId { get; set; }
        public int? JobId { get; set; }
        public string Extension { get; set; }
        public bool? IsReceiveMailNotification { get; set; }
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
    public class StudyRouteUpdate : BasePut
    {
        [Required(ErrorMessage = "Học viên không được bỏ trống")]
        public int? StudentId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn chương trình")]
        public int? ProgramId { get; set; }
        public string Note { get; set; }
        public int? Status { get; set; }
    }
    public class CertificateTemplateUpdate : BasePut
    {
        /// <summary>
        /// Tên mẫu chứng chỉ
        /// </summary>
        public string Name { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Hình nền
        /// </summary>
        public string Background { get; set; }
        /// <summary>
        /// Mặt sau
        /// </summary>
        public string Backside { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
    public class ComboUpdate : BasePut
    {
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Chương trình học trong combo
        /// </summary>
        public string ProgramIds { get; set; }
        public int? Type { get; set; }
        public double? Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class CertificateConfigUpdate : BasePut
    {
        /// <summary>
        /// Tên chứng chỉ
        /// </summary>
        public string CertificateName { get; set; }
        /// <summary>
        /// Tên khóa học
        /// </summary>
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
        public string Type { get; set; }
    }

    public class StudentPointRecordUpdate : BasePut
    {
        ///<summary>
        ///Hạnh kiểm			
        /// </summary>
        public string Behaviour { get; set; } = null;
        ///<summary>
        ///Học lực		
        /// </summary>
        public string AcademicPerformance { get; set; } = null;
        ///<summary>
        ///Ghi chú và đề xuất của giáo viên (nếu có)	
        /// </summary>
        public string Note { get; set; } = null;
    }
    public class CommissionConfigUpdate : BasePut
    {
        public string Name { get; set; }
        public string Descriptions { get; set; }
        public List<CommissionNormUpdate> CommissionNormUpdate { get; set; }
    }

    public class CommissionNormUpdate : BasePut
    {
        public string Name { get; set; }
        public double? MinNorm { get; set; }
        public double? MaxNorm { get; set; }
        public double? PercentNew { get; set; }
        public double PercentRenewals { get; set; }
    }
    public class CommissionCampaignUpdate : BasePut
    {
        public double RevenueTargets { get; set; }
    }
    public class CommissionUpdate : BasePut
    {
        //public int StudentId { get; set; } 
        [Required(ErrorMessage = "Tên nhân viên không được bỏ trống")]
        public int SaleId { get; set; }
        // bảng hoa hồng cho 1 tháng duy nhất
        [Required(ErrorMessage = "Vui lòng chọn năm")]
        public int Year { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn tháng")]
        public int Month { get; set; }
        // Tổng giá trị nv đó mang về trong tháng đó
        public double TotalTuitionFee { get; set; }
        // mã chiến dịch, không có thì khỏi tính hoa hồng
        public int CampaignId { get; set; }
        // tổng hoa hồng
        public double Commission { get; set; }
    }

    public class ClosingCommission : BasePut
    {
        // Tổng giá trị nv đó mang về trong tháng đó
        public double Commission { get; set; }
        // mã chiến dịch, không có thì khỏi tính hoa hồng
        public string Description { get; set; }
        // tổng hoa hồng
        public double Percent { get; set; }
    }


    public class RatingSheetUpdate : BasePut
    {
        public string Name { get; set; }
        public string Note { get; set; }
    }
    public class RatingQuestionUpdate : BasePut
    {
        public string Name { get; set; }
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
    public class RatingOptionUpdate : BasePut
    {
        public string Name { get; set; }
        /// <summary>
        /// 0: False
        /// 1: True
        /// </summary>
        public bool TrueOrFalse { get; set; } = false;
        public string Essay { get; set; }
    }
    public class StudentRatingChoiceUpdate : BasePut
    {
        [Required(ErrorMessage = "Vui lòng chọn bảng khảo sát ")]
        public int StudentRatingFormId { get; set; }
        public string ListRatingAnswer { get; set; }
    }
    public class LessionVideoTestUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class LessionVideoQuestionUpdate : BasePut
    {
        //[Required(ErrorMessage = "Vui lòng chọn video")]
        //public int LessionVideoId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 1:Dễ
        /// 2:Tb
        /// 3:Khó
        /// </summary> 
        public int? Level { get; set; }
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
    public class LessionVideoOptionUpdate : BasePut
    {
        //[Required(ErrorMessage = "Vui lòng chọn câu hỏi")]
        //public int QuestionId { get; set; } 
        public string Content { get; set; }
        public bool? TrueFalse { get; set; }
    }
    public class StudentMonthlyDebtUpdate : BasePut
    {
        public bool? IsPaymentDone { get; set; } = true;
    }

    public class CurriculumVitaeUpdate : BasePut
    {
        [RegularExpression("^[a-zA-Z\\s]*$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
        public string FullName { get; set; }
        [RegularExpression(@"^[0-9]+${9,11}", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "Email có định dạng không hợp lệ")]
        public string Email { get; set; }
        public string LinkCV { get; set; }
        public int? BranchId { get; set; }
        public int? JobPositionId { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
    }

    public class InterviewAppointmentUpdate : BasePut
    {
        //người tổ chức
        public int? OrganizerId { get; set; }
        //ngày phỏng vấn
        public DateTime? InterviewDate { get; set; }

        //nếu sau buổi pv mà pass thì update các thông tin sau
        //ngày ứng viên có thể bắt đầu đi làm
        public DateTime? WorkStartDate { get; set; }
        //mức lương 
        [Range(0, double.MaxValue, ErrorMessage = "Offer phải là một số không âm")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Offer chỉ được chứa các chữ số")]
        public double? Offer { get; set; }
        /// <summary>
        /// 1 - chưa phỏng vấn
        /// 2 - đạt yêu cầu
        /// 3 - không đạt yêu cầu
        /// </summary>
        public int? Status { get; set; }
        [JsonIgnore]
        public string StatusName
        {
            get
            {
                return Status == 1 ? "chưa phỏng vấn"
                    : Status == 2 ? "Đạt yêu cầu"
                    : Status == 3 ? "Không đạt yêu cầu" : null;
            }
        }
    }

    public class InputTestResultUpdate : BasePut
    {
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
    public class ScoreboardTemplateUpdate : BasePut
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class ScoreColumnTemplateUpdate : BasePut
    {
        //bảng mẫu
        public int? ScoreBoardTemplateId { get; set; }
        //tên cột 
        public string Name { get; set; }
        //hệ số
        public int? Factor { get; set; }
        //vị trí
        public int? Index { get; set; }
        /// <summary>
        /// loại cột
        /// 1 - cột điểm 
        /// 2 - cột điểm trung binh
        /// 3 - cột ghi chú (cột này không tính vào điểm trung bình)
        /// </summary>
        public int? Type { get; set; }
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
    public class ScoreColumnUpdate : BasePut
    {
        //lớp
        public int? ClassId { get; set; }
        //tên cột 
        public string Name { get; set; }
        //hệ số
        public int? Factor { get; set; }
        //vị trí
        public int? Index { get; set; }
        /// <summary>
        /// loại cột
        /// 1 - cột điểm 
        /// 2 - cột điểm trung binh
        /// 3 - cột ghi chú (cột này không tính vào điểm trung bình)
        /// </summary>
        public int? Type { get; set; }
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
    public class ScoreUpdate : BasePut
    {
        //học viên
        public int? StudentId { get; set; }
        //lớp
        public int? ClassId { get; set; }
        //cột
        public int? ScoreColumnId { get; set; }
        //giá trị cột ( điểm số hoặc ghi chú )
        public string Value { get; set; }
    }

    public class TrainingRouteUpdate : BasePut
    {
        public string Name { get; set; }
        public string CurrentLevel { get; set; }
        public string TargetLevel { get; set; }
        public int? Age { get; set; }
    }
    public class TrainingRouteFormUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class TrainingRouteDetailUpdate : BasePut
    {
        public string Skill { get; set; }
        /// <summary>
        /// 0 - Basic
        /// 1 - Advance
        /// 2 - Master
        /// </summary>
        public int? Level { get; set; }
        public int? IeltsExamId { get; set; }
    }
    public class JobPositionUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class TagUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class ReasonOutUpdate : BasePut
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class TagCategoryUpdate : BasePut
    {
        public string Name { get; set; }
    }
    public class AttendaceConfigUpdate : BasePut
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AttendaceTypeUpdate> AttendanceTypes { get; set; }
        public bool? Active { get; set; }
    }
    public class AttendaceTypeUpdate : BasePut
    {
        public int TypeId { get; set; }
        [JsonIgnore]
        public string TypeName { get { return lmsEnum.AttendaceTypeName(TypeId); } }
        public string TimeAttendace { get; set; }
    }

    public class ListTeacherInProgram
    {
        [Required(ErrorMessage = "Vui lòng nhập Id giáo viên")]
        public int? TeacherId { get; set; }
        public double? TeachingFee { get; set; }
        public bool? IsActive { get; set; }
    }
    public class SetCustomerForSaler
    {
        public int? CustomerId { get; set; }
    }
    public class SetStudentForSaler
    {
        public int? StudentId { get; set; }
    }
    public class SpendingConfigUpdate : BasePut
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
    }
    public class PopupConfigUpdate : BasePut
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime? STime { get; set; }
        public DateTime? ETime { get; set; }
        public double? Durating { get; set; }
        public string Url { get; set; }
        public bool? IsShow { get; set; }
        public string BranchIds { get; set; }
    }

    public class StudentDeviceUpdate : BasePut
    {
        public bool? Allowed { get; set; }
    }

    public class DeviceConfigUpdate : BasePut
    {
        public int? Allowed { get; set; }
        public int? Quantity { get; set; }
    }
    public class EmployeeBranchUpdate
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin quản lý")]
        public int ManagerId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập thông tin nhân  viên")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập thông tin chi nhánh")]
        public string BranchIds { get; set; }
    }

    public class HomeworkFileInCurriculumUpdate
    {
        public int? Id { get; set; }
        public string File { get; set; }
        [JsonIgnore]
        public HomeworkFileType Type { get; set; }
    }

    public class HomeworkInCurriculumUpdate : BasePut
    {
        /// <summary>
        /// tên btvn
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// thời gian bắt đầu làm bài
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// thời gian kết thúc làm bài
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// id bộ đề
        /// </summary>
        public int? IeltsExamId { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        public string Note { get; set; }
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
        /// <summary>
        /// Giáo viên chấm bài
        /// </summary>
        public HomeworkType Type { get; set; }
        [JsonIgnore]
        public string TypeName
        {
            get
            {
                return HomeworkTypeName(Type);
            }
        }
        public string HomeworkContent { get; set; }
        public List<HomeworkFileInCurriculumUpdate> Files { get; set; } = new List<HomeworkFileInCurriculumUpdate>();
    }

    public class IndexHomeworkInCurriculum
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin của bài tập trong giáo trình")]
        public int? Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập vị trí của bài tập trong giáo trình")]
        public int? Index { get; set; }
    }

    public class AllowHomeworkSequenceInClassUpdate
    {
        /// <summary>
        /// Id lớp học
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập thông tin của lớp học")]
        public int? Id { get; set; }
        /// <summary>
        /// Cho phép ràng buộc làm bài tập theo thứ tự
        /// </summary>
        public bool? IsAllow { get; set; }
    }

    public class AllowHomeworkSequenceInCurriculumUpdate
    {
        /// <summary>
        /// Id giáo trình
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập thông tin của giáo trình")]
        public int? Id { get; set; }
        /// <summary>
        /// Cho phép ràng buộc làm bài tập theo thứ tự
        /// </summary>
        public bool? IsAllow { get; set; }
    }
    public class IndexHomeworkUpdate
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin của bài tập")]
        public int? Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập vị trí của bài tập")]
        public int? Index { get; set; }
    }

    public class HomeworkForTeacherUpdate
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin của giáo viên")]
        public int? TeacherId { get; set; }
        public List<int> HomeworkIds { get; set; }
    }

    public class IndexCustomerStatusUpdate
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin của trạng thái khách hàng")]
        public int? Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập vị trí của trạng thái khách hàng")]
        public int? Index { get; set; }
    }
}