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
    public class TemplateService
    {
        public class Guide
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public static async Task<List<Guide>> GetGuide(int type)
        {
            var result = new List<Guide>();
            if (type == 1)//Hợp đồng
            {
                result.Add(new Guide { Code = "{HoVaTen}", Name = "Họ và tên" });
                result.Add(new Guide { Code = "{MaHocVien}", Name = "Mã học viên" });
                result.Add(new Guide { Code = "{NgaySinh}", Name = "Ngày sinh" });
                result.Add(new Guide { Code = "{SoDienThoai}", Name = "Số điện thoại" });
                result.Add(new Guide { Code = "{EmailHocVien}", Name = "EmailHocVien" });
                result.Add(new Guide { Code = "{Ngay}", Name = "Ngày" });
                result.Add(new Guide { Code = "{Thang}", Name = "Tháng" });
                result.Add(new Guide { Code = "{Nam}", Name = "Năm" });
            }
            else if (type == 2)//Điều khoản
            {
                result.Add(new Guide { Code = "{TenCongTy}", Name = "Tên công ty" });
            }
            else if (type == 3)//Mẫu phiếu thu
            {
                result.Add(new Guide { Code = "{HoVaTen}", Name = "Họ và tên" });
                result.Add(new Guide { Code = "{MaHocVien}", Name = "Mã học viên" });
                result.Add(new Guide { Code = "{Ngay}", Name = "Ngày" });
                result.Add(new Guide { Code = "{Thang}", Name = "Tháng" });
                result.Add(new Guide { Code = "{Nam}", Name = "Năm" });
                result.Add(new Guide { Code = "{LyDo}", Name = "Lý do" });
                result.Add(new Guide { Code = "{SoTienThu}", Name = "Số tiền thu" });
                result.Add(new Guide { Code = "{NguoiThu}", Name = "Người thu" });
            }
            else if (type == 4)//Mẫu phiếu chi
            {
                result.Add(new Guide { Code = "{HoVaTen}", Name = "Họ và tên" });
                result.Add(new Guide { Code = "{MaHocVien}", Name = "Mã học viên" });
                result.Add(new Guide { Code = "{Ngay}", Name = "Ngày" });
                result.Add(new Guide { Code = "{Thang}", Name = "Tháng" });
                result.Add(new Guide { Code = "{Nam}", Name = "Năm" });
                result.Add(new Guide { Code = "{LyDo}", Name = "Lý do" });
                result.Add(new Guide { Code = "{SoTienChi}", Name = "Số tiền chi" });
                result.Add(new Guide { Code = "{NguoiChi}", Name = "Người Chi" });
            }
            else if (type == 5)//Mẫu thư mời phỏng vấn
            {
                result.Add(new Guide { Code = "{HoVaTen}", Name = "Họ và tên" });
                result.Add(new Guide { Code = "{ViTriUngTuyen}", Name = "Vị trí ứng tuyển" });
                result.Add(new Guide { Code = "{ThoiGian}", Name = "Thời gian" });
                result.Add(new Guide { Code = "{NgayThangNam}", Name = "Ngày / Tháng / Năm" });
                result.Add(new Guide { Code = "{DiaChi}", Name = "Địa chỉ" });
                result.Add(new Guide { Code = "{PhuongXa}", Name = "Phường / Xã" });
                result.Add(new Guide { Code = "{QuanHuyen}", Name = "Quận / Huyện" });
                result.Add(new Guide { Code = "{TinhThanhPho}", Name = "Tỉnh / Thành Phố" });
                result.Add(new Guide { Code = "{TenLienHe}", Name = "Tên người liên hệ" });
                result.Add(new Guide { Code = "{DienThoaiLienHe}", Name = "Số Điện thoại người liên hệ" });
            }
            else if (type == 6)//Mẫu thông báo đậu phỏng vấn
            {
                result.Add(new Guide { Code = "{HoVaTen}", Name = "Họ và tên" });
                result.Add(new Guide { Code = "{ViTriUngTuyen}", Name = "Vị trí ứng tuyển" });
                result.Add(new Guide { Code = "{NgayThangNam}", Name = "Ngày / Tháng / Năm" });
                result.Add(new Guide { Code = "{ThoiGianBatDau}", Name = "Thời gian bắt đầu" });
                result.Add(new Guide { Code = "{ThoiGianKetThuc}", Name = "Thời gian kết thúc" });         
                result.Add(new Guide { Code = "{DiaChi}", Name = "Địa chỉ" });
                result.Add(new Guide { Code = "{PhuongXa}", Name = "Phường / Xã" });
                result.Add(new Guide { Code = "{QuanHuyen}", Name = "Quận / Huyện" });
                result.Add(new Guide { Code = "{TinhThanhPho}", Name = "Tỉnh / Thành Phố" });
                result.Add(new Guide { Code = "{TenLienHe}", Name = "Tên người liên hệ" });
                result.Add(new Guide { Code = "{EmailLienHe}", Name = "Email người liên hệ" });
                result.Add(new Guide { Code = "{DienThoaiLienHe}", Name = "Số Điện thoại người liên hệ" }); 
            }
            else if (type == 7)//Mẫu thông báo trượt phỏng vấn
            {
                result.Add(new Guide { Code = "{HoVaTen}", Name = "Họ và tên" });
                result.Add(new Guide { Code = "{ViTriUngTuyen}", Name = "Vị trí ứng tuyển" });
            }
            else if (type == 8)//Mẫu biên lai
            {
                result.Add(new Guide { Code = "{HoVaTen}", Name = "Họ và tên" });
                result.Add(new Guide { Code = "{MaHocVien}", Name = "Mã học viên" });
                result.Add(new Guide { Code = "{Ngay}", Name = "Ngày" });
                result.Add(new Guide { Code = "{Thang}", Name = "Tháng" });
                result.Add(new Guide { Code = "{Nam}", Name = "Năm" });
                result.Add(new Guide { Code = "{NguoiChi}", Name = "Người Chi" });
                result.Add(new Guide { Code = "{SanPham}", Name = "Sản phẩm" });
                result.Add(new Guide { Code = "{SoLuong}", Name = "Số lượng" });
                result.Add(new Guide { Code = "{DonGia}", Name = "Đơn giá" });
                result.Add(new Guide { Code = "{ThanhTien}", Name = "Thành tiền" });
                result.Add(new Guide { Code = "{TongSoLuong}", Name = "Tổng số lượng" });
                result.Add(new Guide { Code = "{TienTruocKM}", Name = "Tiền trước khuyến mãi" });
                result.Add(new Guide { Code = "{TienKM}", Name = "Tiền khuyến mãi" });
                result.Add(new Guide { Code = "{TienSauKM}", Name = "Tiền sau khuyến mãi" });
            }
            return result;
        }
        public static async Task<tbl_Template> Update(TemplateUpdate itemModel, tbl_UserInformation user)
        {
            using (var db = new lmsDbContext())
            {
                var entity = await db.tbl_Template.FirstOrDefaultAsync(x => x.Type == itemModel.Type);
                if (entity == null)
                {
                    entity = new tbl_Template(itemModel);
                    entity.CreatedBy = entity.ModifiedBy = user.FullName;
                    entity.CreatedOn = entity.ModifiedOn = DateTime.Now;
                    entity.Enable = true;
                    db.tbl_Template.Add(entity);
                    await db.SaveChangesAsync();
                }
                else
                {
                    entity.Content = itemModel.Content ?? entity.Content;
                    entity.ModifiedBy = user.FullName;
                    entity.ModifiedOn = DateTime.Now;
                    await db.SaveChangesAsync();
                }
                return entity;
            }
        }
        public static async Task<tbl_Template> GetByType(int type)
        {
            var data = await GetAll();
            return data.FirstOrDefault(x => x.Type == type);
        }
        public static async Task<List<tbl_Template>> GetAll()
        {
            using (var db = new lmsDbContext())
            {
                var data = await db.tbl_Template.Where(x => x.Enable == true).ToListAsync();
                if (!data.Any(x => x.Type == 1))
                    data.Add(new tbl_Template { Type = 1, TypeName = "Hợp đồng", Content = "", Enable = true });
                if (!data.Any(x => x.Type == 2))
                    data.Add(new tbl_Template { Type = 2, TypeName = "Điều khoản", Content = "", Enable = true });
                if (!data.Any(x => x.Type == 3))
                    data.Add(new tbl_Template { Type = 3, TypeName = "Phiếu thu", Content = "", Enable = true });
                if (!data.Any(x => x.Type == 4))
                    data.Add(new tbl_Template { Type = 4, TypeName = "Phiếu chi", Content = "", Enable = true });
                //if (!data.Any(x => x.Type == 5))
                //    data.Add(new tbl_Template { Type = 5, TypeName = "Thư mời phỏng vấn", Content = "", Enable = true });
                //if (!data.Any(x => x.Type == 6))
                //    data.Add(new tbl_Template { Type = 6, TypeName = "Thông báo trúng tuyển", Content = "", Enable = true });
                //if (!data.Any(x => x.Type == 7))
                //    data.Add(new tbl_Template { Type = 7, TypeName = "Thông báo kết quả phỏng vấn", Content = "", Enable = true });
                //if (!data.Any(x => x.Type == 8))
                //    data.Add(new tbl_Template { Type = 8, TypeName = "Biên lai thu tiền", Content = "", Enable = true });
                return data;
            }
        }
    }
}