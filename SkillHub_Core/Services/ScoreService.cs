using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static LMSCore.Services.Class.ClassService;

namespace LMSCore.Services
{
    public class ScoreService
    {
        /// <summary>
        /// lấy danh bảng điểm theo lớp
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        public static async Task<AppDomainResult> GetAll(ScoreSearch baseSearch)
        {
            using (var db = new lmsDbContext())
            {
                if (baseSearch == null) baseSearch = new ScoreSearch();
                string sql = $"Get_Score " +
                    $"@PageIndex = {baseSearch.PageIndex}," +
                    $"@PageSize = {baseSearch.PageSize}," +
                    $"@ClassId = {baseSearch.ClassId}," +
                    $"@TranscriptId = {baseSearch.TranscriptId}";
                var data = await db.SqlQuery<Get_Score>(sql);
                if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
                var totalRow = data[0].TotalRow;
                var result = data.Select(i => new tbl_Score(i)).ToList();
                return new AppDomainResult { TotalRow = totalRow, Data = result };
            }
        }

        /// <summary>
        /// Tìm kiếm bảng điểm theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<tbl_Score> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var data = await db.tbl_Score.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public static bool IsPoint(string input)
        {
            string pattern = @"^\d+(\.\d+)?$";
            return Regex.IsMatch(input, pattern);
        }
        public static async Task<tbl_Score> InsertOrUpdate(ScoreCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var student = await db.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == itemModel.StudentId);
                if (student == null)
                    throw new Exception("Không tìm thấy học viên");
                var _class = await db.tbl_Class.SingleOrDefaultAsync(x => x.Id == itemModel.ClassId && x.Enable == true);
                if (_class == null)
                    throw new Exception("Không tìm thấy lớp");
                var transcript = await db.tbl_Transcript.SingleOrDefaultAsync(x => x.Id == itemModel.TranscriptId && x.Enable == true);
                if (transcript == null)
                    throw new Exception("Không tìm thấy đợt thi");
                var column = await db.tbl_ScoreColumn.SingleOrDefaultAsync(x => x.Id == itemModel.ScoreColumnId && x.Enable == true);
                if (column == null)
                    throw new Exception("Không tìm thấy cột điểm");
                var entity = await db.tbl_Score.FirstOrDefaultAsync(x => x.ClassId == itemModel.ClassId && x.StudentId == itemModel.StudentId && x.TranscriptId == itemModel.TranscriptId && x.ScoreColumnId == itemModel.ScoreColumnId && x.Enable == true);
                if (entity == null)
                {
                    entity = new tbl_Score(itemModel);            
                    entity.CreatedBy = entity.ModifiedBy = user.FullName;
                    db.tbl_Score.Add(entity);
                }
                else
                {
                    entity.Value = itemModel.Value ?? entity.Value;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    if(column.Type == 1)
                    {
                        if (IsPoint(entity.Value) == false)
                            throw new Exception("Vui lòng nhập đúng định dạng điểm số");
                    }                
                }
                await db.SaveChangesAsync();
                return entity;
            }
        }

        /*/// <summary>
        /// Thêm mới điểm
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_Score> Insert(ScoreCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {

                try
                {
                    var model = new tbl_Score(itemModel);
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    db.tbl_Score.Add(model);
                    await db.SaveChangesAsync();

                    return model;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        /// <summary>
        /// Chỉnh sửa điểm
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<tbl_Score> Update(ScoreUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_Score.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.ClassId = itemModel.ClassId ?? entity.ClassId;
                    entity.StudentId = itemModel.StudentId ?? entity.StudentId;
                    entity.ScoreColumnId = itemModel.ScoreColumnId ?? entity.ScoreColumnId;
                    entity.Value = itemModel.Value ?? entity.Value;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                    return entity;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }*/

        /// <summary>
        /// Xóa điểm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    var entity = await db.tbl_Score.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
                    if (entity == null)
                        throw new Exception("Không tìm thấy dữ liệu");
                    entity.Enable = false;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// điẻm trung bình
        /// </summary>
        /// <param name="ClassId"></param>
        /// <param name="TranscriptId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task CalculateMediumScore(CalculatorMediumScoreCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        //lấy danh sách học viên của lớp
                        var StudentInClass = await db.tbl_StudentInClass
                            .Where(x => x.Enable == true && x.ClassId == itemModel.ClassId)
                            .ToListAsync();

                        //kiểm tra xem bảng điểm lớp đã có cột điểm trung bình hay chưa
                        var hasMediumColumn = await db.tbl_ScoreColumn
                            .Where(x => x.Enable == true && x.ClassId == itemModel.ClassId && x.Type == 2)
                            .FirstOrDefaultAsync();

                        //nếu chưa có thì thêm cột điểm trung bình vào bảng điểm lớp
                        if(hasMediumColumn == null)
                        {
                            var maxIndex = 0;
                            var query = db.tbl_ScoreColumn
                                .Where(x => x.Enable == true && x.ClassId == itemModel.ClassId);

                            if (await query.AnyAsync())
                            {
                                maxIndex = await query.MaxAsync(x => x.Index);
                            }

                            hasMediumColumn = new tbl_ScoreColumn
                            {
                                ClassId = itemModel.ClassId,
                                Name = "Điểm trung bình",
                                Type = 2,
                                TypeName = "Điểm trung bình",
                                Factor = 0,
                                Index = maxIndex + 1,
                                Enable = true,
                                CreatedOn = DateTime.Now,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = user.FullName,
                                ModifiedBy = user.FullName
                            };
                            db.tbl_ScoreColumn.Add(hasMediumColumn);
                            await db.SaveChangesAsync();
                        }
                        
                        //tính điểm trung bình
                        foreach (var student in StudentInClass)
                        {
                            var ListScore = await db.tbl_Score
                                .Where(x => x.Enable == true && x.StudentId == student.StudentId && x.TranscriptId == itemModel.TranscriptId && x.ClassId == itemModel.ClassId)
                                .ToListAsync();

                            double MediumScore = 0;
                            double TotalScore = 0;
                            double TotalFactor = 0;

                            foreach (var item in ListScore)
                            {
                                //kiểm tra xem cột đó phải cột điểm không
                                var ScoreColumn = await db.tbl_ScoreColumn
                                    .Where(x => x.Enable == true && x.Id == item.ScoreColumnId && x.Type == 1)
                                    .FirstOrDefaultAsync();

                                if (ScoreColumn != null)
                                {
                                    double Score = double.Parse(item.Value);
                                    TotalFactor += ScoreColumn.Factor;
                                    TotalScore += Score * ScoreColumn.Factor;
                                }
                            }
                            if(TotalScore == 0)
                            {
                                MediumScore = 0;
                            }
                            else
                            {
                                MediumScore = Math.Round((TotalScore / TotalFactor), 2);
                            }
                            
                            //kiểm tra xem bảng điểm sinh viên đã add cột điểm trung bình chưa
                            var MediumScoreColumn = ListScore.FirstOrDefault(x => x.ScoreColumnId == hasMediumColumn.Id && x.Enable == true);

                            if (MediumScoreColumn != null)
                            {
                                MediumScoreColumn.Value = MediumScore.ToString();
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                var insertMediumScoreColumn = new tbl_Score
                                {
                                    StudentId = (int)student.StudentId,
                                    ClassId = itemModel.ClassId,
                                    ScoreColumnId = hasMediumColumn.Id,
                                    TranscriptId = itemModel.TranscriptId,
                                    Value = MediumScore.ToString(),
                                    Enable = true,
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.FullName,
                                    ModifiedOn = DateTime.Now,
                                    ModifiedBy = user.FullName
                                };
                                db.tbl_Score.Add(insertMediumScoreColumn);
                                await db.SaveChangesAsync();
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }          
        }

        /*public static async Task CalculateMediumScore(int ClassId, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                try
                {
                    
                    var StudentInClass = await db.tbl_StudentInClass.Where(x => x.Enable == true && x.ClassId == ClassId).ToListAsync();
                    
                    var hasMediumColumn = await db.tbl_ScoreColumn.Where(x => x.Enable == true && x.ClassId == ClassId && x.Type == 2).FirstOrDefaultAsync();
                    
                    if(hasMediumColumn != null)
                    {
                        foreach (var student in StudentInClass) 
                        {
                            var ListScore = await db.tbl_Score.Where(x => x.Enable == true && x.StudentId == student.StudentId).ToListAsync();
                            double MediumScore = 0;
                            double TotalScore = 0;
                            double TotalFactor = 0;
                            foreach (var item in ListScore)
                            {                               
                                var ScoreColumn = await db.tbl_ScoreColumn.Where(x => x.Enable == true && x.Id == item.ScoreColumnId && x.Type != 2 && x.Type != 3).FirstOrDefaultAsync();
                                if(ScoreColumn != null)
                                {
                                    double Score = double.Parse(item.Value);
                                    TotalFactor += ScoreColumn.Factor;
                                    TotalScore += Score * ScoreColumn.Factor;
                                }
                            }
                            MediumScore = Math.Round((TotalScore / TotalFactor),2);
                            foreach (var item in ListScore)
                            {
                                var ScoreColumn = await db.tbl_ScoreColumn.Where(x => x.Enable == true && x.Id == item.ScoreColumnId && x.Type == 2).FirstOrDefaultAsync();
                                if (ScoreColumn != null)
                                {
                                    item.Value = MediumScore.ToString();
                                    break;
                                }
                            }
                        }
                    }
                    //chưa có thì sau khi tính điểm trung tình tạo 1 cột điểm trung bình và lưu lại
                    else
                    {
                        foreach (var student in StudentInClass)
                        {
                            var ListScore = await db.tbl_Score.Where(x => x.Enable == true && x.StudentId == student.StudentId).ToListAsync();
                            double MediumScore = 0;
                            double TotalScore = 0;
                            double TotalFactor = 0;
                            foreach (var item in ListScore)
                            {
                                var ScoreColumn = await db.tbl_ScoreColumn.Where(x => x.Enable == true && x.Id == item.ScoreColumnId && x.Type != 2 && x.Type != 3).FirstOrDefaultAsync();
                                if (ScoreColumn != null)
                                {
                                    double Score = double.Parse(item.Value);
                                    TotalFactor += ScoreColumn.Factor;
                                    TotalScore += Score * ScoreColumn.Factor;
                                }
                            }
                            MediumScore = Math.Round((TotalScore / TotalFactor), 2);
                            //tạo cột điểm trung bình
                            var maxIndex = await db.tbl_ScoreColumn
                                    .Where(x => x.Enable == true && x.ClassId == ClassId)
                                    .MaxAsync(x => x.Index);
                            var ScoreColumnCreate = new tbl_ScoreColumn
                            {
                                ClassId = ClassId,
                                Name = "Điểm trung bình",
                                Type = 2,
                                TypeName = "Điểm trung bình",
                                Factor = 0,
                                Index = maxIndex + 1,
                                Enable = true,
                                CreatedOn = DateTime.Now,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = user.FullName,
                                ModifiedBy = user.FullName
                            };
                            await db.SaveChangesAsync();
                            var MediumColumn = await db.tbl_Score.Where(x => x.ScoreColumnId == ScoreColumnCreate.Id && x.Enable == true).FirstOrDefaultAsync();
                            MediumColumn.Value = MediumScore.ToString();
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }*/
    }
}