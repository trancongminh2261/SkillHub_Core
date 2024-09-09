namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_HistoryDonate : DomainEntity
    {
        /// <summary>
        /// Người tặng
        /// </summary>
        public int CreateById { get; set; }
        /// <summary>
        /// Người nhận
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 1 - Khóa video
        /// 2 - Bộ đề
        /// 3 - Lượt chấm bài
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string Note { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string UserCode { get; set; }
        public tbl_HistoryDonate() : base() { }
        public tbl_HistoryDonate(object model) : base(model) { }
    }
    public class Get_HistoryDonate : DomainEntity
    {
        /// <summary>
        /// Người tặng
        /// </summary>
        public int CreateById { get; set; }
        /// <summary>
        /// Người nhận
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 1 - Khóa video
        /// 2 - Bộ đề
        /// 3 - Lượt chấm bài
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string Note { get; set; }
        public string FullName { get; set; }
        public string UserCode { get; set; }
        public int TotalRow { get; set; }
    }
}