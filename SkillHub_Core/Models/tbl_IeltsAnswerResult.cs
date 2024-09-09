using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LMSCore.Models
{
    public class tbl_IeltsAnswerResult : DomainEntity
    {
        public int IeltsQuestionResultId { get; set; }
        public int IeltsQuestionId { get; set; }
        public int IeltsAnswerId { get; set; }
        public string IeltsAnswerContent { get; set; }
        /// <summary>
        /// true - Đáp án đúng
        /// </summary>
        public bool Correct { get; set; }
        /// <summary>
        /// Áp dụng cho câu sắp xếp, các dạng khác truyền 0
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Lựa chọn của học viên
        /// </summary>
        public bool MyChoice { get; set; }
        /// <summary>
        /// Đáp án học viên viên
        /// </summary>
        public int MyIeltsAnswerId { get; set; }
        /// <summary>
        /// Nội dung làm bài của học viên
        /// </summary>
        public string MyIeltsAnswerContent { get; set; }
        /// <summary>
        /// Đáp án sắp xếp của học viên
        /// </summary>
        public int MyIndex { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public int Type { get; set; }
        public tbl_IeltsAnswerResult() : base() { }
        public tbl_IeltsAnswerResult(object model) : base(model) { }
    }
    public class IeltsAnswerResultModel
    {
        public int Id { get; set; }
        public int IeltsAnswerId { get; set; }
        public string IeltsAnswerContent { get; set; }
        /// <summary>
        /// true - Đáp án đúng
        /// </summary>
        public bool Correct { get; set; }
        /// <summary>
        /// Áp dụng cho câu sắp xếp, các dạng khác truyền 0
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Lựa chọn của học viên
        /// </summary>
        public bool MyChoice { get; set; }
        /// <summary>
        /// Đáp án học viên viên
        /// </summary>
        public int MyIeltsAnswerId { get; set; }
        /// <summary>
        /// Nội dung làm bài của học viên
        /// </summary>
        public string MyIeltsAnswerContent { get; set; }
        /// <summary>
        /// Đáp án sắp xếp của học viên
        /// </summary>
        public int MyIndex { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public int Type { get; set; }
    }
}