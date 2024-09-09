namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_CustomerStatus : DomainEntity
    {
        /// <summary>
        /// 1 - Cần tư vấn
        /// 2 - Khác
        /// 3 - Đã hẹn test
        /// </summary>
        public int? Type { get; set; }
        public string Name { get; set; }
        public string ColorCode { get; set; }
        /// <summary>
        /// Vị trí của từng trạng thái
        /// </summary>
        public int? Index { get; set; }
        public tbl_CustomerStatus() : base() { }
        public tbl_CustomerStatus(object model) : base(model) { }
    }
    public class Get_CustomerStatus : DomainEntity
    {
        /// <summary>
        /// 1 - Cần tư vấn
        /// 2 - Khác
        /// 3 - Đã hẹn test
        /// 4 - Đã học
        /// 5 - Từ chối học
        /// </summary>
        public int? Type { get; set; }
        public string Name { get; set; }
        public string ColorCode { get; set; }
        public int StatusCount { get; set; }
        public int? Index { get; set; }
    }
}