namespace AP5PW_Helpdesk.Entities
{
	public class Order
	{
		public int Id									{ get; set; }
		public DateTime? ExpeditionDate					{ get; set; }
		public bool IsBuyOrder							{ get; set; }

		public int UserId								{ get; set; }
		public User User								{ get; set; } = null!;

		public int CompanyId							{ get; set; }
		public Company Company							{ get; set; } = null!;
		public int WarehouseId							{ get; set; }
		public Warehouse Warehouse						{ get; set; } = null!;

		public ICollection<OrderedGoods> OrderedGoods	{ get; set; } = [];
	}
}
