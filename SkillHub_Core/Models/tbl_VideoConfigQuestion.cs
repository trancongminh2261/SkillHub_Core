using LMSCore.Models;

namespace SkillHub_Core.Models
{
    public class tbl_VideoConfigQuestion : DomainEntity
    {
        public int VideoConfigId { get; set; }
        public string Content { get; set; }
        public int Index { get; set; }
        public tbl_VideoConfigQuestion() : base() { }
        public tbl_VideoConfigQuestion(object model) : base(model) { }
    }
}
