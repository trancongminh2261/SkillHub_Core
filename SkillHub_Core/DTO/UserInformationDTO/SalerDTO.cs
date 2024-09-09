namespace LMSCore.DTO.UserInformationDTO
{
    // Dùng cho getAll Saler bao gồm saler đã bị khóa và bị xóa
    public class SalerDTO
    {
        public int? UserInformationId { get; set; }
        public string UserCode { get; set; }
        public string FullName { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public string BranchIds { get; set; }
        public bool? Enable { get; set; }
        public int? StatusId { get; set; }
        public int TotalRow { get; set; }
    }
}
