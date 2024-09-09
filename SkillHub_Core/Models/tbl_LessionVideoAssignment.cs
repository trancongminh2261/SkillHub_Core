namespace LMSCore.Models
{
    public class tbl_LessionVideoAssignment : DomainEntity
    {
        public int StudentId { get; set; }
        public int TestId { get; set; }
        public tbl_LessionVideoAssignment() : base() { }
        public tbl_LessionVideoAssignment(object model) : base(model) { }
    }
    public class Get_LessionVideoAssignment : DomainEntity
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; } 
        public string TestDetail { get; set; }
        public int TotalRow { get; set; }
    }
}