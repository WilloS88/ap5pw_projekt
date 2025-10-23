namespace AP5PW_Helpdesk.Entities
{
	public class WarehouseGood
	{
		public int Id { get; set; }
		public int Quantity { get; set; }

		public int WarehouseId { get; set; }
		public Warehouse Warehouse { get; set; } = null!;

		public int GoodsId { get; set; }
		public Good Goods { get; set; } = null!;
	}
}
