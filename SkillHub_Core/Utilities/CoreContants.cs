using Microsoft.AspNetCore.Mvc.Formatters;
using OfficeOpenXml.Packaging.Ionic.Zip;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static LMSCore.Models.lmsEnum;

namespace LMSCore.Utilities
{
    public class CoreContants
    {
        public static List<int> ListStaffEnum = new List<int> {
            ((int)RoleEnum.admin),
            ((int)RoleEnum.teacher),
            ((int)RoleEnum.manager),
            ((int)RoleEnum.sale),
            ((int)RoleEnum.accountant),
            ((int)RoleEnum.academic),
            ((int)RoleEnum.tutor),
        };

        public static bool IsStaff(int role)
        {
            return ListStaffEnum.Contains(role);
        }
        public static bool ExistsBranch(string branchIds1, string branchIds2)
        {
            if (string.IsNullOrEmpty(branchIds1) || string.IsNullOrEmpty(branchIds2))
                return false;
            var listBranchId1 = branchIds1.Split(',').ToList();
            var listBranchId2 = branchIds2.Split(',').ToList();
            foreach (var item1 in listBranchId1)
            {
                foreach (var item2 in listBranchId2)
                {
                    if (item1 == item2)
                        return true;
                }
            }
            return false;
        }
        public static bool UserNameFormat(string value)
        {
            string[] arr = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ"," ",};
            foreach (var item in arr)
            {
                if (value.Contains(item))
                    return false;
            }
            return true;
        }
        public static string GetRoleName(int roleId)
        {
            return roleId == ((int)RoleEnum.admin) ? "Admin"
                  : roleId == ((int)RoleEnum.teacher) ? "Giáo viên"
                  : roleId == ((int)RoleEnum.student) ? "Học viên"
                  : roleId == ((int)RoleEnum.manager) ? "Quản lý"
                  : roleId == ((int)RoleEnum.sale) ? "Tư vấn viên"
                  : roleId == ((int)RoleEnum.accountant) ? "Kế toán"
                  : roleId == ((int)RoleEnum.academic) ? "Học vụ"
                  : roleId == ((int)RoleEnum.parents) ? "Phụ huynh"
                  : roleId == ((int)RoleEnum.tutor) ? "Trợ giảng"
                  : "";
        }
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            string pattern = @"^(0[3|5|7|8|9])+([0-9]{8})$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(phoneNumber);
        }
        public static bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }
    }
}
