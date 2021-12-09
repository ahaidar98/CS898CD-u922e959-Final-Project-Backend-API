using System;
using System.ComponentModel.DataAnnotations;

namespace BabAl_SalamWebAPI.Models
{
    public class User
    {
        [EmailAddress]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Id { get; internal set; }
        public bool Status { set; get; }
        public bool IsActive { set; get; }
        public string Location { set; get; }
    }

    public class UserDTO
    {
        [EmailAddress]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Id { get; internal set; }
        public bool Status { set; get; }
        public bool IsActive { set; get; }
        public string Location { set; get; }
    }
}
