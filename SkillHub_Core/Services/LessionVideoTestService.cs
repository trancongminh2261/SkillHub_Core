using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class LessionVideoTestService : DomainService
    {
        public LessionVideoTestService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_LessionVideoTest> GetById(int id)
        {
            return await dbContext.tbl_LessionVideoTest.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
        }
        //Lấy thông tin bài làm, lấy  các câu đã làm
        public async Task<TestDetail> GetTestDetail(int id)
        {
            var test = await GetById(id);
            if (test == null)
                throw new Exception("Không tìm thấy đề thi");
            TestDetail testDetail = new TestDetail();
            testDetail.TestId = test.Id;
            testDetail.TestName = test.Name;
            List<GetListQuestion> listQuestion = new List<GetListQuestion>();
            var listQuest = await dbContext.tbl_LessionVideoQuestion.Where(x => x.TestId == id && x.Enable == true).ToListAsync();
            foreach(var quest in listQuest)
            {
                GetListQuestion q = new GetListQuestion();
                q.QuestionId = quest.Id;
                q.QuestionName = quest.Name;
                List<GetListOption> listOption = new List<GetListOption>(); 
                var opts = await dbContext.tbl_LessionVideoOption.Where(x => x.QuestionId == quest.Id).ToListAsync();
                foreach(var option in opts)
                {
                    GetListOption o = new GetListOption();
                    o.OptionId = option.Id;
                    o.OptionName = option.Content;
                    o.TrueFalse = option.TrueFalse;
                    listOption.Add(o);
                }
                q.ListOption = listOption;
                listQuestion.Add(q);
            }
            testDetail.ListQuestion = listQuestion;

            return testDetail;
        }
        public class TestDetail
        {
            public int TestId { get; set; }
            public string TestName { get; set; }
            public List<GetListQuestion> ListQuestion { get; set; }
        }
        public class GetListQuestion
        {
            public int QuestionId { get; set; }
            public string QuestionName { get; set; }
            public List<GetListOption> ListOption { get; set; }
        }
        public class GetListOption
        {
            public int OptionId { get; set; }
            public string OptionName { get; set; } 
            public bool? TrueFalse { get; set; }
        }
        public async Task<AppDomainResult> GetAll(LessionVideoTestSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new LessionVideoTestSearch();
            string sql = $"Get_LessionVideoTest @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@SectionId = {baseSearch.SectionId}," +
                $"@Search = N'{baseSearch.Search ?? ""}'";
            var data = await dbContext.SqlQuery<Get_LessionVideoTest>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }
        public async Task<tbl_LessionVideoTest> Insert(LessionVideoTestCreate itemModel, tbl_UserInformation userLog)
        {
            try
            {
                var model = new tbl_LessionVideoTest(itemModel);
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                dbContext.tbl_LessionVideoTest.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<tbl_LessionVideoTest> Update(LessionVideoTestUpdate itemModel, tbl_UserInformation user)
        {
            try
            {
                var entity = await dbContext.tbl_LessionVideoTest.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy bài tập video");
                entity.Name = itemModel.Name ?? entity.Name;

                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await dbContext.SaveChangesAsync();
                return entity;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task Delete(int id)
        {
            try
            {
                var entity = await dbContext.tbl_LessionVideoTest.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy bài tập video");
                entity.Enable = false;
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}