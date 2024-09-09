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
using LMSCore.Services.PaymentSession;
using LMSCore.Services.Bill;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/PaymentSession")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class PaymentSessionController : BaseController
    {
        private lmsDbContext dbContext;
        private PaymentSessionService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public PaymentSessionController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new PaymentSessionService(this.dbContext, _hostingEnvironment);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await PaymentSessionService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody] PaymentSessionCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await PaymentSessionService.Insert(model, GetCurrentUser());
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
        [Route("v2")]
        public async Task<IActionResult> InsertV2([FromBody] PaymentSessionCreateV2 model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string baseUrl = Request.Scheme + "://" + Request.Host;
                    var data = await PaymentSessionService.InsertV2(model, GetCurrentUser(), baseUrl);
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
        public async Task<IActionResult> Update([FromBody] PaymentSessionUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await PaymentSessionService.Update(model, GetCurrentUser());
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
        [Route("v2")]
        public async Task<IActionResult> UpdateV2([FromBody] PaymentSessionUpdateV2 model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await PaymentSessionService.UpdateV2(model, GetCurrentUser());
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
                await PaymentSessionService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] PaymentSessionSearch baseSearch)
        {
            var data = await PaymentSessionService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRevenue = data.TotalRevenue,
                totalIncome = data.TotalIncome,
                totalExpense = data.TotalExpense,
                totalRow = data.TotalRow,
                data = data.Data
            });
        }

        [HttpGet]
        [Route("get_paymentSession_billId")]
        public async Task<IActionResult> GetPaymentSessionByBillId([FromQuery] PaymentSessionOfStudentSearch baseSearch)
        {
            try
            {
                var data = await PaymentSessionService.GetPaymentSessionByBillId(baseSearch, GetCurrentUser());
                if (data.TotalRow == 0)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new
                {
                    message = "Thành công !",
                    totalRevenue = data.TotalRevenue,
                    totalIncome = data.TotalIncome,
                    totalExpense = data.TotalExpense,
                    totalRow = data.TotalRow,
                    data = data.Data
                });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
