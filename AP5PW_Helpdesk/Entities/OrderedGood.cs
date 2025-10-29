namespace AP5PW_Helpdesk.Entities

{
	public class OrderedGoods
	{
		public int Id			{ get; set; }

		public int GoodId		{ get; set; }
		public Good Good		{ get; set; } = null!;

		public int OrderId		{ get; set; }
		public Order Order		{ get; set; } = null!;

		public int Quantity		{ get; set; }
	}
}
