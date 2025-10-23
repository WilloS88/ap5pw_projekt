using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels
{
	public class GoodVM
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; } = "";

		[Required]
		[Range(0.01, 999999.99, ErrorMessage = "Cena musí být kladná.")]
		[DataType(DataType.Currency)]
		public decimal Price { get; set; }

		[Required]
		public string? ProductNum { get; set; }
	}
}
