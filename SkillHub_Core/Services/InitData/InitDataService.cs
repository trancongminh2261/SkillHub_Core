using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.Class.ClassService;

namespace LMSCore.Services.InitData
{
    public class InitDataService : DomainService
    {
        public InitDataService(lmsDbContext dbContext) : base(dbContext) { }
        public async Task<int> GetIdByQuery(string query)
        {
            var data = await dbContext.SqlQuery<int>("Select Id from tbl_Purpose where Name = N'Du học'");
            return data.FirstOrDefault();
        }

        public async Task ExecuteQuery(string tableName, string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                using (var tran = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Câu lệnh INSERT đã được truyền vào thông qua tham số query
                        await dbContext.Database.ExecuteSqlRawAsync(query);
                        await tran.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tran.RollbackAsync();
                        throw new Exception($"Đã có lỗi xảy ra khi thêm dữ liệu vào bảng {tableName}: {ex.Message}", ex);
                    }
                }
            }            
        }

        public class InitForeign
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class InitUserForeign
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
        }
        public async Task<List<InitUserForeign>> GetCustomer()
        {
            return await dbContext.tbl_Customer.Where(x => x.Enable == true).Select(x => new InitUserForeign
            {
                Id = x.Id,
                Name = x.FullName,
                Code = x.Code,
                Mobile = x.Mobile,
                Email = x.Email
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetTuitionPackage()
        {
            return await dbContext.tbl_TuitionPackage.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Code = x.Code,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetStudyTime()
        {
            return await dbContext.tbl_StudyTime.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitUserForeign>> GetParent()
        {
            return await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.parents).Select(x => new InitUserForeign
            {
                Id = x.UserInformationId,
                Code = x.UserCode,
                Name = x.FullName,
                Mobile = x.Mobile,
                Email = x.Email
            }).ToListAsync();
        }
        public async Task<List<InitUserForeign>> GetStudent()
        {
            return await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId == (int)RoleEnum.student).Select(x => new InitUserForeign
            {
                Id = x.UserInformationId,
                Code = x.UserCode,
                Name = x.FullName,
                Mobile = x.Mobile,
                Email = x.Email
            }).ToListAsync();
        }
        public async Task<List<InitUserForeign>> GetStaff()
        {
            return await dbContext.tbl_UserInformation.Where(x => x.Enable == true && x.StatusId == 0 && x.RoleId != (int)RoleEnum.student && x.RoleId != (int)RoleEnum.parents).Select(x => new InitUserForeign
            {
                Id = x.UserInformationId,
                Code = x.UserCode,
                Name = x.FullName,
                Mobile = x.Mobile,
                Email = x.Email
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetCurriculum()
        {
            return await dbContext.tbl_Curriculum.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetProgram()
        {
            return await dbContext.tbl_Program.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetGrade()
        {
            return await dbContext.tbl_Grade.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetCustomerStatus()
        {
            return await dbContext.tbl_CustomerStatus.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetJob()
        {
            return await dbContext.tbl_Job.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetPurpose()
        {
            return await dbContext.tbl_Purpose.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetSource()
        {
            return await dbContext.tbl_Source.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetLearningNeed()
        {
            return await dbContext.tbl_LearningNeed.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetRoom()
        {
            return await dbContext.tbl_Room.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetBranch()
        {
            return await dbContext.tbl_Branch.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetArea()
        {
            return await dbContext.tbl_Area.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }
        public async Task<List<InitForeign>> GetDistrict()
        {
            return await dbContext.tbl_District.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }

        public async Task<List<InitForeign>> GetWard()
        {
            return await dbContext.tbl_Ward.Where(x => x.Enable == true).Select(x => new InitForeign
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
        }

        public async Task<string> GenColorCode()
        {
            // Khởi tạo đối tượng Random
            var random = new Random();

            // Sinh ra 3 giá trị ngẫu nhiên cho các thành phần màu đỏ, xanh lá và xanh dương
            int r = random.Next(0, 256);
            int g = random.Next(0, 256);
            int b = random.Next(0, 256);

            // Chuyển đổi các giá trị màu thành chuỗi dạng HEX
            string colorCode = $"#{r:X2}{g:X2}{b:X2}";

            // Trả về mã màu
            return await Task.FromResult(colorCode);
        }

        public async Task<string> ProgramNewIndex(int gradeId)
        {
            var lastIndex = await dbContext.tbl_Program
                .Where(x => x.GradeId == gradeId && x.Enable == true)
                .OrderByDescending(x => x.Index)
                .FirstOrDefaultAsync();
            if (lastIndex == null)
                return "1";
            return $"{lastIndex.Index + 1}";
        }

        public async Task<bool> ValidBranch(string code)
        {
            var checkCode = await dbContext.tbl_Branch.AnyAsync(x => x.Enable == true && x.Code.ToLower() == code.ToLower());
            if (checkCode)
                return false;
            return true;
        }

        public async Task<bool> ValidRoom(int branchId, string code)
        {
            var checkCode = await dbContext.tbl_Room.AnyAsync(x => x.Enable == true && x.BranchId == branchId && x.Code.ToLower() == code.ToLower());
            if (checkCode)
                return false;
            return true;
        }

        public async Task<bool> ValidFrequentlyQuestion(string question)
        {
            var checkQuestion = await dbContext.tbl_FrequentlyQuestion.AnyAsync(x => x.Enable == true && x.Question.ToLower() == question.ToLower());
            if (checkQuestion)
                return false;
            return true;
        }

        public async Task<bool> ValidLearningNeed(string name)
        {
            var checkName = await dbContext.tbl_LearningNeed.AnyAsync(x => x.Enable == true && x.Name.ToLower() == name.ToLower());
            if (checkName)
                return false;
            return true;
        }

        public async Task<bool> ValidSource(string name)
        {
            var checkName = await dbContext.tbl_Source.AnyAsync(x => x.Enable == true && x.Name.ToLower() == name.ToLower());
            if (checkName)
                return false;
            return true;
        }

        public async Task<bool> ValidJob(string name)
        {
            var checkName = await dbContext.tbl_Job.AnyAsync(x => x.Enable == true && x.Name.ToLower() == name.ToLower());
            if (checkName)
                return false;
            return true;
        }

        public async Task<bool> ValidCustomerStatus(string name)
        {
            var checkName = await dbContext.tbl_CustomerStatus.AnyAsync(x => x.Enable == true && x.Name.ToLower() == name.ToLower());
            if (checkName)
                return false;
            return true;
        }

        public async Task<bool> ValidPurpose(string name)
        {
            var checkName = await dbContext.tbl_Purpose.AnyAsync(x => x.Enable == true && x.Name.ToLower() == name.ToLower());
            if (checkName)
                return false;
            return true;
        }

        public async Task<bool> ValidGrade(string code)
        {
            var checkCode = await dbContext.tbl_Grade.AnyAsync(x => x.Enable == true && x.Code.ToLower() == code.ToLower());
            if (checkCode)
                return false;
            return true;
        }

        public async Task<bool> ValidProgram(int gradeId, string code)
        {
            var checkCode = await dbContext.tbl_Program.AnyAsync(x => x.Enable == true && x.GradeId == gradeId && x.Code.ToLower() == code.ToLower());
            if (checkCode)
                return false;
            return true;
        }

        public async Task<bool> ValidCurriculum(int programId, string name)
        {
            var checkName = await dbContext.tbl_Curriculum.AnyAsync(x => x.Enable == true && x.ProgramId == programId && x.Name.ToLower() == name.ToLower());
            if (checkName)
                return false;
            return true;
        }

        public async Task<bool> ValidUser(string email, string mobile)
        {
            var checkInfor = await dbContext.tbl_UserInformation.AnyAsync(x => x.Enable == true && x.Email.ToLower() == email.ToLower() && x.Mobile == mobile);
            if (checkInfor)
                return false;
            return true;
        }

        public async Task<bool> ValidCustomer(string email, string mobile)
        {
            var checkInfor = await dbContext.tbl_Customer.AnyAsync(x => x.Enable == true && x.Email.ToLower() == email.ToLower() && x.Mobile == mobile);
            if (checkInfor)
                return false;
            return true;
        }

        public async Task<bool> ValidTeacherInProgram(string programCode, string teacherCode)
        {
            var checkCode = await dbContext.tbl_TeacherInProgram.AnyAsync(x => x.Enable == true && dbContext.tbl_UserInformation.Any(y => y.UserCode.ToLower() == teacherCode.ToLower()) && dbContext.tbl_Program.Any(z => z.Code.ToLower() == programCode.ToLower()));
            if (checkCode)
                return false;
            return true;
        }

        public async Task<bool> ValidStudyTime(string startTime, string endTime)
        {
            var checkTime = await dbContext.tbl_StudyTime.AnyAsync(x => x.Enable == true && x.StartTime == startTime && x.EndTime == endTime);
            if (checkTime)
                return false;
            return true;
        }

        public async Task<bool> ValidTuitionPackage(string code)
        {
            var checkCode = await dbContext.tbl_TuitionPackage.AnyAsync(x => x.Enable == true && x.Code.ToLower() == code.ToLower());
            if (checkCode)
                return false;
            return true;
        }

        public async Task<bool> ValidDiscount(string code)
        {
            var checkCode = await dbContext.tbl_Discount.AnyAsync(x => x.Enable == true && x.Code.ToLower() == code.ToLower());
            if (checkCode)
                return false;
            return true;
        }
    }
}
