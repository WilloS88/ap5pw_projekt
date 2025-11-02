using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels.Order
{
	public class OrderItemVM
	{
		[Required]
		public int GoodId		{ get; set; }
		public string GoodName	{ get; set; } = "";

		[Range(1, int.MaxValue, ErrorMessage = "The quantity must be atleast 1")]
		public int Quantity		{ get; set; } = 1;
	}
}
