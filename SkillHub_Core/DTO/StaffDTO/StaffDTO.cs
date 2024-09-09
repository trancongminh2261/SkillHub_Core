using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LMSCore.DTO.StaffDTO
{
    public class StaffDTO
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public DateTime? DOB { get; set; }
        /// <summary>
        /// 0 - Khác
        /// 1 - Nam
        /// 2 - Nữ
        /// </summary>
        public int? Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public string Avatar { get; set; }
        public string AvatarReSize { get; set; }
        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }
        private string BranchesJson { get; set; }
        public List<StaffBranch> Branches { get { return JsonConvert.DeserializeObject<List<StaffBranch>>(BranchesJson);  } }
    }
    public class StaffBranch
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
