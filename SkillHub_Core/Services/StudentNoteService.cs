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
    public class StudentNoteService
    {
        public static async Task<tbl_StudentNote> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_StudentNote.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_StudentNote> Insert(StudentNoteCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_UserInformation.AnyAsync(x => x.UserInformationId == itemModel.StudentId
                                                                              && x.RoleId == 3
                                                                              && x.Enable==true);
                if (!student)
                    throw new Exception("Không tìm thấy thông tin sinh viên");
                var model = new tbl_StudentNote(itemModel);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_StudentNote.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_StudentNote.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<AppDomainResult> GetAll(StudentNoteSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new StudentNoteSearch();

                var l = await db.tbl_StudentNote.Where(x => x.Enable == true 
                                                         && x.StudentId == baseSearch.StudentId
                                                       ).OrderByDescending(x => x.Id).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = l.Count();
                var result = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }
    }
}