using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMSCore.Models
{
    public static class lmsEnum
    {
        public enum ClassRegistrationStatus {
            WaitingClassPlacement = 1, // CHỜ XẾP LỚP
            Classed, // ĐÃ XẾP LỚP
            Refunded // ĐÃ HOÀN TIỀN
        }
        public static string ClassRegistrationStatusName(int? Key)
        {
            if (Key == null)
                return string.Empty;
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)ClassRegistrationStatus.WaitingClassPlacement,
                    Value = "Chờ xếp lớp"
                },
                new EnumObject
                {
                    Key = (int)ClassRegistrationStatus.Classed,
                    Value = "Đã xếp lớp"
                },
                new EnumObject
                {
                    Key = (int)ClassRegistrationStatus.Refunded,
                    Value = "Đã hoàn tiền"
                },
            };
            return data.FirstOrDefault(x => x.Key == Key)?.Value;
        }
        public enum BillType
        {
            RegisterStudy = 1,
            BuyServices,
            SignUpTutoringClass,
            CreateManually,
            MonthlyTuition,
            ClassChangeFee,
            BuyComboPack
        }
        public static string BillTypeName(int? Key)
        {
            if (Key == null)
                return string.Empty;
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)BillType.RegisterStudy,
                    Value = "Đăng ký học"
                },
                new EnumObject
                {
                    Key = (int)BillType.BuyServices,
                    Value = "Mua dịch vụ"
                },
                new EnumObject
                {
                    Key = (int)BillType.SignUpTutoringClass,
                    Value = "Đăng ký lớp dạy kèm"
                },
                new EnumObject
                {
                    Key = (int)BillType.CreateManually,
                    Value = "Tạo thủ công"
                },
                new EnumObject
                {
                    Key = (int)BillType.MonthlyTuition,
                    Value = "Học phí hằng tháng"
                },
                new EnumObject
                {
                    Key = (int)BillType.ClassChangeFee,
                    Value = "Phí chuyển lớp"
                },
                new EnumObject
                {
                    Key = (int)BillType.BuyComboPack,
                    Value = "Mua gói combo"
                },
            };
            return data.FirstOrDefault(x => x.Key == Key)?.Value;
        }
        public enum ComboType
        {
            Percent = 1,
            Money,
        }
        public static string ComboTypeName(int? Key)
        {
            if (Key == null)
                return string.Empty;
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)ComboType.Percent,
                    Value = "Giảm phần trăm"
                },
                new EnumObject
                {
                    Key = (int)ComboType.Money,
                    Value = "Giảm tiền"
                },
            };
            return data.FirstOrDefault(x => x.Key == Key)?.Value;
        }
        public enum ComboStatus
        {
            CommingSoon = 1, // sắp bắt đầu
            IsGoingOn, // đang diễn ra
            HasEnded, // đã kết thúc
        }
        public static string ComboStatusName(ComboStatus? Key)
        {
            if (Key == null)
                return string.Empty;
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)ComboStatus.CommingSoon,
                    Value = "Sắp bắt đầu"
                },
                new EnumObject
                {
                    Key = (int)ComboStatus.IsGoingOn,
                    Value = "Đang diễn ra"
                },
                new EnumObject
                {
                    Key = (int)ComboStatus.HasEnded,
                    Value = "Đã kết thúc"
                },
            };
            return data.FirstOrDefault(x => x.Key == (int)Key)?.Value;
        }
        public enum TypeQuestionEnum
        {
            TracNghiem = 1,
            ChonTuVaoOTong,
            DienVaoOTrong,
            GhepDapAn,
            ChonDungSai,
            SapXep,
            Viet,
            Noi
        }
        public enum LevelQuestionEnum
        {
            De = 1,
            TrungBinh,
            Kho
        }
        public enum AttendaceType
        {
            DauNgay = 1,
            DauGioHoc,
            CuoiGioHoc,
            SauBuoiHoc,
            CuoiNgay
        }
        public enum CertificateType
        {
            Certificate_1_Vertical = 1,
            Certificate_1_Horizontal,
            Certificate_2_Vertical,
            Certificate_2_Horizontal,
            Certificate_3_Vertical,
            Certificate_3_Horizontal,
            Certificate_4_Vertical,
            Certificate_4_Horizontal
        }
        public enum HomeworkResultType
        {
            GotPoint = 1, // Đã chấm điểm
            NoPointYet,// Chưa chấm điểm
        }
        public static string HomeworkResultTypeName(HomeworkResultType? Key)
        {
            if (Key == null)
                return string.Empty;
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)HomeworkResultType.GotPoint,
                    Value = "Đã chấm điểm"
                },
                new EnumObject
                {
                    Key = (int)HomeworkResultType.NoPointYet,
                    Value = "Chưa chấm điểm"
                },
            };
            return data.FirstOrDefault(x => x.Key == (int)Key)?.Value;
        }
        public enum HomeworkType
        {
            Homework = 1,
            Exam,
        }
        public static string HomeworkTypeName(HomeworkType? Key)
        {
            if (Key == null)
                return string.Empty;
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)HomeworkType.Homework,
                    Value = "Bài tập"
                },
                new EnumObject
                {
                    Key = (int)HomeworkType.Exam,
                    Value = "Giải đề"
                },
            };
            return data.FirstOrDefault(x => x.Key == (int)Key)?.Value;
        }
        public enum HomeworkFileType
        {
            GiveHomework = 1,
            SubmitHomework,
        }
        public static string HomeworkFileTypeName(HomeworkFileType Key)
        {
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)HomeworkFileType.GiveHomework,
                    Value = "Giao bài tập"
                },
                new EnumObject
                {
                    Key = (int)HomeworkFileType.SubmitHomework,
                    Value = "Nộp bài tập"
                },
            };
            return data.FirstOrDefault(x => x.Key == (int)Key)?.Value;
        }
        public static string AttendaceTypeName(int Key)
        {
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)AttendaceType.DauNgay,
                    Value = "Đầu ngày"
                },
                new EnumObject
                {
                    Key = (int)AttendaceType.DauGioHoc,
                    Value = "Đầu giờ học"
                },

                new EnumObject
                {
                    Key = (int)AttendaceType.CuoiGioHoc,
                    Value = "Cuối giờ học"
                },
                new EnumObject
                {
                    Key = (int)AttendaceType.SauBuoiHoc,
                    Value = "Sau buổi học"
                },
                new EnumObject
                {
                    Key = (int)AttendaceType.CuoiNgay,
                    Value = "Cuối ngày"
                },
            };
            return data.FirstOrDefault(x => x.Key == Key)?.Value;
        }
        public static List<EnumObject> ListAttendaceType()
        {
            return new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)AttendaceType.DauNgay,
                    Value = "Đầu ngày"
                },
                new EnumObject
                {
                    Key = (int)AttendaceType.DauGioHoc,
                    Value = "Đầu giờ học"
                },

                new EnumObject
                {
                    Key = (int)AttendaceType.CuoiGioHoc,
                    Value = "Cuối giờ học"
                },
                new EnumObject
                {
                    Key = (int)AttendaceType.SauBuoiHoc,
                    Value = "Sau buổi học"
                },
                new EnumObject
                {
                    Key = (int)AttendaceType.CuoiNgay,
                    Value = "Cuối ngày"
                },
            };
        }
        public static List<EnumObject> ListTypeQuestionEnum()
        {
            return new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)TypeQuestionEnum.TracNghiem,
                    Value = "Trắc Nghiệm"
                },
                 new EnumObject
                {
                    Key = (int)TypeQuestionEnum.ChonTuVaoOTong,
                    Value = "Chọn Từ Vào Ô Trống"
                },
                 new EnumObject
                {
                    Key = (int)TypeQuestionEnum.DienVaoOTrong,
                    Value = "Điền Vào Ô Trống"
                },
                new EnumObject
                {
                    Key = (int)TypeQuestionEnum.GhepDapAn,
                    Value = "Mindmap"
                },
                new EnumObject
                {
                    Key = (int)TypeQuestionEnum.ChonDungSai,
                    Value = "True/False/Not Given"
                },
                new EnumObject
                {
                    Key = (int)TypeQuestionEnum.SapXep,
                    Value = "Sắp Xếp"
                },
                new EnumObject
                {
                    Key = (int)TypeQuestionEnum.Viet,
                    Value = "Viết"
                },
                new EnumObject
                {
                    Key = (int)TypeQuestionEnum.Noi,
                    Value = "Nói"
                }
            };
        }
        public static List<EnumObject> ListLevelQuestionEnum()
        {
            return new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)LevelQuestionEnum.De,
                    Value = "Dễ"
                },
                new EnumObject
                {
                    Key = (int)LevelQuestionEnum.TrungBinh,
                    Value = "Trung Bình"
                },
                new EnumObject
                {
                    Key = (int)LevelQuestionEnum.Kho,
                    Value = "Khó"
                }
            };
        }
        public enum FeedbackStatus
        {
            MoiGui = 1,
            DangXuLy,
            DaXong
        }
        public enum AnswerType
        {
            Text,
            Image,
            Audio
        }
        public enum TagCategoryType
        {
            Video = 1,
        }
        public enum LessonFileType
        {
            LinkYoutube = 1,
            FileUpload
        }
        public enum AnswerEssay
        {
            Exist = 1,
            NotExist
        }
        public enum ExerciseLevel
        {
            Easy = 1,
            Normal,
            Difficult
        }
        public enum ExerciseType
        {
            MultipleChoice = 1,
            DragDrop,
            FillInTheBlank,
            Mindmap,
            TrueOrFalse,
            Sort,
            Write,
            Speak,
        }
        public enum SeminarStatus
        {
            ChuaDienRa = 1,
            DangDienRa,
            KetThuc
        }

        public enum JobRoleEnum
        {
            teacherIelts = 1,
            teacherToeic,
            manager,
            sale,
            accountant,
            academic
        }

        public enum RoleEnum
        {
            admin = 1,
            teacher,
            student,
            manager,
            sale,
            accountant,
            academic,
            parents,
            tutor,
        }
        public enum AccountStatus
        {
            active,
            inActive
        }
        public enum GenderEnum
        {
            nu = 0,
            nam = 1,
            khac = 2
        }
        public enum AllowRegister
        {
            Allow,
            UnAllow
        }
        public static string GetAllowRegister(int? value)
        {
            var data = new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)AllowRegister.Allow,
                    Value = "Allow"
                },
                new EnumObject
                {
                    Key = (int)AllowRegister.UnAllow,
                    Value = "UnAllow"
                }
            };
            return data.FirstOrDefault(x => x.Key == value)?.Value;
        }
        public enum ChangeInfoStatus
        {
            Approve,
            Cancel
        }
        public enum LessonType
        {
            Video = 1,
            BaiTap
        }

        public enum StudyRouteStatus
        {
            ChuaHoc = 1,
            DangHoc = 2,
            HoanThanh = 3
        }

        public enum StatisticalOverviewGroups
        {
            KhachHang = 1,//liên quan tới khách hàng
            HocTap = 2,// liên quan tới học viên, buổi học
            DoanhThu = 3,// liên quan tới thu chi
            GiangDay = 4// liên quan tới lớp học, buổi dạy
        }

        public enum StudentHomeworkStatus
        {
            ChuaLam = 1,//chưa làm
            DangLam = 2,//đang làm
            DaNop = 3,//đã nộp (nộp đúng giờ)
            KhongNop = 4,//không nộp
            // Không còn làm bài tập khi đã quá hạn nữa nên sẽ không còn nôp trễ
            //NopTre = 5,//nộp trễ

        }
        public enum OTPStatus
        {
            DaXacNhan = 1,//chưa làm
            ChuaXacNhan = 0,//đang làm

        }

        public enum ZnsTemplateType
        {
            ThongBaoHocPhi = 1,//thong báo học phí
            ThongBaoBaiKiemTra = 2,//thông báo bài kiểm
        }

        public enum CommissionConfigStatus
        {
            ChuaChot = 0,//chưa làm
            DaChot = 1,//đang làm

        }
        public enum UploadType
        {
            Document = 1,
            Image,
            Audio,
        }
        public static List<EnumObject> GetRoleName()
        {
            return new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)RoleEnum.admin,
                    Value = "Admin"
                },
                new EnumObject
                {
                    Key = (int)RoleEnum.teacher,
                    Value = "Giáo viên"
                },
                new EnumObject
                {
                    Key = (int)RoleEnum.student,
                    Value = "Học sinh"
                },
                new EnumObject
                {
                    Key = (int)RoleEnum.manager,
                    Value = "Quản lý"
                },
                new EnumObject
                {
                    Key = (int)RoleEnum.sale,
                    Value = "Tư vấn viên"
                },
                new EnumObject
                {
                    Key = (int)RoleEnum.accountant,
                    Value = "Kế toán"
                },
                new EnumObject
                {
                    Key = (int)RoleEnum.academic,
                    Value = "Học vụ"
                },
                new EnumObject
                {
                    Key = (int)RoleEnum.parents,
                    Value = "Phụ huynh"
                },
                new EnumObject
                {
                    Key = (int)RoleEnum.tutor,
                    Value = "Trợ giảng"
                },
            };
        }
        public static List<EnumObject> ListStudyRouteStatus()
        {
            return new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)StudyRouteStatus.ChuaHoc,
                    Value = "Chưa học"
                },
                new EnumObject
                {
                    Key = (int)StudyRouteStatus.DangHoc,
                    Value = "Đang học"
                },
                 new EnumObject
                {
                    Key = (int)StudyRouteStatus.HoanThanh,
                    Value = "Hoàn Thành"
                },
            };
        }
        public static List<EnumObject> ListStudentHomeworkStatus()
        {
            return new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)StudentHomeworkStatus.ChuaLam,
                    Value = "Chưa làm"
                },
                new EnumObject
                {
                    Key = (int)StudentHomeworkStatus.DangLam,
                    Value = "Đang làm"
                },
                 new EnumObject
                {
                    Key = (int)StudentHomeworkStatus.DaNop,
                    Value = "Đã nộp"
                },
                 new EnumObject
                {
                    Key = (int)StudentHomeworkStatus.KhongNop,
                    Value = "Không nộp"
                },
            };
        }
        public static List<EnumObject> ListStatisticalOverviewGroups()
        {
            return new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)StatisticalOverviewGroups.KhachHang,
                    Value = "Khách hàng"
                },
                new EnumObject
                {
                    Key = (int)StatisticalOverviewGroups.HocTap,
                    Value = "Học tập"
                },
                 new EnumObject
                {
                    Key = (int)StatisticalOverviewGroups.DoanhThu,
                    Value = "Lợi nhuận"
                },
                 new EnumObject
                {
                    Key = (int)StatisticalOverviewGroups.GiangDay,
                    Value = "Giảng dạy"
                },
            };
        }

        public static List<EnumObject> ListZnsTemplateType()
        {
            return new List<EnumObject> {
                new EnumObject
                {
                    Key = (int)ZnsTemplateType.ThongBaoHocPhi,
                    Value = "Thông báo học phí"
                },
                new EnumObject
                {
                    Key = (int)ZnsTemplateType.ThongBaoBaiKiemTra,
                    Value = "Thông báo bài kiểm tra"
                },
            };
        }
    }

    public class EnumObject
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

}