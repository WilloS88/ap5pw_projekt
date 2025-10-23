using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels
{
	public class CompanyVM
	{
		public int Id			{ get; set; }

		[Required]
		public string Name		{ get; set; } = "";

		[StringLength(100)]
		public string? Street	{ get; set; }

		[Required]
		[StringLength(100)]
		public string? City		{ get; set; }

		[Required]
		[StringLength(20)]
		public string? Postcode { get; set; }
	}
}
