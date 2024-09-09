using LMSCore.Areas.Models;
using LMSCore.Models;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using LMSCore.Utilities;
using LMSCore.Services;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/LeadSwitchTest")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class LeadSwitchTestController : BaseController
    {
        private lmsDbContext dbContext;
        private LeadSwitchTestService domainService;
        public LeadSwitchTestController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new LeadSwitchTestService(this.dbContext);
        }
        [HttpGet]
        [Route("AllSale")]
        public async Task<IActionResult> GetAllSale([FromQuery] AllSaleSearch baseSearch)
        {
            var data = await domainService.GetAllSale(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("BySale")]
        public async Task<IActionResult> GetBySale([FromQuery] LeadSwitchTestSearch baseSearch)
        {
            var data = await domainService.GetBySale(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
