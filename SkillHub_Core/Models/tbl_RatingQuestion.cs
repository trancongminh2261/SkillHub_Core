namespace LMSCore.Models
{
    public class tbl_RatingQuestion: DomainEntity
    {
        public string Name { get; set; }
        public int RatingSheetId { get; set; }
        /// <summary>
        /// 1: Trách nghiệm
        /// 2: Tự luận
        /// </summary>
        public int Type { get; set; } = 1;
        public string TypeName { get; set; }
        public tbl_RatingQuestion() : base() { }
        public tbl_RatingQuestion(object model) : base(model) { }
    }
    public class Get_RatingQuestionSearch : DomainEntity
    {
        public string Name { get; set; }
        public int RatingSheetId { get; set; }
        public string RatingSheetName { get; set; }
        /// <summary>
        /// 1: Trách nghiệm
        /// 2: Tự luận
        /// </summary>
        public int Type { get; set; } = 1;
        public string TypeName { get; set; }
        public int TotalRow { get; set; }
    } 
}