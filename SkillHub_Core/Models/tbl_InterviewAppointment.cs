using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_InterviewAppointment : DomainEntity
    {
        //ứng viên
        public int CurriculumVitaeId { get; set; }
        //người tổ chức
        public int OrganizerId { get; set; }
        //trung tâm
        public int BranchId { get; set; }
        /// <summary>
        /// 1 - Giáo viên Ielts
        /// 2 - Giáo viên Toeic
        /// 3 - Quản lý
        /// 4 - Tư vấn viên
        /// 5 - Kế toán
        /// 6 - Học vụ
        /// </summary>
        public int JobPositionId { get; set; }
        //ngày phỏng vấn
        public DateTime InterviewDate { get; set; }

        //nếu sau buổi pv mà pass thì update các thông tin sau
        //ngày ứng viên có thể bắt đầu đi làm
        public DateTime? WorkStartDate { get; set; }
        //mức lương 
        public double? Offer { get; set; }
        /// <summary>
        /// 1 - chưa phỏng vấn
        /// 2 - đạt yêu cầu
        /// 3 - không đạt yêu cầu
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string JobPositionName { get; set; }
        public tbl_InterviewAppointment() : base() { }
        public tbl_InterviewAppointment(object model) : base(model) { }
    }
    public class Get_InterviewAppointment : DomainEntity
    {
        public int CurriculumVitaeId { get; set; }
        public int OrganizerId { get; set; }
        public int BranchId { get; set; }
        public int JobPositionId { get; set; }
        public DateTime InterviewDate { get; set; }
        public DateTime? WorkStartDate { get; set; }
        public double? Offer { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string FullName { get; set; }
        public string JobPositionName { get; set; }
        public int TotalRow { get; set; }
    }
}