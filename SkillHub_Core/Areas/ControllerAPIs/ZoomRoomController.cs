//using ExcelDataReader;
//using LMSCore.Areas.Models;
//using LMSCore.Areas.Request;
//using LMSCore.LMS;
//using LMSCore.Models;
//using LMSCore.Services;
//using LMSCore.Users;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Web;
//
//using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
//using static LMSCore.Models.lmsEnum;

//namespace LMSCore.Areas.ControllerAPIs
//{
//    [ClaimsAuthorize]
//    public class ZoomRoomController : BaseController
//    {
//        public object LockObject = new object();
//        [HttpPost]
//        [Route("api/ZoomRoom/CreateRoom/{seminarId}")]
//        public IActionResult CreateRoom(int seminarId)
//        {
//            ///Không cho tạo phòng cùng lúc
//            lock (LockObject)
//            {
//                try
//                {
//                    var data = Task.Run(() => ZoomRoomService.CreateRoom(seminarId, GetCurrentUser())).Result;
//                    if(data.Success == false) 
//                        return StatusCode((int)HttpStatusCode.BadRequest, new { message = data.ResultMessage });
//                    LockObject = new object();
//                    return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data.Data });
//                }
//                catch (Exception e)
//                {
//                    return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
//                }
//            }
//        }
//        [HttpPut]
//        [Route("api/ZoomRoom/CloseRoom/{seminarId}")]
//        public async Task<IActionResult> CloseRoom(int seminarId)
//        {
//            try
//            {
//                await ZoomRoomService.CloseRoom(seminarId, GetCurrentUser());
//                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !" });
//            }
//            catch (Exception e)
//            {
//                return StatusCode((int)HttpStatusCode.BadRequest, new { message = e.Message });
//            }
//        }
//        [HttpGet]
//        [Route("api/ZoomRoom/GetActive")]
//        public async Task<IActionResult> GetActive([FromQuery] SearchOptions search)
//        {
//            var data = await ZoomRoomService.GetActive(search);
//            if (data.TotalRow == 0)
//                return StatusCode((int)HttpStatusCode.NoContent);
//            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
//        }
//        [HttpGet]
//        [Route("api/ZoomRoom/GetRecord/{seminarId}")]
//        public async Task<IActionResult> GetRecord(int seminarId)
//        {
//            var data = await ZoomRoomService.GetRecord(seminarId);
//            if (!data.Any())
//                return StatusCode((int)HttpStatusCode.NoContent);
//            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data = data });
//        }
//    }
//}
