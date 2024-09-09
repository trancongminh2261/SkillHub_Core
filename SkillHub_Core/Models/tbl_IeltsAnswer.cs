using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LMSCore.Models
{
    public class tbl_IeltsAnswer : DomainEntity
    {
        public int IeltsQuestionId { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// true - Đáp án đúng
        /// </summary>
        public bool Correct { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// Áp dụng cho câu sắp xếp, các dạng khác truyền 0
        /// </summary>
        public int Index { get; set; }
        public tbl_IeltsAnswer() : base() { }
        public tbl_IeltsAnswer(object model) : base(model) { }
    }
    public class IeltsAnswerModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// true - Đáp án đúng
        /// </summary>
        public bool? Correct { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// Áp dụng cho câu sắp xếp
        /// </summary>
        public int Index { get; set; }
    }
}