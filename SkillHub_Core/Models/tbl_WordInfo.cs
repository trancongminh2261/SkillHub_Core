namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_WordInfo : DomainEntity
    {
        public tbl_WordInfo(int? UtteranceInfoId)
        {
            this.UtteranceInfoId = UtteranceInfoId;
        }
        public int? UtteranceInfoId { get; set; }
        /// <summary>
        /// Vị trí bắt đầu của từ trong câu.
        /// </summary>
        public double? StartIndex { get; set; }

        /// <summary>
        /// Vị trí kết thúc của từ trong câu.
        /// </summary>
        public double? EndIndex { get; set; }

        /// <summary>
        /// Thời gian bắt đầu của từ trong giây.
        /// </summary>
        public double? StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc của từ trong giây.
        /// </summary>
        public double? EndTime { get; set; }

        /// <summary>
        /// Phiên âm theo Arpabet của từ.
        /// </summary>
        public string TransArpabet { get; set; }

        /// <summary>
        /// Cho biết từ đã được giải mã hay chưa.
        /// </summary>
        public bool Decoded { get; set; }

        /// <summary>
        /// Phien am quoc te
        /// </summary>
        public string Ipa { get; set; }

        /// <summary>
        /// Nội dung của từ sau khi được giải mã.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Nội dung gốc của từ trước khi giải mã.
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// Điểm mức độ bản xứ của từ (từ 0-100).
        /// </summary>
        public double? NativenessScore { get; set; }

        /// <summary>
        /// Đánh giá mức độ thành thạo phát âm của từ.
        /// </summary>
        public string Decision { get; set; }

        /// <summary>
        /// Json: List PhonemesInfo / Thông tin về các âm tiết của từ.
        /// </summary>
        public string Phonemes { get; set; }

        /// <summary>
        /// Json: List WordStressInfo / Thông tin về tần số nhấn âm tiết của từ.
        /// </summary>
        public string WordStress { get; set; }
        public tbl_WordInfo() : base() { }
        public tbl_WordInfo(object model) : base(model) { }
    }
}