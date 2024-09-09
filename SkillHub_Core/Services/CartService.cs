using LMSCore.Areas.Models;
using LMSCore.Areas.Request;
using LMSCore.LMS;
using LMSCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Services
{
    public class CartService
    {
        public static async Task<tbl_Cart> GetById(int id)
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Cart.SingleOrDefaultAsync(x => x.Id == id);
                return data;
            }
        }
        public static async Task<tbl_Cart> Insert(CartCreate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var product = await db.tbl_Product.SingleOrDefaultAsync(x => x.Id == itemModel.ProductId);
                if (product == null)
                    throw new Exception("Không tìm thấy sản phẩm");
                var model = await db.tbl_Cart
                    .FirstOrDefaultAsync(x => x.UserId == user.UserInformationId && x.ProductId == itemModel.ProductId && x.Enable == true);
                if (product.Type == 2)// Không cho phép mua trùng bộ đề
                {
                    var checkCart = await db.tbl_Cart.AnyAsync(x => x.UserId == user.UserInformationId && x.ProductId == product.Id && x.Enable == true);
                    if (checkCart)
                        throw new Exception("Bạn đã có bộ đề này trong giỏ hàng");

                    var checkPackageStudent = await db.tbl_PackageStudent
                        .AnyAsync(x => x.StudentId == user.UserInformationId && x.PackageId == product.Id && x.Enable == true);
                    if (checkPackageStudent)
                        throw new Exception("Bạn đã mua bộ đề này");

                    var billDetails = await db.tbl_BillDetail
                        .Where(x => x.ProductId == product.Id && x.StudentId == user.UserInformationId && x.Enable == true).ToListAsync();
                    if (billDetails.Any())
                    {
                        foreach (var item in billDetails)
                        {
                            var checkBill = await db.tbl_Bill.AnyAsync(x => x.Id == item.BillId && x.Debt > 0);
                            if (checkBill)
                                throw new Exception("Bạn đã đặt mua bộ đề này, vui lòng thanh toán để nhận");
                        }
                    }
                }
                if (model != null)
                {
                    model.Quantity += itemModel.Quantity;
                }
                else
                {
                    model = new tbl_Cart(itemModel);
                    model.UserId = user.UserInformationId;
                    model.CreatedBy = model.ModifiedBy = user.FullName;
                    db.tbl_Cart.Add(model);
                }
                if (model.Quantity <= 0)
                    throw new Exception("Số lượng không phù hợp");
                await db.SaveChangesAsync();
                return model;
            }
        }
        public static async Task<tbl_Cart> Update(CartUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Cart.SingleOrDefaultAsync(x => x.Id == itemModel.Id.Value);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Quantity = itemModel.Quantity ?? entity.Quantity;
                entity.ModifiedBy = user.FullName;
                entity.ModifiedOn = DateTime.Now;

                if (entity.Quantity <= 0)
                    throw new Exception("Số lượng không phù hợp");
                await db.SaveChangesAsync();
                return entity;
            }
        }
        public static async Task Delete(int id)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Cart.SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new Exception("Không tìm thấy dữ liệu");
                entity.Enable = false;
                await db.SaveChangesAsync();
            }
        }
        public static async Task<List<tbl_Cart>> GetMyCart(tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                string sql = $"Get_Cart @UserId = N'{user.UserInformationId}'";
                var data = await db.SqlQuery<Get_Cart>(sql);
                var result = data.Select(i => new tbl_Cart(i)).ToList();
                return result;
            }
        }
    }
}