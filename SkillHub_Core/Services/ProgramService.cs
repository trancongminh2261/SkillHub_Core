using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;
using Microsoft.VisualBasic;

namespace LMSCore.Services
{
    public class ProgramService
    {
        public static async Task<tbl_Program> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_Program> Insert(ProgramCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var grade = await db.tbl_Grade.SingleOrDefaultAsync(x => x.Id == itemModel.GradeId);
                if (grade == null)
                    throw new Exception("Không tìm thấy chuyên môn");
                var checkCode = await db.tbl_Program.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Enable == true);
                if (checkCode)
                    throw new Exception("Mã đã tồn tại");
                var model = new tbl_Program(itemModel);
                model.Index = await NewIndex(db, grade.Id);
                model.CreatedBy = model.ModifiedBy = user.FullName;
                db.tbl_Program.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<int> NewIndex(lmsDbContext dbContext, int gradeId)
        {
            var lastIndex = await dbContext.tbl_Program
                .Where(x => x.GradeId == gradeId && x.Enable == true)
                .OrderByDescending(x => x.Index).FirstOrDefaultAsync();
            if (lastIndex == null)
                return 1;
            return lastIndex.Index + 1;
        }
        public static async Task<tbl_Program> Update(ProgramUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (itemModel.Code != null)
                {
                    var checkCode = await db.tbl_Program.AnyAsync(x => x.Code.ToUpper() == itemModel.Code.ToUpper() && x.Enable == true && x.Id != entity.Id);
                    if (checkCode)
                        throw new Exception("Mã đã tồn tại");
                }
                entity.Code = itemModel.Code ?? entity.Code;
                entity.Name = itemModel.Name ?? entity.Name;
                entity.Price = itemModel.Price ?? entity.Price;
                entity.Description = itemModel.Description ?? entity.Description;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                // Update index
                await UpdateIndex(db, entity);
                // xóa chương trình học nếu có trong combo
                var combo = await db.tbl_Combo.Where(x => x.Enable == true && ("," + x.ProgramIds + ",").Contains("," + id + ",")).ToListAsync();
                if (combo.Any())
                    foreach (var item in combo)
                    {
                        var programArr = item.ProgramIds.Split(',').Select(x => "," + x + ",").ToList();
                        item.ProgramIds = string.Join(",", programArr.Where(x => x != ("," + id + ",")).Select(x => x.Replace(",", "")).ToList());
                        if (string.IsNullOrEmpty(item.ProgramIds))
                            item.Enable = false;
                    }
                await db.SaveChangesAsync();
            }
        }
        public static async Task UpdateIndex(lmsDbContext db, tbl_Program model)
        {
            var programs = await db.tbl_Program.Where(x => x.Enable == true
            && x.GradeId == model.GradeId
            && x.Id != model.Id).ToListAsync();
            int index = 1;
            foreach (var program in programs)
            {
                program.Index = index;
                index++;
            }
            await db.SaveChangesAsync();
        }
        public static async Task<AppDomainResult> GetAll(ProgramSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ProgramSearch();
                string sql = $"Get_Program @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@Name = N'{baseSearch.Name ?? ""}'," +
                    $"@Code = N'{baseSearch.Code ?? ""}'," +
                    $"@Search = N'{baseSearch.Search ?? ""}'," +
                    $"@GradeId = N'{baseSearch.GradeId}'," +
                    $"@Sort = N'{baseSearch.Sort}'," +
                    $"@SortType = N'{(baseSearch.SortType ? 1 : 0)}'";
                var data = await db.SqlQuery<Get_Program>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Program(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }
        public static async Task<bool> SwapRollIndex(ChangeIndexItem itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                if (itemModel.Index <= 0)
                    throw new Exception("Vị trí mới phải lớn hơn 0");
                var oldIndex = entity.Index;
                if (itemModel.Index < oldIndex)
                {
                    var programs = await db.tbl_Program.Where(x => x.Enable == true
                       && x.GradeId == entity.GradeId
                       && x.Index >= itemModel.Index
                       && x.Index < oldIndex)
                    .OrderBy(x => x.Index).ToListAsync();
                    if (programs.Any())
                        if (programs.Select(x => x.Index).Max(x => x) < itemModel.Index)
                            throw new Exception("Vị trí mới nằm ngoài danh sách câu hỏi");
                    // đánh lại Index
                    int index = itemModel.Index;
                    foreach (var p in programs)
                    {
                        index++;
                        p.Index = index;
                    }
                    entity.Index = itemModel.Index;
                }
                else
                {
                    var programs = await db.tbl_Program.Where(x => x.Enable == true
                       && x.GradeId == entity.GradeId
                       && x.Index > oldIndex
                       && x.Index <= itemModel.Index)
                    .OrderBy(x => x.Index).ToListAsync();
                    if (programs.Any())
                        if (programs.Select(x => x.Index).Max(x => x) < itemModel.Index)
                            throw new Exception("Vị trí mới nằm ngoài danh sách câu hỏi");
                    // đánh lại Index
                    int index = oldIndex;
                    foreach (var p in programs)
                    {
                        p.Index = index;
                        index++;
                    }
                    entity.Index = itemModel.Index;
                }
                await db.SaveChangesAsync();
                return true;
            }
        }
        //public static async Task ChangeIndex(ChangeIndexModel itemModel, tbl_UserInformation currentUser)
        //{
        //    using (var db = new lmsDbContext())
        //    {
        //        using (var tran = await db.Database.BeginTransactionAsync())
        //        {
        //            try
        //            {
        //                if (!itemModel.Items.Any())
        //                    throw new Exception("Không tìm thấy dữ liệu");
        //                foreach (var item in itemModel.Items)
        //                {
        //                    var data = await db.tbl_Program.SingleOrDefaultAsync(x => x.Id == item.Id);
        //                    if (data == null)
        //                        throw new Exception("Không tìm thấy dữ liệu");
        //                    data.Index = item.Index;
        //                    data.ModifiedOn = DateTime.Now;
        //                    data.ModifiedBy = currentUser.FullName;
        //                    await db.SaveChangesAsync();
        //                }
        //                await tran.CommitAsync();
        //            }
        //            catch (Exception e)
        //            {
        //                await tran.RollbackAsync();
        //                throw new Exception(e.Message);
        //            }
        //        }
        //    }
        //}
        public class TeacherInProgramModel
        {
            public int? TeacherId { get; set; }
            public string TeacherName { get; set; }
            public string TeacherCode { get; set; }
            public int? ProgramId { get; set; }
            public double? TeachingFee { get; set; }
            public bool Allow { get; set; }
        }
        public static async Task<AppDomainResult> GetTeacherInProgram(TeacherInProgramSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {

                if (baseSearch == null) baseSearch = new TeacherInProgramSearch();
                //var l = await db.tbl_UserInformation.Where(x => x.Enable == true
                //&& x.RoleId == ((int)RoleEnum.teacher) && x.StatusId == ((int)AccountStatus.active))
                //    .Select(x => new { x.UserInformationId, x.FullName, x.UserCode, x.BranchIds }).OrderBy(x => x.FullName).ToListAsync();

                string sql = $"Get_StaffWhenAllowProgram @PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@RoleIds = '2'," +
                    $"@BranchIds = N'{baseSearch.BranchIds}'";
                var l = await db.SqlQuery<Get_StaffWhenAllowProgram>(sql);

                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };

                int totalRow = l[0].TotalRow;
                var data = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                var teacherInPrograms = await db.tbl_TeacherInProgram.Where(x => x.ProgramId == baseSearch.ProgramId).ToListAsync();
                var teacher = teacherInPrograms
                    .Select(x => new { x.TeacherId, x.TeachingFee, x.Enable })
                    .ToList();
                var result = (from i in data
                              join t in teacher on i.UserInformationId equals t.TeacherId into pg
                              from t in pg.DefaultIfEmpty()
                              select new TeacherInProgramModel
                              {
                                  TeacherId = i.UserInformationId,
                                  TeacherName = i.FullName,
                                  TeacherCode = i.UserCode,
                                  ProgramId = baseSearch.ProgramId,
                                  TeachingFee = t?.TeachingFee ?? 0,
                                  Allow = t?.Enable ?? false
                              }).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }

