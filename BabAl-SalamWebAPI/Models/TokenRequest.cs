using System;
using System.ComponentModel.DataAnnotations;

namespace BabAl_SalamWebAPI.Models
{
    public class TokenRequest
    {
        [Required]
        public string Token { set; get; }
        [Required]
        public string RefreshToken { set; get; }
    }
}
