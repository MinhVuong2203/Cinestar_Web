using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Web.Attributes;

public class RequiredSelectAttribute : ValidationAttribute, IClientModelValidator
{
    public void AddValidation(ClientModelValidationContext context)
    {
        // Kích hoạt rule "data-val-requiredselect"
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-requiredselect", ErrorMessage ?? "Vui lòng chọn giá trị hợp lệ.");
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult(ErrorMessage ?? "Vui lòng chọn giá trị hợp lệ.");

        if (value.ToString().Trim() == "0" || value.ToString().StartsWith("--"))
            return new ValidationResult(ErrorMessage ?? "Vui lòng chọn giá trị hợp lệ.");

        return ValidationResult.Success;
    }

    private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key)) return false;
        attributes.Add(key, value);
        return true;
    }
}

