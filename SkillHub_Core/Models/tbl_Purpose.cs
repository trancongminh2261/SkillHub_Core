namespace LMSCore.Models
{
    public class tbl_Purpose : DomainEntity
    {
        public string Name { get; set; }
        public tbl_Purpose() : base() { }
        public tbl_Purpose(object model) : base(model) { }
    }
}