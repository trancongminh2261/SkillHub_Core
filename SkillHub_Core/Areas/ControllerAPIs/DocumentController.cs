using ExcelDataReader;
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
using LMSCore.Models;
using LMSCore.LMS;


namespace LMS_Project.Areas.ControllerAPIs
{
    [ClaimsAuthorize]
    public class DocumentController : BaseController
    {
        private lmsDbContext dbContext;
        private DocumentService documentService;
        private string[] allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
        public DocumentController()
        {
            this.dbContext = new lmsDbContext();
            this.documentService = new DocumentService(this.dbContext);
        }

        [HttpGet]
        [Route("api/Document/{id:int}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var data = await documentService.GetById(Id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("api/Document")]
        public async Task<IActionResult> GetAll([FromQuery] DocumentSearch baseSearch)
        {
            var data = await documentService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
        [HttpPut]
        [Route("api/Document")]
        public async Task<IActionResult> Update()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string link = "";
                    string baseUrl = $"{Request.Scheme}://{Request.Host}";

                    // Đọc file từ Request
                    var file = Request.Form.Files.FirstOrDefault();
                    string topicId = Request.Form["TopicId"];
                    string currentId = Request.Form["Id"];

                    if (file != null && !string.IsNullOrEmpty(topicId) && !string.IsNullOrEmpty(currentId))
                    {
                        DocumentUpdate request = new DocumentUpdate();
                        string ext = Path.GetExtension(file.FileName).ToLower();

                        string fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine($"{baseUrl}/Upload/Documents", fileName); // Đường dẫn lưu file trên server
                        link = $"{baseUrl}/Upload/Documents/{fileName}";

                        // Tạo thư mục nếu chưa tồn tại
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                        // Lưu file
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // Update thông tin
                        request.Id = int.Parse(currentId);
                        request.TopicId = int.Parse(topicId);
                        request.FileName = file.FileName;
                        request.FileType = ext;
                        request.AbsolutePath = link;

                        var data = await documentService.Update(request, GetCurrentUser());
                        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.NOT_FOUND });
                    }
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }

            var message = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }


        [HttpPost]
        [Route("api/Document")]
        public async Task<IActionResult> Insert()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string link = "";
                    string baseUrl = $"{Request.Scheme}://{Request.Host}";

                    // Lấy file và các thông tin từ form
                    var file = Request.Form.Files.FirstOrDefault();
                    string topicId = Request.Form["TopicId"];

                    if (file != null && !string.IsNullOrEmpty(topicId))
                    {
                        DocumentCreate request = new DocumentCreate();
                        string ext = Path.GetExtension(file.FileName).ToLower();

                        string fileName = Guid.NewGuid() + ext;
                        var path = Path.Combine($"{baseUrl}/Upload/Documents", fileName); // Đường dẫn lưu file trên server
                        link = $"{baseUrl}/Upload/Documents/{fileName}";

                        // Tạo thư mục nếu chưa tồn tại
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                        // Lưu file
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // Insert thông tin
                        request.TopicId = int.Parse(topicId);
                        request.FileName = file.FileName;
                        request.FileType = ext;
                        request.AbsolutePath = link;
                        var data = await documentService.Insert(request, GetCurrentUser());

                        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công!", data });
                    }
                    else
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = ApiMessage.NOT_FOUND });
                    }
                }
                catch (Exception e)
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
                }
            }

            var message = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage;
            return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        }


        [HttpDelete]
        [Route("api/Document/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await documentService.Delete(id);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
    }
}
