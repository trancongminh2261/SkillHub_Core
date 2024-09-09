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
    [Route("api/StudentRatingChoice")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class StudentRatingChoiceController : BaseController
    {
        private lmsDbContext dbContext;
        private StudentRatingChoiceService domainService;
        public StudentRatingChoiceController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new StudentRatingChoiceService(this.dbContext);
        }
        [HttpGet]
        [Route("{StudentRatingFormId}")]
        public async Task<IActionResult> GetById(int StudentRatingFormId)
        {
            var data = await domainService.GetByStudentRatingFormId(StudentRatingFormId);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        //[HttpGet]
        //[Route("")]
        //public async Task<IActionResult> GetAll([FromQuery] StudentRatingChoiceSearch baseSearch)
        //{
        //    var data = await domainService.GetAll(baseSearch);
        //    if (data.TotalRow == 0)
        //        return StatusCode((int)HttpStatusCode.NoContent);
        //    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        //}
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]  StudentRatingChoiceCreate baseCreate)
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

        //[HttpDelete]
        //[Route("")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            await domainService.Delete(id);
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}
    }
}