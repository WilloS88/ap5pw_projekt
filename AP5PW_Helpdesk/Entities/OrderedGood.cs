namespace AP5PW_Helpdesk.Entities

{
	public class OrderedGoods
	{
		public int Id { get; set; }
		public int Quantity { get; set; }

		public int GoodsId { get; set; }
		public Goods Goods { get; set; } = null!;

		public int OrderId { get; set; }
		public Order Order { get; set; } = null!;
	}
}
