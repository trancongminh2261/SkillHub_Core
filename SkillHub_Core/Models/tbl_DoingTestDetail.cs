namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public class tbl_DoingTestDetail : DomainEntity
    {
        public int DoingTestId { get; set; }
        /// <summary>
        /// Nhóm câu hỏi
        /// </summary>
        public int IeltsQuestionGroupId { get; set; }
        /// <summary>
        /// Câu hỏi
        /// </summary>
        public int IeltsQuestionId { get; set; }
        /// <summary>
        /// Đáp án chọn
        /// </summary>
        public int IeltsAnswerId { get; set; }
        /// <summary>
        /// Nội dung đáp án
        /// </summary>
        public string IeltsAnswerContent { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// Xử dụng cho câu hỏi dạng sắp xếp
        /// </summary>
        public int Index { get; set; }
        public tbl_DoingTestDetail() : base() { }
        public tbl_DoingTestDetail(object model) : base(model) { }
    }
    public class DoingTestDetailModel
    {
        public int Id { get; set; }
        /// <summary>
        /// Đáp án chọn
        /// </summary>
        public int IeltsAnswerId { get; set; }
        /// <summary>
        /// Nội dung đáp án
        /// </summary>
        public string IeltsAnswerContent { get; set; }
        /// <summary>
        /// 1 - Chữ 
        /// 2 - Hình ảnh
        /// 3 - Tệp âm thanh
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// Sử dụng cho câu hỏi dạng sắp xếp
        /// </summary>
        public int Index { get; set; }
    }
}