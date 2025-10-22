using System;
using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels
{
    public class UserVM
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = "";

        [StringLength(50)]
        public string LastName { get; set; } = "";

        [Required, StringLength(30)]
        public string Nickname { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; } = "";

        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
