namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_TagCategory : DomainEntity
    {
        public string Name { get; set; }
        /// <summary>
        /// 1 - Khóa Video
        /// 2 - Câu hỏi
        /// 3 - Bộ đề
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public tbl_TagCategory() : base() { }
        public tbl_TagCategory(object model) : base(model) { }
    }
}