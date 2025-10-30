using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels.Auth
{
	public class LoginVM
	{
		[Required]
		public string UserName		{ get; set; } = "";

		[Required, DataType(DataType.Password)]
		public string Password		{ get; set; } = "";

		public string? ReturnUrl	{ get; set; }
	}
}
