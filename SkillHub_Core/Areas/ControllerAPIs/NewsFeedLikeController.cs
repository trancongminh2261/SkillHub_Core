using LMSCore.Areas.Request;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using LMSCore.Utilities;

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/NewsFeedLike")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class NewsFeedLikeController : BaseController
    {
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Insert([FromBody]NewsFeedLikeCreate model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = await NewsFeedLikeService.Insert(model, GetCurrentUser());
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
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll(int? NewsFeedId)
        {
            var data = await NewsFeedLikeService.GetAll(NewsFeedId);
            if (data == null)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow ,data = data.Data });
        }
    }
}
