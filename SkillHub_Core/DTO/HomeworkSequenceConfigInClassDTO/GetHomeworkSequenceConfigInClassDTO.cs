namespace LMSCore.DTO.HomeworkSequenceConfigDTO
{
    public class GetHomeworkSequenceConfigInClassDTO
    {
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public bool? IsAllow { get; set; }
    }
}
