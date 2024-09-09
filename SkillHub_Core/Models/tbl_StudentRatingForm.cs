namespace LMSCore.Models
{
    public class tbl_StudentRatingForm : DomainEntity
    {
        public int StudentId { get; set; }
        public int RatingSheetId { get; set; } 
        public tbl_StudentRatingForm() : base() { }
        public tbl_StudentRatingForm(object model) : base(model) { }
    }
    public class Get_StudentRatingFormSearch : DomainEntity
    {

        public int StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public int RatingSheetId { get; set; }
        public string RatingSheetName { get; set; }
        public int TotalRow { get; set; }
    }
}