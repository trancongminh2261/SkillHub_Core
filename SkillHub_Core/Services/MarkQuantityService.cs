using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class MarkQuantityService
    {
        public static async Task<tbl_MarkQuantity> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_MarkQuantity.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public class DonateMarkQuantity
        {
            [Required(ErrorMessage = "Vui lòng chọn học viên")]
            public int? StudentId { get; set; }
            [Required(ErrorMessage = "Vui lòng nhập số lượng")]
            public int? Quantity { get; set; }
        }
        public static async Task Donate(DonateMarkQuantity itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId && x.RoleId == ((int)lmsEnum.RoleEnum.student) && x.Enable == true);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var markQuantity = await db.tbl_MarkQuantity.FirstOrDefaultAsync(x => x.StudentId == itemModel.StudentId && x.Enable == true);
                if (markQuantity == null)
                {
                    markQuantity = new tbl_MarkQuantity
                    {
                        CreatedBy = userLog.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = userLog.FullName,
                        ModifiedOn = DateTime.Now,
                        Quantity = itemModel.Quantity ?? 0,
                        StudentId = itemModel.StudentId ?? 0,
                        UsedQuantity = 0,
                    };
                    db.tbl_MarkQuantity.Add(markQuantity);
                }
                else
                {
                    markQuantity.Quantity += itemModel.Quantity ?? 0;
                    markQuantity.ModifiedBy = userLog.FullName;
                    markQuantity.ModifiedOn = DateTime.Now;
                }

                db.tbl_HistoryDonate.Add(new tbl_HistoryDonate
                {
                    CreateById = userLog.UserInformationId,
                    CreatedBy = userLog.FullName,
                    CreatedOn = DateTime.Now,
                    Enable = true,
                    ModifiedBy = userLog.FullName,
                    ModifiedOn = DateTime.Now,
                    Type = 3,
                    TypeName = "Tặng lượt chấm bài",
                    UserId = itemModel.StudentId ?? 0,
                    Note = $"{userLog.FullName} đã tặng {markQuantity.Quantity} lượt chấm cho học viên {student.FullName}"
                });

                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(MarkQuantitySearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new MarkQuantitySearch();
                string sql = $"Get_MarkQuantity @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName}'," +
                    $"@UserCode = N'{baseSearch.UserCode}'";
                var data = await db.SqlQuery<Get_MarkQuantity>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_MarkQuantity(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}