using Newtonsoft.Json;
using System.Collections.Generic;

namespace LMSCore.Areas.Models
{
    public class ElsaApiResponse
    {
        /// <summary>
        /// Phiên bản API đang được gọi.
        /// </summary>
        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Tổng thời gian của âm thanh
        /// </summary>
        [JsonProperty("total_time")]
        public string TotalTime { get; set; }

        /// <summary>
        /// Thông tin về các lời nói và kết quả.
        /// </summary>
        [JsonProperty("utterance")]
        public List<UtteranceInfo> Utterances { get; set; }

        /// <summary>
        /// Kết quả thành công hay không.
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class UtteranceInfo
    {
        /// <summary>
        /// Có khung im lặng ở đầu âm thanh hay không.
        /// </summary>
        [JsonProperty("initial_silence")]
        public bool InitialSilence { get; set; }

        /// <summary>
        /// Chuỗi chứa từ mà người dùng được yêu cầu phát âm.
        /// </summary>
        [JsonProperty("sentence")]
        public string Sentence { get; set; }

        /// <summary>
        /// ID của câu được xử lý, sử dụng trong trường hợp nhiều câu đầu vào.
        /// </summary>
        [JsonProperty("sentence_id")]
        public double? SentenceId { get; set; }

        /// <summary>
        /// Thời gian của lời nói trong giây.
        /// </summary>
        [JsonProperty("total_time")]
        public string TotalTime { get; set; }

        /// <summary>
        /// Có âm thanh trong lời nói hay không.
        /// </summary>
        [JsonProperty("has_speech")]
        public bool HasSpeech { get; set; }

        /// <summary>
        /// Loại nỗ lực phát âm.
        /// </summary>
        [JsonProperty("attempt_type")]
        public string AttemptType { get; set; }

        /// <summary>
        /// Tỷ lệ tín hiệu/độ nhiễu trong âm thanh.
        /// </summary>
        [JsonProperty("snr")]
        public double? SignalToNoiseRatio { get; set; }

        /// <summary>
        /// Đánh giá mức độ thành thạo phát âm.
        /// </summary>
        [JsonProperty("decision")]
        public string Decision { get; set; }

        /// <summary>
        /// Biểu diễn IPA của câu.
        /// </summary>
        [JsonProperty("ipa")]
        public string IPA { get; set; }

        /// <summary>
        /// Khả năng nhấn mạnh từ khóa trong câu (0-100).
        /// </summary>
        [JsonProperty("intonation_score")]
        public double? IntonationScore { get; set; }

        /// <summary>
        /// Khả năng sử dụng nhịp điệu tự nhiên và tạm dừng đúng lúc (0-100)
        /// </summary>
        [JsonProperty("fluency_score")]
        public double? NativenessScorePartial { get; set; }

        /// <summary>
        /// Thông tin về tốc độ phát âm.
        /// </summary>
        [JsonProperty("speed_metrics")]
        public SpeedMetrics SpeedMetrics { get; set; }

        /// <summary>
        /// Thông tin về tốc độ phát âm từng phần.
        /// </summary>
        [JsonProperty("pronunciation_rate_metrics")]
        public PronunciationRateMetrics PronunciationRateMetrics { get; set; }

        /// <summary>
        /// Thông tin về tốc độ nghỉ giữa các phần.
        /// </summary>
        [JsonProperty("pausing_metrics")]
        public PausingMetrics PausingMetrics { get; set; }

        /// <summary>
        /// Confidence level for advanced metrics. Possible values: "NONE", "LOW", "MEDIUM", "HIGH".
        /// </summary>
        [JsonProperty("advanced_metrics_confidence")]
        public string AdvancedMetricsConfidence { get; set; }

        /// <summary>
        /// An IELTS level estimate based on eps_score (1.5-9).
        /// </summary>
        [JsonProperty("ielts_score")]
        public string IELTSScore { get; set; }

        /// <summary>
        /// A CEFR level estimate based on eps_score (A1-C2).
        /// </summary>
        [JsonProperty("cefr_score")]
        public string CEFRScore { get; set; }

        /// <summary>
        /// A TOEFL iBT speaking score estimate based on eps_score (0-30).
        /// </summary>
        [JsonProperty("toefl_score")]
        public string TOEFLScore { get; set; }

        /// <summary>
        /// A TOEIC speaking score estimate based on eps_score (0-200).
        /// </summary>
        [JsonProperty("toeic_score")]
        public string TOEICScore { get; set; }

        /// <summary>
        /// A PTE score estimate based on eps_score (0-90).
        /// </summary>
        [JsonProperty("pte_score")]
        public string PTEScore { get; set; }

        /// <summary>
        /// Nativeness score achieved by the user in the current utterance (0-100 range).
        /// </summary>
        [JsonProperty("pronunciation_score")]
        public double? PronunciationScore { get; set; }

        /// <summary>
        /// Nativeness score achieved by the user on those words that have been decoded and have a minimum score of at least 25%.
        /// </summary>
        [JsonProperty("pronunciation_score_partial")]
        public double? PronunciationScorePartial { get; set; }

        /// <summary>
        /// Global ELSA Proficiency Score, combining pronunciation, fluency, and double?onation (0-100 range).
        /// </summary>
        [JsonProperty("eps_score")]
        public double? EPSScore { get; set; }

        /// <summary>
        /// Characterizes the quality of the recording. Possible values: “ok”, “loud”, “quiet”, “noisy”, “mixed” (mixed issues).
        /// </summary>
        [JsonProperty("recording_quality")]
        public string RecordingQuality { get; set; }

        /// <summary>
        /// Danh sách các từ tạo thành câu và cách người dùng phát âm chúng.
        /// </summary>
        [JsonProperty("words")]
        public List<WordInfo> Words { get; set; }
    }

