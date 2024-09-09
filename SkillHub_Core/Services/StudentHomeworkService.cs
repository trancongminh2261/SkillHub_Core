using LMSCore.Areas.Models;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class StudentHomeworkService : DomainService
    {
        public StudentHomeworkService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_StudentHomework> GetById(int id)
        {
            var data = await dbContext.tbl_StudentHomework.SingleOrDefaultAsync(x => x.Id == id);
            return data;
        }
        public async Task<List<Get_StudentHomework>> GetAll(StudentHomeworkSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new StudentHomeworkSearch();
           
            string sql = $"Get_StudentHomework @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," + $"@PageSize = {baseSearch.PageSize}," +
                $"@HomeworkId = {baseSearch.HomeworkId ?? 0}," +
                $"@StudentIds = N'{baseSearch.StudentIds ?? ""}'," +             
                $"@Statuses = N'{baseSearch.Statuses ?? ""}'," +
                $"@ClassId = '{baseSearch.ClassId ?? 0}'";
            var data = await dbContext.SqlQuery<Get_StudentHomework>(sql);
            return data;
        }
        public static async Task UpdateStudentStatus()
        {
            using (var db = new lmsDbContext())
            {

                List<int> listHomework = await db.tbl_Homework.Where(x => x.Enable == true && x.ToDate <= DateTime.Now).Select(x => x.Id).ToListAsync();
                if (!listHomework.Any())
                    return;
                List<tbl_StudentHomework> listStudentHomework = await db.tbl_StudentHomework.Where(x => x.Enable == true && x.Status == (int)StudentHomeworkStatus.DangLam && listHomework.Contains(x.HomeworkId)).ToListAsync();
                if (listStudentHomework.Any())
                {
                    foreach (var sh in listStudentHomework)
                    {
                        int timeSpent = (int)DateTime.Now.Subtract(sh.FromDate.Value).TotalMinutes;
                        if (timeSpent > sh.Time)
                        {
                            sh.Status = (int)StudentHomeworkStatus.KhongNop;
                            sh.StatusName = ListStudentHomeworkStatus().SingleOrDefault(x => x.Key == sh.Status)?.Value;
                        }
                    }
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}