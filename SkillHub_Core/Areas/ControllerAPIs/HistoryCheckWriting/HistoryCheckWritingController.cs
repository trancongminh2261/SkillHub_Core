using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.HistoryCheckWriting;
using LMSCore.Models;
using LMSCore.Services.HistoryCheckWriting;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using System;

namespace LMSCore.Areas.ControllerAPIs.HistoryCheckWriting
{
    [Route("api/HistoryCheckWriting")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class HistoryCheckWritingController : BaseController
    {
        private lmsDbContext dbContext;
        private HistoryCheckWritingService domainService;
        public HistoryCheckWritingController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new HistoryCheckWritingService(this.dbContext);
        }
        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(HistoryCheckWritingDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpGet]
        [Route("detail/{id:int}")]
        [ProducesResponseType(typeof(HistoryCheckWritingDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetail(int id)
        {
            var data = await domainService.GetDetail(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(HistoryCheckWritingDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Insert([FromBody] HistoryCheckWritingPost model)
        {
            try
            {
                var data = await domainService.Insert(model, GetCurrentUser());
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
                await domainService.Delete(id, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(AppDomainResult<HistoryCheckWritingDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] HistoryCheckWritingSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("me")]
        [ProducesResponseType(typeof(AppDomainResult<HistoryCheckWritingDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByMe([FromQuery] SearchOptions baseSearch)
        {
            var data = await domainService.GetByMe(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
