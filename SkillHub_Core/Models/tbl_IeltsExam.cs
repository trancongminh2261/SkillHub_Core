using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LMSCore.Models
{
    public class tbl_IeltsExam : DomainEntity
    {
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Code { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public int QuestionsAmount { get; set; }
        /// <summary>
        /// Số lượng hỏi trắc nghiệm
        /// </summary>
        public int QuestionMultipleChoiceAmount { get; set; }
        /// <summary>
        /// Số lượng câu hỏi tự luận
        /// </summary>
        public int QuestionEssayAmount { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ
        /// </summary>
        public int QuestionsEasy { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình
        /// </summary>
        public int QuestionsNormal { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó
        /// </summary>
        public int QuestionsDifficult { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// Tổng thời gian làm bài từng kỹ năng
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// true - hiện
        /// </summary>
        public bool Active { get; set; }
        public double Point { get; set; }
        public int UserCreate { get; set; }
        /// <summary>
        /// Cấp độ của đề:
        /// 1: Dễ
        /// 2: Trung bình
        /// 3: Khó
        /// </summary>
        public int LevelExam { get; set; }
        public string LevelExamName { get; set; }
        [NotMapped]
        public string AvatarUserCreate { get; set; }
        [NotMapped]
        public int TotalChoices { get; set; } // trắc nghiệm
        [NotMapped]
        public int TotalTF { get; set; } // true false
        [NotMapped]
        public int TotaFillInWords { get; set; } // Điền ô
        [NotMapped]
        public int TotalChooseWord { get; set; } // Chọn từ
        [NotMapped]
        public int TotalWrite { get; set; } // viết
        [NotMapped]
        public int TotalArrangement { get; set; } // sắp xếp
        [NotMapped]
        public int TotalMindmap { get; set; } // mind map
        [NotMapped]
        public int TotalSpeak { get; set; } // nói
        public tbl_IeltsExam() : base() { }
        public tbl_IeltsExam(object model) : base(model) { }
    }
    public class Get_IeltsExam : DomainEntity
    {
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Code { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public int QuestionsAmount { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ
        /// </summary>
        public int QuestionsEasy { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình
        /// </summary>
        public int QuestionsNormal { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó
        /// </summary>
        public int QuestionsDifficult { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// Tổng thời gian làm bài từng kỹ năng
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// true - hiện
        /// </summary>
        /// 
        public bool Active { get; set; }
        public int LevelExam { get; set; }
        public string LevelExamName { get; set; }
        public double Point { get; set; }
        public string AvatarUserCreate { get; set; }
        public int UserCreate { get; set; }
        public int TotalChoices { get; set; } // trắc nghiệm
        public int TotalTF { get; set; } // true false
        public int TotaFillInWords { get; set; } // Điền ô
        public int TotalChooseWord { get; set; } // Chọn từ
        public int TotalWrite { get; set; } // viết
        public int TotalArrangement { get; set; } // sắp xếp
        public int TotalMindmap { get; set; } // mind map
        public int TotalSpeak { get; set; } // nói
        public int TotalRow { get; set; }
    }
    public class NumberQuestionModel
    {
        public int TotalChoices { get; set; } // trắc nghiệm
        public int TotalTF { get; set; } // true false
        public int TotaFillInWords { get; set; } // Điền ô
        public int TotalChooseWord { get; set; } // Chọn từ
        public int TotalWrite { get; set; } // viết
        public int TotalArrangement { get; set; } // sắp xếp
        public int TotalMindmap { get; set; } // mind map
        public int TotalSpeak { get; set; } // nói
    }
    public class Get_IeltsExamOverview
    {
         public int Id { get; set; }
		 public string Name { get; set; }
		 public int Time { get; set; }
		 public string Audio { get; set; }
		 public int QuestionsAmount { get; set; }
		 public int QuestionsEasy { get; set; }
		 public int QuestionsNormal { get; set; }
		 public int QuestionsDifficult { get; set; }
		 public int Index { get; set; }
		 public int IeltsSectionId { get; set; }
		 public string IeltsSectionName { get; set; }
		 public string IeltsSectionExplain { get; set; }
		 public string IeltsSectionReadingPassage { get; set; }
		 public string IeltsSectionAudio { get; set; }
		 public int IeltsSectionIndex { get; set; }

    }
    public class IeltsExamOverviewModel
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Code { get; set; }
        /// <summary>
        /// Số lượng câu hỏi
        /// </summary>
        public int QuestionsAmount { get; set; }
        /// <summary>
        /// Số lương câu hỏi dễ
        /// </summary>
        public int QuestionsEasy { get; set; }
        /// <summary>
        /// Số lương câu hỏi trung bình
        /// </summary>
        public int QuestionsNormal { get; set; }
        /// <summary>
        /// Số lương câu hỏi khó
        /// </summary>
        public int QuestionsDifficult { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// Tổng thời gian làm bài từng kỹ năng
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        public double Point { get; set; }
        public List<IeltsSkillOverviewModel> IeltsSkills { get; set; }
    }
    public class IeltsSkillOverviewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Time { get; set; }
        public string Audio { get; set; }
        public int QuestionsAmount { get; set; }
        public int QuestionsEasy { get; set; }
        public int QuestionsNormal { get; set; }
        public int QuestionsDifficult { get; set; }
        public int Index { get; set; }
        public List<IeltsSectionOverviewModel> IeltsSections { get; set; }
    }
    public class IeltsSectionOverviewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Explain { get; set; }
        public string ReadingPassage { get; set; }
        public string Audio { get; set; }
        public int Index { get; set; }
    }
    public class IeltsQuestionOverviewModel
    { 
        public int IeltsQuestionGroupId { get; set; }
        public int IeltsQuestionId { get; set; }
        public bool IsDone { get; set; }
        public int Index { get; set; }
        public string InputId { get; set; }
    }
    public class Get_IeltsQuestionInSection
    {
        public int IeltsQuestionGroupId { get; set; }
        public int IeltsQuestionId { get; set; }
        /// <summary>
        /// Đã làm câu này
        /// </summary>
        public bool IsDone { get; set; }
        public string InputId { get; set; }
    }
}