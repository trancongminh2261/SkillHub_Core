namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_ClassChange : DomainEntity
    {
        public int? StudentId { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        [NotMapped]
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Lớp cũ
        /// </summary>
        public int? OldClassId { get; set; }
        public double? OldPrice { get; set; }
        /// <summary>
        /// Lớp mới
        /// </summary>
        public int? NewClassId { get; set; }
        public double? NewPrice { get; set; }
        /// <summary>
        /// Trung tâm
        /// </summary>
        public int? BranchId { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Số tiền đã trả
        /// </summary>
        public double? Paid { get; set; }
        public int? StudentInClassId { get; set; }
        [NotMapped]
        public string NewClassName { get; set; }
        [NotMapped]
        public string OldClassName { get; set; }
        [NotMapped]
        public string BranchName { get; set; }
        public tbl_ClassChange() : base() { }
        public tbl_ClassChange(object model) : base(model) { }
    }
    public class Get_ClassChange : DomainEntity
    {

        public int? StudentId { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        /// <summary>
        /// Lớp cũ
        /// </summary>
        public int? OldClassId { get; set; }
        public double? OldPrice { get; set; }
        /// <summary>
        /// Lớp mới
        /// </summary>
        public int? NewClassId { get; set; }
        public double? NewPrice { get; set; }
        public double? Paid { get; set; }
        /// <summary>
        /// Trung tâm
        /// </summary>
        public int? BranchId { get; set; }
        public string Note { get; set; }
        public int? StudentInClassId { get; set; }
        public string NewClassName { get; set; }
        public string OldClassName { get; set; }
        public string BranchName { get; set; }
        public int TotalRow { get; set; }
    }
}