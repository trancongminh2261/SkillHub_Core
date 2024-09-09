using LMSCore.Areas.Request;
using LMSCore.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class CurriculumInClassService
    {       
        public static async Task<tbl_CurriculumInClass> Update (CurriculumInClassUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_CurriculumInClass.SingleOrDefaultAsync(x => x.Id == itemModel.Id && x.Enable == true);
                if (entity == null)
                    throw new Exception("Không tìm thấy giáo trình");
                entity.Name = string.IsNullOrEmpty(itemModel.Name) ? entity.Name : itemModel.Name;
                await db.SaveChangesAsync();
                return entity;
            }
        }



        public static async Task<List<Get_CurriculumInClass>> GetCurriculumInClass(int classId)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_CurriculumInClass @ClassId = N'{classId}' ";
                var data = await db.SqlQuery<Get_CurriculumInClass>(sql);
                return data;
            }    
        }
    }


}