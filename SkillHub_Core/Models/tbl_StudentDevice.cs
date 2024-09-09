namespace LMSCore.Models
{
    public class tbl_StudentDevice : DomainEntity
    {
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public int? RoleId { get; set; }
        public string DeviceName { get; set; }
        /// <summary>
        /// Hệ điều hành
        /// </summary>
        public string OS { get; set; }
        public string Browser { get; set; }
        /// <summary>
        /// Cho phép thiết bị sử dụng để đăng nhập hay không
        /// True: Có
        /// False: Không
        /// </summary>
        public bool? Allowed { get; set; }
        public tbl_StudentDevice() : base() { }
        public tbl_StudentDevice(object model) : base(model) { }
    }
}
