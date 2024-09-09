using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.Bill.BillService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using LMSCore.LMS;
using LMSCore.Utilities;
using LMSCore.Services.Bill;

namespace LMSCore.Areas.ControllerAPIs
{
    [ValidateModelState]
    [Route("api/Bill")]
    [ClaimsAuthorize]
    public class BillController : BaseController
    {
        private lmsDbContext dbContext;
        private BillService domainService;
        private static IWebHostEnvironment _hostingEnvironment;
        public BillController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.dbContext = new lmsDbContext();
            this.domainService = new BillService(this.dbContext, _hostingEnvironment);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await BillService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("class-available")]
        public async Task<IActionResult> GetClassAvailable([FromQuery] GetClassAvailableSearch baseSearch)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            if (baseSearch == null)
                baseSearch = new GetClassAvailableSearch();
            var data = await BillService.GetClassAvailable(baseSearch);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("class-reserve-option")]
        public async Task<IActionResult> GetClassReserveOption([FromQuery] ClassReserveOptionSearch baseSearch)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await BillService.GetClassReserveOption(baseSearch);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("tuition-package-option")]
        public async Task<IActionResult> GetTuitionPackageOption()
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await BillService.GetTuitionPackageOption();
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        //[HttpPost]
        //[Route("")]
        //public async Task<IActionResult> Insert([FromBody]BillCreate model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (var db = new lmsDbContext())
        //        {
        //            using (var tran = db.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    var data = await BillService.Insert(model, GetCurrentUser(), db);
        //                    tran.Commit();
        //                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        //                }
        //                catch (Exception e)
        //                {
        //                    tran.Rollback();
        //                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //                }
        //            }
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}

        [HttpPost]
        [Route("register-tuition-package")]
        public async Task<IActionResult> RegisterTuitionPackage([FromBody] RegisterTuitionPackageModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await BillService.RegisterTuitionPackage(model, GetCurrentUser());
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
        public async Task<IActionResult> InsertV2([FromBody] BillCreateV2 model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new lmsDbContext())
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            string baseUrl = Request.Scheme + "://" + Request.Host;
                            var data = await BillService.InsertV2(model, GetCurrentUser(), db, baseUrl);
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
        [HttpPost]
        [Route("buy-combo")]
        public async Task<IActionResult> InsertBuyCombo([FromBody] BillByComboCreate model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new lmsDbContext())
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            string baseUrl = Request.Scheme + "://" + Request.Host;
                            var data = await BillService.InsertBuyCombo(model, GetCurrentUser(), db, baseUrl);
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
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            try
            {
                await BillService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        //[HttpPost]
        //[Route("one-month")]
        //public async Task<IActionResult> OneMonthDebt(MonthlyBillCreate model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (var db = new lmsDbContext())
        //        {
        //            using (var tran = db.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    var billRequest = await BillService.ValidateMonthlyBill(model, db);
        //                    var data = await BillService.Insert(billRequest, GetCurrentUser(), db);
        //                    tran.Commit();
        //                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        //                }
        //                catch (Exception e)
        //                {
        //                    tran.Rollback();
        //                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //                }
        //            }
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}

        [HttpPost]
        [Route("payment")]
        public async Task<IActionResult> Payment([FromBody] PaymentCreate itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string baseUrl = Request.Scheme + "://" + Request.Host;
                    var data = await BillService.Payment(itemModel, baseUrl, GetCurrentUser());
                    string successMessage = "Thành công";
                    if (GetCurrentUser().RoleId == ((int)RoleEnum.admin) || GetCurrentUser().RoleId == ((int)RoleEnum.accountant))
                        successMessage = "Đã gửi yêu cầu duyệt thanh toán, vui lòng đợi duyệt";
                    return StatusCode((int)HttpStatusCode.OK, new { message = successMessage, data });
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
        [Route("payment/v2")]
        public async Task<IActionResult> PaymentV2([FromBody] PaymentCreateV2 itemModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string baseUrl = Request.Scheme + "://" + Request.Host;
                    var data = await BillService.PaymentV2(itemModel, baseUrl, GetCurrentUser());
                    string successMessage = "Thành công";
                    if (GetCurrentUser().RoleId == ((int)RoleEnum.admin) || GetCurrentUser().RoleId == ((int)RoleEnum.accountant))
                        successMessage = "Đã gửi yêu cầu duyệt thanh toán, vui lòng đợi duyệt";
                    return StatusCode((int)HttpStatusCode.OK, new { message = successMessage, data });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] BillSearch baseSearch)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await BillService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRow = data.TotalRow,
                data = data.Data,
                sumDebt = data.SumDebt,
                sumPaid = data.SumPaid,
                sumReduced = data.SumReduced,
                sumtotalPrice = data.SumtotalPrice,
                typeAll = data.Type_All,
                typeRegis = data.Type_Regis,
                typeService = data.Type_Service,
                typeTutorial = data.Type_Tutorial,
                typeManual = data.Type_Manual,
                typeMonthly = data.Type_Monthly,
                typeClassChange = data.Type_ClassChange,
            });
        }

        //[HttpGet]
        //[Route("GetDiscountHistory")]
        //public async Task<IActionResult> GetDiscountHistory([FromQuery] BillSearch baseSearch)
        //{
        //    var data = await BillService.GetDiscountHistory(baseSearch, GetCurrentUser());
        //    if (data.TotalRow == 0)
        //        return StatusCode((int)HttpStatusCode.NoContent);
        //    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        //}

        [HttpGet]
        [Route("discount-history")]
        public async Task<IActionResult> GetDiscountHistory([FromQuery] DiscountHistorySearch baseSearch)
        {
            var data = await BillService.GetDiscountHistory(baseSearch, GetCurrentUser());
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpGet]
        [Route("detail/{billId}")]
        public async Task<IActionResult> GetDetail(int billId)
        {
            var data = await BillService.GetDetail(billId);
            if (!data.Any())
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        //[HttpGet]
        //[Route("NotificationPayment")]
        //public async Task<IActionResult> NotificationPayment( )
        //{
        //    await BillService.PaymentNotification();
        //    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        //}

        [HttpGet]
        [Route("appointment-due-soon")]
        public async Task<IActionResult> GetAllAppointmentDueSoon([FromQuery] AppointmentDueSoonSearch baseSearch)
        {
            var data = await BillService.GetAppointmentDueSoon(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công !",
                totalRow = data.TotalRow,
                data = data.Data,
                sumDebt = data.SumDebt,
                sumPaid = data.SumPaid,
                sumtotalPrice = data.SumtotalPrice,
                type_All = data.Type_All,
                type_Regis = data.Type_Regis,
                type_Service = data.Type_Service,
                type_Tutorial = data.Type_Tutorial,
                type_Manual = data.Type_Manual,
                type_Monthly = data.Type_Monthly,
                type_ClassChange = data.Type_ClassChange,
            });
        }

        [HttpGet]
        [Route("export-bill-to-pdf/{billId}")]
        public async Task<IActionResult> ExportBillToPdf(int billId)
        {
            var httpContext = HttpContext;
            var path = Path.Combine(_hostingEnvironment.ContentRootPath, "Upload");
            var pathViews = Path.Combine(_hostingEnvironment.ContentRootPath, "Views");
            string strPathAndQuery = httpContext.Request.Path + httpContext.Request.QueryString;
            var fileUrl = $"{Request.Scheme}://{Request.Host}";
            if (fileUrl.IndexOf("https") == -1)
                fileUrl = fileUrl.Replace("http", "https");
            var data = await BillService.ExportBill(billId, path, pathViews, fileUrl);
            return StatusCode((int)HttpStatusCode.OK, new
            {
                message = "Thành công",
                data
            });
        }
    }
}
