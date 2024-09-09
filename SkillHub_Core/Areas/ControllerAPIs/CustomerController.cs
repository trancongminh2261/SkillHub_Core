using ExcelDataReader;
using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;
using static LMSCore.Services.Customer.CustomerService;
using OfficeOpenXml;
using LMSCore.Utilities;
using LMSCore.Services.Customer;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/Customer")]
    [ValidateModelState]
    public class CustomerController : BaseController
    {
        private lmsDbContext dbContext;
        private CustomerService domainService;
        public CustomerController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new CustomerService(this.dbContext);
        }
        [HttpGet]
        [Route("{id}")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await CustomerService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpGet]
        [Route("check-exist")]
        [ClaimsAuthorize]
        public async Task<IActionResult> CheckExist([FromQuery] CheckExistModel itemModel)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            var data = await CustomerService.CheckExist(itemModel);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("send-mail")]
        [ClaimsAuthorize]
        public async Task<IActionResult> SendMail([FromBody] SendMailModel itemModel)
        {
            if (!ModelState.IsValid)
            {
                var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
            }
            await CustomerService.SendMail(itemModel);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        }
        [HttpPost]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> Insert([FromBody]CustomerCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CustomerService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody]CustomerUpdate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CustomerService.UpdateV2(model, GetCurrentUser());
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
                await CustomerService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        /// <summary>
        /// Nhân viên thuộc trung nào chỉ thấy khách hàng ở trung tâm đó, Tư vấn viên chỉ thấy khách hàng của chính mình
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetAll([FromQuery] CustomerSearch baseSearch)
        {
            var data = await CustomerService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("minhdaica29")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetByMinhDaiCa([FromQuery] CustomerSearch baseSearch)
        {
            var data = await domainService.GetByMinhDaiCa(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        /// <summary>
        /// điều chỉnh so với v1: admin được lọc theo saleId, ngày tạo, branchIds
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v2")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetAllV2([FromQuery] CustomerV2Search baseSearch)
        {
            var data = await CustomerService.GetAllV2(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpPost]
        [Route("ImportCustomer")]
        [ClaimsAuthorize]
        public async Task<IActionResult> ImportCustomer()
        {
            try
            {
                var httpRequest = HttpContext.Request;

                var model = new List<CustomerCreate>();

                if (httpRequest.Form.Files.Count > 0)
                {
                    var file = httpRequest.Form.Files.GetFile("File");

                    using (var stream = file.OpenReadStream())
                    {
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;

                            for (int row = 4; row <= rowCount; row++)
                            {
                                var item = new CustomerCreate
                                {
                                    FullName = worksheet.Cells[row, 1].Value?.ToString(),
                                    Email = worksheet.Cells[row, 2].Value?.ToString(),
                                    Mobile = worksheet.Cells[row, 3].Value?.ToString(),
                                };

                                if (GetCurrentUser().RoleId == (int)RoleEnum.sale)
                                {
                                    item.SaleId = GetCurrentUser().UserInformationId;
                                }
                                else
                                {
                                    item.SaleId = await GetSaleRadom(GetCurrentUser().BranchIds, 2);
                                }

                                if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.Email) || string.IsNullOrEmpty(item.Mobile))
                                {
                                    return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin khách hàng" });
                                }

                                model.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    return BadRequest(new { message = "File không được tìm thấy." });
                }

                await CustomerService.ImportData(model, GetCurrentUser());

                return Ok(new { message = "Thêm thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //public async Task<IActionResult> ImportCustomer()
        //{
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        DataSet dsexcelRecords = new DataSet();
        //        IExcelDataReader reader = null;
        //        HttpPostedFile Inputfile = null;
        //        Stream FileStream = null;
        //        using (var db = new lmsDbContext())
        //        {
        //            var model = new List<CustomerCreate>();
        //            if (httpRequest.Files.Count > 0)
        //            {
        //                Inputfile = httpRequest.Files.Get("File");
        //                FileStream = Inputfile.InputStream;
        //                if (Inputfile != null && FileStream != null)
        //                {
        //                    if (Inputfile.FileName.EndsWith(".xls"))
        //                        reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
        //                    else if (Inputfile.FileName.EndsWith(".xlsx"))
        //                        reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
        //                    else
        //                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không đúng định dạng." });
        //                    dsexcelRecords = reader.AsDataSet();
        //                    reader.Close();
        //                    if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
        //                    {
        //                        DataTable dtStudentRecords = dsexcelRecords.Tables[0];
        //                        for (int i = 3; i < dtStudentRecords.Rows.Count; i++)
        //                        {
        //                            var item = new CustomerCreate
        //                            {
        //                                FullName = dtStudentRecords.Rows[i][0].ToString(),
        //                                Email = dtStudentRecords.Rows[i][1].ToString(),
        //                                Mobile = dtStudentRecords.Rows[i][2].ToString(),
        //                            };
        //                            if (GetCurrentUser().RoleId == ((int)RoleEnum.sale))
        //                                item.SaleId = GetCurrentUser().UserInformationId;
        //                            else item.SaleId = await GetSaleRadom(GetCurrentUser().BranchIds, 2);
        //                            if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.Email) || string.IsNullOrEmpty(item.Mobile))
        //                            {
        //                                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Vui lòng điền đầy đủ tin khách hàng" });
        //                            }
        //                            model.Add(item);
        //                        }
        //                    }
        //                    else
        //                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không có dữ liệu." });
        //                }
        //                else
        //                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = "File lỗi." });
        //            }
        //            await CustomerService.ImportData(model, GetCurrentUser());
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thêm thành công" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
        //    }
        //}
        [HttpGet]
        [Route("customer-history-status/{id}")]
        [ClaimsAuthorize]
        public async Task<IActionResult> GetCustomerHistoryStatusById(int id)
        {
            var data = await CustomerService.GetCustomerStatusById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("ImportCustomer/v2")]
        [ClaimsAuthorize]
        public async Task<IActionResult> ImportCustomerV2(string BranchIds)
        {
            try
            {
                var httpRequest = HttpContext.Request;

                var model = new List<CustomerCreate>();
                CustomerImport data = new CustomerImport();

                if (httpRequest.Form.Files.Count > 0)
                {
                    var file = httpRequest.Form.Files.GetFile("File");

                    using (var stream = file.OpenReadStream())
                    {
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;

                            using (var db = new lmsDbContext())
                            {
                                for (int row = 3; row <= rowCount; row++)
                                {
                                    var learningNeedName = worksheet.Cells[row, 4].Value?.ToString();
                                    var purposeName = worksheet.Cells[row, 5].Value?.ToString();
                                    var sourceName = worksheet.Cells[row, 6].Value?.ToString();
                                    var saleName = worksheet.Cells[row, 7].Value?.ToString();

                                    var learningNeedId = db.tbl_LearningNeed.FirstOrDefault(x => x.Name.Contains(learningNeedName))?.Id;
                                    var purposeId = db.tbl_Purpose.FirstOrDefault(x => x.Name.Contains(purposeName))?.Id;
                                    var sourceId = db.tbl_Source.FirstOrDefault(x => x.Name.Contains(sourceName))?.Id;
                                    var saleId = db.tbl_UserInformation.FirstOrDefault(x => x.FullName.Contains(saleName) && x.RoleId == (int)RoleEnum.sale)?.UserInformationId;

                                    var item = new CustomerCreate
                                    {
                                        FullName = worksheet.Cells[row, 1].Value?.ToString(),
                                        Email = worksheet.Cells[row, 2].Value?.ToString(),
                                        Mobile = worksheet.Cells[row, 3].Value?.ToString(),
                                        LearningNeedId = learningNeedId,
                                        PurposeId = purposeId,
                                        SourceId = sourceId,
                                        SaleId = saleId
                                    };

                                    if (GetCurrentUser().RoleId == (int)RoleEnum.sale)
                                    {
                                        item.SaleId = GetCurrentUser().UserInformationId;
                                    }
                                    else
                                    {
                                        item.SaleId = await GetSaleRadom(GetCurrentUser().BranchIds, 2);
                                    }

                                    if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.Email) || string.IsNullOrEmpty(item.Mobile))
                                    {
                                        return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin khách hàng" });
                                    }

                                    model.Add(item);
                                }
                            }
                        }
                    }
                }
                else
                {
                    return BadRequest(new { message = "File không được tìm thấy." });
                }

                data.BranchIds = BranchIds;
                data.DataImports = model;
                await CustomerService.ImportDataV2(data, GetCurrentUser());

                return Ok(new { message = "Thêm thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //public async Task<IActionResult> ImportCustomerV2(string BranchIds)
        //{
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        DataSet dsexcelRecords = new DataSet();
        //        IExcelDataReader reader = null;
        //        HttpPostedFile Inputfile = null;
        //        Stream FileStream = null;
        //        CustomerImport data = new CustomerImport();
        //        using (var db = new lmsDbContext())
        //        {
        //            var model = new List<CustomerCreate>();
        //            if (httpRequest.Files.Count > 0)
        //            {
        //                Inputfile = httpRequest.Files.Get("File");
        //                FileStream = Inputfile.InputStream;
        //                if (Inputfile != null && FileStream != null)
        //                {
        //                    if (Inputfile.FileName.EndsWith(".xls"))
        //                        reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
        //                    else if (Inputfile.FileName.EndsWith(".xlsx"))
        //                        reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
        //                    else
        //                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không đúng định dạng." });
        //                    dsexcelRecords = reader.AsDataSet();
        //                    reader.Close();
        //                    if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
        //                    {
        //                        DataTable dtStudentRecords = dsexcelRecords.Tables[0];
        //                        for (int i = 2; i < dtStudentRecords.Rows.Count; i++)
        //                        {
        //                            var learningNeedName = dtStudentRecords.Rows[i][3].ToString();
        //                            var purposeName = dtStudentRecords.Rows[i][4].ToString();
        //                            var sourceName = dtStudentRecords.Rows[i][5].ToString();
        //                            var saleName = dtStudentRecords.Rows[i][6].ToString();

        //                            var learningNeedId = db.tbl_LearningNeed.FirstOrDefault(x => x.Name.Contains(learningNeedName)).Id;
        //                            var purposeId = db.tbl_Purpose.FirstOrDefault(x => x.Name.Contains(purposeName)).Id;
        //                            var sourceId = db.tbl_Source.FirstOrDefault(x => x.Name.Contains(sourceName)).Id;
        //                            var saleId = db.tbl_UserInformation.FirstOrDefault(x => x.FullName.Contains(saleName) && x.RoleId == (int)RoleEnum.sale).UserInformationId;

        //                            var item = new CustomerCreate
        //                            {
        //                                FullName = dtStudentRecords.Rows[i][0].ToString(),
        //                                Email = dtStudentRecords.Rows[i][1].ToString(),
        //                                Mobile = dtStudentRecords.Rows[i][2].ToString(),
        //                                LearningNeedId = learningNeedId,
        //                                PurposeId = purposeId,
        //                                SourceId = sourceId,
        //                                SaleId = saleId,

        //                            };
        //                            if (GetCurrentUser().RoleId == ((int)RoleEnum.sale))
        //                                item.SaleId = GetCurrentUser().UserInformationId;
        //                            else item.SaleId = await GetSaleRadom(GetCurrentUser().BranchIds, 2);
        //                            if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.Email) || string.IsNullOrEmpty(item.Mobile))
        //                            {
        //                                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Vui lòng điền đầy đủ tin khách hàng" });
        //                            }
        //                            model.Add(item);
        //                        }
        //                    }
        //                    else
        //                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không có dữ liệu." });
        //                }
        //                else
        //                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = "File lỗi." });
        //            }
        //            data.BranchIds = BranchIds;
        //            data.DataImports = model;
        //            await CustomerService.ImportDataV2(data, GetCurrentUser());
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thêm thành công" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
        //    }

        //}
        /// <summary>
        /// Xuất file mẫu thêm khách hàng tiềm năng
        /// </summary>
        /// <param name="branchId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("export-sample-excel")]
        [ClaimsAuthorize]
        public async Task<IActionResult> ExportToSampleExcel(int branchId)
        {
            try
            {
                string baseUrl = Request.Scheme + "://" + Request.Host;
                var result = CustomerService.ExportCustomer(branchId, baseUrl, GetCurrentUser());
                if (result.Result == null) return StatusCode((int)HttpStatusCode.BadRequest);
                return StatusCode((int)HttpStatusCode.OK, new { statusCode = (int)HttpStatusCode.OK, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }

        }
        [HttpPost]
        [Route("view-import-customer")]
        [ClaimsAuthorize]
        public async Task<IActionResult> ViewImportCustomer()
        {
            try
            {
                var httpRequest = HttpContext.Request;

                var data = new List<CustomerModel>();

                if (httpRequest.Form.Files.Count > 0)
                {
                    var file = httpRequest.Form.Files.GetFile("File");

                    using (var stream = file.OpenReadStream())
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;

                            for (int row = 4; row <= rowCount; row++)
                            {
                                var item = new CustomerModel
                                {
                                    FullName = worksheet.Cells[row, 1].Value?.ToString(),
                                    Email = worksheet.Cells[row, 2].Value?.ToString(),
                                    Mobile = worksheet.Cells[row, 3].Value?.ToString(),
                                    LearningNeedName = worksheet.Cells[row, 4].Value?.ToString(),
                                    PurposeName = worksheet.Cells[row, 5].Value?.ToString(),
                                    SourceName = worksheet.Cells[row, 6].Value?.ToString(),
                                    SaleName = worksheet.Cells[row, 7].Value?.ToString(),
                                    ParentName = worksheet.Cells[row, 8].Value?.ToString(),
                                    ParentEmail = worksheet.Cells[row, 9].Value?.ToString(),
                                    ParentMobile = worksheet.Cells[row, 10].Value?.ToString(),
                                    EntryPoint = worksheet.Cells[row, 11].Value?.ToString(),
                                    DesiredOutputScore = worksheet.Cells[row, 12].Value?.ToString(),
                                    DesiredProgram = worksheet.Cells[row, 13].Value?.ToString(),
                                };

                                if (string.IsNullOrEmpty(item.FullName))
                                {
                                    return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin khách hàng" });
                                }

                                data.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    return BadRequest(new { message = "File không được tìm thấy." });
                }

                return Ok(new { message = "Thành công !", totalRow = data.Count, data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //public async Task<IActionResult> ViewImportCustomer()
        //{
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        DataSet dsexcelRecords = new DataSet();
        //        IExcelDataReader reader = null;
        //        HttpPostedFile Inputfile = null;
        //        Stream FileStream = null;
        //        using (var db = new lmsDbContext())
        //        {
        //            var data = new List<CustomerModel>();
        //            if (httpRequest.Files.Count > 0)
        //            {
        //                Inputfile = httpRequest.Files.Get("File");
        //                FileStream = Inputfile.InputStream;
        //                if (Inputfile != null && FileStream != null)
        //                {
        //                    if (Inputfile.FileName.EndsWith(".xls"))
        //                        reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
        //                    else if (Inputfile.FileName.EndsWith(".xlsx"))
        //                        reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
        //                    else
        //                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không đúng định dạng." });
        //                    dsexcelRecords = reader.AsDataSet();
        //                    reader.Close();
        //                    if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
        //                    {
        //                        DataTable dtCustomerRecords = dsexcelRecords.Tables[0];
        //                        for (int i = 2; i < dtCustomerRecords.Rows.Count; i++)
        //                        {
        //                            var item = new CustomerModel
        //                            {
        //                                FullName = dtCustomerRecords.Rows[i][0].ToString(),
        //                                Email = dtCustomerRecords.Rows[i][1].ToString(),
        //                                Mobile = dtCustomerRecords.Rows[i][2].ToString(),
        //                                LearningNeedName = dtCustomerRecords.Rows[i][3].ToString(),
        //                                PurposeName = dtCustomerRecords.Rows[i][4].ToString(),
        //                                SourceName = dtCustomerRecords.Rows[i][5].ToString(),
        //                                SaleName = dtCustomerRecords.Rows[i][6].ToString(),
        //                            };
        //                            if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.Email) || string.IsNullOrEmpty(item.Mobile))
        //                            {
        //                                return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Vui lòng điền đầy đủ tin khách hàng" });
        //                            }
        //                            data.Add(item);
        //                        }
        //                    }
        //                    else
        //                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Không có dữ liệu." });
        //                }
        //                else
        //                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = "File lỗi." });
        //            }
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.Count, data = data });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
        //    }

        //}
        [HttpPost]
        [Route("insert-after-import")]
        [ClaimsAuthorize]
        public async Task<IActionResult> InsertAffterImport([FromBody] List<CustomerAfterImport> model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CustomerService.InsertAffterImport(model, GetCurrentUser());
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.Count, data = data });

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
        [Route("excel")]
        [ClaimsAuthorize]
        public async Task<IActionResult> ExportCustomerToExcel([FromQuery] CustomerExportSearch baseSearch)
        {
            var data = await CustomerService.RepairDataCustomerToExport(baseSearch, GetCurrentUser());
            string folder = "CustomerExport";
            string template = "Export_Customer.xlsx";
            string fileNameToSave = "DanhSachKhachHang";
            string baseUrl = Request.Scheme + "://" + Request.Host;
            var result = ExcelExportService.ExportV3(data, template, fileNameToSave, folder, baseUrl);
            return StatusCode((int)HttpStatusCode.OK, new { statusCode = (int)HttpStatusCode.OK, data = result });
        }
        [HttpPost]
        [Route("website")]
        public async Task<IActionResult> InsertWebsite([FromBody] CustomerCreateByWebsite model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //var apiKey = Request.Headers.GetValues("Api-key").FirstOrDefault();
                    var apiKey = HttpContext.Request.Headers["Api-key"].FirstOrDefault();

                    if (apiKey != "website-key-2024")
                        return StatusCode((int)HttpStatusCode.Unauthorized, new { message = "API KEY không phù hợp"});
                    
                    var itemModel = new CustomerCreate
                    {
                        FullName = model.FullName,
                        Mobile = model.Mobile,
                        Email = model.Email,
                        Address = model.Address,
                        BranchId = model.BranchId
                    };
                    var data = await CustomerService.Insert(itemModel, new tbl_UserInformation
                    { 
                        FullName = "Website"
                    });
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

        /// <summary>
        /// Thêm 1 tư vấn viên cho nhiều khách hàng
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="branchId"></param>
        /// <param name="saleId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("customers-for-saler")]
        [ClaimsAuthorize]
        public async Task<IActionResult> CustomersForSaler([FromBody] List<SetCustomerForSaler> itemModel, int branchId, int saleId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await CustomerService.CustomersForSaler(itemModel, branchId, saleId, GetCurrentUser());
                    if (data == true) return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !"});
                    else return StatusCode((int)HttpStatusCode.BadRequest, new { message = "Thất bại !" });
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }
            var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }
        /// <summary>
        /// Lấy danh sách khách hàng cho Popup thêm 1 tư vấn viên cho nhiều khách hàng
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("customer-search")]
        [ClaimsAuthorize]
        public async Task<IActionResult> CustomerForSalerSearch([FromQuery] CustomerForSalerSearch baseSearch)
        {
            try
            {
                var data = await CustomerService.GetCustomerForSaler(baseSearch, GetCurrentUser());
                if (data.TotalRow == 0)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }

    }
}
