using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BabAl_SalamWebAPI.Models
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string TokenExpiration { set; get; }
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public UserDTO User { get; set; }
    }
}
