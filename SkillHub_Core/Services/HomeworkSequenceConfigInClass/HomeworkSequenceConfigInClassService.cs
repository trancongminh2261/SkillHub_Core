using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.HomeworkSequenceConfigDTO;
using LMSCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services.HomeworkConfigInClass
{
    public class HomeworkSequenceConfigInClassService : DomainService
    {
        private IConfiguration configuration = new ConfigurationBuilder()
                           .AddJsonFile("appsettings.json")
                           .Build();
        private readonly IWebHostEnvironment _hostingEnvironment;
        public HomeworkSequenceConfigInClassService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<GetHomeworkSequenceConfigInClassDTO> GetHomeworkSequenceConfigInClass(int id)
        {
            var _class = await dbContext.tbl_Class.Where(x => x.Id == id && x.Enable == true).Select(x => new {x.Id, x.Name}).SingleOrDefaultAsync();
            if (_class == null)
                throw new Exception("Không tìm thấy thông tin lớp học");
            var homeworkSequence = await dbContext.tbl_HomeworkSequenceConfigInClass.Where(x => x.Enable == true && x.ClassId == id)
                .Select(x => new {x.IsAllow})
                .FirstOrDefaultAsync();
            var model = new GetHomeworkSequenceConfigInClassDTO
            {
                ClassId = id,
                ClassName = _class.Name,
                IsAllow = homeworkSequence == null ? false : homeworkSequence.IsAllow,
            };
            return model;
        }

        public async Task<tbl_HomeworkSequenceConfigInClass> AllowHomeworkSequenceInClass(AllowHomeworkSequenceInClassUpdate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var _class = await dbContext.tbl_Class.Where(x => x.Id == itemModel.Id && x.Enable == true).Select(x => new { x.Id, x.Name }).FirstOrDefaultAsync();
                if (_class == null)
                    throw new Exception("Không tìm thấy thông tin lớp học");
                var homeworkSequence = await dbContext.tbl_HomeworkSequenceConfigInClass.FirstOrDefaultAsync(x => x.Enable == true && x.ClassId == itemModel.Id);
                if (homeworkSequence == null)
                {
                    var model = new tbl_HomeworkSequenceConfigInClass
                    {
                        ClassId = itemModel.Id,
                        IsAllow = itemModel.IsAllow ?? false,
                        Enable = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = userLog.FullName,
                        ModifiedOn = DateTime.Now,
                        ModifiedBy = userLog.FullName,
                    };
                    await dbContext.tbl_HomeworkSequenceConfigInClass.AddAsync(model);
                    await dbContext.SaveChangesAsync();
                    return model;
                }
                else
                {
                    homeworkSequence.IsAllow = itemModel.IsAllow ?? homeworkSequence.IsAllow;
                    homeworkSequence.ModifiedBy = userLog.FullName;
                    homeworkSequence.ModifiedOn = DateTime.Now;
                    await dbContext.SaveChangesAsync();
                    return homeworkSequence;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
