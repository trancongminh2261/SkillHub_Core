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
    [Route("api/ClassChange")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class ClassChangeController : BaseController
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await ClassChangeService.GetById(id);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]ClassChangeCreate model)
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
                            var data = await ClassChangeService.Insert(model, GetCurrentUser(), db, baseUrl);
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
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] ClassChangeSearch baseSearch)
        {
            var data = await ClassChangeService.GetAll(baseSearch, GetCurrentUser());
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}
