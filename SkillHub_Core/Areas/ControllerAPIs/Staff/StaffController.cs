using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Services.IeltsExamService;
using LMSCore.Services.Staff;
using LMSCore.DTO.StaffDTO;
using LMSCore.DTO.ClassTranscript;
using Microsoft.AspNetCore.Http;
using LMSCore.Services.Customer;
using static LMSCore.Services.Class.ClassService;

namespace LMSCore.Areas.ControllerAPIs.Staff
{
    [Route("api/[controller]")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StaffController : BaseController
    {
        private lmsDbContext dbContext;
        private StaffService domainService;
        public StaffController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new StaffService(this.dbContext);
        }
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(AppActionResultDetail<StaffDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
            {
                var currentUser = await GetCurrentUserAsync();
                if (!await domainService.HasPermission(
                    currentUser,
                    data.Id,
                    data.Information.RoleId.Value,
                    data.Information.BranchIds))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, new AppActionResultDetail<StaffDetailDTO>
                    {
                        message = ApiMessage.UNAUTHORIZED
                    });
                }
                return StatusCode((int)HttpStatusCode.OK, new AppActionResultDetail<StaffDetailDTO>
                {
                    message = "Thành công",
                    data = data
                });
            }
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpGet]
        [Route("me")]
        [ProducesResponseType(typeof(AppActionResultDetail<StaffDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMe()
        {
            var currentUser = await GetCurrentUserAsync();
            var data = await domainService.GetMe(currentUser);
            if (data != null)
            {
                return StatusCode((int)HttpStatusCode.OK, new AppActionResultDetail<StaffDetailDTO>
                {
                    message = "Thành công",
                    data = data
                });
            }
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(AppActionResultDetail<StaffDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Insert([FromBody] StaffDetailPost itemModel)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (!await domainService.HasPermission(
                    currentUser,0 , itemModel.Information.RoleId.Value, itemModel.Information.BranchIds))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, new AppActionResultDetail<StaffDetailDTO>
                    {
                        message = ApiMessage.UNAUTHORIZED
                    });
                }
                var data = await domainService.Insert(itemModel, currentUser);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("me")]
        [ProducesResponseType(typeof(AppActionResultDetail<StaffDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateMe([FromBody] StaffDetailMePut itemModel)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                var data = await domainService.UpdateMe(itemModel, currentUser);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPut]
        [Route("")]
        [ProducesResponseType(typeof(AppActionResultDetail<StaffDetailDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] StaffDetailPut itemModel)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (!await domainService.HasPermission(
                    currentUser, itemModel.Id.Value, 0, ""))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, new AppActionResultDetail<StaffDetailDTO>
                    {
                        message = ApiMessage.UNAUTHORIZED
                    });
                }
                var data = await domainService.Update(itemModel, currentUser);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (!await domainService.HasPermission(
                    currentUser, id, 0, ""))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, new AppActionResultDetail<StaffDetailDTO>
                    {
                        message = ApiMessage.UNAUTHORIZED
                    });
                }
                await domainService.Delete(id, currentUser);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(AppActionResult<StaffDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] StaffSearch baseSearch)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (!await domainService.HasPermissionBranch(baseSearch.BranchId, currentUser))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, new AppActionResultDetail<StaffDetailDTO>
                    {
                        message = ApiMessage.UNAUTHORIZED
                    });
                }
                var data = await domainService.Get(baseSearch, GetCurrentUser());
                if (data.TotalRow == 0)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new AppActionResult<StaffDTO>
                {
                    message = "Thành công !", 
                    pageIndex = baseSearch.PageIndex,
                    pageSize = baseSearch.PageSize,
                    totalRow = data.TotalRow, 
                    data = data.Data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
