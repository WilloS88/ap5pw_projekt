namespace AP5PW_Helpdesk.Entities
{
	public class Company
	{
		public int Id				{ get; set; }
		public string Name			{ get; set; } = "";
		public string? Street		{ get; set; }
		public string? City			{ get; set; }
		public string? Postcode		{ get; set; }

		public ICollection<User> Users				{ get; set; } = [];
		public ICollection<Order> Orders			{ get; set; } = [];
		public ICollection<Warehouse> Warehouses	{ get; set; } = [];
	}
}
