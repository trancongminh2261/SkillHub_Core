using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
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
    [Route("api/CommissionCampaign")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class CommissionCampaignController : BaseController
    {
        private lmsDbContext dbContext;
        private CommissionCampaignService domainService;
        public CommissionCampaignController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new CommissionCampaignService(this.dbContext);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (ModelState.IsValid)
            {
                var data = await domainService.GetById(id);
                if (data != null)
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new
            {
                message = message
            });
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] CommissionCampaignSearch baseSearch)
        {
            if (ModelState.IsValid)
            {
                var data = await domainService.GetAll(baseSearch);
                if (data.TotalRow == 0)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody]CommissionCampaignUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Update(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] CommissionCampaignCreate baseCreate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Insert(baseCreate, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await domainService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

        [HttpPost]
        [Route("ClosingCommission")]
        public async Task<IActionResult> ClosingCommission([FromBody] List<ClosingCommission> baseCreate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await domainService.ClosingCommission(baseCreate, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Chốt thành công !" });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
    }
}