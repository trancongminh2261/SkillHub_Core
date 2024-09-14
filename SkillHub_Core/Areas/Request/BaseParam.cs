using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Areas.Request
{
    public class ParamOnList
    {
        public virtual int PageSize { get; set; } = 20;
        public virtual int PageIndex { get; set; } = 1;
        public virtual string Search { get; set; }
    }
    public class StaffSearch : ParamOnList 
    {
        /// <summary>
        /// 1 - Tên A -> Z
        /// 2 - Tên Z -> A
        /// 3 - Chức vụ A -> Z
        /// 4 - Chức vụ Z -> A
        /// default Id A - Z
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// Lọc nhiều Role => 1,2,3
        /// </summary>
        public string RoleIds { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập vào chi nhánh")]
        public int BranchId { get; set; }
        public StaffSearch()
        { 
            
        }
    }
    public class ParamOnDetail
    {
        public virtual int id { get; set; } 
    }

    public class ScheduleParam
    {
        public string TeacherIds { get; set; }
        public int? ClassId { get; set; }
        public int? StudentId { get; set; }
    }

    public class UserInfoParam
    {
        public int UserId { get; set; }
    }
    public class ClassParam
    {
        public int Class { get; set; }
    }

    public class FeedbackParam
    {
        public int FeedbackId { get; set; }
    }
    public class SalaryParam : ParamOnList
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
    }

    public class StudentParam
    {
        public int StudentIds { get; set; }
    }
}