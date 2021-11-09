using System;
using System.ComponentModel.DataAnnotations;

namespace BabAl_SalamWebAPI.Models
{
    public class UserLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
