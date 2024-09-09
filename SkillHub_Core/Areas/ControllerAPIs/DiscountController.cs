using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
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
using static LMSCore.Models.lmsEnum;
using LMSCore.Utilities;
using LMSCore.Services.Discount;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Discount")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class DiscountController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await DiscountService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("by-code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                var data = await DiscountService.GetByCode(code);
                if (data == null)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]DiscountCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await DiscountService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody]DiscountUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await DiscountService.Update(model, GetCurrentUser());
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
            try
            {
                await DiscountService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] DiscountSearch baseSearch)
        {
            var data = await DiscountService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }

        [HttpPost]
        [Route("generate-discount")]
        public async Task<IActionResult> GenerateDiscount([FromBody] List<int> classIds)
        {
            if (ModelState.IsValid)
            {
                using (var db = new lmsDbContext())
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var data = await DiscountService.GenerateVoucher(classIds, db);
                            tran.Commit();
                            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
                        }
                        catch (Exception e)
                        {
                            tran.Rollback();
                            return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                        }
                    }
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
    }
}
