using System;

namespace AP5PW_Helpdesk.ViewModels.Order
{
	public class OrderVM
	{
		public int Id						{ get; set; }

		public int CompanyId				{ get; set; }
		public string CompanyName			{ get; set; } = "";

		public int UserId					{ get; set; }
		public string UserName				{ get; set; } = "";

		public DateTime? ExpeditionDate		{ get; set; }
		public bool IsBuyOrder				{ get; set; }

		public int ItemsCount				{ get; set; }
		public decimal? TotalPrice			{ get; set; }
	}
}
