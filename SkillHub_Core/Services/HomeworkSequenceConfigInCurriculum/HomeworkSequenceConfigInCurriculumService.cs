using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.HomeworkSequenceConfigDTO;
using LMSCore.DTO.HomeworkSequenceConfigInCurriculumDTO;
using LMSCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services.HomeworkConfigInClass
{
    public class HomeworkSequenceConfigInCurriculumService : DomainService
    {
        private IConfiguration configuration = new ConfigurationBuilder()
                           .AddJsonFile("appsettings.json")
                           .Build();
        private readonly IWebHostEnvironment _hostingEnvironment;
        public HomeworkSequenceConfigInCurriculumService(lmsDbContext dbContext, IWebHostEnvironment hostingEnvironment) : base(dbContext)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<GetHomeworkSequenceConfigInCurriculumDTO> GetHomeworkSequenceConfigInCurriculum(int id)
        {
            var curriculum = await dbContext.tbl_Curriculum.Where(x => x.Id == id && x.Enable == true).Select(x => new {x.Id, x.Name}).FirstOrDefaultAsync();
            if (curriculum == null)
                throw new Exception("Không tìm thấy thông tin giáo trình");
            var homeworkSequence = await dbContext.tbl_HomeworkSequenceConfigInCurriculum.Where(x => x.Enable == true && x.CurriculumId == id)
                .Select(x => new {x.IsAllow})
                .FirstOrDefaultAsync();
            var model = new GetHomeworkSequenceConfigInCurriculumDTO
            {
                CurriculumId = id,
                CurriculumName = curriculum.Name,
                IsAllow = homeworkSequence == null ? false : homeworkSequence.IsAllow,
            };
            return model;
        }

        public async Task<tbl_HomeworkSequenceConfigInCurriculum> AllowHomeworkSequenceInCurriculum(AllowHomeworkSequenceInCurriculumUpdate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var curriculum = await dbContext.tbl_Curriculum.Where(x => x.Id == itemModel.Id && x.Enable == true).Select(x => new { x.Id, x.Name }).FirstOrDefaultAsync();
                if (curriculum == null)
                    throw new Exception("Không tìm thấy thông tin giáo trình");
                var homeworkSequence = await dbContext.tbl_HomeworkSequenceConfigInCurriculum.FirstOrDefaultAsync(x => x.Enable == true && x.CurriculumId == itemModel.Id);
                if (homeworkSequence == null)
                {
                    var model = new tbl_HomeworkSequenceConfigInCurriculum
                    {
                        CurriculumId = itemModel.Id,
                        IsAllow = itemModel.IsAllow ?? false,
                        Enable = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = userLog.FullName,
                        ModifiedOn = DateTime.Now,
                        ModifiedBy = userLog.FullName,
                    };
                    await dbContext.tbl_HomeworkSequenceConfigInCurriculum.AddAsync(model);
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
