using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using RestSharp.Validation;

namespace LMSCore.Services
{
    public class ComboService : DomainService
    {
        public ComboService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<tbl_Combo> GetById(int id)
        {
            var data = await dbContext.tbl_Combo.SingleOrDefaultAsync(x => x.Id == id);
            if (data != null)
            {
                data.Programs = await CheckAndGetProgram(data.ProgramIds);
                data.PriceProgram = data.Programs.Where(x => x.Price.HasValue).Sum(p => p.Price.Value);
                if (data.Type == (int)ComboType.Money)
                    data.ReducePrice = data.Value ?? 0;
                else if (data.Type == (int)ComboType.Percent)
                    data.ReducePrice = (data.PriceProgram * (data.Value ?? 0)) / 100;
                data.TotalPrice = data.PriceProgram - data.ReducePrice;
                data.TotalPrice = data.TotalPrice < 0 ? 0 : data.TotalPrice;

            }
            return data;
        }
        public async Task Validate(tbl_Combo model)
        {
            if (model.StartDate > model.EndDate)
                throw new Exception("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc");
            if (String.IsNullOrEmpty(model.ProgramIds))
                throw new Exception("Vui lòng chọn chương trình học");
        }
        public async Task<List<ProgramModel>> CheckAndGetProgram(string programIds)
        {
            var result = new List<ProgramModel>();
            if (String.IsNullOrEmpty(programIds))
                throw new Exception("Không tìm thấy chương trình học");
            var programArrId = programIds.Split(',').ToList();
            foreach (var programId in programArrId)
            {
                var program = await dbContext.tbl_Program.SingleOrDefaultAsync(x => x.Id.ToString() == programId && x.Enable == true);
                if (program == null)
                    throw new Exception("Không tìm thấy chương trình học");
                var grade = await dbContext.tbl_Grade.SingleOrDefaultAsync(x => x.Id == program.GradeId && x.Enable == true);
                result.Add(new ProgramModel()
                {
                    Id = program.Id,
                    Code = program.Code,
                    Name = program.Name,
                    Price = program.Price,
                    Description = program.Description,
                    Index = program.Index,
                    GradeId = program.GradeId,
                    GradeCode = grade?.Code,
                    GradeName = grade?.Name
                });
            }
            return result;
        }
        public (ComboStatus, string) GetStatus(DateTime sDate, DateTime eDate)
        {
            var now = GetDateTime.Now;
            if (sDate > now)
                return (ComboStatus.CommingSoon, ComboStatusName(ComboStatus.CommingSoon));
            else if (sDate <= now && eDate >= now)
                return (ComboStatus.IsGoingOn, ComboStatusName(ComboStatus.IsGoingOn));
            else
                return (ComboStatus.HasEnded, ComboStatusName(ComboStatus.HasEnded));
        }
        public async Task<tbl_Combo> Insert(ComboCreate itemModel, tbl_UserInformation user)
        {
            var now = GetDateTime.Now;
            var entity = new tbl_Combo(itemModel);
            await Validate(entity);

            entity.Programs = await CheckAndGetProgram(entity.ProgramIds);
            entity.PriceProgram = entity.Programs.Where(x => x.Price.HasValue).Sum(p => p.Price.Value);
            if (entity.Type == (int)ComboType.Money)
                entity.ReducePrice = entity.Value ?? 0;
            else if (entity.Type == (int)ComboType.Percent)
                entity.ReducePrice = (entity.PriceProgram * (entity.Value ?? 0)) / 100;
            entity.TotalPrice = entity.PriceProgram - entity.ReducePrice;
            entity.TotalPrice = entity.TotalPrice < 0 ? 0 : entity.TotalPrice;

            if (entity.EndDate < now)
                throw new Exception("Thời gian kết thúc của combo phải lớn hơn hiện tại");
            var statusInfo = GetStatus(entity.StartDate.Value, entity.EndDate.Value);
            entity.Status = (int)statusInfo.Item1;
            entity.StatusName = statusInfo.Item2;
            entity.CreatedBy = entity.ModifiedBy = user.FullName;
            dbContext.tbl_Combo.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<tbl_Combo> Update(ComboUpdate itemModel, tbl_UserInformation user)
        {
            var entity = await dbContext.tbl_Combo.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
            if (entity == null)
                throw new Exception("Không tìm thấy combo");
            entity.Name = itemModel.Name ?? entity.Name;
            entity.Description = itemModel.Description ?? entity.Description;
            entity.ProgramIds = itemModel.ProgramIds ?? entity.ProgramIds;

            entity.Type = itemModel.Type ?? entity.Type;
            entity.TypeName = lmsEnum.ComboTypeName(itemModel.Type);
            entity.Value = itemModel.Value ?? entity.Value;

            entity.StartDate = itemModel.StartDate ?? entity.StartDate;
            entity.EndDate = itemModel.EndDate ?? entity.EndDate;

            var statusInfo = GetStatus(entity.StartDate.Value, entity.EndDate.Value);
            entity.Status = (int)statusInfo.Item1;
            entity.StatusName = statusInfo.Item2;

            entity.ModifiedBy = user.FullName;
            entity.ModifiedOn = DateTime.Now;
            await Validate(entity);

            entity.Programs = await CheckAndGetProgram(entity.ProgramIds);
            entity.PriceProgram = entity.Programs.Where(x => x.Price.HasValue).Sum(p => p.Price.Value);
            if (entity.Type == (int)ComboType.Money)
                entity.ReducePrice = entity.Value ?? 0;
            else if (entity.Type == (int)ComboType.Percent)
                entity.ReducePrice = (entity.PriceProgram * (entity.Value ?? 0)) / 100;
            entity.TotalPrice = entity.PriceProgram - entity.ReducePrice;
            entity.TotalPrice = entity.TotalPrice < 0 ? 0 : entity.TotalPrice;

            await dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task Delete(int id)
        {
            var entity = await dbContext.tbl_Combo.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (entity == null)
                throw new Exception("Không tìm thấy combo");
            entity.Enable = false;
            await dbContext.SaveChangesAsync();
        }
        public async Task<AppDomainResult> GetAll(ComboSearch baseSearch)
        {
            if (baseSearch == null) baseSearch = new ComboSearch();
            var status = 0;
            if (baseSearch.Status.HasValue)
                status = (int)baseSearch.Status;
            var type = 0;
            if (baseSearch.Type.HasValue)
                type = (int)baseSearch.Type;
            string sql = $"Get_Combo @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@Status = {status}," +
                $"@Type = {type}," +
                $"@ProgramIds = '{baseSearch.ProgramIds ?? ""}'," +
                $"@StartDate = '{baseSearch.StartDate}'," +
                $"@EndDate = '{baseSearch.EndDate}'";
            var data = await dbContext.SqlQuery<Get_Combo>(sql);
            if (!data.Any()) return new AppDomainResult { TotalRow = 0, Data = null };
            foreach (var d in data)
            {
                d.Programs = await CheckAndGetProgram(d.ProgramIds);
                d.PriceProgram = d.Programs.Where(x => x.Price.HasValue).Sum(p => p.Price.Value);
                if (d.Type == (int)ComboType.Money)
                    d.ReducePrice = d.Value ?? 0;
                else if (d.Type == (int)ComboType.Percent)
                    d.ReducePrice = (d.PriceProgram * (d.Value ?? 0)) / 100;
                d.TotalPrice = d.PriceProgram - d.ReducePrice;
                d.TotalPrice = d.TotalPrice < 0 ? 0 : d.TotalPrice;
            }
            var totalRow = data[0].TotalRow;
            return new AppDomainResult { TotalRow = totalRow, Data = data };
        }
        public async Task<List<tbl_Combo>> GetComboDropdown()
        {
            var data = await dbContext.tbl_Combo.Where(x => x.Enable == true && x.Status == (int)ComboStatus.IsGoingOn).ToListAsync();
            if (data.Any())
            {
                foreach (var d in data)
                {
                    d.Programs = await CheckAndGetProgram(d.ProgramIds);
                    d.PriceProgram = d.Programs.Where(x => x.Price.HasValue).Sum(p => p.Price.Value);
                    if (d.Type == (int)ComboType.Money)
                        d.ReducePrice = d.Value ?? 0;
                    else if (d.Type == (int)ComboType.Percent)
                        d.ReducePrice = (d.PriceProgram * (d.Value ?? 0)) / 100;
                    d.TotalPrice = d.PriceProgram - d.ReducePrice;
                    d.TotalPrice = d.TotalPrice < 0 ? 0 : d.TotalPrice;
                }
            }
            return data;
        }
        public static async Task AutoUpdateComboStatus()
        {
            using (var db = new lmsDbContext())
            {
                var now = GetDateTime.Now;
                // Lấy toàn bộ combo ngoại trừ combo hết hạn
                var combo = await db.tbl_Combo.Where(x => x.Enable == true && x.Status != (int)ComboStatus.HasEnded).ToListAsync();
                // Cập nhật trạng thái hết hạn
                foreach (var c in combo.Where(x => x.Status == (int)ComboStatus.IsGoingOn))
                {
                    if (c.EndDate.Value < now)
                    {
                        c.Status = (int)ComboStatus.HasEnded;
                        c.StatusName = ComboStatusName(ComboStatus.HasEnded);
                    }
                }
                // Cập nhật trạng thái đang diễn ra
                foreach (var c in combo.Where(x => x.Status == (int)ComboStatus.CommingSoon))
                {
                    if (c.StartDate <= now && c.EndDate.Value >= now)
                    {
                        c.Status = (int)ComboStatus.IsGoingOn;
                        c.StatusName = ComboStatusName(ComboStatus.IsGoingOn);
                    }
                }
                await db.SaveChangesAsync();
            }
        }

    }
}