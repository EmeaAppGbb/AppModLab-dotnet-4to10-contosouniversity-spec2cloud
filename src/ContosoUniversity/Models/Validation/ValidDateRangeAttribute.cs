using System;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.Models.Validation
{
    public class ValidDateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date == DateTime.MinValue || date == default(DateTime))
                    return new ValidationResult("Please enter a valid date.");
                if (date < new DateTime(1753, 1, 1) || date > new DateTime(9999, 12, 31))
                    return new ValidationResult("Date must be between 1753 and 9999.");
            }
            return ValidationResult.Success;
        }
    }
}
