namespace LMSCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    using System.Reflection;
    using static LMSCore.Models.lmsEnum;
    using static LMSCore.Services.PermissionService;

    public class tbl_Permission
    {
        [Key]
        public int Id { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string Allowed { get; set; }
    }
    public class PermissionModel
    {
        public int Id { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string Allowed { get; set; }
        public string RoleName { get
            {
                string result = "";
                if (!string.IsNullOrEmpty(Allowed))
                {
                    var data = new List<RoleModel>()
                    {
                        new RoleModel { Id = ((int)RoleEnum.admin), Name = "Admin" },
                        new RoleModel { Id = ((int)RoleEnum.teacher), Name = "Giáo viên" },
                        new RoleModel { Id = ((int)RoleEnum.student), Name = "Học viên" },
                        new RoleModel { Id = ((int)RoleEnum.manager), Name = "Quản lý" },
                        new RoleModel { Id = ((int)RoleEnum.sale), Name = "Tư vấn viên" },
                        new RoleModel { Id = ((int)RoleEnum.accountant), Name = "Kế toán" },
                        new RoleModel { Id = ((int)RoleEnum.academic), Name = "Học vụ" },
                        new RoleModel { Id = ((int)RoleEnum.parents), Name = "Phụ huynh" },
                        new RoleModel { Id = ((int)RoleEnum.tutor), Name = "Trợ giảng" },
                    };
                    string[] alloweds = Allowed.Split(',');
                    if (alloweds.Length > 0)
                    {
                        for (int i = 0; i < alloweds.Length; i++)
                        {
                            int id = int.Parse(alloweds[i]);
                            if (i == 0)
                                result += data.Find(x => x.Id == id).Name;
                            else
                                result += $", {data.Find(x => x.Id == id).Name}";
                        }
                    }
                }
                return result;
            } }
        public PermissionModel() { }
        public PermissionModel(object model)
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