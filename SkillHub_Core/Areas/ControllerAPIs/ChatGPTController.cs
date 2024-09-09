using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.DTO.SampleTranscript;
using LMSCore.Models;
using LMSCore.Services.SampleTranscript;
using LMSCore.Users;
using LMSCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using LMSCore.Services.ChatGPT;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using IronOcr;
using LMSCore.DTO.BandDescriptor;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/ChatGPT")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ChatGPTController : BaseController
    {
        private lmsDbContext dbContext;
        private ChatGPTService domainService;
        private readonly IronTesseract _ocr;

        public ChatGPTController(IWebHostEnvironment env)
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new ChatGPTService(this.dbContext);
            _ocr = new IronTesseract();
        }

        [HttpPost]
        [Route("scan")]
        public async Task<IActionResult> ScanImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var ocrResult = await Task.Run(() =>
                    {
                        using (var ocrInput = new OcrInput())
                        {
                            ocrInput.LoadImage(stream);
                            // Optionally apply filters if needed
                            // ocrInput.Deskew();  // use only if image is not straight
                            // ocrInput.DeNoise(); // use only if image contains digital noise

                            return _ocr.Read(ocrInput);
                        }
                    });

                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = ocrResult.Text });
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        [Route("check-writing")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckWriting([FromBody] CheckWritingRequestPost request)
        {
            try
            {
                var data = await domainService.CheckWriting(request, GetCurrentUser());
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpPost]
        [Route("band-descriptor")]
        [ProducesResponseType(typeof(IList<BandDescriptorOption>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBandDescriptor([FromQuery] int taskOrder)
        {
            try
            {
                var data = await domainService.GetBandDescriptor(taskOrder);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }      
    }
}
