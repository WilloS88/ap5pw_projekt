namespace AP5PW_Helpdesk.Entities
{
	public class User
	{
		public int Id						{ get; set; }
		public string UserName				{ get; set; } = "";
		public string LastName				{ get; set; } = "";
		public string Password				{ get; set; } = "";
		public string? PasswordHash			{ get; set; } = "";
		public int RoleId					{ get; set; }
		public Role Role					{ get; set; } = null!;
		public int? CompanyId				{ get; set; }
		public Company? Company				{ get; set; }
		public ICollection<Order> Orders	{ get; set; } = [];
	}
}
