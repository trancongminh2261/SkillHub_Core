using System.Collections.Generic;

namespace LMSCore.Models
{
    public class tbl_StudentRatingChoice : DomainEntity
    {
        public int StudentRatingFormId { get; set; }
        public string ListRatingAnswer { get; set; } 
        
        public tbl_StudentRatingChoice() : base() { }
        public tbl_StudentRatingChoice(object model) : base(model) { }
    }
    public class Get_StudentRatingChoiceSearch : DomainEntity
    { 
        public int StudentRatingFormId { get; set; } 
        public string ListRatingAnswer { get; set; }
    }
    public class RatingChoice
    {
        public int QuestionId { get; set; } 
        public List<ListChoice> ListChoice { get; set; }
    }
    public class ListChoice
    {
        public int OptionId { get; set; }
        public bool IsChoose { get; set; } = false;

    }
}