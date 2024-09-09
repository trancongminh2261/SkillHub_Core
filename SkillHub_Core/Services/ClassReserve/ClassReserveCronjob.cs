using LMSCore.Areas.Request;
using LMSCore.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LMSCore.Services.ClassReserve
{
    public class ClassReserveCronjob
    {
        /// <summary>
        /// Cập nhật trạng thái bảo lưu
        /// </summary>
        /// <returns></returns>
        public static async Task UpdateStatus()
        {
            using (var dbContext = new lmsDbContext())
            {
                var timenow = DateTime.Now;
                var classReserves = await dbContext.tbl_ClassReserve.Where(x => x.Status == 1 && x.Enable == true && timenow > x.Expires).ToListAsync();
                if (classReserves.Any())
                {
                    foreach (var item in classReserves)
                    {
                        item.Status = 4;
                        item.StatusName = "Hết hạn bảo lưu";

                        var learningHistoryService = new LearningHistoryService(dbContext);
                        await learningHistoryService.Insert(new LearningHistoryCreate
                        {
                            StudentId = item.StudentId,
                            Content = $"Hết hạn bảo lưu"
                        });
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
