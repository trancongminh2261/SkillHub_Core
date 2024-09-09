using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using Microsoft.AspNetCore.Mvc.Routing;

namespace LMSCore.Services
{
    public class PermissionService
    {
        public static async Task<tbl_Permission> InsertOrUpdate(PermissionInsertOrUpdate model)
        {
            using (var db = new lmsDbContext())
            {
                var entity = new tbl_Permission();
                // Tạo mới
                if (model.Id == 0)
                {
                    entity.Controller = model.Controller;
                    entity.Action = model.Action;
                    entity.Description = model.Description;
                    entity.Allowed = model.Allowed;
                    db.tbl_Permission.Add(entity);
                }
                else
                {
                    entity = await db.tbl_Permission.SingleOrDefaultAsync(x => x.Id == model.Id);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Allowed = model.Allowed;
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task<List<PermissionModel>> GetAll(PermissionSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new PermissionSearch();
                var data = GetMapRouter().Where(x =>
               x.ControllerName.Contains(baseSearch.search ?? "")
               || x.ActionName.Contains(baseSearch.search ?? "")
               || string.IsNullOrEmpty(baseSearch.search ?? ""))
                   .OrderBy(x => x.ControllerName).ThenBy(x => x.ActionName).ToList();
                var listPermission = new List<tbl_Permission>();

                foreach (var d in data)
                {
                    var per = new tbl_Permission();
                    per = await db.tbl_Permission.SingleOrDefaultAsync(x => x.Controller == d.ControllerName && x.Action == d.ActionName);
                    if (per == null)
                    {
                        per = new tbl_Permission()
                        {
                            Controller = d.ControllerName,
                            Action = d.ActionName,
                            Description = "router - " + d.Router,
                        };
                    }
                    listPermission.Add(per);
                }

                var result = (from i in listPermission
                              select new PermissionModel()
                              {
                                  Id = i.Id,
                                  Action = i.Action,
                                  Description = i.Description,
                                  Allowed = i.Allowed,
                                  Controller = i.Controller
                              }).ToList();
                return result;
            }
        }
        public static List<ControllerActionInfo> GetMapRouter()
        {
            List<ControllerActionInfo> controllerActionInfoList = new List<ControllerActionInfo>();

            var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && type.Name.EndsWith("Controller"));

            foreach (var controllerType in controllerTypes)
            {
                var controllerName = controllerType.Name.Replace("Controller", "");
                if (controllerName != "Base"
                    && controllerName != "AutoNoti"
                    && controllerName != "Permission"
                    )
                {
                    var actionNames = GetActionNamesForController(controllerType);
                    if (actionNames.Any())
                        actionNames.ForEach(item =>
                        {
                            if (!(controllerName.Contains("Account") && item.Item1.Contains("Login")))
                                controllerActionInfoList.Add(new ControllerActionInfo
                                {
                                    ControllerName = controllerName,
                                    ActionName = item.Item1,
                                    Router = item.Item2,
                                    Method = item.Item3
                                });
                        });
                }
            }
            return controllerActionInfoList;
        }
        private static List<(string, string, string)> GetActionNamesForController(Type controllerType)
        {
            var actionNames = new List<(string, string, string)>();

            var methods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute)));
            foreach (var method in methods)
            {
                string httpMethod = method.GetCustomAttributes().OfType<HttpMethodAttribute>().Select(attr => attr.HttpMethods)
                    .FirstOrDefault().First();
                string ctlRouter = "";
                if (method.DeclaringType.CustomAttributes.FirstOrDefault() != null)
                {
                    ctlRouter = $"{method.DeclaringType.CustomAttributes.FirstOrDefault().ConstructorArguments.FirstOrDefault()}";
                }
                string router = $"{ctlRouter}/{method.CustomAttributes.LastOrDefault().ConstructorArguments.FirstOrDefault()}";
                var actionName = method.Name;
                if (router.Contains("System.Reflection.CustomAttributeTypedArgument"))
                    router = router.Replace("System.Reflection.CustomAttributeTypedArgument", "");
                router = router.Replace("\"", "")
                    .Replace("new LMSCore.Models.lmsEnum+RoleEnum[0] {  }", "");
                actionNames.Add((actionName, router, httpMethod));
            }
            return actionNames;
        }
        public class PermissionSearch
        {
            public string search { get; set; }
        }
        public class ControllerActionInfo
        {
            public string ControllerName { get; set; }
            public string ActionName { get; set; }
            public string Method { get; set; }
            public string Router { get; set; }
        }
        public class RoleModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public static async Task<List<RoleModel>> GetRole()
        {
            var data = new List<RoleModel>()
            {
                new RoleModel { Id = ((int)RoleEnum.admin), Name = "Admin" },
                new RoleModel { Id = ((int)RoleEnum.teacher), Name = "Giáo viên" },
                new RoleModel { Id = ((int)RoleEnum.tutor), Name = "Trợ giảng" },
                new RoleModel { Id = ((int)RoleEnum.student), Name = "Học viên" },
                new RoleModel { Id = ((int)RoleEnum.manager), Name = "Quản lý" },
                new RoleModel { Id = ((int)RoleEnum.sale), Name = "Tư vấn viên" },
                new RoleModel { Id = ((int)RoleEnum.accountant), Name = "Kế toán" },
                new RoleModel { Id = ((int)RoleEnum.academic), Name = "Học vụ" },
                new RoleModel { Id = ((int)RoleEnum.parents), Name = "Phụ huynh" },
            };
            return data;
        }
        public static async Task<List<RoleModel>> GetRoleStaff()
        {
            var data = new List<RoleModel>()
            {
                new RoleModel { Id = ((int)RoleEnum.admin), Name = "Admin" },
                new RoleModel { Id = ((int)RoleEnum.teacher), Name = "Giáo viên" },
                new RoleModel { Id = ((int)RoleEnum.tutor), Name = "Trợ giảng" },
                new RoleModel { Id = ((int)RoleEnum.manager), Name = "Quản lý" },
                new RoleModel { Id = ((int)RoleEnum.sale), Name = "Tư vấn viên" },
                new RoleModel { Id = ((int)RoleEnum.accountant), Name = "Kế toán" },
                new RoleModel { Id = ((int)RoleEnum.academic), Name = "Học vụ" },
            };
            return data;
        }
        public class PermissionInsertOrUpdate
        {
            public int Id { get; set; }
            /// <summary>
            /// danh sách Id quyền, mẫu 1,2,3
            /// </summary>
            public string Allowed { get; set; }
            // Tạo mới
            public string Controller { get; set; }
            public string Action { get; set; }
            public string Description { get; set; }
        }
        //public static async Task<tbl_Permission> Insert(PermissionCreate itemModel)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        var check = await db.tbl_Permission.AnyAsync(x => x.Controller.ToUpper() == itemModel.Controller.ToUpper()
        //        && x.Action.ToUpper() == itemModel.Action.ToUpper());
        //        if (check)
        //            throw new Exception("Đã có");
        //        var model = new tbl_Permission
        //        {
        //            Action = itemModel.Action,
        //            Allowed = "",
        //            Controller = itemModel.Controller,
        //            Description = itemModel.Description
        //        };
        //        db.tbl_Permission.Add(model);
        //        await db.SaveChangesAsync();
        //        return model;
        //    }
        //}
        //public async Task<PermissionModel> GetRouter()
        //{
        //    System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
        //    Assembly[] assems = currentDomain.GetAssemblies();
        //    var controllers = new List<ControllerModel>();
        //    foreach (Assembly assem in assems)
        //    {
        //        var controller = assem.GetTypes().Where(type =>
        //        typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
        //          .Select(e =>
        //          {
        //              id = e.Name.Replace("Controller", string.Empty),
        //              name = string.Format("{0}", ReflectionUtilities.GetClassDescription(e)).Replace("Controller", string.Empty),
        //              actions = (from i in e.GetMembers().Where(x => (
        //                         x.Module.Name == "API.dll" ||
        //                         x.Module.Name == "BaseAPI.dll")
        //                         && x.Name != ".ctor"
        //                         && x.Name != "Validate"
        //                         && x.Name != "UploadFile"
        //                         && x.GetCustomAttributes(typeof(NonActionAttribute)).Select(x => x.GetType().Name).FirstOrDefault() != typeof(NonActionAttribute).Name
        //                         )
        //                         select new ActionModel
        //                         {
        //                             id = $"{e.Name.Replace("Controller", string.Empty)}-{i.Name}",
        //                             name = i.GetCustomAttributes(typeof(DescriptionAttribute), true)
        //                                         .Cast<DescriptionAttribute>().Select(d => d.Description)
        //                                         .SingleOrDefault() ?? i.Name
        //                         }).OrderBy(d => d.name).ToList()
        //          })
        //          .Where(e => e.id != "Role" && e.id != "Auth" && e.id != "Home")
        //          .OrderBy(e => e.name)
        //          .Distinct();

        //        controllers.AddRange(controller);
        //    }

        //    var role = await roleService.GetSingleAsync(x => x.code.ToUpper() == code.ToString().ToUpper() && x.deleted == false);
        //    if (role == null)
        //        throw new AppException(MessageKeyContants.role_nf);
        //    var permissions = new List<Permission>();
        //    if (!string.IsNullOrEmpty(role.permissions))
        //        permissions = JsonConvert.DeserializeObject<List<Permission>>(role.permissions);
        //    var data = new RoleModel
        //    {
        //        id = role.id,
        //        code = role.code,
        //        name = role.name,
        //        controllers = (from i in controllers
        //        select new ControllerModel
        //        {
        //            id = i.id,
        //                           name = i.name,
        //                           grant = code == CoreContants.Role.Admin ? true : permissions.Where(x => x.controller.ToUpper() == i.id.ToUpper() && x.grant == true).Any()
        //                          ? true : false,
        //                           actions = (from a in i.actions
        //                                      select new ActionModel
        //                                      {
        //                                          id = a.id,
        //                                          name = a.name,
        //                                          grant = code == CoreContants.Role.Admin ? true : permissions
        //                                          .Where(x => x.controller.ToUpper() == i.id.ToUpper() && x.action.ToUpper() == a.id.ToUpper() && x.grant == true).Any()
        //                                          ? true : false

        //                                      }).ToList()
        //                       }).ToList()
        //    };
        //}
    }
}