    public class SpeedMetrics
    {
        /// <summary>
        /// Số từ trên phút (bao gồm cả khi sai).
        /// </summary>
        [JsonProperty("words_per_minute")]
        public double? WordsPerMinute { get; set; }

        /// <summary>
        /// Số âm tiết trên phút (bao gồm cả khi sai).
        /// </summary>
        [JsonProperty("syllables_per_minute")]
        public double? SyllablesPerMinute { get; set; }

        /// <summary>
        /// Số âm tiết phiên âm trên phút (bao gồm cả khi sai).
        /// </summary>
        [JsonProperty("phones_per_minute")]
        public double? PhonesPerMinute { get; set; }

        /// <summary>
        /// Số từ trên phút bỏ qua khoảng im lặng (chỉ tính thời gian phát âm).
        /// </summary>
        [JsonProperty("articulated_words_per_minute")]
        public double? ArticulatedWordsPerMinute { get; set; }

        /// <summary>
        /// Số âm tiết trên phút bỏ qua khoảng im lặng (chỉ tính thời gian phát âm).
        /// </summary>
        [JsonProperty("articulated_syllables_per_minute")]
        public double? ArticulatedSyllablesPerMinute { get; set; }

        /// <summary>
        /// Số âm tiết phiên âm trên phút bỏ qua khoảng im lặng (chỉ tính thời gian phát âm).
        /// </summary>
        [JsonProperty("articulated_phones_per_minute")]
        public double? ArticulatedPhonesPerMinute { get; set; }
    }

    public class PronunciationRateMetrics
    {
        /// <summary>
        /// Số từ đúng trên phút.
        /// </summary>
        [JsonProperty("correct_words_per_minute")]
        public double? CorrectWordsPerMinute { get; set; }

        /// <summary>
        /// Số âm tiết đúng trên phút.
        /// </summary>
        [JsonProperty("correct_syllables_per_minute")]
        public double? CorrectSyllablesPerMinute { get; set; }

        /// <summary>
        /// Số âm tiết phiên âm đúng trên phút.
        /// </summary>
        [JsonProperty("correct_phones_per_minute")]
        public double? CorrectPhonesPerMinute { get; set; }

        /// <summary>
        /// Tỷ lệ từ đúng trong câu (từ 0-1).
        /// </summary>
        [JsonProperty("correct_words_ratio")]
        public double? CorrectWordsRatio { get; set; }

        /// <summary>
        /// Tỷ lệ âm tiết đúng trong câu (từ 0-1).
        /// </summary>
        [JsonProperty("correct_syllables_ratio")]
        public double? CorrectSyllablesRatio { get; set; }

        /// <summary>
        /// Tỷ lệ âm tiết phiên âm đúng trong câu (từ 0-1).
        /// </summary>
        [JsonProperty("correct_phones_ratio")]
        public double? CorrectPhonesRatio { get; set; }
    }

    public class PausingMetrics
    {
        /// <summary>
        /// Tỷ lệ thời gian dừng lại trong câu (từ 0-1).
        /// </summary>
        [JsonProperty("pause_ratio")]
        public double? PauseRatio { get; set; }

        /// <summary>
        /// Tỷ lệ thời gian dừng lại trong câu (loại bỏ khoảng im lặng ban đầu, từ 0-1).
        /// </summary>
        [JsonProperty("pause_ratio_without_initial_sil")]
        public double? PauseRatioWithoutInitialSilence { get; set; }

        /// <summary>
        /// Tỷ lệ phần trăm tạm dừng chính xác trong cách nói (0-100).
        /// </summary>
        [JsonProperty("pausing_score_percentage")]
        public double? PausingScorePercentage { get; set; }
    }

    public class WordInfo
    {
        /// <summary>
        /// Vị trí bắt đầu của từ trong câu.
        /// </summary>
        [JsonProperty("start_index")]
        public double? StartIndex { get; set; }

        /// <summary>
        /// Vị trí kết thúc của từ trong câu.
        /// </summary>
        [JsonProperty("end_index")]
        public double? EndIndex { get; set; }

        /// <summary>
        /// Thời gian bắt đầu của từ trong giây.
        /// </summary>
        [JsonProperty("start_time")]
        public double? StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc của từ trong giây.
        /// </summary>
        [JsonProperty("end_time")]
        public double? EndTime { get; set; }

