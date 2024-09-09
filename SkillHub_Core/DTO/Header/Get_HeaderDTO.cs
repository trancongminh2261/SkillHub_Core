using static LMSCore.Models.lmsEnum;

namespace LMSCore.DTO.Header
{
    public class Get_HeaderDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ThumbnailResize { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int TotalRow { get; set; } = 0;
    }
}
