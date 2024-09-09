using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;


using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Services.TemplateService;

namespace LMSCore.Services
{
    public class InterviewAppointmentService
    {
        public static async Task<AppDomainResult> GetAll(InterviewAppointmentSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new InterviewAppointmentSearch();
                string sql = $"Get_InterviewAppointment " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@CurriculumVitaeName = N'{baseSearch.CurriculumVitaeName ?? ""}'," +
                    $"@BranchId = {baseSearch.BranchId ?? 0}," +
                    $"@SortType = {(baseSearch.SortType ? 1 : 0)}";
                var data = await db.SqlQuery<Get_InterviewAppointment>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_InterviewAppointment(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<tbl_InterviewAppointment> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                return data;
            }
        }
        public static async Task ValidateCreate(lmsDbContext db, InterviewAppointmentCreate itemModel)
        {
            var CurriculumVitae = await db.tbl_CurriculumVitae.FirstOrDefaultAsync(x => x.Id == itemModel.CurriculumVitaeId && x.Enable == true);
            if (CurriculumVitae.Status == 2)
                throw new Exception("Ứng viên đã có lịch phỏng vấn");
            if (CurriculumVitae.Status == 3)
                throw new Exception("Ứng viên đã tham gia phỏng vấn");
            var curriculumVitae = await db.tbl_CurriculumVitae.AnyAsync(x => x.Id == itemModel.CurriculumVitaeId && x.Enable == true);
            if (!curriculumVitae)
                throw new Exception("Không tìm thấy ứng viên");
            var User = await db.tbl_UserInformation.AnyAsync(x => x.Enable == true && x.UserInformationId == itemModel.OrganizerId);
            if (!User)
                throw new Exception("Không tìm thấy người tổ chức phỏng vấn");
            var UserRole = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.Enable == true && x.UserInformationId == itemModel.OrganizerId);
            if (UserRole.RoleId != 1 && UserRole.RoleId != 4)
                throw new Exception("Người này không có quyền tổ chức phỏng vấn");
            /*if (itemModel.InterviewDate < DateTime.Now)
                throw new Exception("Lịch phỏng vấn phải lớn hơn hoặc bằng ngày hiện tại");*/
            
        }
        public static async Task<tbl_InterviewAppointment> Insert(InterviewAppointmentCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        //validate input
                        await ValidateCreate(db, itemModel);
                        var curriculumVitae = await db.tbl_CurriculumVitae.FirstOrDefaultAsync(x => x.Id == itemModel.CurriculumVitaeId && x.Enable == true);
                        //tạo lịch phỏng vấn
                        var model = new tbl_InterviewAppointment(itemModel);
                        model.BranchId = curriculumVitae.BranchId;
                        model.JobPositionId = curriculumVitae.JobPositionId;
                        model.CreatedBy = model.ModifiedBy = user.FullName;
                        db.tbl_InterviewAppointment.Add(model);
                        await db.SaveChangesAsync();
                        //tạo lịch phỏng vấn xong tiến hành update lại trạng thái của hồ sơ thành "đã hẹn lịch phỏng vấn"                      
                        curriculumVitae.Status = 2;
                        curriculumVitae.StatusName = "Đã hẹn phỏng vấn";
                        await db.SaveChangesAsync();
                        // Hoàn thành transaction
                        transaction.Commit();
                        return model;
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi xảy ra, hủy bỏ transaction
                        transaction.Rollback();

                        // Xử lý lỗi hoặc ném lại lỗi để bên ngoài xử lý
                        throw ex;
                    }
                }

            }
        }
        public static async Task ValidateUpdate(lmsDbContext db, InterviewAppointmentUpdate itemModel)
        {

            var entity = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
            if (entity == null)
                throw new Exception("Không tìm thấy dữ liệu");
            var User = await db.tbl_UserInformation.AnyAsync(x => x.Enable == true && x.UserInformationId == itemModel.OrganizerId);
            if (!User && itemModel.OrganizerId != null)
                throw new Exception("Không tìm thấy người tổ chức phỏng vấn");
            var UserRole = await db.tbl_UserInformation.FirstOrDefaultAsync(x => x.Enable == true && x.UserInformationId == itemModel.OrganizerId);
            if (UserRole.RoleId != 1 && UserRole.RoleId != 4)
                throw new Exception("Người này không có quyền tổ chức phỏng vấn");
            /* if (itemModel.InterviewDate < entity.CreatedOn)
                 throw new Exception("Ngày bạn chọn đã qua");
             if(itemModel.WorkStartDate <= DateTime.Now)
                 throw new Exception("Ngày nhận việc phải lớn hơn ngày hiện tại");
             if (itemModel.WorkStartDate <= entity.InterviewDate)
                 throw new Exception("Ngày nhận việc phải lớn hơn ngày phỏng vấn");*/
        }
        public static async Task<tbl_InterviewAppointment> Update(InterviewAppointmentUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    await ValidateUpdate(db, itemModel);
                    var entity = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.OrganizerId = itemModel.OrganizerId ?? entity.OrganizerId;
                    entity.InterviewDate = itemModel.InterviewDate ?? entity.InterviewDate;
                    entity.WorkStartDate = itemModel.WorkStartDate ?? entity.WorkStartDate;
                    entity.Offer = itemModel.Offer ?? entity.Offer;
                    entity.Status = itemModel.Status ?? entity.Status;
                    entity.StatusName = itemModel.StatusName ?? entity.StatusName;
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
                    var entity = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
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
        public static async Task ValidateSendMailInviteInterview(lmsDbContext db, SendMailInviteInterViewCreate itemModel)
        {
            var curriculumVitae = await db.tbl_CurriculumVitae.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumVitaeId && x.Enable == true);
            if (curriculumVitae == null)
                throw new Exception("Không tìm thấy hồ sơ ứng viên");
            var interviewAppointment = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.CurriculumVitaeId == itemModel.CurriculumVitaeId && x.Enable == true);
            if (interviewAppointment == null)
                throw new Exception("Ứng viên này chưa có lịch phỏng vấn");
            //Kiểm tra tỉnh thành phố
            var area = await db.tbl_Area.AnyAsync(x => x.Id == itemModel.AreaId && x.Enable == true);
            if (!area)
                throw new Exception("Không tìm thấy tỉnh / thành phố");
            //Kiểm tra quận huyện
            var district = await db.tbl_District.AnyAsync(x => x.Id == itemModel.DistrictId && x.Enable == true && x.AreaId == itemModel.AreaId);
            if (!district)
                throw new Exception("Quận / huyện này không thuộc tỉnh / thành phố bạn chọn");
            //Kiểm tra phường xã
            var ward = await db.tbl_Ward.AnyAsync(x => x.Id == itemModel.WardId && x.Enable == true && x.DistrictId == itemModel.DistrictId);
            if (!ward)
                throw new Exception("Phường / xã này không thuộc quận / huyện mà bạn chọn");
        }
        public static async Task SendMailInviteInterview(SendMailInviteInterViewCreate itemModel)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    await ValidateSendMailInviteInterview(db, itemModel);
                    var curriculumVitae = await db.tbl_CurriculumVitae.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumVitaeId && x.Enable == true);
                    var interviewAppointment = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.CurriculumVitaeId == itemModel.CurriculumVitaeId && x.Enable == true);
                    if(interviewAppointment.Status == 2 || interviewAppointment.Status == 3)
                    {
                        throw new Exception("Ứng viên đã tham gia phỏng vấn");
                    }
                    string result = "";
                    var template = await db.tbl_Template.FirstOrDefaultAsync(x => x.Type == 5 && x.Enable == true);
                    if (template != null)
                        result = template.Content;
                    var ward = await db.tbl_Ward.FirstOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.WardId);
                    var district = await db.tbl_District.FirstOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.DistrictId);
                    var area = await db.tbl_Area.FirstOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.AreaId);
                    var jobPosition = await db.tbl_JobPosition.FirstOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.JobPositionId);
                    result = result.Replace("{HoVaTen}", curriculumVitae.FullName);
                    result = result.Replace("{ViTriUngTuyen}", jobPosition.Name);
                    result = result.Replace("{ThoiGian}", interviewAppointment.InterviewDate.ToString("HH:mm"));
                    result = result.Replace("{NgayThangNam}", interviewAppointment.InterviewDate.ToString("dd/MM/yyyy"));
                    result = result.Replace("{DiaChi}", itemModel.Address);
                    result = result.Replace("{PhuongXa}", ward.Name);
                    result = result.Replace("{QuanHuyen}", district.Name);
                    result = result.Replace("{TinhThanhPho}", area.Name);
                    result = result.Replace("{TenLienHe}", itemModel.ContractName);
                    result = result.Replace("{DienThoaiLienHe}", itemModel.ContractPhone);

                    AssetCRM.SendMail(curriculumVitae.Email, template.TypeName, result);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public static async Task ValidateSendMailInterviewResult(lmsDbContext db, SendMailInterviewResultCreate itemModel)
        {
            var curriculumVitae = await db.tbl_CurriculumVitae.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumVitaeId && x.Enable == true);
            if (curriculumVitae == null)
                throw new Exception("Không tìm thấy hồ sơ ứng viên");
            var interviewAppointment = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.CurriculumVitaeId == itemModel.CurriculumVitaeId && x.Enable == true);
            
            if(interviewAppointment.WorkStartDate == null)
                throw new Exception("Vui lòng cập nhật thông tin về buổi phỏng vấn");
            
           /* if (itemModel.StartTime >= itemModel.EndTime)
                throw new Exception("Thời gian bắt đầu không thể lớn hơn thời gian kết thúc");*/
        }
        public static async Task SendMailInterviewResult(SendMailInterviewResultCreate itemModel)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    //lấy thông tin hồ sơ ứng viên
                    var curriculumVitae = await db.tbl_CurriculumVitae.SingleOrDefaultAsync(x => x.Id == itemModel.CurriculumVitaeId && x.Enable == true);
                    //lấy thông tin trung tâm
                    var branch = await db.tbl_Branch.SingleOrDefaultAsync(x => x.Id == curriculumVitae.BranchId && x.Enable == true);
                    //lấy thông tin vị trí tuyển dụng
                    var jobPosition = await db.tbl_JobPosition.FirstOrDefaultAsync(x => x.Enable == true && x.Id == itemModel.JobPositionId);
                    string title = "";
                    string result = "";
                    //lấy thông tin buổi phỏng vấn
                    var interviewAppointment = await db.tbl_InterviewAppointment.SingleOrDefaultAsync(x => x.CurriculumVitaeId == itemModel.CurriculumVitaeId && x.Enable == true);
                    if (interviewAppointment == null)
                        throw new Exception("Ứng viên này chưa có lịch phỏng vấn");
                    //nếu chưa phỏng vấn hoặc phỏng vấn rồi nhưng chưa update thông tin
                    if (interviewAppointment.Status == 1)
                        throw new Exception("Vui lòng kiểm tra trạng thái của buổi phỏng vấn");                   
                    //nếu kết quả buổi phỏng vấn là pass
                    else if (interviewAppointment.Status == 2)
                    {
                        await ValidateSendMailInterviewResult(db, itemModel);                      
                        var template = await db.tbl_Template.FirstOrDefaultAsync(x => x.Type == 6 && x.Enable == true);
                        if (template != null)
                        {
                            title = template.TypeName;
                            result = template.Content;
                        }                          
                        var ward = await db.tbl_Ward.FirstOrDefaultAsync(x => x.Enable == true && x.Id == branch.WardId);
                        var district = await db.tbl_District.FirstOrDefaultAsync(x => x.Enable == true && x.Id == branch.DistrictId);
                        var area = await db.tbl_Area.FirstOrDefaultAsync(x => x.Enable == true && x.Id == branch.AreaId);
                        
                        result = result.Replace("{HoVaTen}", curriculumVitae.FullName);
                        result = result.Replace("{ViTriUngTuyen}", jobPosition.Name);
                        result = result.Replace("{NgayThangNam}", interviewAppointment.WorkStartDate?.ToString("dd/MM/yyyy"));
                        result = result.Replace("{ThoiGianBatDau}", itemModel.StartTime.ToString("HH:mm"));
                        result = result.Replace("{ThoiGianKetThuc}", itemModel.EndTime.ToString("HH:mm"));
                        result = result.Replace("{DiaChi}", branch.Address);
                        result = result.Replace("{PhuongXa}", ward.Name);
                        result = result.Replace("{QuanHuyen}", district.Name);
                        result = result.Replace("{TinhThanhPho}", area.Name);
                        result = result.Replace("{TenLienHe}", itemModel.ContractName);
                        result = result.Replace("{EmailLienHe}", itemModel.ContractEmail);
                        result = result.Replace("{DienThoaiLienHe}", itemModel.ContractPhone);
                    }
                    //nếu kết quả buổi phỏng vấn là fail
                    else if (interviewAppointment.Status == 3)
                    {
                        var template = await db.tbl_Template.FirstOrDefaultAsync(x => x.Type == 7 && x.Enable == true);
                        if (template != null)
                        {
                            title = template.TypeName;
                            result = template.Content;
                        }
                        result = result.Replace("{HoVaTen}", curriculumVitae.FullName);
                        result = result.Replace("{ViTriUngTuyen}", jobPosition.Name);
                    }
                    AssetCRM.SendMail(curriculumVitae.Email, title, result);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}