namespace LMSCore.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using static LMSCore.Models.lmsEnum;
    public class tbl_UtteranceInfo : DomainEntity
    {
        /// <summary>
        /// Có khung im lặng ở đầu âm thanh hay không.
        /// </summary>
        public bool InitialSilence { get; set; }

        /// <summary>
        /// Chuỗi chứa từ mà người dùng được yêu cầu phát âm.
        /// </summary>
        public string Sentence { get; set; }

        /// <summary>
        /// ID của câu được xử lý, sử dụng trong trường hợp nhiều câu đầu vào.
        /// </summary>
        public double? SentenceId { get; set; }

        /// <summary>
        /// Thời gian của lời nói trong giây.
        /// </summary>
        public string TotalTime { get; set; }

        /// <summary>
        /// Có âm thanh trong lời nói hay không.
        /// </summary>
        public bool HasSpeech { get; set; }

        /// <summary>
        /// Loại nỗ lực phát âm.
        /// </summary>
        public string AttemptType { get; set; }

        /// <summary>
        /// Tỷ lệ tín hiệu/độ nhiễu trong âm thanh.
        /// </summary>
        public double? SignalToNoiseRatio { get; set; }

        /// <summary>
        /// Đánh giá mức độ thành thạo phát âm.
        /// </summary>
        public string Decision { get; set; }

        /// <summary>
        /// Biểu diễn IPA của câu.
        /// </summary>
        public string IPA { get; set; }

        /// <summary>
        /// Khả năng nhấn mạnh từ khóa trong câu (0-100).
        /// </summary>
        public double? IntonationScore { get; set; }

        /// <summary>
        /// Khả năng sử dụng nhịp điệu tự nhiên và tạm dừng đúng lúc (0-100)
        /// </summary>
        public double? NativenessScorePartial { get; set; }

        /// <summary>
        /// Json: Thông tin về tốc độ phát âm.
        /// </summary>
        public string SpeedMetrics { get; set; }

        /// <summary>
        /// Json: Thông tin về tốc độ phát âm từng phần.
        /// </summary>
        public string PronunciationRateMetrics { get; set; }

        /// <summary>
        /// Json: Thông tin về tốc độ nghỉ giữa các phần.
        /// </summary>
        public string PausingMetrics { get; set; }

        /// <summary>
        /// Confidence level for advanced metrics. Possible values: "NONE", "LOW", "MEDIUM", "HIGH".
        /// </summary>
        public string AdvancedMetricsConfidence { get; set; }

        /// <summary>
        /// An IELTS level estimate based on eps_score (1.5-9).
        /// </summary>
        public string IELTSScore { get; set; }

        /// <summary>
        /// A CEFR level estimate based on eps_score (A1-C2).
        /// </summary>
        public string CEFRScore { get; set; }

        /// <summary>
        /// A TOEFL iBT speaking score estimate based on eps_score (0-30).
        /// </summary>
        public string TOEFLScore { get; set; }

        /// <summary>
        /// A TOEIC speaking score estimate based on eps_score (0-200).
        /// </summary>
        public string TOEICScore { get; set; }

        /// <summary>
        /// A PTE score estimate based on eps_score (0-90).
        /// </summary>
        public string PTEScore { get; set; }

        /// <summary>
        /// Nativeness score achieved by the user in the current utterance (0-100 range).
        /// </summary>
        public double? PronunciationScore { get; set; }

        /// <summary>
        /// Nativeness score achieved by the user on those words that have been decoded and have a minimum score of at least 25%.
        /// </summary>
        public double? PronunciationScorePartial { get; set; }

        /// <summary>
        /// Global ELSA Proficiency Score, combining pronunciation, fluency, and double?onation (0-100 range).
        /// </summary>
        public double? EPSScore { get; set; }

        /// <summary>
        /// Characterizes the quality of the recording. Possible values: “ok”, “loud”, “quiet”, “noisy”, “mixed” (mixed issues).
        /// </summary>
        public string RecordingQuality { get; set; }
        public tbl_UtteranceInfo() : base() { }
        public tbl_UtteranceInfo(object model) : base(model) { }
    }
}