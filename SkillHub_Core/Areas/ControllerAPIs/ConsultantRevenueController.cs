using LMSCore.Areas.Models;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/ConsultantRevenue")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ConsultantRevenueController : BaseController
    {
        private lmsDbContext dbContext;
        private ConsultantRevenueService domainService;
        public ConsultantRevenueController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new ConsultantRevenueService(this.dbContext);
        }
        [HttpGet]
        [Route("all-sale")]
        public async Task<IActionResult> GetAllSale([FromQuery] AllSaleSearch baseSearch)
        {
            var data = await domainService.GetAllSale(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("by-sale")]
        public async Task<IActionResult> GetBySale([FromQuery] ConsultantRevenueSearch baseSearch)
        {
            var data = await domainService.GetBySale(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
