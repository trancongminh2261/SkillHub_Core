using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;


namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/TraningRoute")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class TraningRouteController : BaseController
    {
        //[HttpGet]
        //[Route("{id}")]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    var data = await TraningRouteService.GetById(id);
        //    if (data == null)
        //        return StatusCode((int)HttpStatusCode.NoContent);
        //    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
        //}
        //[HttpPost]
        //[Route("")]
        //public async Task<IActionResult> Insert([FromBody]TraningRouteCreate model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var data = await TraningRouteService.Insert(model, GetCurrentUser());
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}
        //[HttpPut]
        //[Route("")]
        //public async Task<IActionResult> Update([FromBody]TraningRouteUpdate model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var data = await TraningRouteService.Update(model, GetCurrentUser());
        //            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //        }
        //    }
        //    var message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
        //    return StatusCode((int)HttpStatusCode.BadRequest, new { message = message });
        //}
        //[HttpDelete]
        //[Route("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        await TraningRouteService.Delete(id);
        //        return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
        //    }
        //}
        ///// <summary>
        ///// Nhân viên thuộc trung nào chỉ thấy học viên ở trung tâm đó, Tư vấn viên chỉ thấy khách hàng của chính mình
        ///// </summary>
        ///// <param name="baseSearch"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("")]
        //public async Task<IActionResult> GetAll([FromQuery] TraningRouteSearch baseSearch)
        //{
        //    var data = await TraningRouteService.GetAll(baseSearch,GetCurrentUser());
        //    if (data.TotalRow == 0)
        //        return StatusCode((int)HttpStatusCode.NoContent);
        //    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        //}
    }
}
