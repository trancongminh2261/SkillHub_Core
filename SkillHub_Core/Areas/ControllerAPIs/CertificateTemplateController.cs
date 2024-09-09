﻿using LMSCore.Areas.Models;
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
    [Route("api/CertificateTemplate")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class CertificateTemplateController : BaseController
    {
        private lmsDbContext dbContext;
        private CertificateTemplateService domainService;
        public CertificateTemplateController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new CertificateTemplateService(this.dbContext);
        }
        [HttpGet]
        [Route("guide")]
        public async Task<IActionResult> GetGuide()
        {
            var data = await domainService.GetGuide();
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]CertificateTemplateCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await domainService.Insert(model, GetCurrentUser());
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
        public async Task<IActionResult> Update([FromBody]CertificateTemplateUpdate model)
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
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await domainService.GetById(id);
                if (data == null)
                    return StatusCode((int)HttpStatusCode.NoContent);
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
            }
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] SearchOptions baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}