        public static async Task<AppDomainResult> GetTutorInProgram(TeacherInProgramSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new TeacherInProgramSearch();

                var l = await db.tbl_UserInformation.Where(x => x.Enable == true
                && x.RoleId == ((int)RoleEnum.tutor) && x.StatusId == ((int)AccountStatus.active))
                    .Select(x => new { x.UserInformationId, x.FullName, x.UserCode }).OrderBy(x => x.FullName).ToListAsync();
                if (!l.Any())
                    return new AppDomainResult { Data = null, TotalRow = 0, Success = true };
                int totalRow = l.Count();
                var data = l.Skip((baseSearch.PageIndex - 1) * baseSearch.PageSize).Take(baseSearch.PageSize).ToList();
                var teacherInPrograms = await db.tbl_TeacherInProgram.Where(x => x.ProgramId == baseSearch.ProgramId && x.Enable == true)
                    .Select(x => x.TeacherId).ToListAsync();
                var result = (from i in data
                              join t in teacherInPrograms on i.UserInformationId equals t into pg
                              from t in pg.DefaultIfEmpty()
                              select new TeacherInProgramModel
                              {
                                  TeacherId = i.UserInformationId,
                                  TeacherName = i.FullName,
                                  TeacherCode = i.UserCode,
                                  ProgramId = baseSearch.ProgramId,
                                  Allow = t == null ? false : true
                              }).ToList();
                return new AppDomainResult { Data = result, TotalRow = totalRow, Success = true };
            }
        }

        public static async Task AllowTeacher(int teacherId, int programId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_TeacherInProgram.Where(x => x.TeacherId == teacherId && x.ProgramId == programId).FirstOrDefaultAsync();
                if (entity != null)
                {
                    entity.Enable = !entity.Enable;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                }
                else
                {
                    entity = new tbl_TeacherInProgram
                    {
                        CreatedBy = user.FullName,
                        CreatedOn = DateTime.Now,
                        Enable = true,
                        ModifiedBy = user.FullName,
                        ModifiedOn = DateTime.Now,
                        ProgramId = programId,
                        TeacherId = teacherId,
                    };
                    db.tbl_TeacherInProgram.Add(entity);
                    await db.SaveChangesAsync();
                }
            }
        }

        // Cho phép giáo viên dạy chương trình gắn thêm lương/buổi dạy
        public static async Task AllowTeacherV2(TeacherInProgramUpdate baseUpdate, tbl_UserInformation user)
        {
            try
            {
                using (var db = new lmsDbContext())
                {
                    if (baseUpdate.TeacherId <= 0) throw new Exception("Id của giáo viên không thể nhỏ hơn hoặc bằng 0!");
                    if (baseUpdate.ProgramId <= 0) throw new Exception("Id của chương trình không thể nhỏ hơn hoặc bằng 0!");
                    if (baseUpdate.TeachingFee < 0) throw new Exception("Lương giáo viên không thể nhỏ hơn 0!");
                    var checkTeacher = await db.tbl_UserInformation.AnyAsync(x => x.UserInformationId == baseUpdate.TeacherId && x.Enable == true && x.RoleId == (int)RoleEnum.teacher);
                    if (!checkTeacher)
                        throw new Exception("Không tìm thấy giáo viên!");
                    var checkProram = await db.tbl_Program.AnyAsync(x => x.Id == baseUpdate.ProgramId && x.Enable == true);
                    if (!checkTeacher)
                        throw new Exception("Không tìm thấy chương trình!");

                    var entity = await db.tbl_TeacherInProgram.Where(x => x.TeacherId == baseUpdate.TeacherId && x.ProgramId == baseUpdate.ProgramId).FirstOrDefaultAsync();
                    if (entity != null)
                    {
                        entity.Enable = baseUpdate.IsActive;
                        entity.TeachingFee = baseUpdate.TeachingFee;
                        entity.ModifiedBy = user.FullName;
                        entity.ModifiedOn = DateTime.Now;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        entity = new tbl_TeacherInProgram
                        {
                            CreatedBy = user.FullName,
                            CreatedOn = DateTime.Now,
                            Enable = baseUpdate.IsActive,
                            ModifiedBy = user.FullName,
                            ModifiedOn = DateTime.Now,
                            ProgramId = baseUpdate.ProgramId,
                            TeacherId = baseUpdate.TeacherId,
                            TeachingFee = baseUpdate.TeachingFee
                        };
                        db.tbl_TeacherInProgram.Add(entity);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public static async Task AllowListTeacher(List<ListTeacherInProgram> baseUpdate, int programId, tbl_UserInformation user)
        {
            try
            {
                using (var db = new lmsDbContext())
                {
                    var userInfor = await db.tbl_UserInformation.Where(x => x.Enable == true && x.RoleId == (int)RoleEnum.teacher)
                                                                .Select(x => new { x.UserInformationId, x.FullName })
                                                                .ToListAsync();
                    var program = await db.tbl_Program.Where(x => x.Enable == true).Select(x => new { x.Id }).ToListAsync();
                    foreach (var tp in baseUpdate)
                    {
                        var checkTeacher = userInfor.FirstOrDefault(x => x.UserInformationId == tp.TeacherId);

                        if (tp.TeachingFee.HasValue)
                        {
                            if (tp.TeachingFee < 0)
                                throw new Exception("Lương giáo viên " + checkTeacher.FullName + " không thể nhỏ hơn 0!");
                        }
                        if (checkTeacher == null)
                            throw new Exception("Không tìm thấy giáo viên!");
                        var checkProram = program.Any(x => x.Id == programId);
                        if (!checkProram)
                            throw new Exception("Không tìm thấy chương trình!");

                        var entity = await db.tbl_TeacherInProgram.FirstOrDefaultAsync(x => x.TeacherId == tp.TeacherId && x.ProgramId == programId);
                        if (entity != null)
                        {
                            entity.Enable = tp.IsActive ?? entity.Enable;
                            entity.TeachingFee = tp.TeachingFee ?? entity.TeachingFee;
                            entity.ModifiedBy = user.FullName;
                            entity.ModifiedOn = DateTime.Now;
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            entity = new tbl_TeacherInProgram
                            {
                                CreatedBy = user.FullName,
                                CreatedOn = DateTime.Now,
                                Enable = tp.IsActive ?? false,
                                ModifiedBy = user.FullName,
                                ModifiedOn = DateTime.Now,
                                ProgramId = programId,
                                TeacherId = tp.TeacherId,
                                TeachingFee = tp.TeachingFee ?? 0
                            };
                            db.tbl_TeacherInProgram.Add(entity);
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static async Task<List<tbl_Program>> GetDropdown(int gradeId)
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_Program.Where(x => x.Enable == true && x.GradeId == gradeId).OrderBy(x => x.Index).ToListAsync();
            }
        }
    }
}