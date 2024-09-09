using LMSCore.Areas.Models;
using LMSCore.LMS;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class WriteLogService
    {
        public class WriteLogCreate
        {
            public string Note { get; set; }
        }
        public static async Task<tbl_WriteLog> Insert(WriteLogCreate itemModel)
        {
            using (var db = new lmsDbContext())
            {
                var item = new tbl_WriteLog
                {
                    Note = itemModel.Note
                };
                db.tbl_WriteLog.Add(item);
                await db.SaveChangesAsync();
                return item;
            }
        }
        public static async Task<List<tbl_WriteLog>> GetAll()
        {
            using (var db = new lmsDbContext())
            {
                return await db.tbl_WriteLog.OrderByDescending(x => x.Id).ToListAsync();
            }
        }
    }
}