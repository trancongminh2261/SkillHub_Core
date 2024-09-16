﻿using ExcelDataReader;
using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;
using LMS_Project.Models;
using LMS_Project.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using LMSCore.Users;
using Microsoft.AspNetCore.Mvc;
using LMSCore.Areas.ControllerAPIs;
using static LMSCore.Models.lmsEnum;
using LMSCore.LMS;

namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class UserInformationController : BaseController
    {
        
        //[HttpPost]
        //[Route("api/UserInformation/Upload")]
        //[ClaimsAuthorize(new lmsEnum.RoleEnum[] { 
        //    lmsEnum.RoleEnum.admin,
        //    lmsEnum.RoleEnum.student,
        //    lmsEnum.RoleEnum.teacher
        //})]
        //public IActionResult Upload()
        //{
        //    try
        //    {
        //        string link = "";
        //        string baseUrl = Request.Scheme + "://" + Request.Host;
        //        var file = HttpContext.Request.Files.Get("File");
        //        if (file != null)
        //        {
        //            WebImage img = new WebImage(file.InputStream);
        //            if (img.Width > 750)
        //                img.Resize(750, 750);

        //            string ext = Path.GetExtension(file.FileName).ToLower();
        //            string fileName = Guid.NewGuid() + ext; // getting File Name
        //            string fileExtension = Path.GetExtension(fileName).ToLower();
        //            var result = AssetCRM.isValIdFileCustom(ext); // ValIdate Header
        //            if (result)
        //            {
        //                fileName = Guid.NewGuid() + ext;
        //                var path = Path.Combine($"{baseUrl}/Upload/Images/"), fileName);
        //                string strPathAndQuery = HttpContext.Request.Url.PathAndQuery;
        //                string strUrl = HttpContext.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
        //                link = strUrl + "Upload/Images/" + fileName;
        //                img.Save(path);
        //                return StatusCode((int)HttpStatusCode.OK, new { data = link, message = ApiMessage.SAVE_SUCCESS });
        //            }
        //            else
        //            {
        //                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.INVALID_FILE });
        //            }
        //        }
        //        else
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.NOT_FOUND });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
        //    }
        //}
        [HttpPost]
        [Route("api/UserInformation")]
        public async Task<IActionResult> Insert([FromBody] UserCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await UserInformation.ValidateUser(model.UserName, model.Email);
                    var data = await UserInformation.Insert(new tbl_UserInformation(model), GetCurrentUser());
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
        [Route("api/UserInformation")]
        public async Task<IActionResult> Update([FromBody] UserUpdate model)
        {
            if (ModelState.IsValid)
            {
                var user = GetCurrentUser();
                if(user.RoleId != ((int)RoleEnum.admin) && user.UserInformationId != model.UserInformationId)
                    return StatusCode((int)HttpStatusCode.Unauthorized, new { message = "Không được phép thực hiện!"});
                try
                {
                    var data = await UserInformation.Update(new tbl_UserInformation(model), user);
                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công", data = data });
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
        [Route("api/UserInformation/{userInformationId}")]
        public async Task<IActionResult> Delete(int userInformationId)
        {
            try
            {
                await UserInformation.Delete(userInformationId);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }

        }
        [HttpGet]
        [Route("api/UserInformation")]
        public async Task<IActionResult> GetAll([FromQuery] UserSearch baseSearch)
        {
            var data = await UserInformation.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpGet]
        [Route("api/UserInformation/{userInformationId}")]
        public async Task<IActionResult> GetById(int userInformationId)
        {
            var data = await UserInformation.GetById(userInformationId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !",data = new UserInformationModel(data) });
        }
        public class OneSignalModel
        { 
            public string OneSignal_deviceId { get; set; }
        }
        [HttpPut]
        [Route("api/Update_OneSignal_DeviceId")]
        public async Task<IActionResult> Update_OneSignal_DeviceId([FromBody] OneSignalModel model)
        {
            var data = await UserInformation.Update_OneSignal_DeviceId(model.OneSignal_deviceId, GetCurrentUser());
            return StatusCode((int)HttpStatusCode.OK);
        }
        [HttpPost]
        [Route("api/UserInformation/ImportStudent")]
        public async Task<IActionResult> ImportStudent()
        {
            try
            {
                // Lấy HttpRequest từ HttpContext
                var httpRequest = HttpContext.Request;

                // Kiểm tra xem request có chứa file hay không
                if (httpRequest.Form.Files.Count == 0)
                {
                    return BadRequest(new { message = "Không có file nào được tải lên." });
                }

                // Lấy file từ request
                var file = httpRequest.Form.Files[0];
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File không hợp lệ." });
                }

                DataSet dsexcelRecords = new DataSet();
                IExcelDataReader reader = null;

                // Đọc file stream
                using (var stream = file.OpenReadStream())
                {
                    if (file.FileName.EndsWith(".xls"))
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else if (file.FileName.EndsWith(".xlsx"))
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    else
                        return BadRequest(new { message = "Không đúng định dạng." });

                    dsexcelRecords = reader.AsDataSet();
                    reader.Close();
                }

                if (dsexcelRecords == null || dsexcelRecords.Tables.Count == 0)
                {
                    return BadRequest(new { message = "Không có dữ liệu." });
                }

                var model = new List<RegisterModel>();
                DataTable dtStudentRecords = dsexcelRecords.Tables[0];

                for (int i = 2; i < dtStudentRecords.Rows.Count; i++)
                {
                    var item = new RegisterModel
                    {
                        FullName = dtStudentRecords.Rows[i][0].ToString(),
                        UserName = dtStudentRecords.Rows[i][1].ToString(),
                        Email = dtStudentRecords.Rows[i][2].ToString(),
                        Mobile = dtStudentRecords.Rows[i][3].ToString(),
                        Password = Encryptor.Encrypt(dtStudentRecords.Rows[i][4].ToString())
                    };

                    if (string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.UserName))
                    {
                        return BadRequest(new { message = "Vui lòng điền đầy đủ họ tên và tài khoản đăng nhập." });
                    }

                    model.Add(item);
                }

                await UserInformation.ImportData(model, GetCurrentUser());
                return Ok(new { message = "Thêm thành công" });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/UserInformation/learning-progress/{userId}")]
        public async Task<IActionResult> LearningProgress(int userId)
        {
            try
            { 
                var data = await UserInformation.LearningProgress(userId);
                if (data == null)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
