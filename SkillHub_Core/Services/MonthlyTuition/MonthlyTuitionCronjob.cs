using LMSCore.Areas.Request;
using LMSCore.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static LMSCore.Services.MonthlyTuitionService;

namespace LMSCore.Services.MonthlyTuition
{
    public class MonthlyTuitionCronjob
    {
        /// <summary>
        /// Tư động phát sinh học phí hằng tháng
        /// </summary>
        /// <returns></returns>
        public static async Task CreateTuition()
        {
            using (var db = new lmsDbContext())
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var date = DateTime.Now;
                        var classes = await db.tbl_Class.Where(x => x.Enable == true && x.PaymentType == 2 && x.EndDay > date)
                            .Select(x => new { x.Id, x.Price, x.BranchId, x.Name }).ToListAsync();
                        if (!classes.Any())
                            return;
                        foreach (var _class in classes)
                        {
                            var studentIds = await db.tbl_StudentInClass
                                .Where(x => x.ClassId == _class.Id && x.Enable == true && x.Type == 1 && x.StudentId.HasValue)
                                .Select(x => x.StudentId).ToListAsync();
                            if (!studentIds.Any())
                                continue;
                            foreach (var studentId in studentIds)
                            {
                                MonthlyTuitionService monthlyTuitionService = new MonthlyTuitionService(db);
                                await monthlyTuitionService.AddItem(new AddItemModel
                                {
                                    StudentId = studentId.Value,
                                    Class = new tbl_Class
                                    {
                                        Id = _class.Id,
                                        Price = _class.Price,
                                        BranchId = _class.BranchId,
                                        Name = _class.Name
                                    },
                                    CreateBy = "Tự động",
                                    Date = date
                                });
                            }
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        return;
                    }
                }
            }
        }
    }
}
