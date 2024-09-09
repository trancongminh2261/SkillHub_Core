using System.Collections.Generic;

namespace LMSCore.Models
{
    public class tbl_RatingSheet : DomainEntity
    {
        public string Name { get; set; }
        public int ClassId { get; set; } 
        public string Note { get; set; }
        public tbl_RatingSheet() : base() { }
        public tbl_RatingSheet(object model) : base(model) { }
    }
    public class Get_RatingSheetSearch : DomainEntity
    {
        public string Name { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string Note { get; set; }
        public int TotalRow { get; set; }
    }
    public class GetRatingSheetContent : DomainEntity
    {

        public string Name { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public string Note { get; set; } 
        public List<QuestionOption> RatingSheetContent { get; set; }
        public GetRatingSheetContent() : base() { }
        public GetRatingSheetContent(object model) : base(model) { }
    }
    public class QuestionOption
    {
        public int QuestionId { get; set; }
        public string QuestionName { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public List<Option> listOption { get; set; }
    }
    public class Option
    {
        public int OptionId { get; set; }
        public string OptionContent { get; set; }
        public bool TrueOrFalse { get; set; }
        public string Essay { get; set; }
    }
}