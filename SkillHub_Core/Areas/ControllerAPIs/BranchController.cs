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
using static LMSCore.Models.lmsEnum;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [ValidateModelState]
    [Route("api/Branch")]
    public class BranchController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await BranchService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Insert([FromBody]BranchCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await BranchService.Insert(model, GetCurrentUser());
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
        [HttpPut]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Update([FromBody]BranchUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await BranchService.Update(model, GetCurrentUser());
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
        [Route("{id}")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await BranchService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetAll([FromQuery] BranchSearch baseSearch)
        {
            var data = await BranchService.GetAll(baseSearch,GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("option")]
        public async Task<IActionResult> GetBranch()
        {
            var data = await BranchService.GetBranchOption();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
    }
}
