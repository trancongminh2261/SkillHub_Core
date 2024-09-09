namespace LMSCore.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using System.Reflection;

    public class tbl_RollUp : DomainEntity
    {
        public int? ClassId { get; set; }
        public int? StudentId { get; set; }
        public int? ScheduleId { get; set; }
        /// <summary>
        ///    co,
        ///    vang_co_phep,
        ///    vang_khong_phep,
        ///    di_muon,
        ///    ve_som,
        ///    nghi_le
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        ///    gioi,
        ///    kha,
        ///    trung_binh,
        ///    kem,
        ///    theo_doi_dac_biet,
        ///    co_co_gang,
        ///    khong_co_gang,
        ///    khong_nhan_xet
        /// </summary>
        public int? LearningStatus { get; set; }
        public string LearningStatusName { get; set; }
        public string Note { get; set; }
        public tbl_RollUp() : base() { }
        public tbl_RollUp(object model) : base(model) { }
        public static string GetStatusName(int status)
        {
            return status == 1 ? "Có mặt"
                : status == 2 ? "Vắng có phép"
                : status == 3 ? "Vắng không phép"
                : status == 4 ? "Đi muộn"
                : status == 5 ? "Về sớm"
                : status == 6 ? "Nghĩ lễ" : "";
        }
    }
    public class Get_RollUp 
    {
        public int? ClassId { get; set; }
        public int? StudentId { get; set; }
        public int? ScheduleId { get; set; }
        /// <summary>
        ///    co,
        ///    vang_co_phep,
        ///    vang_khong_phep,
        ///    di_muon,
        ///    ve_som,
        ///    nghi_le
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        ///    gioi,
        ///    kha,
        ///    trung_binh,
        ///    kem,
        ///    theo_doi_dac_biet,
        ///    co_co_gang,
        ///    khong_co_gang,
        ///    khong_nhan_xet
        /// </summary>
        public int? LearningStatus { get; set; }
        public string LearningStatusName { get; set; }
        public string Note { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int TotalRow { get; set; }
        public string ScheduleModel { get; set; }
    }
    public class RollUpModel
    {
        public int? ClassId { get; set; }
        public int? StudentId { get; set; }
        public int? ScheduleId { get; set; }
        /// <summary>
        ///    co,
        ///    vang_co_phep,
        ///    vang_khong_phep,
        ///    di_muon,
        ///    ve_som,
        ///    nghi_le
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        ///    gioi,
        ///    kha,
        ///    trung_binh,
        ///    kem,
        ///    theo_doi_dac_biet,
        ///    co_co_gang,
        ///    khong_co_gang,
        ///    khong_nhan_xet
        /// </summary>
        public int? LearningStatus { get; set; }
        public string LearningStatusName { get; set; }
        public string Note { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string ScheduleModel { get; set; }
        public RollUpModel() { }
        public RollUpModel(object model)
        {
            foreach (PropertyInfo me in this.GetType().GetProperties())
            {
                foreach (PropertyInfo item in model.GetType().GetProperties())
                {
                    if (me.Name == item.Name)
                    {
                        me.SetValue(this, item.GetValue(model));
                    }
                }
            }
        }
    }
    public class ScheduleAttendanceDTO
    {
        public int Id { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

    }
    public class AttendanceDTO
    { 
        public int ScheduleId { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string Note { get; set; }
    }
    public class Get_StudentInClassWhenAttendance
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string ClassName { get; set; }
        public int TotalRow { get; set; }
    }
    public class StudentInClassWhenAttendanceDTO
    { 
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string ClassName { get; set; }
        public List<AttendanceDTO> Attendances { get; set; }
    }
}