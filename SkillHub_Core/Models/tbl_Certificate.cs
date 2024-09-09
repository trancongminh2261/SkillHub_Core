namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_Certificate : DomainEntity
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Hình nền
        /// </summary>
        public string Background { get; set; }
        /// <summary>
        /// Mặt sau
        /// </summary>
        public string Backside { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string PDFUrl { get; set; }
        public tbl_Certificate() : base() { }
        public tbl_Certificate(object model) : base(model) { }
    }
    public class Get_Certificate : DomainEntity
    {
        public string Name { get; set; }
        public int? UserId { get; set; }
        public string Content { get; set; }
        public string UserCode { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; } // số điện thoại
        public string Email { get; set; }
        public string Avatar { get; set; }
        public int TotalRow { get; set; }
    }
}