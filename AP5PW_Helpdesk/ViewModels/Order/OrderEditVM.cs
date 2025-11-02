using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels.Order
{
	public class OrderEditVM
	{
		public int Id					{ get; set; }

		[Display(Name = "Company"), Required]
		public int CompanyId			{ get; set; }
		public string CompanyName		{ get; set; } = "";

		[Display(Name = "By user"), Required]
		public int UserId				{ get; set; }
		public string UserName			{ get; set; } = "";	

		[Display(Name = "Date of expedition")]
		[DataType(DataType.Date)]
		public DateTime? ExpeditionDate { get; set; }

		[Display(Name = "Buy Order")]
		public bool IsBuyOrder			{ get; set; } = true;

		[Display(Name = "Warehouse"), Required(ErrorMessage = "Choose warehouse.")]
		public int WarehouseId			{ get; set; }
		public string WarehouseName		{ get; set; } = "";

		[MinLength(1, ErrorMessage = "The order need to have atleast one item.")]
		public List<OrderItemVM> Items	{ get; set; } = [];
	}
}
