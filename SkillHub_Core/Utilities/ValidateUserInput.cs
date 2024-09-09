using System.Text.RegularExpressions;

namespace LMSCore.Utilities
{
    public class ValidateUserInput
    {
        // Check email
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            try
            {
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        // Chech số điện thoại
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            // Biểu thức chính quy kiểm tra số điện thoại từ 9 đến 11 chữ số
            string pattern = @"^[0-9]{9,11}$";
            if (Regex.IsMatch(phoneNumber, pattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Kiểm tra formartDate từ file excel
        public class FormartDate
        {
            public string[] formats = {"dd/MM/yyyy",
                                        "MM/dd/yyyy",
                                        "yyyy-MM-dd",
                                        "dd-MM-yyyy",
                                        "MM-dd-yyyy",
                                        "d/M/yyyy",
                                        "M/d/yyyy",
                                        "yyyy/M/d",
                                        "d-M-yyyy",
                                        "M-d-yyyy",
                                        "dd/MM/yy",
                                        "MM/dd/yy",
                                        "yyyy-MM-dd",
                                        "dd-MM-yy",
                                        "MM-dd-yy",
                                        "d/M/yy",
                                        "M/d/yy",
                                        "yyyy/M/d",
                                        "d-M-yy",
                                        "M-d-yy"};
        }

    }
}
