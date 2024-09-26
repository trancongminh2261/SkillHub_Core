using LMS_Project.Areas.Models;
using LMS_Project.Areas.Request;

using LMS_Project.Models;
using LMSCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace LMS_Project.Services
{
    public class DomainService
    {
        protected lmsDbContext dbContext;
        //protected IHttpContextAccessor _httpContextAccessor;
        public DomainService(lmsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        /*public DomainService(lmsDbContext dbContext, IHttpContextAccessor _httpContextAccessor)
        {
            this.dbContext = dbContext;
            this._httpContextAccessor = _httpContextAccessor;
        }*/
    }
}