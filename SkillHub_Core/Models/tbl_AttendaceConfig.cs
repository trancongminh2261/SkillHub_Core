using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSCore.Models
{
    public partial class tbl_AttendaceConfig : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string AttendanceTypeIds { get; set; }
        [NotMapped]
        public List<tbl_AttendaceType> AttendaceTypeModel { get; set; }
        public bool? Active { get; set; }
        public tbl_AttendaceConfig() : base() { }
        public tbl_AttendaceConfig(object model) : base(model) { }
    }
    public partial class Get_AttendaceConfig : DomainEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string AttendanceTypeIds { get; set; }
        public bool? Active { get; set; }
        public int TotalRow { get; set; } = 0;
    }
}
