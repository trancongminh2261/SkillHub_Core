﻿using LMSCore.Areas.Models;
using LMSCore.LMS;
using LMSCore.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMSCore.Services
{
    public class Ward
    {
        public static async Task<tbl_Ward> GetbyId(int Id)
        {
            using (var db = new lmsDbContext())
            {
                var l = await db.tbl_Ward.SingleOrDefaultAsync(x => x.Id == Id);
                return l;
            }

        }
        public static async Task<ObjectResult> GetAll(WardSearch search)
        {
            using (var db = new lmsDbContext())
            {
                if (search == null) return new ObjectResult { obj = null, TotalRow = 0 };
                var l = await db.tbl_Ward.Where(x => x.Enable == true
                && x.DistrictId == (search.DistrictId == null ? x.DistrictId : search.DistrictId)
                && (x.Name.Contains(search.Name ?? "") || string.IsNullOrEmpty(search.Name))).OrderBy(x => x.Name).ToListAsync();
                int totalRow = l.Count();
                var result = l.Skip((search.PageIndex - 1) * search.PageSize).Take(search.PageSize).ToList();
                return new ObjectResult { obj = result, TotalRow = totalRow };

            }
        }
    }
}