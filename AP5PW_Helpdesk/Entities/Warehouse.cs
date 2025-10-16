namespace AP5PW_Helpdesk.Entities
{
	public class Warehouse
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";

		public int CompanyId { get; set; }
		public Company Company { get; set; } = null!;

		public ICollection<WarehouseGood> WarehousesGoods { get; set; } = [];
	}
}
