using LMSCore.Areas.Models;
using LMSCore.Models;
using LMSCore.Services;
using LMSCore.Users;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities; 

namespace LMSCore.Areas.ControllerAPIs
{
    [Route("api/LearningHistory")]
    [ClaimsAuthorize]
    [ValidateModelState]
    public class LearningHistoryController : BaseController
    {
        private lmsDbContext dbContext;
        private LearningHistoryService domainService;
        public LearningHistoryController()
        {
            this.dbContext = new lmsDbContext();
            this.domainService = new LearningHistoryService(this.dbContext);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await domainService.GetById(id);
            if (data != null)
                return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", data });
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] LearningHistorySearch baseSearch)
        {
            var data = await domainService.GetAll(baseSearch);
            if (data.TotalRow == 0)
                return StatusCode((int)HttpStatusCode.NoContent);
            return StatusCode((int)HttpStatusCode.OK, new { message = "Thành công !", totalRow = data.TotalRow, data = data.Data });
        }
    }
}