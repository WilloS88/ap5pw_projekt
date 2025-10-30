using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels.Auth
{
	public class RegisterVM
	{
		[Required]
		[Display(Name = "Username")]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3–50 characters.")]
		public string UserName				{ get; set; } = "";

		[Required]
		[DataType(DataType.Password)]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
		public string Password				{ get; set; } = "";

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
		public string ConfirmPassword		{ get; set; } = "";

		[Display(Name = "Last name")]
		public string? LastName				{ get; set; }

		[Display(Name = "Company")]
		public int? CompanyId				{ get; set; }

		// Volitelný seznam firem (pro select)
		public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? CompanyList { get; set; }
	}
}
