namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class tbl_Product : DomainEntity
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        /// <summary>
        /// 1 - Khóa video
        /// 2 - Bộ đề
        /// 3 - Lượt chấm bài
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// Giá bán
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Dùng cho khóa video
        /// </summary>
        public int BeforeCourseId { get; set; }
        [NotMapped]
        public string BeforeCourseName { get; set; }
        /// <summary>
        /// Dùng cho lượt chấm
        /// </summary>
        public int MarkQuantity { get; set; }
        public double TotalRate { get; set; }
        public double TotalStudent { get; set; }
        public int UserCreate { get; set; }
        [NotMapped]
        public bool Disable { get; set; }
        [NotMapped]
        public int TotalPackageSection { get; set; }
        [NotMapped]
        public string AvatarUserCreate { get; set; }
        public tbl_Product() : base() { }
        public tbl_Product(object model) : base(model) { }
    }
    public class Get_Product : DomainEntity
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public int BeforeCourseId { get; set; }
        public string BeforeCourseName { get; set; }
        /// <summary>
        /// Dùng cho lượt chấm
        /// </summary>
        public int MarkQuantity { get; set; }
        public double TotalRate { get; set; }
        public double TotalStudent { get; set; }
        /// <summary>
        /// Giá bán
        /// </summary>
        public double Price { get; set; }
        public int Type { get; set; }
        public int TotalPackageSection { get; set; }
        public string AvatarUserCreate { get; set; }
        public int UserCreate { get; set; }
        public int TotalRow { get; set; }
    }
    public class ProductByStudent : DomainEntity
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public int BeforeCourseId { get; set; }
        public string BeforeCourseName { get; set; }
        /// <summary>
        /// Dùng cho lượt chấm
        /// </summary>
        public int MarkQuantity { get; set; }
        public double TotalRate { get; set; }
        public double TotalStudent { get; set; }
        /// <summary>
        /// 1 - Chưa mua 
        /// 2 - Đã mua
        /// </summary>
        public int Status { get; set; }
        public string StatusName
        {
            get
            {
                return Status == 1 ? "Chưa mua"
                   : Status == 2 ? "Đã mua" : "";
                //: Status == 3 ? "Hoàn thành" : "";
            }
        }
        public bool Disable { get; set; }
        /// <summary>
        /// Giá bán
        /// </summary>
        public double Price { get; set; }
        public int Type { get; set; }
        public int TotalPackageSection { get; set; }
        public string AvatarUserCreate { get; set; }
        public int UserCreate { get; set; }
    }
}