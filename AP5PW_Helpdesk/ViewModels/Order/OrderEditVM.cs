using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels.Order
{
	public class OrderEditVM
	{
		public int Id					{ get; set; }

		[Display(Name = "Firma"), Required]
		public int CompanyId			{ get; set; }
		public string CompanyName		{ get; set; } = "";

		[Display(Name = "Zadal uživatel"), Required]
		public int UserId				{ get; set; }
		public string UserName			{ get; set; } = "";

		[Display(Name = "Datum expedice")]
		[DataType(DataType.Date)]
		public DateTime? ExpeditionDate { get; set; }

		[Display(Name = "Nákupní objednávka")]
		public bool IsBuyOrder			{ get; set; } = true;

		[Display(Name = "Sklad"), Required(ErrorMessage = "Vyberte sklad.")]
		public int WarehouseId			{ get; set; }
		public string WarehouseName		{ get; set; } = "";

		[MinLength(1, ErrorMessage = "Objednávka musí mít alespoň jednu položku.")]
		public List<OrderItemVM> Items	{ get; set; } = [];
	}
}
