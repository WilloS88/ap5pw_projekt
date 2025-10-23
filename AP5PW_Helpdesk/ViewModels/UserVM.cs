using System.ComponentModel.DataAnnotations;
namespace AP5PW_Helpdesk.ViewModels
{
    public class UserVM
    {
        public int Id                   { get; set; }

        [Required, StringLength(50)]
        public string UserName          { get; set; } = "";

        [StringLength(50)]
        public string LastName          { get; set; } = "";

        [Required]
        public int RoleId               { get; set; }

        public string RoleName          { get; set; } = "";

		public int? CompanyId           { get; set; }

		public string? CompanyName      { get; set; } = "";

		[DataType(DataType.Password)]
        public string? Password         { get; set; }
    }
}
