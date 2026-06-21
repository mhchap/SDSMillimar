using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SDSMillimar.Utils
{
    public static class ValidateEntity
    {
        public static bool Validate(this object entity, out string errorMessage)
        {
            var context = new ValidationContext(entity);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                entity,
                context,
                results,
                validateAllProperties: true);

            errorMessage = isValid
                ? null
                : string.Join("\n", results.Select(r => r.ErrorMessage));

            return isValid;
        }
    }
}
