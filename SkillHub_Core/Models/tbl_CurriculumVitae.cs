using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Models
{
    public class tbl_CurriculumVitae : DomainEntity
    {
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string LinkCV { get; set; }
        //trung tâm muốn ứng tuyển
        public int BranchId { get; set; }
        public int JobPositionId { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// 1 - chờ lịch phỏng vấn
        /// 2 - chưa phỏng vấn
        /// 3 - đã phỏng vấn
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        [NotMapped]
        public string JobPositionName { get; set; }
        [NotMapped]
        public string AreaName { get; set; }
        [NotMapped]
        public string DistrictName { get; set; }
        [NotMapped]
        public string WardName { get; set; }
        public tbl_CurriculumVitae() : base() { }
        public tbl_CurriculumVitae(object model) : base(model) { }
    }

    public class Get_CurriculumVitae : DomainEntity
    {
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string LinkCV { get; set; }
        public int BranchId { get; set; }
        public int JobPositionId { get; set; }
        public int? AreaId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string BranchName { get; set; }
        public string JobPositionName { get; set; }
        public string AreaName { get; set; }
        public string DistrictName { get; set; }
        public string WardName { get; set; }
        public int TotalRow { get; set; }
    }
}