namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_Cart : DomainEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public string Thumbnail { get; set; }
        [NotMapped]
        public string Description { get; set; }
        [NotMapped]
        public double Price { get; set; }
        [NotMapped]
        public double TotalPrice { get; set; }
        /// <summary>
        /// 1 - Khóa video
        /// 2 - Bộ đề
        /// 3 - Lượt chấm bài
        /// </summary>
        [NotMapped]
        public int Type { get; set; }
        [NotMapped]
        public string TypeName { get
            {
                return Type == 1 ? "Khóa video"
                        : Type == 2 ? "Bộ đề"
                        : Type == 3 ? "Lượt chấm bài" : "";
            }
        }
        public tbl_Cart() : base() { }
        public tbl_Cart(object model) : base(model) { }
    }
    public class Get_Cart : DomainEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string Thumbnail { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get
            {
                return Quantity * Price;
            }
        }
        /// <summary>
        /// 1 - Khóa video
        /// 2 - Bộ đề
        /// 3 - Lượt chấm bài
        /// </summary>
        public int Type { get; set; }
    }
}