using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;using Microsoft.AspNetCore.Mvc;using LMSCore.Utilities;

namespace LMSCore.Areas.Request
{
    public class TimeRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime? sDateValue = (DateTime?)value;
            DateTime? eDateValue = (DateTime?)validationContext.ObjectType.GetProperty("EndTime")?.GetValue(validationContext.ObjectInstance);

            if (sDateValue.HasValue && eDateValue.HasValue && sDateValue >= eDateValue)
            {
                return new ValidationResult("Giờ bắt đầu không lớn hơn giờ kết thúc");
            }

            return ValidationResult.Success;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CurrencyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }
            if (value is double doubleValue && doubleValue < 0)
            {
                return new ValidationResult(ErrorMessage ?? "Vui lòng nhập giá tiền không âm");
            }
            return ValidationResult.Success;
        }
    }
}