        /// <summary>
        /// Phiên âm theo Arpabet của từ.
        /// </summary>
        [JsonProperty("trans_arpabet")]
        public string TransArpabet { get; set; }

        /// <summary>
        /// Cho biết từ đã được giải mã hay chưa.
        /// </summary>
        [JsonProperty("decoded")]
        public bool Decoded { get; set; }

        /// <summary>
        /// Phien am quoc te
        /// </summary>
        [JsonProperty("ipa")]
        public string Ipa { get; set; }

        /// <summary>
        /// Nội dung của từ sau khi được giải mã.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Nội dung gốc của từ trước khi giải mã.
        /// </summary>
        [JsonProperty("word")]
        public string Word { get; set; }

        /// <summary>
        /// Điểm mức độ bản xứ của từ (từ 0-100).
        /// </summary>
        [JsonProperty("nativeness_score")]
        public double? NativenessScore { get; set; }

        /// <summary>
        /// Đánh giá mức độ thành thạo phát âm của từ.
        /// </summary>
        [JsonProperty("decision")]
        public string Decision { get; set; }

        /// <summary>
        /// Thông tin về các âm tiết của từ.
        /// </summary>
        [JsonProperty("phonemes")]
        public List<PhonemeInfo> Phonemes { get; set; }

        /// <summary>
        /// Thông tin về tần số nhấn âm tiết của từ.
        /// </summary>
        [JsonProperty("word_stress")]
        public List<WordStressInfo> WordStress { get; set; }
    }

    public class PhonemeInfo
    {
        /// <summary>
        /// Vị trí bắt đầu của ký tự trong câu 
        /// </summary>
        [JsonProperty("start_index")]
        public double? StartIndex { get; set; }

        /// <summary>
        /// Vị trí kết thúc của ký tự trong câu
        /// </summary>
        [JsonProperty("end_index")]
        public double? EndIndex { get; set; }

        /// <summary>
        /// Tập hợp các ký tự trong từ tương ứng với âm tiết
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Phiên âm của âm thanh được biểu diễn 
        /// </summary>
        [JsonProperty("word")]
        public string Word { get; set; }

        /// <summary>
        /// Phiên âm của âm thanh được biểu diễn, định dạng ArpaBET.
        /// </summary>
        [JsonProperty("trans_arpabet")]
        public string TranscriptionArpabet { get; set; }

        /// <summary>
        /// Thời gian bắt đầu của âm thanh, giây.
        /// </summary>
        [JsonProperty("start_time")]
        public double? StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc của âm thanh, giây.
        /// </summary>
        [JsonProperty("end_time")]
        public double? EndTime { get; set; }

        /// <summary>
        /// Đánh giá việc phát âm âm thanh bởi người dùng.
        /// </summary>
        [JsonProperty("decision")]
        public string Decision { get; set; }

        /// <summary>
        /// Điểm mức độ bản xứ của người dùng cho âm thanh.
        /// </summary>
        [JsonProperty("nativeness_score")]
        public double? NativenessScore { get; set; }

        /// <summary>
        /// Thông tin về lỗi âm tiết nếu có (khi Decision == "incorrect").
        /// </summary>
        [JsonProperty("phoneme_error")]
        public string PhonemeError { get; set; }

        /// <summary>
        /// Thông tin về lỗi âm tiết nếu có (khi Decision == "incorrect").
        /// </summary>
        [JsonProperty("phoneme_error_arpabet")]
        public string PhonemeErrorArpabet { get; set; }
        [JsonProperty("feedback")]
        public List<FeedbackInfo> Feedback { get; set; }
    }

    public class FeedbackInfo
    {
        /// <summary>
        /// Định danh số nội bộ cho câu phản hồi.
        /// </summary>
        [JsonProperty("id")]
        public double? Id { get; set; }

        /// <summary>
        /// Liên kết âm thanh đến ghi âm với gợi ý dưới dạng giọng nói.
        /// </summary>
        [JsonProperty("audio_link")]
        public string AudioLink { get; set; }

        /// <summary>
        /// Gợi ý được biểu diễn dưới dạng văn bản.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Ngôn ngữ mà gợi ý được trình bày. Mặc định là tiếng Anh trong kết quả API.
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }
    }

    public class WordStressInfo
    {
        /// <summary>
        /// Vị trí bắt đầu của ký tự trong câu 
        /// </summary>
        [JsonProperty("start_index")]
        public double? StartIndex { get; set; }

        /// <summary>
        /// Vị trí kết thúc của ký tự trong câu 
        /// </summary>
        [JsonProperty("end_index")]
        public double? EndIndex { get; set; }

        /// <summary>
        /// Đánh giá việc đặt nhấn âm tiết của người dùng.
        /// </summary>
        [JsonProperty("decision")]
        public string Decision { get; set; }

        /// <summary>
        /// Mức độ nhấn âm tiết, "high" or "low"
        /// </summary>
        [JsonProperty("stress_level_measured")]
        public string PhonemeError { get; set; }
    }
}

