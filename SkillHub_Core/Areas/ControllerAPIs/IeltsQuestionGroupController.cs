using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/IeltsQuestionGroup")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class IeltsQuestionGroupController : BaseController
    {
        private lmsDbContext dbContext;
        private IeltsQuestionGroupService domainService;
        public IeltsQuestionGroupController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new IeltsQuestionGroupService(this.dbContext);
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
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]IeltsQuestionGroupCreate model)
        {
            using (var tran = dbContext.Database.BeginTransaction())
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var data = await domainService.Insert(model, GetCurrentUser());
                        tran.Commit();
                        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                    }
                }
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
        }
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody]IeltsQuestionGroupUpdate model)
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
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (ModelState.IsValid)
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
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }

        [HttpPut]
        [Route("change-index")]
        public async Task<IActionResult> ChangeIndex([FromBody] ChangeIndexModel model)
        {
            try
            {
                await domainService.ChangeIndex(model,GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] IeltsQuestionGroupSearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
