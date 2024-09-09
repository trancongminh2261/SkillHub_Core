namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_UserInNFGroup : DomainEntity
    {
        public int? NewsFeedGroupId { get; set; }
        public int? UserId { get; set; }
        /// <summary>
        /// 1 - Quản trị viên
        /// 2 - Thành viên
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        [NotMapped]
        public string RoleName { get; set; }
        [NotMapped]
        public string Avatar { get; set; }
        public tbl_UserInNFGroup() : base() { }
        public tbl_UserInNFGroup(object model) : base(model) { }
    }

    public class Get_UserInNFGroup : DomainEntity
    {
        public int? NewsFeedGroupId { get; set; }
        public int? UserId { get; set; }
        /// <summary>
        /// 1 - Quản trị viên
        /// 2 - Thành viên
        /// </summary>
        public int? Type { get; set; }
        public string TypeName { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string RoleName { get; set; }
        public string Avatar { get; set; }
        public int TotalRow { get; set; }
    }

    public class Get_UserNotInNFGroup
    {
        public int? UserInformationId { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public string Avatar { get; set; }
    }
}