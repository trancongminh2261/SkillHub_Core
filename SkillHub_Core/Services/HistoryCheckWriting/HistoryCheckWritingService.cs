using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.HistoryCheckWriting;
using LMSCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace LMSCore.Services.HistoryCheckWriting
{
    public class HistoryCheckWritingService : DomainService
    {
        public HistoryCheckWritingService(lmsDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<HistoryCheckWritingDTO> GetById(int id)
        {
            var data = await dbContext.tbl_HistoryCheckWriting.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data != null)
            {
                return new HistoryCheckWritingDTO(data);
            }
            return null;
        }
        public async Task<HistoryCheckWritingDetailDTO> GetDetail(int id)
        {
            var data = await dbContext.tbl_HistoryCheckWriting.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data != null)
            {
                var result = new HistoryCheckWritingDetailDTO(data);
                var user = await dbContext.tbl_UserInformation.SingleOrDefaultAsync(x => x.UserInformationId == data.UserId);
                if (user == null)
                    throw new Exception("Không tìm thấy thông tin nhân viên");
                result.FullName = user.FullName;
                result.UserCode = user.UserCode;
                result.Avatar = user.Avatar;
                var details = new List<ResponseDetail>();
                var checkWritingResponses = await dbContext.tbl_CheckWritingResponse.Where(x => x.HistoryCheckWritingId == data.Id && x.Enable == true).ToListAsync();
                if (checkWritingResponses.Count > 0)
                {
                    foreach (var response in checkWritingResponses)
                    {
                        var detail = new ResponseDetail();
                        var bandDescriptor = await dbContext.tbl_BandDescriptor.SingleOrDefaultAsync(x => x.Enable == true && x.Id == response.BandDescriptorId);
                        detail.BandDescriptorId = bandDescriptor.Id;
                        detail.BandDescriptorName = bandDescriptor.Name;
                        detail.Score = response.Score;
                        detail.GPTAnswer = response.GPTAnswer;
                        details.Add(detail);
                    }                   
                }
                result.Details = details;
                return result;
            }
            return null;
        }
        public async Task<HistoryCheckWritingDTO> Insert(HistoryCheckWritingPost request, tbl_UserInformation currentUser)
        {
            var data = new tbl_HistoryCheckWriting(request);
            data.UserId = currentUser.UserInformationId;
            data.CreatedBy = data.ModifiedBy = currentUser.FullName;
            dbContext.tbl_HistoryCheckWriting.Add(data);
            await dbContext.SaveChangesAsync();
            return await GetById(data.Id);
        }
        public async Task Delete(int id, tbl_UserInformation currentUser)
        {
            var data = await dbContext.tbl_HistoryCheckWriting.SingleOrDefaultAsync(x => x.Id == id && x.Enable == true);
            if (data == null)
                throw new Exception("Không tìm thấy dữ liệu");
            data.Enable = false;
            data.ModifiedBy = currentUser.FullName;
            data.ModifiedOn = DateTime.Now;
            await dbContext.SaveChangesAsync();
        }

        public async Task<AppDomainResult<HistoryCheckWritingDTO>> GetAll(HistoryCheckWritingSearch baseSearch, tbl_UserInformation currentUser)
        {
            if (baseSearch == null) baseSearch = new HistoryCheckWritingSearch();
            string sql = $"Get_HistoryCheckWriting @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@UserId = {baseSearch.UserId ?? 0}";
            var data = await dbContext.SqlQuery<HistoryCheckWritingDTO>(sql);
            if (!data.Any()) return new AppDomainResult<HistoryCheckWritingDTO> { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult<HistoryCheckWritingDTO> { TotalRow = totalRow ?? 0, Data = data };
        }

        public async Task<AppDomainResult<HistoryCheckWritingDTO>> GetByMe(SearchOptions baseSearch, tbl_UserInformation currentUser)
        {
            if (baseSearch == null) baseSearch = new SearchOptions();
            string sql = $"Get_HistoryCheckWriting @PageIndex = {baseSearch.PageIndex}," +
                $"@PageSize = {baseSearch.PageSize}," +
                $"@Search = N'{baseSearch.Search ?? ""}'," +
                $"@UserId = {currentUser.UserInformationId}";
            var data = await dbContext.SqlQuery<HistoryCheckWritingDTO>(sql);
            if (!data.Any()) return new AppDomainResult<HistoryCheckWritingDTO> { TotalRow = 0, Data = null };
            var totalRow = data[0].TotalRow;
            return new AppDomainResult<HistoryCheckWritingDTO> { TotalRow = totalRow ?? 0, Data = data };
        }
    }
}
