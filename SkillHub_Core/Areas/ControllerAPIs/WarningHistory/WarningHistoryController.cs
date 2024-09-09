using LMSCore.Areas.Models;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Services.WarningHistory;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace LMSCore.Areas.ControllerAPIs.WarningHistory
{
    [Route("api/WarningHistory")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class WarningHistoryController : BaseController
    {
        private lmsDbContext dbContext;
        private WarningHistoryService domainService;
        public WarningHistoryController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new WarningHistoryService(this.dbContext);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] WarningHistorySearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
