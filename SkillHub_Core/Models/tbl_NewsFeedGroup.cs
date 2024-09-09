namespace LMSCore.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using System.Linq;
    using static LMSCore.Models.lmsEnum;
    public class tbl_NewsFeedGroup : DomainEntity
    {
        /// <summary>
        /// Tên nhóm
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Hình nền
        /// </summary>
        public string BackGround { get; set; }
        /// <summary>
        /// Số lượng thành viên
        /// </summary>
        public int? Members { get; set; }
        public int? ClassId { get; set; }     
        [NotMapped]
        public string ClassName { get; set; }
        public tbl_NewsFeedGroup() : base() { }
        public tbl_NewsFeedGroup(object model) : base(model) { }
    }
    public class Get_NewsFeedGroup : DomainEntity
    {
        /// <summary>
        /// Tên nhóm
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Hình nền
        /// </summary>
        public string BackGround { get; set; }
        /// <summary>
        /// Số lượng thành viên
        /// </summary>
        public int? Members { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int? TotalRow { get; set; }
    }
}