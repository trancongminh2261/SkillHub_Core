namespace LMSCore.Models
{
    public class tbl_ScheduleRecord : DomainEntity
    {
        public int ScheduleId { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 1 - upload trực tiếp
        /// 2 - youtube
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string UrlLink { get; set; }
        public tbl_ScheduleRecord() : base() { }
        public tbl_ScheduleRecord(object model) : base(model) { }
        public static string GetTypeName(int? Type)
        {
            return Type == 1 ? "Upload trực tiếp"
                : Type == 2 ? "Youtube" : "";
        }
    }
}
