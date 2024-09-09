using System.Reflection;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.DTO.Header
{
    public class HeaderDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ThumbnailResize { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public HeaderDTO(object model)
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
    public class MenuNumberDTO
    {
        public int Feedback { get; set; } = 0;
        public int ProgramRegistration { get; set; } = 0;
    }
}
