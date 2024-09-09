using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class PackageStudentService
    {
        public static async Task<tbl_PackageStudent> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_PackageStudent.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_PackageStudent> Insert(PackageStudentCreate itemModel, tbl_UserInformation userLog)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var package = await db.tbl_Product.FirstOrDefaultAsync(x => x.Id == itemModel.PackageId && x.Type == 2 && x.Enable == true);
                if (package == null)
                    throw new Exception("Không tìm thấy bộ đề");

                var checkCart = await db.tbl_Cart.AnyAsync(x => x.UserId == itemModel.StudentId && x.ProductId == itemModel.PackageId && x.Enable == true);
                if (checkCart)
                    throw new Exception("Học viện đã có bộ đề này trong giỏ hàng");

                var checkPackageStudent = await db.tbl_PackageStudent
                    .AnyAsync(x => x.StudentId == itemModel.StudentId && x.PackageId == itemModel.PackageId && x.Enable == true);
                if (checkPackageStudent)
                    throw new Exception("Học viện đã mua bộ đề này");

                var billDetails = await db.tbl_BillDetail
                    .Where(x => x.ProductId == itemModel.PackageId && x.StudentId == itemModel.StudentId && x.Enable == true).ToListAsync();
                if (billDetails.Any())
                {
                    foreach (var item in billDetails)
                    {
                        var checkBill = await db.tbl_Bill.AnyAsync(x => x.Id == item.Id && x.Debt > 0);
                        if (checkBill)
                            throw new Exception("Học viện đã đặt mua bộ đề này, vui lòng thanh toán để nhận");
                    }
                }

                var model = new tbl_PackageStudent(itemModel);
                model.CreatedBy = model.ModifiedBy = userLog.FullName;
                db.tbl_PackageStudent.Add(model);

                db.tbl_HistoryDonate.Add(new tbl_HistoryDonate
                {
                    CreateById = userLog.UserInformationId,
                    CreatedBy = userLog.FullName,
                    CreatedOn = DateTime.Now,
                    Enable = true,
                    ModifiedBy = userLog.FullName,
                    ModifiedOn = DateTime.Now,
                    Type = 2,
                    TypeName = "Tặng bộ đề",
                    UserId = itemModel.StudentId ?? 0,
                    Note = $"{userLog.FullName} đã tặng bộ đề {package.Name} cho học viên {student.FullName}"
                });

                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_PackageStudent.SingleOrDefaultAsync(x => x.Id == id);
                if (data == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                data.Enable = false;
                var product = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == data.PackageId);
                product.TotalStudent -= 1;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(PackageStudentSearch baseSearch, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PackageStudentSearch();
                if (user.RoleId == ((int)RoleEnum.student))
                    baseSearch.StudentId = user.UserInformationId;
                string sql = $"Get_PackageStudent @Search = N'{baseSearch.Search ?? ""}', @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@Tags = N'{baseSearch.Tags ?? ""}'," +
                    $"@StudentId = {baseSearch.StudentId}," +
                    $"@Sort = {baseSearch.Sort}," +
                    $"@SortType = {(baseSearch.SortType == false ? 0 : 1)}";
                var data = await db.SqlQuery<Get_PackageStudent>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_PackageStudent(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
    }
}