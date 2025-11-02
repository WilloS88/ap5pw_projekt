using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.RegularExpressions;

namespace AP5PW_Helpdesk.Validation
{
	public class MustContainNumberAttribute : ValidationAttribute, IClientModelValidator
	{
		public MustContainNumberAttribute()
		{
			ErrorMessage = "The password needs to contain atleast one number (0-9).";
		}

		// SERVER VALIDACE
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
				return ValidationResult.Success;

			string str = value.ToString() ?? "";
			if (!Regex.IsMatch(str, @"\d"))
			{
				return new ValidationResult(ErrorMessage);
			}

			return ValidationResult.Success;
		}

		public void AddValidation(ClientModelValidationContext context)
		{
			MergeAttribute(context.Attributes, "data-val", "true");
			MergeAttribute(context.Attributes, "data-val-mustcontainnumber", ErrorMessage!);
		}

		private void MergeAttribute(IDictionary<string, string> attrs, string key, string value)
		{
			if (!attrs.ContainsKey(key))
			{
				attrs.Add(key, value);
			}
		}
	}
}
