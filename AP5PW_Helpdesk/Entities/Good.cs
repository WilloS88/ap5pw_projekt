namespace AP5PW_Helpdesk.Entities
{
	public class Good
	{
		public int Id				{ get; set; }
		public string Name			{ get; set; } = "";
		public decimal Price		{ get; set; }
		public string? ProductNum	{ get; set; }

		public ICollection<OrderedGoods> OrderedGoods		{ get; set; } = [];
		public ICollection<WarehouseGood> WarehousesGoods	{ get; set; } = [];
	}
}
