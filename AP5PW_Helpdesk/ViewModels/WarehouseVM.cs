using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels
{
	public class WarehouseVM
	{
		public int Id					{ get; set; }

		[Required, StringLength(200)]
		public string Name				{ get; set; } = "";

		[Display(Name = "Company")]
		[Required]
		public int CompanyId			{ get; set; }

		public string CompanyName		{ get; set; } = "";
	}

	public class WarehouseDetailVM : WarehouseVM
	{
		public List<WarehouseStockItemVM> Stock { get; set; } = [];
	}

	public class WarehouseStockItemVM
	{
		public int GoodId		{ get; set; }
		public string GoodName	{ get; set; } = "";
		public int Quantity		{ get; set; }
	}
}
