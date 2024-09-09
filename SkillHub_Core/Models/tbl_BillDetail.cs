namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_BillDetail : DomainEntity
    {
        public int BillId { get; set; }
        public int StudentId { get; set; }
        /// <summary>
        /// Lớp
        /// </summary>
        public int? ClassId { get; set; }
        /// <summary>
        /// Chương trình học
        /// </summary>
        public int? ProgramId { get; set; }
        /// <summary>
        /// Giáo trình học
        /// </summary>
        public int? CurriculumId { get; set; }
        /// <summary>
        /// Giỏ hàng
        /// </summary>
        public int? CartId { get; set; }

        /// <summary>
        /// Sản phẩm
        /// </summary>
        public int? ProductId { get; set; }
        /// <summary>
        /// Gói combo 
        /// </summary>
        public int? ComboId { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public double? MonthAvailable { get; set; }
        /// <summary>
        /// Chuyển lớp
        /// </summary>
        public int ClassChangeId { get; set; }
        /// <summary>
        /// Lớp cũ [Chuyển lớp]
        /// </summary>
        public int? OldClassId { get; set; }
        /// <summary>
        /// Lớp mới [Chuyển lớp]
        /// </summary>
        public int? NewClassId { get; set; }
        [NotMapped]
        public string ClassName { get; set; }
        [NotMapped]
        public string ProgramName { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public string CurriculumName { get; set; }
        [NotMapped]
        public int? CompletedLesson { get; set; }
        [NotMapped]
        public int? TotalLesson { get; set; }
        [NotMapped]
        public string OldClassName { get; set; }
        [NotMapped]
        public string NewClassName { get; set; }
        public tbl_BillDetail() : base() { }
        public tbl_BillDetail(object model) : base(model) { }
    }
    public class Get_BillDetail : DomainEntity 
    {
        public int BillId { get; set; }
        public int StudentId { get; set; }
        /// <summary>
        /// Lớp
        /// </summary>
        public int? ClassId { get; set; }
        /// <summary>
        /// Chương trình học
        /// </summary>
        public int? ProgramId { get; set; }
        /// <summary>
        /// Giáo trình học
        /// </summary>
        public int? CurriculumId { get; set; }
        /// <summary>
        /// Giỏ hàng
        /// </summary>
        public int? CartId { get; set; }
        /// <summary>
        /// Sản phẩm
        /// </summary>
        public int? ProductId { get; set; }
        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public string ClassName { get; set; }
        public string ProgramName { get; set; }
        public string ProductName { get; set; }
        public string CurriculumName { get; set; }
        /// <summary>
        /// Chuyển lớp
        /// </summary>
        public int ClassChangeId { get; set; }
        /// <summary>
        /// Lớp cũ [Chuyển lớp]
        /// </summary>
        public int? OldClassId { get; set; }
        /// <summary>
        /// Lớp mới [Chuyển lớp]
        /// </summary>
        public int? NewClassId { get; set; }
        public string OldClassName { get; set; }
        public string NewClassName { get; set; }
        public int? ComboId { get; set; }
        public string ComboName { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public List<ComboProgram> ComboProgram { get; set; } = new List<ComboProgram>();
    }

    public class OldClass
    {
        public int? Classid { get; set; }
        public int? StudentId { get; set; }
        public int? BillId { get; set; }
    }
    public class ComboProgram {
        public int ProgramId { get; set; }
        public string ProgramCode { get; set; }
        public string ProgramName { get; set; }
        public string GradeName { get; set; }
        public double TotalPrice { get; set; }
    }
}