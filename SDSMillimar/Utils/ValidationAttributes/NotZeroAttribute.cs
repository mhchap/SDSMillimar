using System.ComponentModel.DataAnnotations;

namespace SDSMillimar.Utils.ValidationAttributes
{
    public class NotZeroAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null)
                return new ValidationResult($"{context.DisplayName}不能为空");

            if (value is long l && l <= 0)
                return new ValidationResult($"请选择{context.DisplayName}");

            return ValidationResult.Success;
        }
    }
}
