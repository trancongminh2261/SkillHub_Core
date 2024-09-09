namespace LMSCore.DTO.StudentDeviceDTO
{
    public class StudentDeviceDTO
    {
        public class Get_StudentDevice
        {
            public int Id { get; set; }
            public int? StudentId { get; set; }
            public string UserName { get; set; }
            public string StudentName { get; set; }
            public string StudentCode { get; set; }
            public string StudentAvatar { get; set; }
            public int? RoleId { get; set; }
            public string BranchId { get; set; }
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
            public int TotalRow { get; set; }
        }

        public class DeviceModel
        {
            public string DeviceName { get; set; }
            /// <summary>
            /// Hê điều hành
            /// </summary>
            public string OS { get; set; }
            public string Browser { get; set; }
        }
    }
}
