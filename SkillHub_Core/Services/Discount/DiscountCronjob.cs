using LMSCore.Areas.Request;
using LMSCore.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace LMSCore.Services.Discount
{
    public class DiscountCronjob
    {
        /// <summary>
        /// Cập nhật trạng thái khuyến mãi khi hết hạn
        /// </summary>
        /// <returns></returns>
        public static async Task Expired()
        {
            using (var db = new lmsDbContext())
            {
                DateTime today = DateTime.Now.AddDays(1).Date;
                var discounts = await db.tbl_Discount.Where(x => x.Status == 1).ToListAsync();
                if (discounts.Any())
                {
                    foreach (var item in discounts)
                    {
                        if (item.UsedQuantity >= item.Quantity)
                        {
                            if (item.Status != 2)
                            {
                                item.Status = 2;
                                item.StatusName = "Đã kết thúc";
                            }
                        }
                        if (item.Expired <= today)
                        {
                            if (item.Status != 2)
                            {
                                item.Status = 2;
                                item.StatusName = "Đã kết thúc";
                            }
                        }
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
