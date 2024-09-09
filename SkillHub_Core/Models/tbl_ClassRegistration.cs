namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Reflection;
    using static LMSCore.Models.lmsEnum;
    public class tbl_ClassRegistration : DomainEntity
    {
        public int? StudentId { get; set; }
        public int? BranchId { get; set; }
        public int? ProgramId { get; set; }
        public double? Price { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Chờ xếp lớp
        /// 2 - Đã xếp lớp
        /// 3 - Đã hoàn tiền
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        [NotMapped]
        public string AvatarReSize { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        [NotMapped]
        public string ProgramName { get; set; }
        [NotMapped]
        public int? SaleId { get; set; }
        [NotMapped]
        public string SalerName { get; set; }
        [NotMapped]
        public string SalerUserCode { get; set; }
        [NotMapped]
        public List<ExpectationModel> Expectations { get; set; }
        public tbl_ClassRegistration() : base() { }
        public tbl_ClassRegistration(object model) : base(model) { }
    }
    public class ExpectationModel
    {
        public int ExectedDay { get; set; }
        public string ExectedDayName
        {
            get
            {
                string result = "";
                switch (ExectedDay)
                {
                    case ((int)DayOfWeek.Monday): result = "Thứ Hai"; break;
                    case ((int)DayOfWeek.Tuesday): result = "Thứ Ba"; break;
                    case ((int)DayOfWeek.Wednesday): result = "Thứ Tư"; break;
                    case ((int)DayOfWeek.Thursday): result = "Thứ Năm"; break;
                    case ((int)DayOfWeek.Friday): result = "Thứ Sáu"; break;
                    case ((int)DayOfWeek.Saturday): result = "Thứ Bảy"; break;
                    case ((int)DayOfWeek.Sunday): result = "Chủ nhật"; break;
                }
                return result;
            }
        }
        public int StudyTimeId { get; set; }
        public string StudyTimeName { get; set; }
    }
    public class Get_ClassRegistration : DomainEntity
    {
        public int? StudentId { get; set; }
        public int? BranchId { get; set; }
        public int? ProgramId { get; set; }
        public double? Price { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// 1 - Chờ xếp lớp
        /// 2 - Đã xếp lớp
        /// 3 - Đã hoàn tiền
        /// </summary>
        public int? Status { get; set; }
        public string StatusName { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int? SaleId { get; set; }
        public string SalerName { get; set; }
        public string SalerUserCode { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public string BranchName { get; set; }
        public string ProgramName { get; set; }
        public int TotalRow { get; set; }
    }
    public class Get_ProgramRegistration
    {
        public int ProgramId { get; set; }
        public string ProgramCode { get; set; }
        public string ProgramName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
    }
    public class Get_ScheduleRegistration
    {
        public int Index { get; set; }
        public string GroupName { get; set; }
        public string ExectedDays { get; set; }
        public string StudyTimeIds { get; set; }
        public int Quantity { get; set; }
        public int ProgramId { get; set; }
        public bool IsUndetermined { get; set; } = false;
        public List<Study> Studys { get; set; }
    }
    public class Study {
        public int ExectedDay { get; set; }
        public string ExectedDayName { get; set; }
        public int StudyTimeId { get; set; }
        public string StudyTimeName { get; set; }

    }
    public class Get_StudentRegistration
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentPhone { get; set; }
        public string SaleName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ExectedDays { get; set; }
        public string StudyTimeIds { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public int? ClassStatus { get; set; }
        public string ClassStatusName { get; set; }
        public DateTime? ClassEndDay { get; set; }

        public int TotalRow { get; set; }
    }
    public class StudentRegistration
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentPhone { get; set; }
        public string SaleName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ExectedDays { get; set; }
        public string StudyTimeIds { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public int? ClassStatus { get; set; }
        public string ClassStatusName { get; set; }
        public DateTime? ClassEndDay { get; set; }
        public List<Study> Studys { get; set; }
        public StudentRegistration(object model)
        {
            foreach (PropertyInfo me in this.GetType().GetProperties())
            {
                foreach (PropertyInfo item in model.GetType().GetProperties())
                {
                    if (me.Name == item.Name)
                    {
                        if (me.PropertyType == typeof(string))
                        {
                            me.SetValue(this, item.GetValue(model) == null ? null : item.GetValue(model).ToString());
                        }
                        else
                        {
                            me.SetValue(this, item.GetValue(model));
                        }
                    }
                }
            }
        }
    }
}