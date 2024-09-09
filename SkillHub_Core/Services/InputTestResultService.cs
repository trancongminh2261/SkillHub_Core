using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Services
{
    public class InputTestResultService
    {
        public static async Task<AppDomainResult> GetAll(InputTestResultSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new InputTestResultSearch();
                string sql = $"Get_InputTestResult " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@FullName = N'{baseSearch.FullName}'," +
                    $"@SortType = {(baseSearch.SortType ? 1 : 0)}";
                var data = await db.SqlQuery<Get_InputTestResult>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_InputTestResult(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<tbl_InputTestResult> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_InputTestResult.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task ValidateCreate(lmsDbContext db, InputTestResultCreate itemModel)
        {
            //Kiểm tra trung tâm
            var interviewAppointment = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.Id == itemModel.InterviewAppointmentId && x.Enable == true);
            if (interviewAppointment == null)
                throw new Exception("Không tìm thấy buổi phỏng vấn");
            if (itemModel.ListeningScore >= 10)
                throw new Exception("Điểm phần nghe không được lớn hơn 10");
            if (itemModel.ReadingScore >= 10)
                throw new Exception("Điểm phần đọc không được lớn hơn 10");
            if (itemModel.SpeakingScore >= 10)
                throw new Exception("Điểm phần nói không được lớn hơn 10");
            if (itemModel.WritingScore >= 10)
                throw new Exception("Điểm phần viết không được lớn hơn 10");
        }

        public static async Task<tbl_InputTestResult> Insert(InputTestResultCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    //Validate input 
                    await ValidateCreate(db, itemModel);
                    var model = new tbl_InputTestResult(itemModel);
                    var interviewAppointment = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.Id == itemModel.InterviewAppointmentId && x.Enable == true);
                    model.CurriculumVitaeId = interviewAppointment.CurriculumVitaeId;
                    model.OrganizerId = interviewAppointment.OrganizerId;
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    db.tbl_InputTestResult.Add(model);
                    await db.SaveChangesAsync();
                    return model;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task ValidateUpdate(lmsDbContext db, InputTestResultUpdate itemModel)
        {
            //check xem có tìm được dữ liệu để update không
            var entity = await db.tbl_InputTestResult.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
            if (entity == null)
                throw new Exception("Không tìm thấy dữ liệu");
            if (itemModel.ListeningScore >= 10 && itemModel.ListeningScore != null)
                throw new Exception("Điểm phần nghe không được lớn hơn 10");
            if (itemModel.ReadingScore >= 10 && itemModel.ReadingScore != null)
                throw new Exception("Điểm phần đọc không được lớn hơn 10");
            if (itemModel.SpeakingScore >= 10 && itemModel.SpeakingScore != null)
                throw new Exception("Điểm phần nói không được lớn hơn 10");
            if (itemModel.WritingScore >= 10 && itemModel.WritingScore != null)
                throw new Exception("Điểm phần viết không được lớn hơn 10");
        }

        public static async Task<tbl_InputTestResult> Update(InputTestResultUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    //validate input
                    await ValidateUpdate(db, itemModel);
                    var entity = await db.tbl_InputTestResult.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value && x.Enable == true);
                    entity.ListeningScore = itemModel.ListeningScore ?? entity.ListeningScore;
                    entity.ReadingScore = itemModel.ReadingScore ?? entity.ReadingScore;
                    entity.WritingScore = itemModel.WritingScore ?? entity.WritingScore;
                    entity.SpeakingScore = itemModel.SpeakingScore ?? entity.SpeakingScore;
                    entity.Type = itemModel.Type ?? entity.Type;
                    entity.TypeName = itemModel.TypeName ?? entity.TypeName;
                    entity.IeltsExamId = itemModel.IeltsExamId ?? entity.IeltsExamId;
                    entity.AttachFile = itemModel.AttachFile ?? entity.AttachFile;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                    return entity;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_InputTestResult.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}