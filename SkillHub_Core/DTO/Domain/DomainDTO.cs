using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System;
using System.Text.Json.Serialization;

namespace LMSCore.DTO.Domain
{
    public class DomainDTO 
    {
        [Key]
        public int Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        [JsonIgnore]
        public int? TotalRow { get; set; } = 0;
        public DomainDTO() { }
        public DomainDTO(object model)
        {
            foreach (PropertyInfo me in this.GetType().GetProperties())
            {
                foreach (PropertyInfo item in model.GetType().GetProperties())
                {
                    if (me.Name == item.Name)
                    {
                        me.SetValue(this, item.GetValue(model));
                    }
                }
            }
        }
    }
}
