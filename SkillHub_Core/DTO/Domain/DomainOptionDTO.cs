using System.Reflection;

namespace SkillHub_Core.DTO.Domain
{
    public class DomainOptionDTO
    {
        public DomainOptionDTO() { }
        public DomainOptionDTO(object model)
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
