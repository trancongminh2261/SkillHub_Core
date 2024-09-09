using LMSCore.Models;

namespace LMSCore.DTO.StudyRoute
{
    public class Get_StudyRoute : DomainEntity
    {
        public int StudentId { get; set; }
        public int ProgramId { get; set; }
        public string Note { get; set; }
        public int Index { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string ProgramModel { get; set; }
    }
}
