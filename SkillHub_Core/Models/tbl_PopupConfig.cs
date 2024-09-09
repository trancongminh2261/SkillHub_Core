using System;

namespace LMSCore.Models
{
    public class tbl_PopupConfig : DomainEntity
    {
        /// <summary>
        /// Tên popup
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Nôi dung popup bao gồm cả hình ảnh
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Thời gian bắt đầu
        /// </summary>
        public DateTime? STime { get; set; }
        /// <summary>
        /// Thời gian kết thúc
        /// </summary>
        public DateTime? ETime { get; set; }
        /// <summary>
        /// Độ trễ
        /// </summary>
        public double? Durating { get; set; } // SECOND
        /// <summary>
        /// Link khi bấm vào popup sẽ chạy đến trang của link
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Hiện popup hay không
        /// </summary>
        public bool IsShow { get; set; } = false;
        /// <summary>
        /// Chi nhánh
        /// </summary>
        public string BranchIds { get; set; }
        public tbl_PopupConfig() : base() { }
        public tbl_PopupConfig(object model) : base(model) { }
    }
}
