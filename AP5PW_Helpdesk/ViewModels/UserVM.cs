<<<<<<< HEAD
﻿using System;
=======
﻿    using System;
>>>>>>> 6432ee709d2c5dafc8d6df14538df5b017f4a644
using System.ComponentModel.DataAnnotations;

namespace AP5PW_Helpdesk.ViewModels
{
    public class UserVM
    {
        public int Id { get; set; }

<<<<<<< HEAD
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
=======
        [Required, StringLength(50)] 
        public string Username          { get; set; } = "";

        [StringLength(50)] 
        public string LastName          { get; set; } = "";

        [Required, StringLength(30)] 
        public string Nickname          { get; set; } = "";

        [Required, EmailAddress] 
        public string Email             { get; set; } = "";
        
        [Required]
        public int RoleId               { get; set; }

        [Required]
        public string RoleName          { get; set; } = "";
>>>>>>> 6432ee709d2c5dafc8d6df14538df5b017f4a644
    }
}
